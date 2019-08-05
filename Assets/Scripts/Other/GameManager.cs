using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;

    internal UIManager m_UIManager;
    internal GravityManager m_GravityManager;

    private void Awake() {
        if(Instance == null) {
            Instance = this;
            m_UIManager = GetComponent<UIManager>();
            m_GravityManager = GetComponent<GravityManager>();
        } else if (Instance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
}
