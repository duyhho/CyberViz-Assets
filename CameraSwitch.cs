using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    public Camera camPerson;
    public Camera camTopView;
    public GameObject firstPersonPlayer;
    void Start() {
        camPerson.enabled = true;
        camTopView.enabled = false;
    }

    void Update() {

        if (Input.GetKeyDown(KeyCode.C)) {
            camPerson.enabled = !camPerson.enabled;
            camTopView.enabled = !camTopView.enabled;

            if (camPerson.enabled == false){
                firstPersonPlayer.GetComponent<CharacterControl>().speed = 0f;
            }
        }
    }
}
