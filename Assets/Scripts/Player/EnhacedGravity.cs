using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhacedGravity : MonoBehaviour
{

    public float fallMultiplier;
    public float lowJumpMultipler;
    public float whileImmuneMultiplier;

    private PlayerController m_PlayerController;

    Rigidbody2D m_Rigidbody2D;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PlayerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if(m_Rigidbody2D.velocity.y < 0 )
        {
            Physics2D.IgnoreLayerCollision(8,9, false);
            m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        } else if ((m_Rigidbody2D.velocity.y > 0 && !Input.GetButton("Jump")) )
        {
            m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultipler - 1f) * Time.deltaTime;
        } else if (m_PlayerController.m_IsImmune) {
            m_Rigidbody2D.velocity += Vector2.up * Physics2D.gravity.y * (whileImmuneMultiplier - 1f) * Time.deltaTime;
        }

        if(m_Rigidbody2D.velocity.y > 0) {
            Physics2D.IgnoreLayerCollision(8,9, true);
        }
    }
}
