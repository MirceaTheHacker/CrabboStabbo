using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FXManagerAbstract : MonoBehaviour
{
    public ParticleSystem m_BurstLavaEmission;
    public ParticleSystem m_LoopLavaEmission;
    public ParticleSystem[] m_GrassParticles;
    
    protected virtual void OnCollisionEnter2D(Collision2D other) {
         if (other.collider.tag == "Ground") {
            EnableGrassParticles();
        }
    }

    public virtual void OnCollisionExit2D(Collision2D other) {
        if (other.collider.tag == "Ground") {
            DisableGrassParticles();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Lava") {
            ActivateLavaParticles();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if(other.tag == "Lava") {
            DeactivateLavaParticles();
        }
    }

    public void EnableGrassParticles() {
        foreach (ParticleSystem grassParticles in m_GrassParticles) {
            grassParticles.Play();
        }
    }

    public bool IsGrassParticlesPlaying() {
        foreach (ParticleSystem grassParticles in m_GrassParticles) {
            if(grassParticles.isPlaying) return true;
        }
        return false;
    }

    public void DisableGrassParticles() {
        foreach (ParticleSystem grassParticles in m_GrassParticles) {
            grassParticles.Stop();
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
