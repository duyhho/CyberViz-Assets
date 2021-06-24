using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject destination;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)

    {   
        Debug.Log(other.gameObject);
        Debug.Log("Player Position:" + other.transform.position); //camera
        Debug.Log("Destination Position:" + destination.transform.position);
        Vector3 newPos = new Vector3(10.0f,2.0f,0f);
        // other.transform.position = newPos;
        other.transform.position = destination.transform.position;
        // other.transform.position = new Vector3(10f,0f,0f);
        Debug.Log("Player Position (after) Position:" + other.transform.position);



    }

}
