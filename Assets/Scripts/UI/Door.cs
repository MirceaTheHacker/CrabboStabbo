using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Canvas m_Canvas;
    public AnimationClip m_Animation;
    public AudioClip m_OpenSFX;
    public AudioClip m_CloseSFX;

    
    private bool m_InProximity;
    private PlayerManager m_PlayerManager;
    private AudioSource m_AudioSource {get {return GetComponent<AudioSource>(); } }
    private Animator m_Animator {get {return GetComponent<Animator>(); } }

    private void Update() {
        if(m_InProximity && Input.GetKeyDown(KeyCode.E)) {
            StartCoroutine(WalkThroughDoor());
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player" && m_PlayerManager == null) {
            m_PlayerManager = other.GetComponent<PlayerManager>();
        }
        m_Canvas.enabled = true;
        m_InProximity = true;
    }

    private void OnTriggerExit2D(Collider2D other) {
        m_Canvas.enabled = false;
        m_InProximity = false;
    }

    internal IEnumerator WalkThroughDoor() {
        m_PlayerManager.DisableScripts();
        // wait until crabbo is on the ground
        while(!m_PlayerManager.m_PlayerController.CheckPlayerAboveSurface()) {
            yield return new WaitForFixedUpdate();
        }
        m_PlayerManager.m_Rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionY;
        Physics2D.IgnoreLayerCollision(8,11);
        OpenDoor();
        m_PlayerManager.m_PlayerController.m_Animator.SetFloat("speed",0);
        yield return new WaitForSeconds(m_Animation.length);
        float direction = GetDirection();
        while (m_InProximity) {
            m_PlayerManager.m_PlayerController.m_Animator.SetFloat("speed",1);
            m_PlayerManager.m_Rigidbody2D.transform.position += new Vector3(direction, 0f, 0f) *
            m_PlayerManager.movementSpeed * Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        CloseDoor();
        Physics2D.IgnoreLayerCollision(8, 11, false);
        m_PlayerManager.m_CapsuleCollider2D.enabled = false;
        yield return new WaitForSeconds(m_Animation.length);
        GameManager.Instance.GameWin();
    }

    private void OpenDoor() {
        m_AudioSource.PlayOneShot(m_OpenSFX);
        m_Animator.SetBool("open",true);
    }

    private void CloseDoor() {
        m_AudioSource.PlayOneShot(m_CloseSFX);
        m_Animator.SetBool("open",false);
    }

    private float GetDirection() {
        Vector2 direction = new Vector2(gameObject.transform.position.x - m_PlayerManager.transform.position.x, 0f);
        direction.Normalize();
        return direction.x;
    }
}
