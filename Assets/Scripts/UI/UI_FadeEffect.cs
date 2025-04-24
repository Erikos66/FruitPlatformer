using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_FadeEffect : MonoBehaviour {
    [SerializeField] private Image fadeImage;

    public void ScreenFadeEffect(float targetAlpha, float duration, System.Action onComplete = null) {
        if (fadeImage == null) {
            Debug.LogError("Fade Image is not assigned in the inspector.");
            return;
        }
        StartCoroutine(FadeCoroutine(targetAlpha, duration, onComplete));
    }

    void Start() {
        if (fadeImage == null) {
            Debug.LogError("Fade Image is not assigned in the inspector.");
            return;
        }
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f);
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration, System.Action onComplete) {
        Color currentcolor = fadeImage.color;
        float startAlpha = currentcolor.a;
        float time = 0f;

        while (time < duration) {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            fadeImage.color = new Color(currentcolor.r, currentcolor.g, currentcolor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(currentcolor.r, currentcolor.g, currentcolor.b, targetAlpha);
        onComplete?.Invoke();
    }
}