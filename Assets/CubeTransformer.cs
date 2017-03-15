using UnityEngine;
using System.Collections;

//In charge of creating, positioning, coloring and rotating the cube

public class CubeTransformer : MonoBehaviour {

    //private GameObject[] blocks;

    private Block[,,] blocks;

    public bool finishedRotation;

    private int framesPerRotationIndex = 4;
    private static int[] framesPerRotationArr = new int[] { 1, 3, 6, 9, 15, 18, 30, 45, 90 };
    private int framesPerRotation = framesPerRotationArr[4];         //How many frames per rotation of the cube

    //The pivots that the cube rotates around
    private Transform topPivot, frontPivot, rightPivot, backPivot, leftPivot, bottomPivot, center;

    private static float PERIOD = 0.01f;

    public static Color WHITE = Color.white, RED = Color.red, BLUE = Color.blue, ORANGE = new Color(1, 165f / 255f, 0), YELLOW = Color.yellow, BLACK = Color.black, GREEN = Color.green, GRAY = Color.gray;

    private static GameObject baseBlock;

    // Use this for initialization
    void Start()
    {
        center = transform;
        
        baseBlock = baseBlock ?? Resources.Load<GameObject>("Block");

        //Creates all the pivots
        topPivot = new GameObject().transform;
        frontPivot = new GameObject("Pivot").transform;
        rightPivot = new GameObject("Pivot").transform;
        backPivot = new GameObject("Pivot").transform;
        leftPivot = new GameObject("Pivot").transform;
        bottomPivot = new GameObject("Pivot").transform;

        //Sets the parent of the pivots to the center of the cube
        topPivot.parent = frontPivot.parent = rightPivot.parent =
            backPivot.parent = leftPivot.parent = bottomPivot.parent = center;

        //Positions all pivots
        topPivot.localPosition = new Vector3(0, 1.05f, 0);
        frontPivot.localPosition = new Vector3(0, 0, -1.05f);
        rightPivot.localPosition = new Vector3(1.05f, 0, 0);
        backPivot.localPosition = new Vector3(0, 0, 1.05f);
        leftPivot.localPosition = new Vector3(-1.05f, 0, 0);
        bottomPivot.localPosition = new Vector3(0, -1.05f, 0);

        MakeUnscrambled(true);        
    }

    private bool firstTime = true;
    public void MakeUnscrambled(bool color)
    {
        StopAllCoroutines();

        if (blocks == null)
            blocks = new Block[3, 3, 3];

        int i, j, k;
        float x, y, z;
        for (i = 0, x = -1.05f; i < 3; i++, x += 1.05f)             //X
        {
            for (j = 0, y = -1.05f; j < 3; j++, y += 1.05f)         //Y
            {
                for (k = 0, z = -1.05f; k < 3; k++, z += 1.05f)     //Z
                {
                    if (blocks[i, j, k] == null)
                        blocks[i, j, k] = new Block();

                    //Sets position of blocks
                    blocks[i, j, k].pos = new Vector3(x, y, z);

                    if (blocks[i, j, k].go == null)
                        blocks[i, j, k].go = Instantiate(baseBlock);

                    blocks[i, j, k].go.transform.parent = center;
                    blocks[i, j, k].go.transform.localPosition = blocks[i, j, k].pos;
                    blocks[i, j, k].go.transform.localRotation = Quaternion.identity;

                    BoxCollider bc;
                    //Colors faces based on x-pos
                    switch (i)
                    {
                        case 0:         //Left
                            blocks[i, j, k].left = color || (j == 1 && k == 1) ? GREEN : blocks[i, j, k].left;
                            blocks[i, j, k].right = BLACK;

                            if (firstTime && !(j == 1 && k == 1))
                            {
                                bc = blocks[i, j, k].go.AddComponent<BoxCollider>();
                                bc.center = new Vector3(-0.4f, 0, 0);
                                bc.size = new Vector3(0.2f, 1, 1);
                                FaceCollider fc = blocks[i, j, k].go.GetComponent<FaceCollider>();
                                if (fc == null)
                                    fc = blocks[i, j, k].go.AddComponent<FaceCollider>();
                                fc.AddCollider(bc, "LEFT");
                                fc.SetBlock(blocks[i, j, k]);
                            }
                            break;
                        case 1:         //Middle
                            blocks[i, j, k].left = BLACK;
                            blocks[i, j, k].right = BLACK;
                            break;
                        case 2:         //Right
                            blocks[i, j, k].left = BLACK;
                            blocks[i, j, k].right = color || (j == 1 && k == 1) ? BLUE : blocks[i, j, k].right;

                            if (firstTime && !(j == 1 && k == 1))
                            {
                                bc = blocks[i, j, k].go.AddComponent<BoxCollider>();
                                bc.center = new Vector3(0.4f, 0, 0);
                                bc.size = new Vector3(0.2f, 1, 1);
                                FaceCollider fc = blocks[i, j, k].go.GetComponent<FaceCollider>();
                                if (fc == null)
                                    fc = blocks[i, j, k].go.AddComponent<FaceCollider>();
                                fc.AddCollider(bc, "RIGHT");
                                fc.SetBlock(blocks[i, j, k]);
                            }
                            break;
                    }
                    //Colors faces based on y-pos
                    switch (j)
                    {
                        case 0:         //Bottom
                            blocks[i, j, k].top = BLACK;
                            blocks[i, j, k].bottom = color || (i == 1 && k == 1) ? YELLOW : blocks[i, j, k].bottom;

                            if (firstTime && !(i == 1 && k == 1))
                            {
                                bc = blocks[i, j, k].go.AddComponent<BoxCollider>();
                                bc.center = new Vector3(0, -0.4f, 0);
                                bc.size = new Vector3(1, 0.2f, 1);
                                FaceCollider fc = blocks[i, j, k].go.GetComponent<FaceCollider>();
                                if (fc == null)
                                    fc = blocks[i, j, k].go.AddComponent<FaceCollider>();
                                fc.AddCollider(bc, "BOTTOM");
                                fc.SetBlock(blocks[i, j, k]);
                            }
                            break;
                        case 1:         //Middle
                            blocks[i, j, k].top = BLACK;
                            blocks[i, j, k].bottom = BLACK;
                            break;
                        case 2:         //Top
                            blocks[i, j, k].top = color || (i == 1 && k == 1) ? WHITE : blocks[i, j, k].top;
                            blocks[i, j, k].bottom = BLACK;

                            if (firstTime && !(i == 1 && k == 1))
                            {
                                bc = blocks[i, j, k].go.AddComponent<BoxCollider>();
                                bc.center = new Vector3(0, 0.4f, 0);
                                bc.size = new Vector3(1, 0.2f, 1);
                                FaceCollider fc = blocks[i, j, k].go.GetComponent<FaceCollider>();
                                if (fc == null)
                                    fc = blocks[i, j, k].go.AddComponent<FaceCollider>();
                                fc.AddCollider(bc, "TOP");
                                fc.SetBlock(blocks[i, j, k]);
                            }
                            break;
                    }
                    //Colors faces based on z-pos
                    switch (k)
                    {
                        case 0:         //Front
                            blocks[i, j, k].front = color || (i == 1 && j == 1) ? RED : blocks[i, j, k].front;
                            blocks[i, j, k].back = BLACK;

                            if (firstTime && !(i == 1 && j == 1))
                            {
                                bc = blocks[i, j, k].go.AddComponent<BoxCollider>();
                                bc.center = new Vector3(0, 0, -0.4f);
                                bc.size = new Vector3(1, 1, 0.2f);
                                FaceCollider fc = blocks[i, j, k].go.GetComponent<FaceCollider>();
                                if (fc == null)
                                    fc = blocks[i, j, k].go.AddComponent<FaceCollider>();
                                fc.AddCollider(bc, "FRONT");
                                fc.SetBlock(blocks[i, j, k]);
                            }
                            break;
                        case 1:         //Middle
                            blocks[i, j, k].front = BLACK;
                            blocks[i, j, k].back = BLACK;
                            break;
                        case 2:         //Back
                            blocks[i, j, k].front = BLACK;
                            blocks[i, j, k].back = color || (i == 1 && j == 1) ? ORANGE : blocks[i, j, k].back;

                            if (firstTime && !(i == 1 && j == 1))
                            {
                                bc = blocks[i, j, k].go.AddComponent<BoxCollider>();
                                bc.center = new Vector3(0, 0, 0.4f);
                                bc.size = new Vector3(1, 1, 0.2f);
                                FaceCollider fc = blocks[i, j, k].go.GetComponent<FaceCollider>();
                                if (fc == null)
                                    fc = blocks[i, j, k].go.AddComponent<FaceCollider>();
                                fc.AddCollider(bc, "BACK");
                                fc.SetBlock(blocks[i, j, k]);
                            }
                            break;
                    }

                    ColorFaces(blocks[i, j, k]);
                    blocks[i, j, k].go.name = i + "" + j + "" + k;
                }
            }
        }

        if (firstTime)
            firstTime = false;
    }

    public void SetBlocks(Block[,,] blks)
    {
        blocks = blks;
    }

    //---------------------------------------------------- COLORING FUNCTIONS -----------------------------------------

    public void Blacken()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    blocks[i, j, k].ColorAllSides(GRAY);
                }
            }
        }
        MakeUnscrambled(false);
    }

    //Colors all faces of the given block
    public void ColorFaces(Block blk)
    {
        Mesh mesh = new Mesh();
        float hs = 0.5f;
        mesh.vertices = new Vector3[] {
             new Vector3(-hs,  hs, -hs), new Vector3( hs,  hs, -hs), new Vector3( hs, -hs, -hs), new Vector3(-hs, -hs, -hs),  // Front
             new Vector3(-hs,  hs,  hs), new Vector3( hs,  hs,  hs), new Vector3( hs,  hs, -hs), new Vector3(-hs,  hs, -hs),  // Top
             new Vector3(-hs, -hs,  hs), new Vector3( hs, -hs,  hs), new Vector3( hs,  hs,  hs), new Vector3(-hs,  hs,  hs),  // Back
             new Vector3(-hs, -hs, -hs), new Vector3( hs, -hs, -hs), new Vector3( hs, -hs,  hs), new Vector3(-hs, -hs,  hs),  // Bottom
             new Vector3(-hs,  hs,  hs), new Vector3(-hs,  hs, -hs), new Vector3(-hs, -hs, -hs), new Vector3(-hs, -hs,  hs),  // Left
             new Vector3( hs,  hs, -hs), new Vector3( hs,  hs,  hs), new Vector3( hs, -hs,  hs), new Vector3( hs, -hs, -hs)}; // Right

        int[] triangles = new int[mesh.vertices.Length / 4 * 2 * 3];
        int iPos = 0;
        for (int i = 0; i < mesh.vertices.Length; i = i + 4)
        {
            triangles[iPos++] = i;
            triangles[iPos++] = i + 1;
            triangles[iPos++] = i + 2;
            triangles[iPos++] = i;
            triangles[iPos++] = i + 2;
            triangles[iPos++] = i + 3;
        }

        mesh.triangles = triangles;
        Color[] colors = {  blk.front, blk.front, blk.front, blk.front,
                            blk.top, blk.top, blk.top, blk.top,
                            blk.back, blk.back, blk.back, blk.back,
                            blk.bottom, blk.bottom, blk.bottom, blk.bottom,
                            blk.left, blk.left, blk.left, blk.left,
                            blk.right, blk.right, blk.right, blk.right };

        mesh.colors = colors;
        mesh.RecalculateNormals();

        blk.go.GetComponent<MeshFilter>().mesh = mesh;
    }

    public Block[,,] GetBlocks()
    {
        return blocks;
    }

    //---------------------------------------------------- ROTATION FUNCTIONS -----------------------------------------

    public void SetRotationSpeedIndex(int n)
    {
        framesPerRotation = framesPerRotationArr[framesPerRotationIndex = n];
    }

    public void SpeedUpRotation()
    {
        framesPerRotation = framesPerRotationArr[--framesPerRotationIndex == -1 ? ++framesPerRotationIndex : framesPerRotationIndex];
    }

    public void SlowDownRotation()
    {
        framesPerRotation = framesPerRotationArr[++framesPerRotationIndex == framesPerRotationArr.Length ? --framesPerRotationIndex : framesPerRotationIndex];
    }

    public void Rotate(string name, bool clockwise)
    {
        StartCoroutine(name, clockwise);
    }

    public void Rotate(Pair<string, bool> p)
    {
        StartCoroutine(p.Item1, p.Item2);
    }

    //Rotates the top of the cube
    private IEnumerator RotateTop(bool clockwise)
    {
        int curFPR = framesPerRotation;         //If the speed is changed mid rotation it can mess the cube up
        foreach (Block blk in blocks)
        {
            if (blk.go.transform.localPosition.y == 1.05f)
                blk.go.transform.parent = topPivot;
        }

        for (int i = 0; i < curFPR; i++)
        {
            topPivot.Rotate(0, 90 / curFPR * (clockwise ? 1 : -1), 0);
            yield return new WaitForSeconds(PERIOD / curFPR);
        }

        Reset();
    }

    //Rotates the bottom of the cube
    private IEnumerator RotateBottom(bool clockwise)
    {
        int curFPR = framesPerRotation;
        foreach (Block blk in blocks)
        {
            if (blk.go.transform.localPosition.y == -1.05f)
                blk.go.transform.parent = bottomPivot;
        }

        for (int i = 0; i < curFPR; i++)
        {
            bottomPivot.Rotate(0, 90 / curFPR * (clockwise ? -1 : 1), 0);
            yield return new WaitForSeconds(PERIOD / curFPR);
        }

        Reset();
    }

    //Rotates the front of the cube
    private IEnumerator RotateFront(bool clockwise)
    {
        int curFPR = framesPerRotation;
        foreach (Block blk in blocks)
        {
            if (blk.go.transform.localPosition.z == -1.05f)
                blk.go.transform.parent = frontPivot;
        }

        for (int i = 0; i < curFPR; i++)
        {
            frontPivot.Rotate(0, 0, 90 / curFPR * (clockwise ? -1 : 1));
            yield return new WaitForSeconds(PERIOD / curFPR);
        }

        Reset();
    }

    //Rotates the back of the cube
    private IEnumerator RotateBack(bool clockwise)
    {
        int curFPR = framesPerRotation;
        foreach (Block blk in blocks)
        {
            if (blk.go.transform.localPosition.z == 1.05f)
                blk.go.transform.parent = backPivot;
        }

        for (int i = 0; i < curFPR; i++)
        {
            backPivot.Rotate(0, 0, 90 / curFPR * (clockwise ? 1 : -1));
            yield return new WaitForSeconds(PERIOD / curFPR);
        }

        Reset();
    }

    //Rotates the left side of the cube
    private IEnumerator RotateLeft(bool clockwise)
    {
        int curFPR = framesPerRotation;
        foreach(Block blk in blocks)
        {
            if (blk.go.transform.localPosition.x == -1.05f)
                blk.go.transform.parent = leftPivot;
        }

        for (int i = 0; i < curFPR; i++)
        {
            leftPivot.Rotate(90 / curFPR * (clockwise ? -1 : 1), 0, 0);
            yield return new WaitForSeconds(PERIOD / curFPR);
        }

        Reset();
    }

    //Rotates the right side of the cube
    private IEnumerator RotateRight(bool clockwise)
    {
        int curFPR = framesPerRotation;
        foreach (Block blk in blocks)
        {
            if (blk.go.transform.localPosition.x == 1.05f)
                blk.go.transform.parent = rightPivot;
        }

        for (int i = 0; i < curFPR; i++)
        {
            rightPivot.Rotate(90 / curFPR * (clockwise ? 1 : -1), 0, 0);
            yield return new WaitForSeconds(PERIOD / curFPR);
        }

        Reset();
    }

    private float[] nums = { -1.05f, 0, 1.05f };

    //Removes the parent of the block
    //Removes any rounding error that occured to the blocks' positions
    //When complete the rotation is done
    private void Reset()
    {
        if (blocks == null)
            return;

        float x = 0, y = 0, z = 0;
        foreach (Block blk in blocks)
        {
            //Removes parent
            blk.go.transform.parent = center.transform;

            //Adjusts coordinates
            //Say the x-coord is 0.00121f it will get adjusted to 0f
            for (int i = 0; i < 3; i++)
            {
                x = (Mathf.Abs(blk.go.transform.localPosition.x - nums[i]) < 0.2f) ? nums[i] : x;
                y = (Mathf.Abs(blk.go.transform.localPosition.y - nums[i]) < 0.2f) ? nums[i] : y;
                z = (Mathf.Abs(blk.go.transform.localPosition.z - nums[i]) < 0.2f) ? nums[i] : z;
            }

            blk.go.transform.localPosition = new Vector3(x, y, z);
        }

        finishedRotation = true;
    }

    //----------------------------------------------------------------------------------------------------------------
}