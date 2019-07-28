using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 1f;
    public float jumpVelocity = 7f;
    public GameObject knifePrefab;
    public GameObject KnifeSpawnPoint;
    public GameObject[] m_GroundCheckers;
    public float immunityTimer = 2f;
    
    internal bool m_IsImmune = false;
    private Rigidbody2D m_Rigidbody2D;
    private bool onASurface = false;
    private Animator m_Animator;
    private Vector2 m_LookingDirection = new Vector2(1,0);
    private Health m_Health;
    private float m_Horizontal;
    private string[] m_InteractibleTags = {"Ground", "Enemy", "Knife", "Lava"};
    private Vector3 m_LastGroundedPostion;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_Health = GetComponent<Health>();
        m_LastGroundedPostion = m_Rigidbody2D.transform.position;
    }

    private void Update() {
        if(m_Rigidbody2D.velocity.y > 0) {
            Physics2D.IgnoreLayerCollision(8,9, true);
        } else {
            Physics2D.IgnoreLayerCollision(8,9, false);
        }
        m_Horizontal = Input.GetAxis("Horizontal");
        
        if(!Mathf.Approximately(m_Horizontal,0f))
        {
            m_LookingDirection.Set(m_Horizontal,0f);
            m_LookingDirection.Normalize();
        }
        CheckPlayerAboveSurface();
        if(Input.GetButtonDown("Jump") && CheckPlayerAboveSurface() && onASurface)
        {
            m_Rigidbody2D.AddForce(new Vector2(0f,jumpVelocity), ForceMode2D.Impulse);
            onASurface = false;
        }
        if (Input.GetButtonDown("Attack"))
        {
            m_Animator.SetTrigger("attack");
            GameObject knifeObject = Instantiate(knifePrefab, KnifeNoise(), Quaternion.identity);
            KnifeController knifeController = knifeObject.GetComponent<KnifeController>();
            knifeController.SetDirection(m_LookingDirection);
        } else {
        m_Animator.SetFloat("lookX", m_LookingDirection.x);
        m_Animator.SetFloat("speed", Mathf.Abs(m_Horizontal));
        }

        StartCoroutine(CheckIfFellOff());
    }

    private void FixedUpdate() {
       m_Rigidbody2D.transform.position += new Vector3(m_Horizontal,0f,0f) * movementSpeed * Time.deltaTime;
    }

    private Vector2 KnifeNoise(){
        float yNoise = 0f;
        yNoise = Random.Range(KnifeSpawnPoint.transform.position.y - 0.1f, KnifeSpawnPoint.transform.position.y + 0.1f);
        return new Vector2(KnifeSpawnPoint.transform.position.x, yNoise);
    }



    private void OnCollisionStay2D(Collision2D other) {
        foreach(string tag in m_InteractibleTags) {
            if(tag == other.collider.tag) {
                onASurface = true;
            } 
        }
        if(other.collider.tag == "Lava") {
            DamagePlayer(1);
            onASurface = true;
        }

    }

    private void OnCollisionExit2D(Collision2D other) {
        foreach(string tag in m_InteractibleTags) {
            if(tag == other.collider.tag) {
                onASurface = false;
            }
        }
    }

    private void Push(float monsterStrength, Transform NPCTransform) {
        Vector2 pushDirection = new Vector2(gameObject.transform.position.x - NPCTransform.position.x, 0f);
        pushDirection.Normalize();
        float Xcomponent = pushDirection.x * monsterStrength * movementSpeed;
        Vector2 pushVector = new Vector2(Xcomponent,
             monsterStrength * movementSpeed / 2f);
        m_Rigidbody2D.AddForce(pushVector,ForceMode2D.Impulse);
        StartCoroutine(RotatePlayer(monsterStrength));
    }

    private void DamagePlayer(int damageValue) {
        if(!m_IsImmune)
        {
            StartCoroutine(ImmunityCooldown());
            m_Health.UpdateHearts(-damageValue);
        }
    }

    public void Attack (int monsterStrength, Transform NPCTransform) {
        if(m_IsImmune) return;
        Push(monsterStrength, NPCTransform);
        DamagePlayer(monsterStrength);
    }

    private IEnumerator ImmunityCooldown(){
        m_IsImmune = true;
        StartCoroutine(FlashPlayer(immunityTimer));
        StartCoroutine(Stun(immunityTimer));
        yield return new WaitForSeconds(immunityTimer);
        m_IsImmune = false;
    }

    public IEnumerator Stun(float stunDuration){
        movementSpeed /= 4;
        yield return new WaitForSeconds(stunDuration);
        movementSpeed *= 4;
    }

    private bool CheckPlayerAboveSurface()
    {
        bool bothGroundersOK = true;
        bool isAboveASurface = false;
        foreach(GameObject groundChecker in m_GroundCheckers) {
            if(CheckObjectAboveSurface(groundChecker)) {
                isAboveASurface = true;
            } else {
                bothGroundersOK = false;
            }
        }
        if(bothGroundersOK) {
            m_LastGroundedPostion = m_Rigidbody2D.transform.position;
        }
        return isAboveASurface;
    }

    private bool CheckObjectAboveSurface(GameObject obj){
        RaycastHit2D hit = Physics2D.Raycast(obj.transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground", "Lava", "Enemy"));
        if(hit.collider != null) {
            return true;
        }
        
        return false;
    }

    private IEnumerator CheckIfFellOff(){
        yield return new WaitForFixedUpdate();
        if(m_Rigidbody2D.position.y < -10)
        {
            m_Rigidbody2D.transform.position = m_LastGroundedPostion;
            DamagePlayer(1);
        }
    }

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void HealPlayer(int healingValue) {
        m_Health.UpdateHearts(healingValue);
    }

    private IEnumerator FlashPlayer(float timer) {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer> ();
        Color originalColor = spriteRenderer.color;
        Color fadedColor = originalColor;
        fadedColor.a = 0.5f;
        float startTime = Time.time;
        while(Time.time - startTime < timer - 0.15f) {
            spriteRenderer.color = fadedColor;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.15f);
        }
    }

    private IEnumerator RotatePlayer(float timer) {
        float startTime = Time.time;
        m_Rigidbody2D.constraints = RigidbodyConstraints2D.None;

        while(Time.time - startTime < timer) {
        m_Rigidbody2D.AddTorque(timer*10,ForceMode2D.Force);
        yield return new WaitForSeconds(0.1f);
        }

        m_Rigidbody2D.transform.eulerAngles = new Vector3(0f,0f,0f);
        m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }


}