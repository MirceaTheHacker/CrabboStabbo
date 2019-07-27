using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFXManager : FXManagerAbstract{
    private Rigidbody2D m_Rigidbody2D;

    private void Awake() {
        m_Rigidbody2D = GetComponent <Rigidbody2D> ();
    }

    // we need to treat the case when the player is jumping against the wall
    private void OnCollisionStay2D(Collision2D other) {
        if(other.collider.tag == "Ground") {
            if (!Mathf.Approximately(0f,m_Rigidbody2D.velocity.y)){
                m_GrassParticles.Stop();
            } else {
                if (!m_GrassParticles.isPlaying) {
                m_GrassParticles.Play();
                }
            }
        }

    }
}
