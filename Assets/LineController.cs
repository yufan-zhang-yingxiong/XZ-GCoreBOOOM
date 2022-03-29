using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform square;
    public Transform circle;
    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    private void Update()
    {
        Vector2 v0 = square.position;
        Vector2 v1 = circle.position;
        lineRenderer.SetPosition(0, v0);
        lineRenderer.SetPosition(1, v1);
    }
}
