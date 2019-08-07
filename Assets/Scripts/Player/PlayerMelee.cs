using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    public float m_MeleeRange = 2f;
    public int m_MeleeStrength = 1;
    public float m_MeleeCooldown = 0.5f;
    public float m_MeleeAnimationDuration = 0.1f; // continuesly check for collisions during this time

    private bool m_MeleeInCooldown = false;
    private Animator m_Animator;
    private PlayerController m_PlayerController;
    private PlayerManager m_PlayerManager;

    private void Awake() {
        m_Animator = GetComponent<Animator>();
        m_PlayerController = GetComponent<PlayerController>();
        m_PlayerManager = GetComponent<PlayerManager>();
    }

    private void Update() {
        if (Input.GetButtonDown("Melee")) {
            Melee(); 
        }
    }

    private void Melee() {
        if(m_MeleeInCooldown) return;
        m_PlayerManager.m_PlayerFXManager.MeleeSoundFX();
        m_Animator.SetTrigger("melee");
        StartCoroutine(MeleeCooldown());
        Debug.DrawRay(gameObject.transform.position, m_PlayerController.m_LookingDirection * m_MeleeRange, Color.green, 2f);
        Debug.DrawRay(gameObject.transform.position, Vector2.up * m_PlayerController.m_Image.bounds.size.y, Color.green, 2f);
        MeleeContinuousCollisionChecker();
    }

    private void MeleeContinuousCollisionChecker() {
        float startTime = Time.time;
        while(Time.time - startTime < m_MeleeAnimationDuration) {
            if (HitSomething()) {
            break;
            }
        }
    }

    private IEnumerator MeleeCooldown() {
        m_MeleeInCooldown = true;
        yield return new WaitForSeconds(m_MeleeCooldown);
        m_MeleeInCooldown = false;
    }

    private bool HitSomething() {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(gameObject.transform.position,
         new Vector2(m_MeleeRange, m_PlayerController.m_Image.bounds.size.y), 0f,
         m_PlayerController.m_LookingDirection, m_MeleeRange,
         LayerMask.GetMask("Enemy", "MonsterThrowable"));
         
        ArrayList uniqueEnemies = FilterDuplicateColliders(hits);
        foreach (GameObject enemy in uniqueEnemies) {
            if (enemy.tag == "Enemy") {
                NPCControllerAbstract npcController = enemy.GetComponentInParent<NPCControllerAbstract>();
                npcController.Attacked(m_MeleeStrength, m_PlayerManager);
                npcController.m_PlayerManager = m_PlayerManager;
            } else if (enemy.tag == "MonsterThrowable") {
                MonsterThrowableController throwableController = enemy.GetComponent<MonsterThrowableController>();
                throwableController.OnMeleeHit();
                throwableController.m_PlayerManager = m_PlayerManager;
                m_PlayerManager.m_PlayerFXManager.OnThrowableHitSoundFX();
            }
        }
        if(hits != null) {
            return true;
        } else {
            return false;
        }
    }

    private ArrayList FilterDuplicateColliders(RaycastHit2D[] hits) {
        // we need to check if the colliders found belong to different gameobjects with transform.root
        ArrayList uniqueEnemies = new ArrayList();
        foreach (RaycastHit2D hit in hits) {
            GameObject currentEnemy = hit.collider.gameObject;
            while(currentEnemy.tag == "Untagged") {
                currentEnemy = currentEnemy.transform.parent.gameObject;
            }
            if (!uniqueEnemies.Contains(currentEnemy)) {
                uniqueEnemies.Add(currentEnemy);
            }
        }
        return uniqueEnemies;
    }
}
