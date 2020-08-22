using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // All audio files, type and their volume
    public SoundAudioClip[] soundAudioClipArray;
    // Singleton
    public static GameController _instance;

    public static GameController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameController>();

                if (_instance == null)
                {
                    GameObject container = new GameObject("GameController");
                    _instance = container.AddComponent<GameController>();
                }
            }

            return _instance;
        }
    }

    [System.Serializable]
    public class SoundAudioClip
    {
        public SoundManager.Sound sound;
        public AudioClip audioClip;
        [Range(0,1)]
        public float volume = 1f;
    }

    private void Awake()
    {
        SoundManager.Initialize();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            ResetPlayer();
    }

    // Reload scene in case player is stuck
    public void ResetPlayer()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
