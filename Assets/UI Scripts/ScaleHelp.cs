using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScaleHelp : MonoBehaviour
{
    private RectTransform container, scrollView, canvas;

    private LayoutElement helpText;

	// Use this for initialization
	void Start ()
    {
        canvas = GameObject.Find("HelpCanvas").GetComponent<RectTransform>();
        container = GameObject.Find("HelpContainer").GetComponent<RectTransform>();
        scrollView = GameObject.Find("Scroll View").GetComponent<RectTransform>();
        helpText = GameObject.Find("HelpText").GetComponent<LayoutElement>();
	}
	
	// Update is called once per frame
	void Update () {
	}

    void OnEnable()
    {
        StartCoroutine(Resize());
    }

    private IEnumerator Resize()
    {
        //If we do not wait until the end of frame then the canvas dimensions will be wrong and will lead improper scaling
        yield return new WaitForEndOfFrame();

        float width = canvas.rect.width, height = canvas.rect.height;

        //Resizes the container of the help screen so that it fits onscreen
        //The container has a scale of 0.25 by 0.25 which must be factored in when computing whether display the max possible dimensions or scale it according to
        //The screen size
        container.sizeDelta = new Vector2(Mathf.Min(width * 0.75f / container.localScale.x, 2000), Mathf.Min(height * 0.75f / container.localScale.y * 0.9f, 4000));

        scrollView.sizeDelta = new Vector2(container.sizeDelta.x * 0.9f, scrollView.sizeDelta.y);

        helpText.preferredWidth = scrollView.sizeDelta.x * 0.95f;
    }
}
