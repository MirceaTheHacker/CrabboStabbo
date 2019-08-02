using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterThrowableController : MonoBehaviour
{
    public float m_Velocity = 5f;
    public float m_DestroyTimer = 5f;
    public int m_Damage = 1;

    private Rigidbody2D m_Rigidbody2D;
    private Vector3 direction;

    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 10f);
    }

    private void Start() {
        m_Rigidbody2D.AddForce(direction,ForceMode2D.Impulse);
        m_Rigidbody2D.AddTorque(90);
    }

    private void FixedUpdate() {
        m_Rigidbody2D.transform.position += direction * m_Velocity * Time.deltaTime;
    }

    internal void SetDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    internal void DestroyMe(){
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag == "Player") {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.Attacked(m_Damage, gameObject.transform);
            DestroyMe();
        }
    }
}