using UnityEngine;
using System.Collections;

public class HelpButtonListener : MonoBehaviour
{
    private GameObject helpCanvas;

    void Start()
    {
        helpCanvas = GameObject.Find("HelpCanvas");
        helpCanvas.SetActive(false);
    }

	public void OnClick(bool b)
    {
        helpCanvas.SetActive(b);
    }
}
