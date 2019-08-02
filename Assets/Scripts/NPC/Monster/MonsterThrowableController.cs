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
    private bool meleeHit = false;
    private SpriteRenderer spriteRenderer;

    internal PlayerController m_PlayerInfo;

    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        Destroy(gameObject, 10f);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        m_Rigidbody2D.AddForce(new Vector2(direction.x, 1),ForceMode2D.Impulse);
        m_Rigidbody2D.AddTorque(-10);
    }

    private void FixedUpdate() {
        m_Rigidbody2D.transform.position += direction * m_Velocity * Time.deltaTime;
    }

    internal void SetDirection(Vector3 direction) {
        this.direction = direction;
    }

    internal void DestroyMe() {
        Destroy(gameObject);
    }

    internal void OnMeleeHit() {
        SetDirection(-direction);
        meleeHit = true;
        spriteRenderer.color = Color.red;
        gameObject.layer = 16;
        m_Rigidbody2D.AddForce(new Vector2(direction.x, 2),ForceMode2D.Impulse);
        m_Rigidbody2D.AddTorque(20);
    }

    private void OnCollisionStay2D(Collision2D other) {
        if(!meleeHit) {
            if(other.gameObject.tag == "Player") {
            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.Attacked(m_Damage, gameObject.transform);
            DestroyMe();
            }
        } else {
            if(other.gameObject.tag == "Enemy") {
                NPCControllerAbstract npcControllerAbstract =
                 other.gameObject.GetComponent<NPCControllerAbstract>();
                npcControllerAbstract.Attacked(1, m_PlayerInfo);
                DestroyMe();
            }
        }
    }
}