using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI m_TextMeshPro;
    public GameObject m_GameOverUI;

    private PlayerManager m_PlayerManager;

        void Update()
    {
        m_TextMeshPro.text = "X " + m_PlayerManager.m_AvailableKnives;
    }

    internal void SetPlayerController(PlayerManager playerManager) {
        m_PlayerManager = playerManager;
        m_PlayerManager.m_HealthManager = GetComponent<HealthManager>();
        m_TextMeshPro.text = "X " + m_PlayerManager.m_AvailableKnives;
    }

    internal void ShowGameOverUI() {
        m_GameOverUI.SetActive(true);
    }

    internal void HideGameOverUI() {
        m_GameOverUI.SetActive(false);
    }
}
