using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于标注每个gameObject该使用什么音频作为clip
/// </summary>

public class AudioDefinition : MonoBehaviour
{
    public PlayAudioEventSO playAudioEvent;
    public AudioClip audioClip;
    public bool playOnEnable;

    private void OnEnable()
    {
        if (playOnEnable)
            PlayAudioClip();
    }

    public void PlayAudioClip()
    {
        playAudioEvent.RaiseEvent(audioClip);
    }
}
