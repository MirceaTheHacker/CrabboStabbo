using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFXAbstract : FXManagerAbstract
{
    public AudioSource m_AudioSource;
    public AudioClip[] m_HitClips;
    public AudioClip[] m_DeathClips;

    private void Awake() {
        DisableGrassParticles();
    }

    private void PlaySound(AudioClip clip) {
        m_AudioSource.PlayOneShot(clip);
    }

    public void PlayHitSoundFX() {
        PlaySound(m_HitClips[(int)Random.Range(0f,m_HitClips.Length-1)]);
    }

    public void PlayDeathSoundFX() {
        PlaySound(m_DeathClips[(int)Random.Range(0f,m_DeathClips.Length-1)]);
    }


    
}
