using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayMusic(musicClip);
    }

    // Method to play a sound effect
    public void PlaySFX(AudioClip clip)
    {
        float volume = UIManager.Instance.SFXVolume / 10f; 
        sfxSource.PlayOneShot(clip, volume);               
    }

    // Method to play looping music
    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.volume = UIManager.Instance.MusicVolume / 10f; 
        musicSource.Play();
    }

    public void ChangeMusicVolume(int v) {
        musicSource.volume = (float)v / 10;
    }

    public void ChangeSFXVolume(int v)
    {
        sfxSource.volume = (float)v / 10;
    }
}

