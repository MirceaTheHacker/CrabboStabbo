using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChefController : NPCControllerAbstract
{
    public float m_AttackRange = 2f;

    protected override void Awake() {
        base.Awake();
        m_Animator.SetBool("IsWalking",true);
        m_Animator.SetFloat("LookX",m_LookingDirection.x);
    }

    protected override void Update() {
        if(!isAlive) return;
        base.Update();
        AttackChecker();
    }

    protected override IEnumerator TurnAround(){
        m_Animator.SetBool("IsWalking",false);
        yield return StartCoroutine(base.TurnAround());
        m_Animator.SetBool("IsWalking",true);
        m_Animator.SetFloat("LookX", m_LookingDirection.x);
    }

    private IEnumerator WaitForDieingAnimation() {
        m_Animator.SetTrigger("Dies");
        yield return new WaitForSeconds(0.5f);
        base.OnDeath();
    }

    protected override void OnDeath() {
        isAlive = false;
        StartCoroutine(WaitForDieingAnimation());
    }
    
    private void AttackChecker() {
        RaycastHit2D hitDown = Physics2D.Raycast(m_GroundDetector.position,Vector2.down, Mathf.Infinity,
        LayerMask.GetMask("Player"));
        RaycastHit2D hitUp = Physics2D.Raycast(m_GroundDetector.position,Vector2.up, Mathf.Infinity,
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
        distanceToPlayer = hit.collider.transform.position.y - m_GroundDetector.position.y;
        playerController = hit.collider.gameObject.GetComponent<PlayerController>();
    }

    private bool WithinRange(float distance) {
        if(Mathf.Abs(distance) < m_AttackRange) {
            return true;
        } else return false;
    }
}
