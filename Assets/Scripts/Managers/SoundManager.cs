using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip m_ClickSFX;
    public AudioClip m_HoverSFX;

    private AudioSource m_AudioSource {get { return GetComponent<AudioSource> (); } }


    public void PlayClickSound() {
        m_AudioSource.PlayOneShot(m_ClickSFX);
    }

    public void PlayHoverSound() {
        m_AudioSource.PlayOneShot(m_HoverSFX);
    }
}
