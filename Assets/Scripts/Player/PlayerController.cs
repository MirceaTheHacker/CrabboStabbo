﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpVelocity = 7f;
    public GameObject[] m_GroundCheckers;

    internal PlayerManager m_PlayerManager;
    internal Vector2 m_LookingDirection = new Vector2(1,0);
    internal SpriteRenderer m_Image;
    internal float m_Horizontal;
    internal Vector2 m_LastGroundedPostion;
    internal Animator m_Animator;
    
    private Rigidbody2D m_Rigidbody2D;
    private bool onASurface = false;
    private string[] m_InteractibleTags = {"Ground", "Enemy", "Knife", "Lava"};
    

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_LastGroundedPostion = m_Rigidbody2D.transform.position;
        m_Image = GetComponent<SpriteRenderer>();
        m_PlayerManager = GetComponent<PlayerManager> ();
    }
    

    private void Update() {
        m_Horizontal = Input.GetAxis("Horizontal");
        
        UpdateLookingDirection();

        if(Input.GetButtonDown("Jump") && CheckPlayerAboveSurface() && onASurface)
        {
            m_Rigidbody2D.AddForce(new Vector2(0f,jumpVelocity), ForceMode2D.Impulse);
            onASurface = false;
        }
        m_Animator.SetFloat("lookX", m_LookingDirection.x);
        m_Animator.SetFloat("speed", Mathf.Abs(m_Horizontal));

        UpdateGroundedPosition();
    }



    private void UpdateLookingDirection() {
        if(!Mathf.Approximately(m_Horizontal,0f))
        {
            m_LookingDirection.Set(m_Horizontal,0f);
            m_LookingDirection.Normalize();
        }
    }

    private void FixedUpdate() {
       m_Rigidbody2D.transform.position += new Vector3(m_Horizontal,0f,0f) *
        m_PlayerManager.movementSpeed * Time.deltaTime;
    }

    private void OnCollisionStay2D(Collision2D other) {
        if(m_PlayerManager.m_IsAlive) {
            foreach(string tag in m_InteractibleTags) {
                if(tag == other.collider.tag) {
                    onASurface = true;
                } 
            }

            m_PlayerManager.m_PlayerFXManager.OnCollisionStayWalkingSoundFXHandler(other, m_Horizontal);
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        foreach(string tag in m_InteractibleTags) {
            if(tag == other.tag) {
                onASurface = true;
            } 
        }
    }

    private void OnCollisionExit2D(Collision2D other) {
        foreach(string tag in m_InteractibleTags) {
            if(tag == other.collider.tag) {
                onASurface = false;
            }
        }
    }

    internal bool CheckPlayerAboveSurface()
    {
        foreach(GameObject groundChecker in m_GroundCheckers) {
            if(CheckGroundersOnASurface(groundChecker.transform.position)) {
                return true;
            }
        }
        return false;
    }

    private void UpdateGroundedPosition() {
        if(m_PlayerManager.m_IsImmune) return;
        bool bothGroundersOK = true;
        foreach(GameObject groundChecker in m_GroundCheckers) {
            if(!CheckGroundersOnASurface(groundChecker.transform.position)) {
                bothGroundersOK = false;
            }
        }
        if(bothGroundersOK) {
            m_LastGroundedPostion = m_Rigidbody2D.transform.position;
        }
    }

    private bool CheckGroundersOnASurface(Vector2 position){
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.1f, LayerMask.GetMask("Ground","Knife","Enemy","Lava"));
        if(hit.collider != null) {
            if(hit.collider.tag == "Lava") {
                return false;
            } else {
                return true;
            }
        }
        return false;
    }

    internal void Respawn() {
        int counter = 0;
        while(!EmptySpace(m_LastGroundedPostion) && counter < 20) {
            counter++;
            Vector2 newPositionRight = new Vector2 (m_LastGroundedPostion.x + counter, m_LastGroundedPostion.y);
             Vector2 newPositionLeft =  new Vector2 (m_LastGroundedPostion.x - counter, m_LastGroundedPostion.y);
            if (EmptySpace(newPositionLeft) && CheckGroundersOnASurface(newPositionLeft)) {
                m_LastGroundedPostion = newPositionLeft;
            } else if (EmptySpace(newPositionRight) && CheckGroundersOnASurface(newPositionRight)) {
                m_LastGroundedPostion = newPositionRight;
            }
        }
        m_Rigidbody2D.transform.position = m_LastGroundedPostion;
        m_PlayerManager.SetStraightY();
    }

    private bool EmptySpace(Vector2 position) {
        if (Physics2D.BoxCast(position, new Vector2(0.1f,0.1f), 0f, Vector2.up, 0.1f, LayerMask.GetMask("Enemy")).collider != null) {
            return false;
        } else {
            return true;
        }
    }
}