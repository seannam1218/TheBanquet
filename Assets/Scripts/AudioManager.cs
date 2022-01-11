using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public GameObject audioSourcePublic;
    public Sound[] sounds;
    private Dictionary<string, Sound> soundsDict = new Dictionary<string, Sound>();
    private Sound curSound;
    private AudioSource source;

    void Awake()
    {    
        foreach (Sound s in sounds)
        {
            s.source = audioSourcePublic.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            soundsDict.Add(s.name, s);
        }
    }

    public void Play(string name)
    {        
        if (curSound == null || curSound.name != name)
        {
            Pause();
            curSound = soundsDict[name];
        }

        if (curSound == null)
        {
            Debug.LogWarning("Sound " + name + " does not exist.");
            return;
        }

        if (!curSound.source.isPlaying)
        {
            curSound.source.Play();
        }
    }

    public void Pause()
    {
        if (curSound != null)
        {
            curSound.source.Pause();
            curSound = null;
        }
            
    }
}
