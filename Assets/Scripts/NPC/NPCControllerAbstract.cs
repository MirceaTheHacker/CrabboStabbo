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

    protected Rigidbody2D rb2d;
    protected internal Vector2 m_LookingDirection = new Vector2(1,0);

    protected Animator m_Animator;
    protected bool isAlive = true;
    private Color alphaColor;
    private float fadeDuration = 1f;
    private bool m_Wait = false;

    protected virtual void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
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
            rb2d.transform.position += new Vector3(m_LookingDirection.x,0f,0f) * movementSpeed * Time.deltaTime;
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
        playerController.Attack(m_Strength, gameObject.transform);  
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

    public virtual void Damage(int value) {
        m_HealthPoints -= value;
    }

    protected virtual void OnDeath() {
        SetAllCollidersStatus(false);
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        StartCoroutine(Utils.FadeGameObject(gameObject, fadeDuration));
        isAlive = false;
    }

    private void SetAllCollidersStatus(bool active) {
        foreach (Collider2D c in GetComponentsInChildren<Collider2D> ())
        {
            c.enabled = active;
        }
    }
}
