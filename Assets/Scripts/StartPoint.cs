using System.Collections;
using UnityEngine;

public class StartPoint : MonoBehaviour {
    private Animator anim;
    [SerializeField] public Transform respawnPoint;


    private void Awake() {
        anim = GetComponentInChildren<Animator>();
    }

    public void AnimateFlag() {
        anim.SetTrigger("waveflag");
    }

}
