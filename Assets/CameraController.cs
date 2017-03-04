using UnityEngine;
using System;

public class CameraController : MonoBehaviour
{
    Vector3 mouse;

    float len;

    private float rotateSpeed = 500f;

    //The transform of the center of the cube
    private Transform tr;

    private GameObject helpCanvas;

    // Use this for initialization
    void Start () {
        len = 10f;
        mouse = transform.position;

        tr = GameObject.Find("Center").transform;
        helpCanvas = GameObject.Find("HelpCanvas");
	}
	
	// Update is called once per frame
	void Update () {
        //Only rotates the cube if the help screen is not displayed
        if (!helpCanvas.activeSelf)
        {
            //Gets left mouse button input
            //If the mouse is being dragged the cube gets rotated
            if (Input.GetMouseButton(0))
                tr.Rotate((Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime), -(Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime), 0, Space.World);
        
            //Gets scroll input
            float scr = Input.GetAxis("Mouse ScrollWheel");

            //If mouse is being scrolled
            if (scr != 0f)
            {
                Vector3 norm = transform.position.normalized;

                //Moves the mouse closer or farther from the cube
                transform.position = transform.position - norm * Mathf.Sign(scr);

                //Clips how close the camera can get
                if (transform.position.sqrMagnitude < 4)
                    transform.position = norm * 2;

                len = transform.position.magnitude;
        }
        
        }
    }
}
