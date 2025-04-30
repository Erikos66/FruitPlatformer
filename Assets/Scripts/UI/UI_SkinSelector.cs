using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SkinSelector : MonoBehaviour {

    [SerializeField] private Animator chrSkinDisplayAnimator;
    [SerializeField] private int selectedSkinIndex = 0;
    [SerializeField] private GameObject firstSelectedButton;


    void OnEnable() {
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    public void NextSkin() {
        selectedSkinIndex++;
        if (selectedSkinIndex > 3) {
            selectedSkinIndex = 0;
        }
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
        UpdateSkin();
    }

    public void PreviousSkin() {
        selectedSkinIndex--;
        if (selectedSkinIndex < 0) {
            selectedSkinIndex = 3;
        }
        AudioManager.Instance.PlayRandomSFX("SFX_MenuSelect");
        UpdateSkin();
    }


    private void UpdateSkin() {
        for (int i = 0; i < chrSkinDisplayAnimator.layerCount; i++) {
            if (i == selectedSkinIndex) {
                chrSkinDisplayAnimator.SetLayerWeight(i, 1f);
            }
            else {
                chrSkinDisplayAnimator.SetLayerWeight(i, 0f);
            }
        }
    }

    void Awake() {
        for (int i = 0; i < chrSkinDisplayAnimator.layerCount; i++) {
            chrSkinDisplayAnimator.SetLayerWeight(i, 0f);
        }
    }

    public void SelectSkin() {
        if (GameManager.Instance != null && PlayerManager.Instance != null) {
            SkinManager.Instance.SetSkin(selectedSkinIndex);
            Debug.Log("Selected skin index: " + selectedSkinIndex);
        }
        else {
            Debug.LogError("Cannot select skin: GameManager or playerManager is not initialized.");
        }
    }
}
