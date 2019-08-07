using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip m_ClickSFX;
    public AudioClip m_HoverSFX;
    public AudioClip m_GameWinSFX;
    public AudioClip m_GameLoseSFX;

    private AudioSource m_AudioSource {get { return GetComponent<AudioSource> (); } }


    public void PlayClickSound() {
        m_AudioSource.PlayOneShot(m_ClickSFX);
    }

    public void PlayHoverSound() {
        m_AudioSource.PlayOneShot(m_HoverSFX);
    }

    public void PlayGameWinSound() {
        m_AudioSource.PlayOneShot(m_GameWinSFX);
    }

    public void PlayGameLoseSound() {
        m_AudioSource.PlayOneShot(m_GameLoseSFX);
    }
}
