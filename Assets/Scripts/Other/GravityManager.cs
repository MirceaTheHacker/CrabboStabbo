using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{

    public float fallMultiplier;
    public float lowJumpMultipler;
    public float whileImmuneMultiplier;

    internal PlayerManager m_PlayerManager;
    internal Rigidbody2D m_Rigidbody2D;

    void Update()
    {
        if(m_Rigidbody2D.velocity.y < 0 )
        {
            m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        } else if ((m_Rigidbody2D.velocity.y > 0 && !Input.GetButton("Jump")) )
        {
            m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultipler - 1f) * Time.deltaTime;
        } else if (m_PlayerManager.m_IsImmune) {
            m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (whileImmuneMultiplier - 1f) * Time.deltaTime;
        }
    }
}
