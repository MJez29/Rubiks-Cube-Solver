using UnityEngine;
using System;
using UnityEngine.UI;

public class HelpTextLoader : MonoBehaviour
{
    // Use this for initialization
    void Start ()
    {
        string text = Resources.Load<TextAsset>("Help").text;
        GetComponent<Text>().text = text;
    }
}
