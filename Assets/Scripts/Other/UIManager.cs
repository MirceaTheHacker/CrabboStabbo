using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI m_TextMeshPro;

    private PlayerManager m_PlayerManager;

        void Update()
    {
        m_TextMeshPro.text = "X " + m_PlayerManager.m_AvailableKnives;
    }

    internal void SetPlayerController(PlayerManager playerManager) {
        m_PlayerManager = playerManager;
        m_PlayerManager.m_Health = GetComponent<HealthManager>();
        m_TextMeshPro.text = "X " + m_PlayerManager.m_AvailableKnives;
    }
}
