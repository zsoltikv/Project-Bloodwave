using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Cutscene : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private bool cutsceneEnded = false;

    void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("VideoPlayer nincs hozzárendelve!");
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void Update()
    {
        if (cutsceneEnded) return;

        // Érintés
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            SkipCutscene();
        }

        // Egér
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            SkipCutscene();
        }

        // Billentyûzet
        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SkipCutscene();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        if (!cutsceneEnded)
            SkipCutscene();
    }

    void SkipCutscene()
    {
        if (cutsceneEnded) return;
        cutsceneEnded = true;

        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        // Átirányítás a MainScene-re
        SceneManager.LoadScene("MainScene");
    }
}