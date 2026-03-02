using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Intro : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private bool introSkipped = false;
    private AsyncOperation authPreload;

    private void Awake()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
        }
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);

        Application.backgroundLoadingPriority = ThreadPriority.Low;

        authPreload = SceneManager.LoadSceneAsync("AuthScene");
        authPreload.allowSceneActivation = false;
    }

    private void Update()
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

    private void OnVideoPrepared(VideoPlayer vp)
    {
        if (vp == null) return;

        vp.Play();
        StartCoroutine(CheckVideoEnd());
    }

    private IEnumerator CheckVideoEnd()
    {
        while (videoPlayer != null && videoPlayer.isPlaying && !introSkipped)
            yield return null;

        if (!introSkipped)
        {
            if (AchievementManager.Instance != null)
                AchievementManager.Instance.UnlockAchievement("movie_buff");

            FadeManager.Instance.ActivatePreloadedSceneWithFade(authPreload);
        }
    }

    private void SkipIntro()
    {
        if (introSkipped) return;

        introSkipped = true;

        if (videoPlayer != null && videoPlayer.isPlaying)
            videoPlayer.Stop();

        FadeManager.Instance.ActivatePreloadedSceneWithFade(authPreload);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.prepareCompleted -= OnVideoPrepared;
    }
}