using UnityEngine;
using System.Collections;

//Checks for right-clicks by the user when in cube coloring mode
//Tells the go that was hit to recolor

public class CubeColorer : MonoBehaviour
{
    public static Color ORANGE = new Color(1, 165f / 255f, 0);

    private Color col;

    void Start()
    {
        col = Color.red;
    }
	
	// Update is called once per frame
	void Update ()
    {
        //If a keyboard key is pressed
        //Changes the current drawing color
        if (Input.GetKeyDown(KeyCode.W))
            col = Color.white;
        else if (Input.GetKeyDown(KeyCode.R))
            col = Color.red;
        else if (Input.GetKeyDown(KeyCode.B))
            col = Color.blue;
        else if (Input.GetKeyDown(KeyCode.O))
            col = ORANGE;
        else if (Input.GetKey(KeyCode.G))
            col = Color.green;
        else if (Input.GetKeyDown(KeyCode.Y))
            col = Color.yellow;


        //Checks for right-click
        if (Input.GetMouseButtonDown(1))
        {
            //Creates ray going from camera position, passing through the mouse position
            Ray ray = GameObject.Find("Main Camera").GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //Checks for a hit
            if (Physics.Raycast(ray, out hit))
            {
                //Tells the gameobject to recolor the side with the hit collider
                hit.collider.gameObject.GetComponent<FaceCollider>().RegisterHit((BoxCollider)hit.collider, col);
            }
        }
    }
}
