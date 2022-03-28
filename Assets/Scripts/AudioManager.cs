using UnityEngine.Audio;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float musicVolume;

    [Range(0f, 1f)]
    public float sfxVolume;

    public Sound[] sounds;

    public static AudioManager instance;

    private bool InGame;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach(Sound s in sounds)
        {

            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        musicVolume = .3f;
        sfxVolume = .3f;
    }
    private void Start()
    {
        FindObjectOfType<AudioManager>().Play("themeHamuhh");
    }

    private void Update()
    {
        foreach(Sound sound in sounds)
        {
            if(sound.soundType == Sound.SoundType.music)
            {
                sound.source.volume = musicVolume;
            }
            else if(sound.soundType == Sound.SoundType.sfx)
            {
                sound.source.volume = sfxVolume; ;
            }
        }


        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            if (InGame == false)
            {
                FindObjectOfType<AudioManager>().Play("roomFire");
                InGame = true;
            }
        }
        else
        {
            FindObjectOfType<AudioManager>().Pause("roomFire");
            InGame = false;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
    }

    public void Pause(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Pause();
    }
}
