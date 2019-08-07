using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public float movementSpeed = 1f;
    public int m_MaxHealth = 10;
    public int m_CurHealth = 1;
    public int m_AvailableKnives = 5;
    public float immunityTimer = 2f;

    internal PlayerController m_PlayerController;
    internal Rigidbody2D m_Rigidbody2D;
    internal PlayerMelee m_PlayerMelee;
    internal PlayerThrow m_PlayerThrow;
    internal PlayerFXManager m_PlayerFXManager;
    internal HealthManager m_HealthManager;
    internal CapsuleCollider2D m_CapsuleCollider2D;
    internal bool m_IsImmune = false;
    internal bool m_IsAlive = true;

    private void Awake() {
        m_PlayerController = GetComponent<PlayerController>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PlayerFXManager = GetComponent<PlayerFXManager>();
        m_PlayerMelee = GetComponent<PlayerMelee>();
        m_PlayerThrow = GetComponent<PlayerThrow>();
        m_CapsuleCollider2D = GetComponent<CapsuleCollider2D>();
    }

    private void Start() {
        SetUIManager();
        SetGravityManager();
    }

    private void SetGravityManager(){
        GameManager.Instance.m_GravityManager.m_PlayerManager = this;
        GameManager.Instance.m_GravityManager.m_Rigidbody2D = m_Rigidbody2D;
    }

    private void SetUIManager() {
        GameManager.Instance.m_UIManager.SetPlayerController(this);
        SetHealth();
    }

    private void SetHealth() {
        m_HealthManager.maxHealth = m_MaxHealth;
        m_HealthManager.curHealth = m_CurHealth;
        m_HealthManager.InitializeHarts();
    }

    private void Update() {
        CheckIfFellOff();
    }

    private void CheckIfFellOff(){
        if(m_Rigidbody2D.position.y < -10)
        {
            DamagePlayer(1);
            if(m_CurHealth > 0) {
                m_Rigidbody2D.velocity = Vector2.zero;
                m_PlayerController.Respawn();
            }
        }
    }

    public void Attacked (int monsterStrength, Transform NPCTransform) {
        if(m_IsImmune || !m_IsAlive) return;
        Push(monsterStrength, NPCTransform);
        DamagePlayer(monsterStrength);
    }
    
    private void Push(float monsterStrength, Transform NPCTransform) {
        Vector2 pushDirection = new Vector2(gameObject.transform.position.x - NPCTransform.position.x, 0f);
        pushDirection.Normalize();
        float Xcomponent = pushDirection.x * monsterStrength * movementSpeed;
        Vector2 pushVector = new Vector2(Xcomponent,
             monsterStrength * movementSpeed / 2f);
        m_Rigidbody2D.AddForce(pushVector,ForceMode2D.Impulse);
        StartCoroutine(RotatePlayer(monsterStrength));
    }

    private void DamagePlayer(int damageValue) {
        if(!m_IsImmune && m_IsAlive)
        {
            m_CurHealth -= damageValue;
            StartCoroutine(ImmunityCooldown());
            m_PlayerFXManager.GettingHitSoundFX();
            if(m_CurHealth <= 0) {
                Restart();
            } else {
                m_HealthManager.UpdateHearts(-damageValue);
            }
        }
    }

    private IEnumerator RotatePlayer(float timer) {
        float startTime = Time.time;
        m_Rigidbody2D.constraints = RigidbodyConstraints2D.None;

        while(Time.time - startTime < timer) {
        m_Rigidbody2D.AddTorque(timer*10,ForceMode2D.Force);
        yield return new WaitForSeconds(0.1f);
        }

        SetStraightY();
    }

    internal void SetStraightY() {
        m_Rigidbody2D.transform.eulerAngles = new Vector3(0f,0f,0f);
        m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private IEnumerator ImmunityCooldown(){
        m_IsImmune = true;
        StartCoroutine(FlashPlayer(immunityTimer));
        StartCoroutine(Stun(immunityTimer));
        Physics2D.IgnoreLayerCollision(8,10,true);
        yield return new WaitForSeconds(immunityTimer);
        Physics2D.IgnoreLayerCollision(8,10,false);
        m_IsImmune = false;
    }

    private IEnumerator FlashPlayer(float timer) {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer> ();
        Color originalColor = spriteRenderer.color;
        Color fadedColor = originalColor;
        fadedColor.a = 0.5f;
        float startTime = Time.time;
        while(Time.time - startTime < timer - 0.15f) {
            spriteRenderer.color = fadedColor;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.15f);
        }
    }

    public IEnumerator Stun(float stunDuration) {
        movementSpeed /= 4;
        yield return new WaitForSeconds(stunDuration);
        movementSpeed *= 4;
    }

    private void OnTriggerStay2D(Collider2D other) {
        if(other.tag == "Lava") {
            DamagePlayer(1);
            StartCoroutine(DelayRespawn());
        }
    }

    private IEnumerator DelayRespawn() {
        yield return new WaitForSeconds(0.5f);
        m_PlayerController.Respawn();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "LootableKnive") {
            Destroy(other.gameObject);
            m_AvailableKnives++;
            m_PlayerFXManager.CollectKniveSoundFX();
        }
    }

    private void HealPlayer(int healingValue) {
        m_HealthManager.UpdateHearts(healingValue);
    }

    private void Restart() {
        m_IsAlive = false;
        DisableScripts();
        m_PlayerFXManager.m_SFXAudioSource.enabled = false;
        m_PlayerFXManager.m_WalkingAudioSource.enabled = false;
        StartCoroutine(GameManager.Instance.GameLoseCoroutine());
    }

    internal void DisableScripts() {
        m_PlayerController.enabled = false;
        m_PlayerMelee.enabled = false;
        m_PlayerThrow.enabled = false;
        this.enabled = false;
    }


}
