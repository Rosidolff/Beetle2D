using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;  // Music for the menus
    [SerializeField] private AudioClip levelMusic; // Music for the levels

    static MusicManager instance; // The music manager that is already playing
    AudioSource audioSource;

    void Awake()
    {
        // If there is already a music manager, destroy this one so the music doesn't restart
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // Keep this object when a new scene is loaded
        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded; // Call OnSceneLoaded every time a scene is loaded
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Levels play the level music, the rest of scenes play the menu music
        AudioClip music = scene.name.StartsWith("Level") ? levelMusic : menuMusic;

        // Only change the music if it is different, so it doesn't restart
        if (audioSource.clip != music)
        {
            audioSource.clip = music;
            audioSource.Play();
        }
    }
}