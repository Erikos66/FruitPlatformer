using UnityEngine;
using UnityEngine.EventSystems;

public class UI_MainMenu : MonoBehaviour {
    [SerializeField] private GameObject firstSelectedButton;

    void OnEnable() {
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
