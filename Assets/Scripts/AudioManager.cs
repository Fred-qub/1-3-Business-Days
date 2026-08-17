using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    
    public AudioClip[] hurtAudioClips;
    public AudioClip attackAudioClip;
    public AudioClip startupAudioClip;
    public AudioClip guardAudioClip;
    public AudioClip stunAudioClip;
    public AudioClip clashAudioClip;
    
    public AudioClip winAudioClip;
    public AudioClip loseAudioClip;
    public AudioClip hoverAudioClip;
    public AudioClip confirmAudioClip;
    
    public uint seed;
    private Unity.Mathematics.Random rng;

    private void Start()
    {
        seed = (uint)System.DateTime.Now.Ticks;
        rng = new Unity.Mathematics.Random(seed);
        
        //PlayRandomSound(hurtAudioClips, this.transform, 100f);
    }

    public void PlaySound(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(this.audioSource, spawnTransform.position, Quaternion.identity);
        
        audioSource.clip = audioClip;
        
        audioSource.volume = volume;
        
        audioSource.Play();
        
        float clipLength = audioSource.clip.length;
        
        Destroy(audioSource.gameObject, clipLength);
    }
    
    public void PlayRandomSound(AudioClip[] audioClipArray, Transform spawnTransform, float volume)
    {
        AudioSource tempAudioSource = Instantiate(audioSource, spawnTransform.position, Quaternion.identity);
        int randomNumber = rng.NextInt(0, audioClipArray.Length);
        
        tempAudioSource.clip = audioClipArray[randomNumber];
        
        tempAudioSource.volume = volume;
        
        tempAudioSource.Play();
        
        float clipLength = tempAudioSource.clip.length;
        
        Destroy(tempAudioSource.gameObject, clipLength);
    }
    
    
}

