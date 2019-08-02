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
    public GameObject[] m_EnemyDetectors;
    public float m_MeleeRange = 2f;
    public int m_MeleeStrength = 1;
    public float m_MeleeCooldown = 0.5f;
    
    internal bool m_IsImmune = false;
    private Rigidbody2D m_Rigidbody2D;
    private bool onASurface = false;
    private Animator m_Animator;
    private Vector2 m_LookingDirection = new Vector2(1,0);
    private Health m_Health;
    private float m_Horizontal;
    private string[] m_InteractibleTags = {"Ground", "Enemy", "Knife", "Lava"};
    private Vector2 m_LastGroundedPostion;
    private PlayerFXManager m_FXManager;
    private bool m_MeleeInCooldown = false;
    private SpriteRenderer m_Image;

    void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_Animator = GetComponent<Animator>();
        m_Health = GetComponent<Health>();
        m_LastGroundedPostion = m_Rigidbody2D.transform.position;
        m_FXManager = GetComponent<PlayerFXManager>();
        m_Image = GetComponent<SpriteRenderer>();
    }

    private void Update() {
        m_Horizontal = Input.GetAxis("Horizontal");
        
        DirectionSwitcher();

        if(Input.GetButtonDown("Jump") && CheckPlayerAboveSurface() && onASurface)
        {
            m_Rigidbody2D.AddForce(new Vector2(0f,jumpVelocity), ForceMode2D.Impulse);
            onASurface = false;
        }
        if (Input.GetButtonDown("Throw")) {
            Throw();
        } else if (Input.GetButtonDown("Melee")) {
            Melee();
            
        } else {
        m_Animator.SetFloat("lookX", m_LookingDirection.x);
        m_Animator.SetFloat("speed", Mathf.Abs(m_Horizontal));
        }

        UpdateGroundedPosition();
        StartCoroutine(CheckIfFellOff());
    }

    private void DirectionSwitcher() {
        if(!Mathf.Approximately(m_Horizontal,0f))
        {
            m_LookingDirection.Set(m_Horizontal,0f);
            m_LookingDirection.Normalize();
            if (m_LookingDirection.x == 1 && KnifeSpawnPoint.transform.localPosition.x < 0) {
                KnifeLocalSpawnPointSwitcher();
            } if (m_LookingDirection.x == -1 && KnifeSpawnPoint.transform.localPosition.x > 0) {
                KnifeLocalSpawnPointSwitcher();
            }
        }
    }

    private void KnifeLocalSpawnPointSwitcher() {
        KnifeSpawnPoint.transform.localPosition = new Vector2 (-KnifeSpawnPoint.transform.localPosition.x,
                 KnifeSpawnPoint.transform.localPosition.y);
    }

    private void FixedUpdate() {
       m_Rigidbody2D.transform.position += new Vector3(m_Horizontal,0f,0f) * movementSpeed * Time.deltaTime;
    }

    private void Throw() {
        m_Animator.SetTrigger("throw");
        GameObject knifeObject = Instantiate(knifePrefab, KnifeNoise(), Quaternion.identity);
        KnifeController knifeController = knifeObject.GetComponent<KnifeController>();
        knifeController.SetDirection(m_LookingDirection);
        knifeController.m_PlayerInfo = this;
    }

    private Vector2 KnifeNoise(){
        float yNoise = 0f;
        yNoise = Random.Range(KnifeSpawnPoint.transform.position.y - 0.1f, KnifeSpawnPoint.transform.position.y + 0.1f);
        return new Vector2(KnifeSpawnPoint.transform.position.x, yNoise);
    }

    private void Melee() {
        if(m_MeleeInCooldown) return;
        m_Animator.SetTrigger("melee");
        StartCoroutine(MeleeCooldown());
        Debug.DrawRay(gameObject.transform.position, m_LookingDirection * m_MeleeRange, Color.green, 2f);
        Debug.DrawRay(gameObject.transform.position, Vector2.up * m_Image.bounds.size.y, Color.green, 2f);
        StartCoroutine(MeleeAttackCoroutine());
    }

    private IEnumerator MeleeAttackCoroutine() {
        // if we haven't hit something we wait for 0.1 seconds because we want to consider the whole animation
        if (!HitSomething()) {
        yield return new WaitForSeconds(0.1f);
        HitSomething();
        }
    }

    private bool HitSomething() {
        RaycastHit2D[] hits = Physics2D.BoxCastAll(gameObject.transform.position, new Vector2(m_MeleeRange, m_Image.bounds.size.y), 0f, m_LookingDirection, m_MeleeRange,
        LayerMask.GetMask("Enemy", "MonsterThrowable"));
        ArrayList uniqueEnemies = FilterDuplicateColliders(hits);
        foreach (GameObject enemy in uniqueEnemies) {
            if (enemy.tag == "Enemy") {
                NPCControllerAbstract npcController = enemy.GetComponentInParent<NPCControllerAbstract>();
                npcController.Attacked(m_MeleeStrength, this);
                npcController.m_PlayerInfo = this;
            } else if (enemy.tag == "MonsterThrowable") {
                MonsterThrowableController throwableController = enemy.GetComponent<MonsterThrowableController>();
                throwableController.OnMeleeHit();
                throwableController.m_PlayerInfo = this;
                //throwableController.DestroyMe();
            }
        }
        if(hits != null) {
            return true;
        } else {
            return false;
        }
    }

    private ArrayList FilterDuplicateColliders(RaycastHit2D[] hits) {
        // we need to check if the colliders found belong to different gameobjects with transform.root
        ArrayList uniqueEnemies = new ArrayList();
        foreach (RaycastHit2D hit in hits) {
            GameObject currentEnemy = hit.collider.gameObject;
            while(currentEnemy.tag == "Untagged") {
                currentEnemy = currentEnemy.transform.parent.gameObject;
            }
            if (!uniqueEnemies.Contains(currentEnemy)) {
                uniqueEnemies.Add(currentEnemy);
            }
        }
        return uniqueEnemies;
    }

    private IEnumerator MeleeCooldown() {
        m_MeleeInCooldown = true;
        yield return new WaitForSeconds(m_MeleeCooldown);
        m_MeleeInCooldown = false;
    }

    private void OnCollisionStay2D(Collision2D other) {
        foreach(string tag in m_InteractibleTags) {
            if(tag == other.collider.tag) {
                onASurface = true;
            } 
        }

        m_FXManager.OnCollisionStayWalkingSoundFXHandler(other, m_Horizontal);
    }

    private void OnTriggerStay2D(Collider2D other) {
        foreach(string tag in m_InteractibleTags) {
            if(tag == other.tag) {
                onASurface = true;
            } 
        }
        if(other.tag == "Lava") {
            DamagePlayer(1);
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
            m_FXManager.HitSoundFX();
        }
    }

    public void Attacked (int monsterStrength, Transform NPCTransform) {
        if(m_IsImmune) return;
        Push(monsterStrength, NPCTransform);
        DamagePlayer(monsterStrength);
    }

    private IEnumerator ImmunityCooldown(){
        m_IsImmune = true;
        StartCoroutine(FlashPlayer(immunityTimer));
        StartCoroutine(Stun(immunityTimer));
        Physics2D.IgnoreLayerCollision(8,10,true);
        yield return new WaitForSeconds(immunityTimer);
        Physics2D.IgnoreLayerCollision(8,10,false);
        m_IsImmune = false;
    }

    public IEnumerator Stun(float stunDuration){
        movementSpeed /= 4;
        yield return new WaitForSeconds(stunDuration);
        movementSpeed *= 4;
    }

    private bool CheckPlayerAboveSurface()
    {
        foreach(GameObject groundChecker in m_GroundCheckers) {
            if(CheckPositionAboveSurface(groundChecker.transform.position)) {
                return true;
            }
        }
        return false;
    }

    private void UpdateGroundedPosition() {
        bool bothGroundersOK = true;
        foreach(GameObject groundChecker in m_GroundCheckers) {
            if(!CheckPositionAboveSurface(groundChecker.transform.position)) {
                bothGroundersOK = false;
            }
        }
        if(bothGroundersOK) {
            m_LastGroundedPostion = m_Rigidbody2D.transform.position;
        }
    }

    private bool CheckPositionAboveSurface(Vector2 position){
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.1f, LayerMask.GetMask("Ground","Knife","Enemy","Lava"));
        if(hit.collider != null) {
            return true;
        }
        return false;
    }

    private IEnumerator CheckIfFellOff(){
        yield return new WaitForFixedUpdate();
        if(m_Rigidbody2D.position.y < -10)
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            Respawn();
            DamagePlayer(1);
        }
    }

    private void Respawn() {
        while(!EmptySpace(m_LastGroundedPostion)) {
            Vector2 newPosition = new Vector2 (Random.Range(m_LastGroundedPostion.x + 2f, m_LastGroundedPostion.x - 2f),m_LastGroundedPostion.y);
            if(EmptySpace(newPosition) && CheckPositionAboveSurface(newPosition)) {
                m_LastGroundedPostion = newPosition;
            }
        }
        m_Rigidbody2D.transform.position = m_LastGroundedPostion;
    }

    private bool EmptySpace(Vector2 position) {
        if (Physics2D.BoxCast(position, new Vector2(1f,1f), 0f, Vector2.up, 1f, LayerMask.GetMask("Enemy")).collider != null) {
            return false;
        } else {
            return true;
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