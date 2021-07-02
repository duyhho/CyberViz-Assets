using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControl : MonoBehaviour
{

    public float speed = 10.0f;
    
    public float sensitivity = 8f;
    public Transform TopView;
    float xRotation = 0f;
    float yRotation = 0f;
    private Camera cam1;
    private Camera cam2;
    private Transform currentCam;

    private CharacterController charController;

    void Start()
    {
    
        Cursor.lockState = CursorLockMode.Locked;

        charController = GetComponent<CharacterController>();

        cam1 =  GameObject.Find("Camera").GetComponent<Camera>();
        cam2 = GameObject.Find("Main Camera").GetComponent<Camera>();

        cam1.enabled = true;
        cam2.enabled = false;
        currentCam = cam1.transform;



    }
    
    void Update()
    {
        if (Input.GetKeyDown("space")) {
            cam1.enabled = !cam1.enabled;
            cam2.enabled = !cam2.enabled;

            if (cam1.enabled) {
                currentCam = cam1.transform;
            }
            else {
                currentCam = cam2.transform;
                // xRotation = 0f;
                // currentCam.localRotation = Quaternion.Euler(56.071f, 269.47f, -0.379f);
            }
            // if (cam1.enabled == false){
            //     speed = 0f;
            // }
        }

        CameraMovement();

        Vector3 move = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical"));

        charController.SimpleMove((Vector3.ClampMagnitude(move, 1.0f) * (Input.GetKey(KeyCode.LeftShift) ? speed * 1.6f : speed)));


    }

    void CameraMovement()
    {
        if (cam1.enabled){
            sensitivity = 8f;
            var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            md = Vector2.Scale(md, new Vector2(sensitivity * Time.deltaTime, sensitivity * Time.deltaTime));

            xRotation -= md.y;
            yRotation += md.x;

            if (Input.GetKey(KeyCode.Q)) {
                xRotation += speed * Time.deltaTime;
                // currentCam.Rotate(Vector3.up * speed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.E))
            {
                xRotation -= speed * Time.deltaTime;
                // currentCam.Rotate(-Vector3.up * speed * Time.deltaTime);

            }
            currentCam.localRotation = Quaternion.Euler(Mathf.Clamp(xRotation, -60, 50), 0, 0);
            // transform.transform.Rotate(Vector3.up * md.x);
            // currentCam.Rotate(Vector3.up * md.x);
            if (Input.GetKey(KeyCode.Z)) {
                yRotation += speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.C))
            {
                yRotation -= speed * Time.deltaTime;
            }
            // transform.localRotation = Quaternion.Euler(0, Mathf.Clamp(yRotation, -60, 50), 0);
            transform.transform.Rotate(Vector3.up * md.x);



        }

        else {
            sensitivity = 15f;
            var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            md = Vector2.Scale(md, new Vector2(sensitivity * Time.deltaTime, sensitivity * Time.deltaTime));
            xRotation -= md.y;
            yRotation += md.x;
            currentCam.localRotation = Quaternion.Euler(Mathf.Clamp(xRotation, -70, 70), 0, 0);
            TopView.Rotate(Vector3.up * md.x);

            if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            {
                TopView.Translate(new Vector3(speed * 5 * Time.deltaTime,0,0));
            }
            if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            {
                TopView.Translate(new Vector3(-speed * 5 * Time.deltaTime,0,0));
            }
            if(Input.GetKey(KeyCode.S))
            {
                TopView.Translate(new Vector3(0,-speed * 5 * Time.deltaTime,0));
            }
            if(Input.GetKey(KeyCode.W))
            {
                TopView.Translate(new Vector3(0,speed *  5 * Time.deltaTime,0));
            }
            if(Input.GetKey(KeyCode.DownArrow))
            {
                TopView.Translate(new Vector3(0,0,-speed * 5 * Time.deltaTime));
            }
            if(Input.GetKey(KeyCode.UpArrow))
            {
                TopView.Translate(new Vector3(0,0,speed *  5 * Time.deltaTime));
            }
            // if (Input.GetKey(KeyCode.Q))
            //     currentCam.Rotate(Vector3.up * speed * Time.deltaTime);

            // if (Input.GetKey(KeyCode.E))
            //     currentCam.Rotate(-Vector3.up * speed * Time.deltaTime);

            }



    }

}