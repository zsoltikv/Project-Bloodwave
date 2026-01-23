using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class Intro : MonoBehaviour
{

    public VideoPlayer videoPlayer;
    private bool introSkipped = false;

    void Start()
    {
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    void Update()
    {
        if (introSkipped) return;

        if (
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        )
        {
            SkipIntro();
        }
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        videoPlayer.Play();
        StartCoroutine(CheckVideoEnd());
    }

    IEnumerator CheckVideoEnd()
    {
        while (videoPlayer.isPlaying && !introSkipped)
        {
            yield return null;
        }

        if (!introSkipped)
        {
            FadeManager.Instance.LoadSceneWithFade("MenuScene");

        }
    }

    void SkipIntro()
    {
        if (introSkipped) return;
        introSkipped = true;

        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        FadeManager.Instance.LoadSceneWithFade("MenuScene");

    }

}