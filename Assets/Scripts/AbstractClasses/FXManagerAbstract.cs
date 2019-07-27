using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManagerAbstract : MonoBehaviour
{
    public ParticleSystem m_BurstLavaEmission;
    public ParticleSystem m_LoopLavaEmission;
    public ParticleSystem m_GrassParticles;
    
    protected virtual void OnCollisionEnter2D(Collision2D other) {
        if(other.collider.tag == "Lava") {
            ActivateLavaParticles();
        } else if (other.collider.tag == "Ground") {
            m_GrassParticles.Play();
        }
    }

    private void OnCollisionExit2D(Collision2D other) {
        if(other.collider.tag == "Lava") {
            DeactivateLavaParticles();
        } else if (other.collider.tag == "Ground") {
            m_GrassParticles.Stop();
        }
    }

    public void ActivateLavaParticles(){
        m_BurstLavaEmission.Play();
        m_LoopLavaEmission.Play();
    }

    public void DeactivateLavaParticles(){
        m_LoopLavaEmission.Stop();
    }
}
