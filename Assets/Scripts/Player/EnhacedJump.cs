using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhacedJump : MonoBehaviour
{

    public float fallMultiplier;
    public float lowJumpMultipler;
    public float whileImmuneMultiplier;

    private PlayerController m_PlayerController;

    Rigidbody2D rbd2;

    void Awake()
    {
        rbd2 = GetComponent<Rigidbody2D>();
        m_PlayerController = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(rbd2.velocity.y < 0 )
        {
            rbd2.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        } else if ((rbd2.velocity.y > 0 && !Input.GetButton("Jump")) )
        {
            rbd2.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultipler - 1f) * Time.deltaTime;
        } else if (m_PlayerController.m_IsImmune) {
            rbd2.velocity += Vector2.up * Physics2D.gravity.y * (whileImmuneMultiplier - 1f) * Time.deltaTime;
        }
    }
}
