using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TextFadeIn : MonoBehaviour
{
    [SerializeField] private float extraDelay = 0f;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float moveDist = 1;

    [SerializeField] AnimationCurve fadeCurve;
    [SerializeField] AnimationCurve moveCurve;

    private TextMeshProUGUI text;
    private Vector3 targetPosition;
    private bool startedMove = false;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        Color startingColor = text.color;
        startingColor.a = 0;
        text.color = startingColor;

        targetPosition = transform.position;
        Vector3 startPosition = transform.position;
        startPosition.y -= moveDist;
        transform.position = startPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (!startedMove)
        {
            StartCoroutine(FadeInTween());
            StartCoroutine(MoveTween());
            startedMove = true;
        }
    }



    private IEnumerator FadeInTween()
    {

        yield return new WaitForSeconds(.5f + extraDelay);

        // set color/opacity variables
        Color startingColor = text.color;
        //startingColor.a = 0;
        Color currentColor = startingColor;
        Color targetColor = text.color;
        targetColor.a = 1;

        float passedTime = 0;
        while (passedTime <= fadeDuration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / fadeDuration;

            //currentColor.a = Mathf.Lerp(startingColor.a, targetColor.a, alpha);
            currentColor.a = Mathf.Lerp(startingColor.a, targetColor.a, fadeCurve.Evaluate(alpha));
            text.color = currentColor;

            yield return null;
        }
        text.color = targetColor;

        yield return null;
    }



    private IEnumerator MoveTween()
    {
        yield return new WaitForSeconds(.5f + extraDelay);

        // set position variables
        Vector3 startingPosition = transform.position;
        Vector3 currentPosition = startingPosition;

        float passedTime = 0;
        while (passedTime <= moveDuration)
        {
            passedTime += Time.deltaTime;
            float alpha = passedTime / moveDuration;

            //currentPosition = Vector3.Lerp(startingPosition, targetPosition, alpha);
            currentPosition = Vector3.Lerp(startingPosition, targetPosition, moveCurve.Evaluate(alpha));
            transform.position = currentPosition;

            yield return null;
        }
        transform.position = targetPosition;
    }
}
