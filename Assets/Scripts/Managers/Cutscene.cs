using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Cutscene : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private bool cutsceneEnded = false;

    private void Start()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    private void Update()
    {
        if (cutsceneEnded) return;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            SkipCutscene();
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            SkipCutscene();
        }

        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SkipCutscene();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!cutsceneEnded)
            SkipCutscene();
    }

    private void SkipCutscene()
    {
        if (cutsceneEnded) return;

        cutsceneEnded = true;

        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        FadeManager.Instance.LoadSceneWithFade("MainScene");
    }
}