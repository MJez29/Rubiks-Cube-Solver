using UnityEngine;
using System.Collections.Generic;

//Found on the faces of the blocks on the cube

public class FaceCollider : MonoBehaviour {

    private Pair<BoxCollider, string>[] colliders;

    private int numColliders;

    private Block block;

    private CubeTransformer ct; 

    void Awake()
    {
        colliders = new Pair<BoxCollider, string>[3];
        numColliders = 0;
    }

	// Use this for initialization
	void Start ()
    {
        ct = GameObject.Find("Center").GetComponent<CubeTransformer>();
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void SetBlock(Block blk)
    {
        block = blk;
    }

    public void AddCollider(BoxCollider coll, string face)
    {
        colliders[numColliders++] = new Pair<BoxCollider, string>(coll, face);
    }

    public void RegisterHit(BoxCollider coll, Color col)
    {
        for (int i = 0; i < numColliders; i++)
        {
            if (coll == colliders[i].Item1)
            {
                block.SetSide(colliders[i].Item2, col);
                ct.ColorFaces(block);
            }
        }
    }
}