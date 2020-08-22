using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    // Type of sounds
    public enum Sound
    {
        playerJump,
        playerHit,
        playerThrow,
        playerLanding,
    }

    private static Dictionary<Sound, float> soundTimerDictionary;
    private static GameObject oneShotGameObject;
    private static AudioSource oneShotAudioSource;

    public static void Initialize()
    {
        soundTimerDictionary = new Dictionary<Sound, float>();
    }

    // Play sound at position
    public static void PlaySound(Sound sound, Vector3 position)
    {
        GameObject soundGameObject = new GameObject("Sound");
        soundGameObject.transform.position = position;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.clip = GetAudioClip(sound);
        audioSource.volume = GetVolume(sound);
        audioSource.maxDistance = 100f;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;
        audioSource.Play();

        Object.Destroy(soundGameObject, audioSource.clip.length);
    }

    // Play sound everywhere
    public static void PlaySound(Sound sound)
    {
        if (oneShotGameObject == null)
        {
            oneShotGameObject = new GameObject("Sound");
            oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
        }
        oneShotAudioSource.volume = GetVolume(sound);
        oneShotAudioSource.PlayOneShot(GetAudioClip(sound));
    }

    // Get audio data from sound type
    private static AudioClip GetAudioClip(Sound sound)
    {
        foreach(GameController.SoundAudioClip soundAudioClip in GameController.Instance.soundAudioClipArray)
        {
            if (soundAudioClip.sound == sound)
                return soundAudioClip.audioClip;
        }

        return null;
    }

    // Get volume of individual sound
    private static float GetVolume(Sound sound)
    {
        foreach (GameController.SoundAudioClip soundAudioClip in GameController.Instance.soundAudioClipArray)
        {
            if (soundAudioClip.sound == sound)
                return soundAudioClip.volume;
        }

        return 0f;
    }
}
