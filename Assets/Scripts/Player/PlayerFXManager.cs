using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFXManager : FXManagerAbstract{
    public AudioSource m_WalkingAudioSource;
    public AudioSource m_SFXAudioSource;
    public AudioClip m_GettingHitSoundFX;

    private Rigidbody2D m_Rigidbody2D;
    private PlayerController m_PlayerController;

    private void Awake() {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PlayerController = GetComponent<PlayerController>();
    }

    private void OnCollisionStay2D(Collision2D other) {
        if(other.collider.tag == "Ground") {
            if (!Mathf.Approximately(0f,m_Rigidbody2D.velocity.y)){
                DisableGrassParticles();
            } else {
                if (!IsGrassParticlesPlaying()) {
                EnableGrassParticles();
                }
            }
        }
    }

    public void OnCollisionStayWalkingSoundFXHandler(Collision2D other, float horizontalMovement) {
        if (other.collider.tag != "Ground") return;
        if (!Mathf.Approximately(0f, horizontalMovement) && CheckIfGrounded()) {
            if(!m_WalkingAudioSource.isPlaying) {
                m_WalkingAudioSource.Play();
            }
        } else {
            m_WalkingAudioSource.Pause();
        }
    }

    public override void OnCollisionExit2D(Collision2D other) {
        base.OnCollisionExit2D(other);
        if(other.collider.tag != "Ground") return;
        m_WalkingAudioSource.Pause();
    }

    private void PlaySound(AudioClip clip){
        m_SFXAudioSource.PlayOneShot(clip, 1f);
    }

    public void HitSoundFX(){
        PlaySound(m_GettingHitSoundFX);
    }

    private bool CheckIfGrounded() {
        bool isAboveGround = true;
        foreach(GameObject groundChecker in m_PlayerController.m_GroundCheckers) {
            RaycastHit2D hit = Physics2D.Raycast(groundChecker.transform.position, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
            if(hit.collider == null) {
                isAboveGround = false;
            }
        }
        return isAboveGround;
    }
}
