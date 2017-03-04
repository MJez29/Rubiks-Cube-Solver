using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CubeController : MonoBehaviour
{
    private GameObject[] blocks;

    private const float SCRAMBLE_PERIOD = 0.01f;
    private const float SOLVE_PERIOD = 2f;

    public float period = 1f;           //How long it takes a face to rotate 90 degrees

    public bool finishedRotation;

    //private int framesPerRotationIndex = 4;
    //private static int[] framesPerRotationArr = new int[] { 1, 3, 6, 9, 15, 18, 30, 45, 90 };
    //private int framesPerRotation = framesPerRotationArr[4];         //How many frames per rotation of the cube

    //The pivots that the cube rotates around
    //private Transform topPivot, frontPivot, rightPivot, backPivot, leftPivot, bottomPivot, center;

    private GameObject playPauseButton, nextButton, lastButton, slowerButton, fasterButton;

    private CubeTransformer ct;

    private Sprite[] buttonSprites;
    private Sprite play;
    private Sprite pause;

    void Awake()
    {
        //center = new GameObject("Center").transform;
    }

	// Use this for initialization
	void Start ()
    {
        buttonSprites = Resources.LoadAll<Sprite>("buttons");
        play = buttonSprites[5];
        pause = buttonSprites[7];

        playPauseButton = GameObject.Find("Play/Pause");
        nextButton = GameObject.Find("Next");
        lastButton = GameObject.Find("Last");
        slowerButton = GameObject.Find("Slower");
        fasterButton = GameObject.Find("Faster");

        finishedRotation = true;

        ct = GetComponent<CubeTransformer>();

        Debug.Log(playPauseButton == null);
        Debug.Log(nextButton == null);
        Debug.Log(lastButton == null);
        Debug.Log(slowerButton == null);
        Debug.Log(fasterButton == null);

        playPauseButton.GetComponent<Button>().interactable = nextButton.GetComponent<Button>().interactable =
            lastButton.GetComponent<Button>().interactable = slowerButton.GetComponent<Button>().interactable =
            fasterButton.GetComponent<Button>().interactable = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnApplicationQuit()
    {
        Debug.Log("STOP REQUESTED");
        CubeSolver.RequestStop();

    }

    private IEnumerator ScrambleCube()
    {
        playPauseButton.GetComponent<Button>().interactable = nextButton.GetComponent<Button>().interactable = 
            lastButton.GetComponent<Button>().interactable = slowerButton.GetComponent<Button>().interactable =
            fasterButton.GetComponent<Button>().interactable = false;

        Pair<string, bool>[] moves = CubeSolver.Scramble();

        period = SCRAMBLE_PERIOD;

        for (int i = 0; i < moves.Length; i++)
        {
            Pair<string, bool> move = moves[i];

            UpdateScrambleFPR((double) i / moves.Length);
            
            ct.finishedRotation = false;
            
            ct.Rotate(move);

            yield return new WaitUntil(() => ct.finishedRotation);
        }

        StartCoroutine(SolveCube());
    }

    //Scrambling eases in and out
    private void UpdateScrambleFPR(double d)
    {
        if (d < 1 / 7.0)
            ct.SetRotationSpeedIndex(4);
        else if (d < 2 / 7.0)
            ct.SetRotationSpeedIndex(3);
        else if (d < 3 / 7.0)
            ct.SetRotationSpeedIndex(2);
        else if (d < 4 / 7.0)
            ct.SetRotationSpeedIndex(1);
        else if (d < 5 / 7.0)
            ct.SetRotationSpeedIndex(2);
        else if (d < 6 / 7.0)
            ct.SetRotationSpeedIndex(3);
        else
            ct.SetRotationSpeedIndex(4);
    }

    private IEnumerator SolveCube(bool ip = true)
    {
        int moves = 0;
        CubeSolver.Solve();

        finishedRotation = true;

        isPaused = ip;

        ct.SetRotationSpeedIndex(4);

        playPauseButton.GetComponent<Button>().interactable = true;
        nextButton.GetComponent<Button>().interactable = lastButton.GetComponent<Button>().interactable = true;
        fasterButton.GetComponent<Button>().interactable = slowerButton.GetComponent<Button>().interactable = true;
        playPauseButton.GetComponent<Image>().sprite = play;

        Debug.Log("SOL LEN: " + CubeSolver.solution.Count);

        yield return new WaitUntil(() => CubeSolver.solution.Count > 0);

        int solutionIndex = -1;
        while (true)
        {
            ct.finishedRotation = false;
            yield return new WaitUntil(() => !isPaused || stepForward || stepBackward);

            if (!isPaused || stepForward)
            {
                if (solutionIndex < CubeSolver.solution.Count - 1)
                {
                    solutionIndex++;
                    ct.Rotate(CubeSolver.solution[solutionIndex]);
                }
                else
                    ct.finishedRotation = true;
            }
            else
            {
                if (solutionIndex >= 0)
                {
                    ct.Rotate(CubeSolver.solution[solutionIndex].Item1, !CubeSolver.solution[solutionIndex].Item2);
                    solutionIndex--;
                }
                else
                    ct.finishedRotation = true;
            }

            stepForward = stepBackward = false;
            yield return new WaitUntil(() => ct.finishedRotation);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ColorCube()
    {
        gameObject.AddComponent<CubeColorer>();

        //All buttons except play/pause button are disabled
        nextButton.GetComponent<Button>().interactable = lastButton.GetComponent<Button>().interactable = 
            fasterButton.GetComponent<Button>().interactable = slowerButton.GetComponent<Button>().interactable = false;

        playPauseButton.GetComponent<Button>().interactable = true;
        playPauseButton.GetComponent<Image>().sprite = play;
        isPaused = true;

        while (true)
        {
            yield return new WaitUntil(() => !isPaused);
            if (CubeSolver.IsValidConfiguration(ct.GetBlocks()))
            {
                CubeColorer cc = gameObject.GetComponent<CubeColorer>();
                if (cc != null)
                    Destroy(cc);
                StartCoroutine(SolveCube(false));
                yield break;
            }
        }
    }

    private bool isPaused;

    //Can only be true when the cube solving is paused
    //Only 1 can be true at a time
    //Either goes one step forward into the solution or one step backward
    private bool stepForward, stepBackward;

    public void OnPlayPauseClick()
    {
        isPaused = !isPaused;
        playPauseButton.GetComponent<Image>().sprite = isPaused ? play : pause;
        Debug.Log("PLAY/PAUSE " + n++ + " " + isPaused);

        nextButton.GetComponent<Button>().interactable = lastButton.GetComponent<Button>().interactable = isPaused;
        
    }

    public void OnNextClick()
    {
        stepForward = true;
    }

    public void OnLastClick()
    {
        stepBackward = true;
    }

    int n = 0;
    public void OnFasterClick()
    {
        ct.SpeedUpRotation();
    }

    public void OnSlowerClick()
    {
        ct.SlowDownRotation();
    }

    //NOT DONE
    public void OnScrambleClick()
    {
        if (!isPaused)
            isPaused = true;

        StopAllCoroutines();

        ct.MakeUnscrambled(true);
        CubeSolver.Reset();

        StartCoroutine(ScrambleCube());
    }

    public void OnResetClick()
    {
        if (!isPaused)
            isPaused = true;

        StopAllCoroutines();
        CubeSolver.Reset();
        ct.MakeUnscrambled(true);
    }

    public void OnRecolorClick()
    {
        if (!isPaused)
            isPaused = true;

        StopAllCoroutines();
        CubeSolver.Reset();

        Debug.Log("Calling Blacken()");
        ct.Blacken();

        StartCoroutine(ColorCube());
    }
}