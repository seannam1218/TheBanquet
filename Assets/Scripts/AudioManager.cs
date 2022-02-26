using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class AudioManager : MonoBehaviourPun
{
    public string currentlyPlayingSound; //displays the name of the current sound that is playing.
    public Sound[] sounds;
    private Dictionary<string, Sound> soundsDict = new Dictionary<string, Sound>();
    private Sound curSound;

    void Awake()
    {
        if (!photonView.IsMine) return;
        photonView.RPC("LoadAudioRpc", RpcTarget.All);
    }

    public void Play(string name)
    {
        if (curSound != null)
        {
            photonView.RPC("PauseAudioRpc", RpcTarget.All, currentlyPlayingSound);
            curSound = soundsDict[name];
        } 
        else
        {
            curSound = soundsDict[name];
        }
        if (!curSound.source.isPlaying)
        {
            photonView.RPC("PlayAudioRpc", RpcTarget.All, name);
        }
    }

    public void Pause()
    {
        if (curSound != null)
        {
            photonView.RPC("PauseAudioRpc", RpcTarget.All, currentlyPlayingSound);
        }
    }

    public void ListSounds()
    {
        Debug.Log("Listing sounds...");
        foreach (var tuple in soundsDict)
        {
            Debug.Log(tuple.Key + ": " + tuple.Value);
        }
    }

    public float GetVolume(string name)
    {
        return soundsDict[name].volume;
    }

    public void SetVolume(string name, float volume)
    {
        soundsDict[name].source.volume = volume;
    }

    public float GetMaxDistance(string name)
    {
        return soundsDict[name].maxDistance;
    }

    public void SetMaxDistance(string name, float distance)
    {
        soundsDict[name].source.maxDistance = distance;
    }

    [PunRPC]
    public void LoadAudioRpc()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.spatialBlend = 1f;
            s.source.rolloffMode = AudioRolloffMode.Linear;
            s.source.maxDistance = s.maxDistance;

            soundsDict.Add(s.name, s);
        }
    }

    [PunRPC]
    public void PlayAudioRpc(string name)
    {
        soundsDict[name].source.Play();
        currentlyPlayingSound = name;
    }

    [PunRPC]
    public void PauseAudioRpc(string name)
    {
        soundsDict[name].source.Pause();
        curSound = null;
        currentlyPlayingSound = "None";
    }

}
