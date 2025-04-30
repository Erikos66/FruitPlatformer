using UnityEngine;
using UnityEngine.EventSystems;
public class UI_Settings : MonoBehaviour {

    [SerializeField] private GameObject firstSelectedButton; // First button to be selected when the menu opens

    void OnEnable() {
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}