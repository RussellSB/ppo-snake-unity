using UnityEngine;
using System;

public class SFXManager : MonoBehaviour
{
    public Sound[] sounds;

    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        s.source.pitch = s.pitch;
        s.source.loop = s.loop;
        s.source.Play();
    }

    public void Click()
    {
        Debug.Log("Click");
        PlaySound("Click");
    }

    //GameObject.FindGameObjectWithTag("SFX").GetComponent<SFXManager>().PlaySound("Click");
}
