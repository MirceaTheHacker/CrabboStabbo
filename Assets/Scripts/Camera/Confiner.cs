using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confiner : MonoBehaviour
{
    internal PolygonCollider2D m_PolygonCollider2D;

    private void Awake() {
        m_PolygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    private void Start() {
       GameManager.Instance.m_Confiner = this;
   }
}
