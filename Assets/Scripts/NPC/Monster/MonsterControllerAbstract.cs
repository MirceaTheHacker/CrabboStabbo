using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MonsterControllerAbstract : NPCControllerAbstract
{
    public GameObject m_LittleMonster;
    public GameObject[] m_OnHitParts; 

    protected abstract void SpawnEnemies();

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

    protected override void Damage(int value){
        base.Damage(value);
        StartCoroutine(OnHit());
    }
}
