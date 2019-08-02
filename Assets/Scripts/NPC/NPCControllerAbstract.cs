﻿using System.Collections;
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
    internal PlayerController m_PlayerInfo;
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
        if(CanWalk() || (m_LockedOnPlayer))
        {
            m_Rigidbody2D.transform.position += new Vector3(m_LookingDirection.x,0f,0f) * movementSpeed * Time.deltaTime;
        } else {
            StartCoroutine(TurnAround());
        }
    }

    protected internal void DetectorsSwitcher() {
        m_ProximitySensor.transform.localPosition = new Vector2(-m_ProximitySensor.transform.localPosition.x,
        m_ProximitySensor.transform.localPosition.y);
        m_DetectingTriggerCapsuleCollider.offset = new Vector2(-m_DetectingTriggerCapsuleCollider.offset.x, 
        m_DetectingTriggerCapsuleCollider.offset.y);
    }

    protected virtual IEnumerator TurnAround() {
        m_Wait = true;
        float randomWait = Random.Range(3f, 5f);
        yield return new WaitForSeconds(randomWait);
        m_LookingDirection = -m_LookingDirection;
        DetectorsSwitcher();
        m_Wait = false;
    }

    internal virtual void FastTurn() {
        m_LookingDirection = -m_LookingDirection;
        DetectorsSwitcher();
    }

    private void OnCollisionStay2D(Collision2D other) {
        if(other.gameObject.tag == "Player") {
            m_PlayerInfo = other.gameObject.GetComponent<PlayerController>();
            m_PlayerInfo.Attacked(m_Strength, gameObject.transform);
            if(!m_LockedOnPlayer) {
                StartCoroutine(PlayerDetectedHandler());
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
        playerController.Attacked(m_Strength, gameObject.transform);  
    }

    private bool CanWalk() {
        if(WalkingIntoSomething()) return false;
        RaycastHit2D hit = Physics2D.Raycast(m_ProximitySensor.position, Vector2.down, 2f, LayerMask.GetMask("Ground"));
        if(hit.collider != null) {
            return true;
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
        } else {
            OnDeath();
        }
    }

    protected virtual void OnDeath() {
        m_FXManager.PlayDeathSoundFX();
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

    private void Push(float hitStrength) {
        Vector2 pushDirection = new Vector2(gameObject.transform.position.x - m_PlayerInfo.transform.position.x, 0f);
        pushDirection.Normalize();
        float Xcomponent = pushDirection.x * hitStrength * m_Rigidbody2D.mass / 2 * m_PushingCoefficient;
        Vector2 pushVector = new Vector2(Xcomponent,
             hitStrength * m_Rigidbody2D.mass / 2 * m_PushingCoefficient);
        m_Rigidbody2D.AddForce(pushVector,ForceMode2D.Impulse);
    }

    internal void Attacked(int hitStrenght, PlayerController playerController) {
        m_PlayerInfo = playerController;
        Damage(hitStrenght);
        Push(hitStrenght);
        if(isAlive) {
            StartCoroutine(PlayerDetectedHandler());
        }
    }

    private bool PlayerDetected() {
        foreach (GameObject playerDetectector in m_PlayerDetectors) {
            float distance = 6f;
            Debug.DrawRay(playerDetectector.transform.position, m_LookingDirection * distance, Color.red, 1f);
            RaycastHit2D hit = Physics2D.Raycast(playerDetectector.transform.position, m_LookingDirection, 
            Mathf.Infinity, LayerMask.GetMask("Player","Ground"));
            if(hit.collider == null) return false;
            if(hit.distance < distance && hit.collider.tag == "Player") {
                m_PlayerInfo = hit.collider.GetComponent<PlayerController>();
                return true;
            }
        }
        return false;
    }

    internal IEnumerator LockOnPlayer() {
        if(!m_LockedOnPlayer) {
            m_LockedOnPlayer = true;
            float startTime = Time.time;
            while(Time.time - startTime < m_StayLockedOnPlayerTime && !m_PlayerInfo.m_IsImmune) {
                if (
                    (gameObject.transform.position.x - m_PlayerInfo.transform.position.x > 0 
                && m_LookingDirection.x != -1) 
                || (gameObject.transform.position.x - m_PlayerInfo.transform.position.x < 0 
                && m_LookingDirection.x != 1)
                    ){
                    FastTurn();
                    yield return new WaitForSeconds(0.5f);
                }
                if (Mathf.Abs(gameObject.transform.position.x - m_PlayerInfo.transform.position.x) > m_SightDistance) break;
                if (PlayerDetected()) {
                    startTime = Time.time; // restart the timer everytime we see the player
                }

                yield return new WaitForEndOfFrame();
            }
            m_LockedOnPlayer = false;
        }
       
    }

    protected abstract IEnumerator PlayerDetectedHandler();

    private void OnTriggerStay2D(Collider2D other) {
        if(!m_LockedOnPlayer) {
            if (other.gameObject.tag == "Player") {
                if(PlayerDetected()) {
                    StartCoroutine(PlayerDetectedHandler());
                }
            }
        }
        if(other.gameObject.tag == "Lava") {
            StartCoroutine(LavaDamageOverTime());
        }
    }
}
