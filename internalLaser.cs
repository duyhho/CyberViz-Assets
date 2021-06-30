using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class internalLaser : MonoBehaviour
{
    GameObject laser;
    LineRenderer myLine;
    public Transform target;
    float distance;
    float currentLength = 10f;
    // Start is called before the first frame update
    void Start()
    {
        GameObject _go_internal = Resources.Load("Internal_Laser") as GameObject;
        laser = (GameObject) Instantiate(_go_internal, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
        // laser.transform.SetParent(transform);
        
        myLine = laser.GetComponent<LineRenderer>();
        myLine.positionCount = 2;// the other function is outdated
        myLine.SetPosition(0, transform.position);
        distance = Vector3.Distance(myLine.transform.position, target.position);
        myLine.material.mainTextureScale = new Vector2 (distance/5, 1);
        // myLine.startWidth = 8f;
        // myLine.endWidth = 8f;
        StartCoroutine(Grow(myLine, target));
    }

    // Update is called once per frame
    void Update()
    {
        
        // myLine.SetPosition(1, target.position);

    }
    IEnumerator Grow(LineRenderer line, Transform target)
    {
        while (true) {
            // Debug.Log("Here");
            // suspend execution for 5 seconds
            yield return new WaitForSeconds(1);
            currentLength += 3;
            Vector3 newPosition = new Vector3(target.position.x, target.position.y, currentLength);
            
            // Vector3 newPosition = target.position;
            // Debug.Log(newPosition);
            line.SetPosition(1, newPosition);
            line.material.mainTextureScale = new Vector2 (distance/5, 1);
        }
        

    }
}
