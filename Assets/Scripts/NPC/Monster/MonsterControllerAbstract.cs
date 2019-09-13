using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterControllerAbstract : NPCControllerAbstract
{
    public GameObject m_LittleMonster;
    public GameObject[] m_OnHitParts; 
    public GameObject m_ThrowablePrefab;
    public float m_FireDelay = 2f;
    public GameObject m_SpawnThrowableLocation;

    protected abstract void SpawnEnemies();

    private bool throwing = false;

    private IEnumerator OnHit(){
        SetOnHitBodyParts(true);
        yield return new WaitForSeconds(2);
        SetOnHitBodyParts(false);
    }

    private void SetOnHitBodyParts(bool value) {
        foreach(GameObject part in m_OnHitParts) {
            SpriteRenderer renderer = part.GetComponent<SpriteRenderer> ();
            renderer.enabled = value;
        }
    }

    protected override void OnDeath(){
        base.OnDeath();
        m_Animator.SetBool("IsDead", true);
        SpawnEnemies();
    }

    protected override void Damage(float value){
        base.Damage(value);
        StartCoroutine(OnHit());
    }

    protected override void PlayerDetectedHandler(){
        if (!m_LockedOnPlayer) {
        StartCoroutine(LockOnPlayer());
        StartCoroutine(Throw());
        }
    }

    private IEnumerator Throw(){
        if (!throwing) {
            throwing = true;
            yield return new WaitForSeconds(1f); // give the player 1 second to prepare
            while(m_LockedOnPlayer && isAlive) {
                while (m_PlayerManager.m_IsImmune) {
                    yield return new WaitForEndOfFrame();
                }
                GameObject throwableInstance = Instantiate (m_ThrowablePrefab,
                    m_SpawnThrowableLocation.transform.position,
                    Quaternion.identity);
                MonsterThrowableController throwableController = 
                throwableInstance.GetComponent<MonsterThrowableController>();
                throwableController.SetDirection(new Vector2(m_LookingDirection.x, 0));
                yield return new WaitForSeconds(m_FireDelay);
            } 
            throwing = false;
        }
    }
}
