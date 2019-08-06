using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickSound : MonoBehaviour
{
    public AudioClip m_ClickSFX;
    public AudioClip m_HoverSFX;

    private Button m_Button {get { return GetComponent<Button> (); } }
    private AudioSource m_AudioSource {get { return GetComponent<AudioSource> (); } }

    private void Start() {
        m_Button.onClick.AddListener(() => PlaySound(m_ClickSFX));
        //m_Button.OnPointerEnter(() => PlaySound(m_HoverSFX));
    }

    void PlaySound(AudioClip clip) {
        m_AudioSource.PlayOneShot(clip);
    }
}
