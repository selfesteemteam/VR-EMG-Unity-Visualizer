using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;

[RequireComponent(typeof(RectTransform))]
public class UIGraph : MonoBehaviour
{
    public UDPListener source;
    public UILineRenderer rLine, gLine, bLine;
    public float timeRange;

    private List<Vector4> points = new List<Vector4>();
    private Vector4 bufferedPoint;

    private RectTransform b;

    private void Start()
    {
        b = GetComponent<RectTransform>();
        source.DataReceived += BufferPoint;
    }

    void FixedUpdate()
    {
        if (bufferedPoint != null)
        {
            // Get time on main thread
            bufferedPoint.w = Time.fixedTime;
            // Add the newest point
            points.Add(bufferedPoint);
        }

        // Trim unneccesary points
        while (points.Count > 1 && (Time.fixedTime - points[1].w) > timeRange)
        {
            points.RemoveAt(0);
        }
        
        List<Vector2> rPlotPoints = new List<Vector2>();
        List<Vector2> gPlotPoints = new List<Vector2>();
        List<Vector2> bPlotPoints = new List<Vector2>();
        foreach (Vector4 p in points)
        {
            float x = Mathf.Lerp(b.rect.xMax, b.rect.xMin, (Time.fixedTime - p.w) / timeRange);
            float yr = Mathf.Lerp(b.rect.yMin, b.rect.yMax, p.x);
            float yg = Mathf.Lerp(b.rect.yMin, b.rect.yMax, p.y);
            float yb = Mathf.Lerp(b.rect.yMin, b.rect.yMax, p.z);

            rPlotPoints.Add(new Vector2(x, yr));
            gPlotPoints.Add(new Vector2(x, yg));
            bPlotPoints.Add(new Vector2(x, yb));
        }

        rLine.Points = rPlotPoints.ToArray();
        bLine.Points = bPlotPoints.ToArray();
        gLine.Points = gPlotPoints.ToArray();
    }

    void BufferPoint(object sender, Vector3 param)
    {
        bufferedPoint = param;
    }
}
