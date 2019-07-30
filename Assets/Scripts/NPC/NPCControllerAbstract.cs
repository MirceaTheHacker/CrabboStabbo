using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class NPCControllerAbstract : MonoBehaviour
{
    public float movementSpeed = 3f;
    public Transform m_GroundDetector;
    public int m_HealthPoints = 3;
    public int m_Strength = 1;
    public float m_PushingCoefficient = 2f;

    protected Rigidbody2D m_Rigidbody2D;
    protected internal Vector2 m_LookingDirection = new Vector2(1,0);

    protected Animator m_Animator;
    protected bool isAlive = true;
    private Color alphaColor;
    private float fadeDuration = 1f;
    private bool m_Wait = false;
    private NPCFXAbstract m_FXManager;

    protected virtual void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_FXManager = GetComponent<NPCFXAbstract>();
    }

    protected virtual void Update() {
        if(!isAlive) return;
        if (m_HealthPoints <= 0){
            OnDeath();
        }
    }

    private void FixedUpdate() {
        if (!isAlive || m_Wait) return;
        if(CanWalk())
        {
            m_Rigidbody2D.transform.position += new Vector3(m_LookingDirection.x,0f,0f) * movementSpeed * Time.deltaTime;
        } else {
            StartCoroutine(TurnAround());
        }
    }

    protected internal void GroundDetectorSwitcher() {
        m_GroundDetector.transform.localPosition = new Vector3(-m_GroundDetector.transform.localPosition.x,m_GroundDetector.transform.localPosition.y,0f);
    }

    protected virtual IEnumerator TurnAround() {
        m_Wait = true;
        float randomWait = Random.Range(3f, 5f);
        yield return new WaitForSeconds(randomWait);
        m_LookingDirection = -m_LookingDirection;
        GroundDetectorSwitcher();
        m_Wait = false;
    }

    private void OnCollisionStay2D(Collision2D other) {
        if(other.gameObject.tag == "Player") {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                AttackPlayer(playerController);
            }
        }
    }

    protected void AttackPlayer(PlayerController playerController) {
        playerController.Attacked(m_Strength, gameObject.transform);  
    }

    private bool CanWalk()
    {
        if(WalkingIntoSomething()) return false;
        RaycastHit2D hit = Physics2D.Raycast(m_GroundDetector.position, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        if(hit.collider != null) {
            return true;
        }
        return false;
    }

    private bool WalkingIntoSomething(){
        RaycastHit2D hit = Physics2D.Raycast(m_GroundDetector.position, Vector2.down, 0f, LayerMask.GetMask("Ground","Enemy"));
        if (hit.collider != null) {
            return true;
        } 
        return false;
    }

    protected virtual void Damage(int hitStrenght) {
        m_HealthPoints -= hitStrenght;
        if(m_HealthPoints > 0) {
            m_FXManager.PlayHitSoundFX();
        } else {
            m_FXManager.PlayDeathSoundFX();
        }
    }

    protected virtual void OnDeath() {
        SetAllCollidersStatus(false);
        m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        StartCoroutine(Utils.FadeGameObject(gameObject, fadeDuration));
        isAlive = false;
    }

    private void SetAllCollidersStatus(bool active) {
        foreach (Collider2D c in GetComponentsInChildren<Collider2D> ())
        {
            c.enabled = active;
        }
    }

    private void Push(float hitStrength, Transform HitTransform) {
        Vector2 pushDirection = new Vector2(gameObject.transform.position.x - HitTransform.position.x, 0f);
        pushDirection.Normalize();
        float Xcomponent = pushDirection.x * hitStrength * movementSpeed * m_Rigidbody2D.mass * m_PushingCoefficient;
        Vector2 pushVector = new Vector2(Xcomponent,
             hitStrength * movementSpeed / 2f * m_Rigidbody2D.mass);
        m_Rigidbody2D.AddForce(pushVector,ForceMode2D.Impulse);
    }

    public void Attacked(int hitStrenght, Transform HitTransform) {
        Damage(hitStrenght);
        Push(hitStrenght, HitTransform);
    }
}
