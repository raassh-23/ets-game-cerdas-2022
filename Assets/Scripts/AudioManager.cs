using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource _audioSource;

    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private AudioClip _clickSFX;
    
    [SerializeField]
    private AudioClip _winSFX;
    
    [SerializeField]
    private AudioClip _loseSFX;

    [SerializeField]
    private AudioClip _deathSFX;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayMusic()
    {
        if (_audioSource.isPlaying) return;
        _audioSource.Play();
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }

    public void PlayClickSFX()
    {
        _audioSource.PlayOneShot(_clickSFX);
    }

    public void PlayWinSFX()
    {
        _audioSource.PlayOneShot(_winSFX);
    }

    public void PlayLoseSFX()
    {
        _audioSource.PlayOneShot(_loseSFX);
    }

    public void PlayDeathSFX()
    {
        _audioSource.PlayOneShot(_deathSFX);
    }
}
