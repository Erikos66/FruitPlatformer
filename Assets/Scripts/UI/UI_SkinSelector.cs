using UnityEngine;

public class UI_SkinSelector : MonoBehaviour {

    [SerializeField] private Animator chrSkinDisplayAnimator;
    [SerializeField] private int selectedSkinIndex = 0;


    public void NextSkin() {
        selectedSkinIndex++;
        if (selectedSkinIndex > 3) {
            selectedSkinIndex = 0;
        }
        UpdateSkin();
    }

    public void PreviousSkin() {
        selectedSkinIndex--;
        if (selectedSkinIndex < 0) {
            selectedSkinIndex = 3;
        }
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
        if (GameManager.instance != null && GameManager.instance.playerManager != null) {
            GameManager.instance.playerManager.SetSkin(selectedSkinIndex);
            Debug.Log("Selected skin index: " + selectedSkinIndex);
        }
        else {
            Debug.LogError("Cannot select skin: GameManager or playerManager is not initialized.");
        }
    }
}
