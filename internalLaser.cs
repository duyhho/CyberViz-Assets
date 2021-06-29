using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class internalLaser : MonoBehaviour
{
    GameObject laser;
    LineRenderer my_line;
    public Transform target;
    // Start is called before the first frame update
    void Start()
    {
        GameObject _go_internal = Resources.Load("Internal_Laser") as GameObject;
        laser = (GameObject) Instantiate(_go_internal, new Vector3(0, 0, 0), Quaternion.Euler(0, 90, 0));
        my_line = laser.GetComponent<LineRenderer>();
        my_line.positionCount = 2;// the other function is outdated
    }

    // Update is called once per frame
    void Update()
    {
        my_line.SetPosition(0, my_line.transform.position);
        my_line.SetPosition(1, target.position);
        float distance = Vector3.Distance(my_line.transform.position, target.position);
        my_line.material.mainTextureScale = new Vector2 (distance/5, 1);

    }
}
