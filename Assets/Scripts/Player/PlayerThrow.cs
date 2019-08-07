using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    public GameObject m_KnivePrefab;
    public GameObject m_KniveSpawnPoint;
    
    
    private Animator m_Animator;
    private PlayerController m_PlayerController;
    private Rigidbody2D m_Rigidbody2D;
    private PlayerManager m_PlayerManager;

    private void Awake() {
        m_Animator = GetComponent<Animator>();
        m_PlayerController = GetComponent<PlayerController>();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PlayerManager = GetComponent<PlayerManager>();
    }

    private void Update() {
        if (Input.GetButtonDown("Throw")) {
            Throw();
        }
    }

    private void UpdateLookingDirection() {
        if(!Mathf.Approximately(m_PlayerController.m_Horizontal, 0f))
        {
            if (m_PlayerController.m_LookingDirection.x == 1 &&
             m_KniveSpawnPoint.transform.localPosition.x < 0) {
                KnifeLocalSpawnPointSwitcher();
            } if (m_PlayerController.m_LookingDirection.x == -1 &&
             m_KniveSpawnPoint.transform.localPosition.x > 0) {
                KnifeLocalSpawnPointSwitcher();
            }
        }
    }

    private void KnifeLocalSpawnPointSwitcher() {
        m_KniveSpawnPoint.transform.localPosition = new Vector2 (-m_KniveSpawnPoint.transform.localPosition.x,
        m_KniveSpawnPoint.transform.localPosition.y);
    }

    private void Throw() {
        if(m_PlayerManager.m_AvailableKnives <= 0) return;
        m_Animator.SetTrigger("throw");
        GameObject knifeObject = Instantiate(m_KnivePrefab, KnifeNoise(), Quaternion.identity);
        KnifeController knifeController = knifeObject.GetComponent<KnifeController>();
        knifeController.SetDirection(m_PlayerController.m_LookingDirection);
        knifeController.m_PlayerManager = m_PlayerManager;
        m_PlayerManager.m_AvailableKnives--;
    }

    private Vector2 KnifeNoise(){
        float yNoise = 0f;
        yNoise = Random.Range(m_KniveSpawnPoint.transform.position.y - 0.1f, m_KniveSpawnPoint.transform.position.y + 0.1f);
        return new Vector2(m_KniveSpawnPoint.transform.position.x, yNoise);
    }
}
