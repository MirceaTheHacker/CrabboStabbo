using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    internal UIManager m_UIManager { get { return GetComponentInChildren<UIManager> (); } }
    internal GravityManager m_GravityManager {get {return GetComponentInChildren<GravityManager>(); } }
    internal SoundManager m_SoundManager {get {return GetComponentInChildren<SoundManager>();}}
    internal Confiner m_Confiner;

    private void Awake() {
        if(Instance == null) {
            Instance = this;
        } else if (Instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    internal IEnumerator GameLoseCoroutine () {
        m_UIManager.ShowGameOverUI();
        m_SoundManager.PlayGameLoseSound();
        m_Confiner.m_PolygonCollider2D.enabled = false;
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        m_UIManager.HideGameOverUI();
    }

    internal void GameWin() {
        m_UIManager.ShowGameWinUI();
        m_SoundManager.PlayGameWinSound();
    }

    public void PlayAgain() {
        m_UIManager.HideGameWinUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PlayCredits() {
        m_UIManager.HideGameWinUI();
        StartCoroutine(m_UIManager.ShowCreditsUI());
    }

    public void PlayGame () {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
    }

    public void QuitGame () {
        Debug.Log ("QUIT!");
        Application.Quit();
    }
}
