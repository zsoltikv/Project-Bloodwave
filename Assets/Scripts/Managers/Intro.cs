using UnityEngine;

using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class IntroSceneScript : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    private bool introSkipped = false;
    public GameObject introPanel;

    void Start()
    {

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void Update()
    {
        if (introSkipped) return;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            SkipIntro();
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SkipIntro();
        }
        else if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SkipIntro();
        }
    }

    private IEnumerator FadeOutAndLoad()
    {
        if (introPanel == null)
        {
            SceneManager.LoadScene("MenuScene");
            yield break;
        }

        CanvasGroup cg = introPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = introPanel.AddComponent<CanvasGroup>();

        cg.alpha = 1f;
        introPanel.transform.localScale = Vector3.one;

        float duration = 0.4f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            cg.alpha = Mathf.Lerp(1f, 0f, t);

            introPanel.transform.localScale = Vector3.Lerp(
                Vector3.one,
                Vector3.one * 0.4f,
                Mathf.Sin(t * Mathf.PI * 0.5f)
            );

            yield return null;
        }

        cg.alpha = 0f;
        introPanel.transform.localScale = Vector3.one * 0.4f;

        SceneManager.LoadScene("MenuScene");
    }


    void OnVideoPrepared(VideoPlayer vp)
    {
        StartCoroutine(PlayWhenReady());
    }

    IEnumerator PlayWhenReady()
    {
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

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
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("MenuScene");
        }
    }

    void SkipIntro()
    {
        if (introSkipped) return;
        introSkipped = true;

        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        StartCoroutine(FadeOutAndLoad());
    }

}