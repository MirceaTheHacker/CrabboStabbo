﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeController : MonoBehaviour
{   
    public float knifeVelocity = 5f;
    public float destroyTimer = 5f;
    public int m_KnfieDamage = 1;
    public ParticleSystem m_BloodTrail;

    private Rigidbody2D m_Rigidbody2D;
    private Vector3 direction;
    private bool m_Collided = false;
    private KnifeFXManager m_FXManager;


    void Awake()
    {
        m_BloodTrail.Stop();
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_FXManager = GetComponent<KnifeFXManager>();
        gameObject.layer = 0;
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(m_Collided) return;
        if(other.gameObject.tag == "Ground")
        {
            m_Collided = true;
            m_FXManager.PlayHitGroundSoundFX();
            StartCoroutine(FallToGround(destroyTimer));
            gameObject.layer = 9;
            m_Rigidbody2D.velocity = new Vector2(0f,0f);
            m_Rigidbody2D.bodyType = RigidbodyType2D.Static;
        } else if(other.gameObject.tag == "Enemy")
        {
            NPCControllerAbstract monsterController = other.gameObject.GetComponent<NPCControllerAbstract>();
            monsterController.Attacked(m_KnfieDamage, gameObject.transform);
            knifeVelocity = 0f;
            m_Rigidbody2D.transform.SetParent(other.collider.gameObject.transform);
            if (other.collider.gameObject.tag != "Untagged") {
                m_BloodTrail.Play();
            }
            StartCoroutine(FallToGround(destroyTimer));
            m_Rigidbody2D.simulated = false;
            m_Collided = true;
        }
    }

    private IEnumerator FallToGround (float time) {
        yield return new WaitForSeconds(time);
        m_Rigidbody2D.simulated = true;
        m_Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
        gameObject.transform.SetParent(null);
        m_Rigidbody2D.constraints = RigidbodyConstraints2D.None;
        m_Rigidbody2D.AddForce(Vector2.down * m_Rigidbody2D.mass,ForceMode2D.Impulse);
        m_Rigidbody2D.AddTorque(-0.1f * m_Rigidbody2D.mass, ForceMode2D.Impulse);
        yield return new WaitForSeconds(time);
        GetComponent<KnifeFXManager>().enabled = false;
        StartCoroutine(Utils.FadeGameObject(gameObject, 1f));
    }

    private void FixedUpdate() {
        if(m_Collided) return;
        m_Rigidbody2D.transform.position += direction * knifeVelocity * Time.deltaTime;
    }

    public void SetDirection(Vector3 direction)
    {
        if(direction.x < 0)
        {
            gameObject.transform.eulerAngles = new Vector3 (0,-180,0);
        }
        this.direction = direction;
    }

    private void DestroyMe(){
        Destroy(gameObject);
    }
}
