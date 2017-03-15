using UnityEngine;
using System.Collections;

public class GuideRenderer : MonoBehaviour {

    private RenderTexture tex;

	// Use this for initialization
	void Start () {
        tex = Resources.Load<RenderTexture>("guidecube");
	}
	
	// Update is called once per frame
	void onGUI () {
        GUI.DrawTexture(new Rect(0, 0, 126, 126), tex, ScaleMode.ScaleToFit, true, 10.0F);
	}
}
