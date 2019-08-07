using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSounds : MonoBehaviour
{
    public void PlayClickSound() {
        GameManager.Instance.m_SoundManager.PlayClickSound();
    }

    public void PlayHoverSound() {
        GameManager.Instance.m_SoundManager.PlayHoverSound();
    }
}
