using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFXAbstract : FXManagerAbstract
{
    public AudioSource m_AudioSource;
    public AudioClip[] hitClips;
    public AudioClip[] deathClips;

    private void Awake() {
        DisableGrassParticles();
    }

    private void PlaySound(AudioClip clip) {
        m_AudioSource.PlayOneShot(clip);
    }

    public void PlayHitSoundFX() {
        PlaySound(hitClips[(int)Random.Range(0f,hitClips.Length-1)]);
    }

    public void PlayDeathSoundFX() {
        PlaySound(deathClips[(int)Random.Range(0f,deathClips.Length-1)]);
    }

    
}
