using UnityEngine;
using System;
using UnityEngine.UI;

public class HelpTextLoader : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        try
        {
            string text = System.IO.File.ReadAllText("Assets/Help.txt");
            GetComponent<Text>().text = text;
        }
        catch (Exception e) { Debug.Log(e.Message); }
	}
}
