using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class NPCControllerAbstract : MonoBehaviour
{
    public float movementSpeed = 3f;
    public Transform m_ProximitySensor;
    public int m_HealthPoints = 3;
    public int m_Strength = 1;
    public float m_PushingCoefficient = 4f;
    public GameObject[] m_PlayerDetectors;
    public GameObject m_DetectingTrigger;
    public float m_StayLockedOnPlayerTime = 3f;
    public float m_SightDistance = 6f;
    public GameObject m_SpawnableKnive;
    public GameObject m_KniveSpawnPoint;

    protected Rigidbody2D m_Rigidbody2D;
    protected internal Vector2 m_LookingDirection = new Vector2(1,0);

    protected Animator m_Animator;
    protected bool isAlive = true;
    private Color alphaColor;
    private float fadeDuration = 1f;
    private bool m_Wait = false;
    private NPCFXAbstract m_FXManager;
    private CapsuleCollider2D m_DetectingTriggerCapsuleCollider;
    private bool m_LavaDamageCooldown = false;
    internal PlayerManager m_PlayerManager;
    internal bool m_LockedOnPlayer = false;


    protected virtual void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_FXManager = GetComponent<NPCFXAbstract>();
        m_DetectingTriggerCapsuleCollider  = m_DetectingTrigger.GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate() {
        if (!isAlive || (m_Wait && (!m_LockedOnPlayer))) return;
        if(CanWalk() || ((m_LockedOnPlayer) && CanFollow()))
        {
            m_Rigidbody2D.transform.position += new Vector3(m_LookingDirection.x,0f,0f) * movementSpeed * Time.deltaTime;
        } else if (!m_LockedOnPlayer){
            StartCoroutine(TurnAroundCoroutine());
        }
    }

    protected internal void DetectorsSwitcher() {
        m_ProximitySensor.transform.localPosition = new Vector2(-m_ProximitySensor.transform.localPosition.x,
        m_ProximitySensor.transform.localPosition.y);
        m_DetectingTriggerCapsuleCollider.offset = new Vector2(-m_DetectingTriggerCapsuleCollider.offset.x, 
        m_DetectingTriggerCapsuleCollider.offset.y);
    }

    protected virtual IEnumerator TurnAroundCoroutine() {
        m_Wait = true;
        bool locked = false;
        float randomWait = Random.Range(3f, 5f);
        float startTime = Time.time;
        while (Time.time - startTime < randomWait) {
            if (m_LockedOnPlayer) {
                locked = true;
                m_Wait = false;
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        if(!locked) {
            TurnAround();
        }
        m_Wait = false;
    }

    internal virtual void TurnAround() {
        m_LookingDirection = -m_LookingDirection;
        DetectorsSwitcher();
    }

    private void OnCollisionStay2D(Collision2D other) {
        if(other.gameObject.tag == "Player") {
            m_PlayerManager = other.gameObject.GetComponent<PlayerManager>();
            m_PlayerManager.Attacked(m_Strength, gameObject.transform);
            if(!m_LockedOnPlayer) {
                PlayerDetectedHandler();
            }
        }
    }

    private IEnumerator LavaDamageOverTime() {
        if (!m_LavaDamageCooldown){
            m_LavaDamageCooldown = true;
            Damage(1);
            yield return new WaitForSeconds(1);
            m_LavaDamageCooldown = false;
        }

    }

    protected void AttackPlayer(PlayerController playerController) {
        playerController.m_PlayerManager.Attacked(m_Strength, gameObject.transform);  
    }

    private bool CanWalk() {
        if(WalkingIntoSomething()) return false;
        RaycastHit2D hit = Physics2D.Raycast(m_ProximitySensor.position, Vector2.down, 2f, LayerMask.GetMask("Ground","Lava"));
        if(hit.collider != null) {
            if(hit.collider.tag == "Ground") {
            return true;
            }
        }
        return false;
    }

    private bool CanFollow() {
        if(WalkingIntoSomething()) return false;
        RaycastHit2D hit = Physics2D.Raycast(m_ProximitySensor.position, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground","Lava"));
        if(hit.collider != null) {
            if(hit.collider.tag == "Ground") {
            return true;
            }
        }
        return false;
    }

    private bool WalkingIntoSomething(){
        RaycastHit2D hit = Physics2D.Raycast(m_ProximitySensor.position, Vector2.down, 0f, LayerMask.GetMask("Ground","Enemy"));
        if (hit.collider != null) {
            return true;
        } 
        return false;
    }

    protected virtual void Damage(int hitStrenght) {
        m_HealthPoints -= hitStrenght;
        if(m_HealthPoints > 0) {
            m_FXManager.PlayHitSoundFX();
        } else if (isAlive) {
            OnDeath();
        }
    }

    protected virtual void OnDeath() {
        m_FXManager.PlayDeathSoundFX();
        SetAllCollidersStatus(false);
        m_Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        StartCoroutine(Utils.FadeGameObject(gameObject, fadeDuration));
        isAlive = false;
        Instantiate(m_SpawnableKnive, m_KniveSpawnPoint.transform.position, Quaternion.identity);
    }

    private void SetAllCollidersStatus(bool active) {
        foreach (Collider2D c in GetComponentsInChildren<Collider2D> ())
        {
            c.enabled = active;
        }
    }

    private void Push(float hitStrength) {
        Vector2 pushDirection = new Vector2(gameObject.transform.position.x - m_PlayerManager.transform.position.x, 0f);
        pushDirection.Normalize();
        float Xcomponent = pushDirection.x * hitStrength * m_Rigidbody2D.mass / 2 * m_PushingCoefficient;
        Vector2 pushVector = new Vector2(Xcomponent,
             hitStrength * m_Rigidbody2D.mass / 2 * m_PushingCoefficient);
        m_Rigidbody2D.AddForce(pushVector,ForceMode2D.Impulse);
    }

    internal void Attacked(int hitStrenght, PlayerManager playerManager) {
        m_PlayerManager = playerManager;
        Damage(hitStrenght);
        Push(hitStrenght);
        if(isAlive) {
            PlayerDetectedHandler();
        }
    }

    private bool PlayerDetected() {
        foreach (GameObject playerDetectector in m_PlayerDetectors) {
            Debug.DrawRay(playerDetectector.transform.position, m_LookingDirection * m_SightDistance, Color.red, 1f);
            RaycastHit2D hit = Physics2D.Raycast(playerDetectector.transform.position, m_LookingDirection, 
            Mathf.Infinity, LayerMask.GetMask("Player","Ground"));
            if(hit.collider == null) return false;
            if(hit.collider.tag == "Player") {
                m_PlayerManager = hit.collider.GetComponent<PlayerManager>();
                return true;
            }
        }
        return false;
    }

    internal IEnumerator LockOnPlayer() {
        if(!m_LockedOnPlayer) {
            m_LockedOnPlayer = true;
            float startTime = Time.time;
            while(Time.time - startTime < m_StayLockedOnPlayerTime) {
                if (
                    (gameObject.transform.position.x - m_PlayerManager.transform.position.x > 0 
                && m_LookingDirection.x != -1) 
                || (gameObject.transform.position.x - m_PlayerManager.transform.position.x < 0 
                && m_LookingDirection.x != 1)
                    ){
                    TurnAround();
                    yield return new WaitForSeconds(0.5f);
                }
                if (Mathf.Abs(gameObject.transform.position.x - m_PlayerManager.transform.position.x) > m_SightDistance) break;
                if (PlayerDetected()) {
                    startTime = Time.time; // restart the timer everytime we see the player
                }

                yield return new WaitForEndOfFrame();
            }
            m_LockedOnPlayer = false;
        }
    }

    protected abstract void PlayerDetectedHandler();

    private void OnTriggerStay2D(Collider2D other) {
        if(!m_LockedOnPlayer) {
            if (other.gameObject.tag == "Player") {
                if(PlayerDetected()) {
                    PlayerDetectedHandler();
                }
            }
        }
        if(other.gameObject.tag == "Lava") {
            StartCoroutine(LavaDamageOverTime());
        }
    }
}
