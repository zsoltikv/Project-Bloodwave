using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;

    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusic;

    [Header("Gameplay Music (Shuffle)")]
    [SerializeField] private List<AudioClip> gameplayTracks;

    private AudioClip lastPlayedTrack;
    private bool isInGameplay;

    private readonly HashSet<string> menuScenes = new HashSet<string>
    {
        "MenuScene",
        "HowToPlayScene",
        "LeaderboardScene",
        "AchievementsScene",
        "GameOverScene"
    };

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (menuScenes.Contains(scene.name))
        {
            PlayMenuMusic();
        }
        else if (scene.name == "MainScene")
        {
            StartGameplayMusic();
        }
        else
        {
            StopMusic();
        }
    }

    private void PlayMenuMusic()
    {
        if (musicSource.clip == menuMusic && musicSource.isPlaying)
            return;

        isInGameplay = false;
        musicSource.loop = true;
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    private void StartGameplayMusic()
    {
        if (isInGameplay)
            return;

        isInGameplay = true;
        musicSource.loop = false;
        PlayRandomGameplayTrack();
    }

    private void PlayRandomGameplayTrack()
    {
        if (gameplayTracks.Count == 0)
            return;

        AudioClip nextTrack;
        do
        {
            nextTrack = gameplayTracks[Random.Range(0, gameplayTracks.Count)];
        }
        while (nextTrack == lastPlayedTrack && gameplayTracks.Count > 1);

        lastPlayedTrack = nextTrack;
        musicSource.clip = nextTrack;
        musicSource.Play();

        Invoke(nameof(PlayRandomGameplayTrack), nextTrack.length);
    }

    private void StopMusic()
    {
        CancelInvoke();
        musicSource.Stop();
        musicSource.clip = null;
        isInGameplay = false;
    }
}