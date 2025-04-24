using UnityEngine;

public class UI_InGame : MonoBehaviour {
    public static UI_InGame Instance { get; private set; }
    public UI_FadeEffect FadeEffect { get; private set; }


    private void Awake() {
        if (Instance == null) {
            Instance = this;
            FadeEffect = GetComponent<UI_FadeEffect>();
        }
        else {
            Destroy(gameObject);
        }
        FadeEffect = GetComponentInChildren<UI_FadeEffect>();
        if (FadeEffect == null) {
            Debug.LogError("FadeEffect is not assigned in the inspector.");
        }
    }

    void Start() {
        FadeEffect.ScreenFadeEffect(0f, 1.5f, GameManager.instance.RespawnPlayer);
    }
}