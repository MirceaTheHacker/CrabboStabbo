using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeFXManager : FXManagerAbstract
{
    public AudioSource m_AudioSource;
    public AudioClip groundHitSoundFX;
    public AudioClip knifeThrowSoundFX;

    private void Awake() {
        PlaySound(knifeThrowSoundFX);
    }

    public void PlayHitGroundSoundFX() {
        PlaySound(groundHitSoundFX);
    }

    private void PlaySound(AudioClip clip) {
        m_AudioSource.PlayOneShot(clip);
    }

    private void Start() {
        DisableGrassParticles();
    }
}
