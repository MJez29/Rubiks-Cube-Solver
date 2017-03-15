using UnityEngine;
using System;

//Controls camera position relative to the cube
//Allows the cube to be physically rotated around with the mouse and to zoom in and out on the cube

public class CameraController : MonoBehaviour
{
    //Distance from the camera to the cube
    float len;

    //How fast the cube rotates when dragged by the mouse
    private float rotateSpeed = 500f;

    //The transform of the center of the cube
    private Transform tr;

    //
    private GameObject helpCanvas;

    //True if the mouse is pressed down in a non-UI area
    private bool mouseDown;

    // Use this for initialization
    void Start () {
        len = 10f;

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
            if (mouseDown && Input.GetMouseButton(0))
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

    //Sets whether the mouse is down or not
    public void OnMouseEvent(bool b)
    {
        mouseDown = b;
    }
}