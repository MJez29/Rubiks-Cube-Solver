using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public struct Guides
{
    public const int
        BLANK = 0,
        WHITE_CROSS = 1,
        WHITE_CORNERS = 2,
        MIDDLE_ROW = 3,
        YELLOW_DOT = 4,
        YELLOW_LINE = 5,
        YELLOW_HALF_CROSS = 6,
        YELLOW_CROSS_OPPOSITES = 7,
        YELLOW_CROSS_ADJACENTS = 8,
        YELLOW_CORNERS_POSITION_ONE = 9,
        YELLOW_CORNERS_POSITION_ALL = 10,
        ORIENT_YELLOW_CORNERS = 11,
        COLORING = 12;
}

public class GuideController : MonoBehaviour
{
    private Text text;

    private Triplet<string, Block[,,], Pair<string, bool>[]>[] guides;

    private bool mouseDown;

    private Transform tr;

    private CubeTransformer ct;

    private IEnumerator cr;

    //How fast the cube rotates when dragged by the mouse
    private float rotateSpeed = 630f;

    private const int NUM_GUIDES = 15;

    // Use this for initialization
    void Start () {
        text = GetComponentInChildren<Text>();
        guides = new Triplet<string, Block[,,], Pair<string, bool>[]>[NUM_GUIDES];

        GameObject go = GameObject.Find("GuideCenter");
        tr = go.transform;
        ct = go.GetComponent<CubeTransformer>();

        SetGuideNum(0);
	}
	
	// Update is called once per frame
	void Update () {
        if (mouseDown)
            tr.Rotate(-(Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime), -(Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime), 0, Space.World);
    }

    //Sets the step number
    public void SetGuideNum(int n)
    {
        StartCoroutine(WaitUntilSafe(n));
    }

    private IEnumerator WaitUntilSafe(int n)
    {
        yield return new WaitUntil(() => safeToStop);
        if (cr != null)
            StopCoroutine(cr);

        //If the step hasn't been loaded
        //Then loads it
        if (guides[n] == null)
            LoadGuide(n);

        text.text = guides[n].Item1;
        StartCoroutine(cr = DoDemo(n));
    }

    private bool safeToStop = true;
    private IEnumerator DoDemo(int n)
    {
        safeToStop = true;
        ct.SetBlocks(guides[n].Item2);
        ct.MakeUnscrambled(false);

        ct.SetRotationSpeedIndex(Random.Range(5, 8));

        yield return new WaitForSeconds(3f);

        for (int i = 0; i < guides[n].Item3.Length; i++)
        {
            safeToStop = false;
            ct.finishedRotation = false;

            ct.Rotate(guides[n].Item3[i]);

            yield return new WaitUntil(() => ct.finishedRotation);
            safeToStop = true;
            yield return new WaitForSeconds(0.25f);
        }
        safeToStop = true;
        yield return new WaitForSeconds(3f);
        StartCoroutine(cr = DoDemo(n));
    }

    //Loads the step
    private void LoadGuide(int n)
    {
        //Loads the text that will appear onscreen
        string s1 = Resources.Load<TextAsset>("guides/" + n + "/info").text;

        //Loads the demo cube and the demo it will perform
        //Refer to ___.txt for info
        string s2 = Resources.Load<TextAsset>("guides/" + n + "/demo").text;

        string[] split = s2.Split('\n');

        Block[,,] blks = new Block[3, 3, 3];

        for (int i = 0; i < 3; i++)
        {
            split[i] = split[i].ToUpper();
            split[3 + i] = split[3 + i].ToUpper();
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    blks[i, j, k] = new Block();
                    blks[i, j, k].go = GameObject.Find("GuideCenter/" + i + "" + j + "" + k);
                    blks[i, j, k].ColorAllSides(Color.black);
                }
            }
        }

        Pair<int, int>[] pos = { new Pair<int, int>(0, 0), new Pair<int, int>(0, 1), new Pair<int, int>(0, 2),
                                 new Pair<int, int>(1, 0), new Pair<int, int>(1, 1), new Pair<int, int>(1, 2),
                                 new Pair<int, int>(2, 0), new Pair<int, int>(2, 1), new Pair<int, int>(2, 2) };
        
        for (int i = 0; i < 9; i++)
        {
            int p1 = pos[i].Item1, p2 = pos[i].Item2;

            blks[p1, 2, p2].top = CharToCol(split[0][i]);
            blks[p1, p2, 0].front = CharToCol(split[1][i]);
            blks[2, p1, p2].right = CharToCol(split[2][i]);
            blks[p1, p2, 2].back = CharToCol(split[3][i]);
            blks[0, p1, p2].left = CharToCol(split[4][i]);
            blks[p1, 0, p2].bottom = CharToCol(split[5][i]);
        }

        ct.SetBlocks(blks);

        int len = int.Parse(split[6]);

        Pair<string, bool>[] arr = new Pair<string, bool>[len];

        for (int i = 0; i < len; i++)
        {
            arr[i] = CharToStep(split[7 + i][0]);
        }

        guides[n] = new Triplet<string, Block[,,], Pair<string, bool>[]>(s1, blks, arr);
    }

    private static Color CharToCol(char ch)
    {
        switch (ch)
        {
            case 'W':
                return Color.white;
            case 'R':
                return Color.red;
            case 'B':
                return Color.blue;
            case 'O':
                return new Color(1, 165f / 255f, 0);
            case 'G':
                return Color.green;
            case 'Y':
                return Color.yellow;
            case 'X':
                return Color.gray;
            default:
                return Color.black;
        }
    }

    //Converts a character to a step
    //The character refers to the color of the center of the side to rotate
    //If its uppercase - rotate clockwise - otherwise counter-clockwise
    private static Pair<string, bool> CharToStep(char ch)
    {
        switch (ch)
        {
            case 'W':
                return new Pair<string, bool>("RotateTop", true);
            case 'w':
                return new Pair<string, bool>("RotateTop", false);

            case 'R':
                return new Pair<string, bool>("RotateFront", true);
            case 'r':
                return new Pair<string, bool>("RotateFront", false);

            case 'B':
                return new Pair<string, bool>("RotateRight", true);
            case 'b':
                return new Pair<string, bool>("RotateRight", false);

            case 'O':
                return new Pair<string, bool>("RotateBack", true);
            case 'o':
                return new Pair<string, bool>("RotateBack", false);

            case 'G':
                return new Pair<string, bool>("RotateLeft", true);
            case 'g':
                return new Pair<string, bool>("RotateLeft", false);

            case 'Y':
                return new Pair<string, bool>("RotateBottom", true);
            case 'y':
                return new Pair<string, bool>("RotateBottom", false);

            default:
                return null;
        }
    }

    //Recieves mouse input to know when the mouse is pressed in the vicinity of the cube so that it can check for dragging by the mouse which will cause
    //The cube to rotate
    public void OnMouseEvent(bool b)
    {
        mouseDown = b;
    }
}