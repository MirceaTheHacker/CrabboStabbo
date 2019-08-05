using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefController : NPCControllerAbstract
{
    public float m_AttackRange = 2f;

    private bool m_Aggroed = false;
    private float m_ChaseSpeed;
    private float m_NormalSpeed;

    protected override void Awake() {
        base.Awake();
        m_Animator.SetBool("IsWalking",true);
        m_Animator.SetFloat("LookX",m_LookingDirection.x);
        m_ChaseSpeed = movementSpeed * 3;
        m_NormalSpeed = movementSpeed;
    }

    private void Update() {
        if(!isAlive) return;
        AttackChecker();
    }

    protected override IEnumerator TurnAroundCoroutine(){
        m_Animator.SetBool("IsWalking",false);
        yield return StartCoroutine(base.TurnAroundCoroutine());
        m_Animator.SetBool("IsWalking",true);
        m_Animator.SetFloat("LookX", m_LookingDirection.x);
    }

    protected override void OnDeath() {
        m_Animator.SetTrigger("Dies");
        base.OnDeath();
    }
    
    private void AttackChecker() {
        RaycastHit2D hitDown = Physics2D.Raycast(m_ProximitySensor.position,Vector2.down, Mathf.Infinity,
        LayerMask.GetMask("Player"));
        RaycastHit2D hitUp = Physics2D.Raycast(m_ProximitySensor.position,Vector2.up, Mathf.Infinity,
        LayerMask.GetMask("Player"));
        if(hitDown.collider != null || hitUp.collider != null) {
            float distanceToPlayer = 0f;
            PlayerController playerController = null;
            if(hitDown.collider !=null){
                GetAttackParameters(hitDown, out distanceToPlayer, out playerController);
            } else if (hitUp.collider != null) {
                GetAttackParameters(hitUp, out distanceToPlayer, out playerController);
            }
            if(WithinRange(distanceToPlayer)) {
                m_Animator.SetFloat("YChecker", distanceToPlayer);
                m_Animator.SetTrigger("Attack");
                AttackPlayer(playerController);
            }
        }
    }

    private void GetAttackParameters (RaycastHit2D hit, out float distanceToPlayer, out PlayerController playerController) {
        distanceToPlayer = 0f;
        playerController = null;
        distanceToPlayer = hit.collider.transform.position.y - m_ProximitySensor.position.y;
        playerController = hit.collider.gameObject.GetComponent<PlayerController>();
    }

    private bool WithinRange(float distance) {
        if(Mathf.Abs(distance) < m_AttackRange) {
            return true;
        } else return false;
    }

    protected override void PlayerDetectedHandler(){
        if(!m_LockedOnPlayer){
        StartCoroutine(AggroManagement());
        StartCoroutine(LockOnPlayer());
        }
    }

    private IEnumerator AggroManagement() {
        if(!m_Aggroed) {
            m_Aggroed = true;
            yield return new WaitForSeconds(1f);
            movementSpeed = m_ChaseSpeed;
            while (m_LockedOnPlayer) {
                while (m_PlayerManager.m_IsImmune) {
                    movementSpeed = m_NormalSpeed;
                    yield return new WaitForFixedUpdate();
                }
                movementSpeed = m_ChaseSpeed;
                yield return new WaitForFixedUpdate();
            }
            movementSpeed = m_NormalSpeed;
            m_Aggroed = false;
        }
    }

    internal override void TurnAround(){
        base.TurnAround();
        m_Animator.SetFloat("LookX", m_LookingDirection.x);
    }
}
