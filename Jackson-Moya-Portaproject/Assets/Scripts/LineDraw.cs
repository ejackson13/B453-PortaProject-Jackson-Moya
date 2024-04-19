using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDraw : MonoBehaviour
{
    public Transform object1; // Reference to the first object
    public Transform object2; // Reference to the second object

    public Transform controlPoint1; // First control point of the Bezier curve
    public Transform controlPoint2; // Second control point of the Bezier curve

    public int numberOfPoints = 50; // Number of points to interpolate along the curve

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numberOfPoints;

        // Calculate points along the Bezier curve
        Vector3[] points = CalculateBezierCurve();

        // Set positions of the Line Renderer
        lineRenderer.SetPositions(points);
    }

    Vector3[] CalculateBezierCurve()
    {
        Vector3[] points = new Vector3[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            float t = i / (float)(numberOfPoints - 1);
            points[i] = CalculateBezierPoint(t);
        }

        return points;
    }

    Vector3 CalculateBezierPoint(float t)
    {
        float oneMinusT = 1f - t;
        float oneMinusTSquared = oneMinusT * oneMinusT;
        float tSquared = t * t;

        Vector3 point = oneMinusTSquared * object1.position +
                        2f * oneMinusT * t * controlPoint1.position +
                        tSquared * controlPoint2.position;

        return point;
    }

    void Update()
    {
        // Update positions if needed (e.g., if objects or control points move)
        Vector3[] points = CalculateBezierCurve();
        lineRenderer.SetPositions(points);
    }
}

