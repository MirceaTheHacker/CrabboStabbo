using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI m_KniveNumber;
    public GameObject m_GameOverUI;
    public GameObject m_GameWinUI;
    public GameObject m_CreditsUI;
    public GameObject m_StatsUI;
    public AnimationClip m_CreditsAnimation;

    private PlayerManager m_PlayerManager;

        void Update()
    {
        if (m_PlayerManager != null) {
            m_KniveNumber.text = "X " + m_PlayerManager.m_AvailableKnives;
        } else {
            m_StatsUI.SetActive(false);
        }
    }

    internal void SetPlayerController(PlayerManager playerManager) {
        m_StatsUI.SetActive(true);
        m_PlayerManager = playerManager;
        m_PlayerManager.m_HealthManager = GetComponentInChildren<HealthManager>();
        m_KniveNumber.text = "X " + m_PlayerManager.m_AvailableKnives;
    }

    internal void ShowGameOverUI() {
        m_GameOverUI.SetActive(true);
    }

    internal void HideGameOverUI() {
        m_GameOverUI.SetActive(false);
    }

    internal void ShowGameWinUI() {
        m_GameWinUI.SetActive(true);
    }

    internal void HideGameWinUI() {
        m_GameWinUI.SetActive(false);
    }

    internal IEnumerator ShowCreditsUI() {
        m_CreditsUI.SetActive(true);
        yield return new WaitForSeconds(m_CreditsAnimation.length);
        m_CreditsUI.SetActive(false);
        SceneManager.LoadScene(0);
    }
}
