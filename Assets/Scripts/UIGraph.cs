using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UIElements;

[RequireComponent(typeof(RectTransform))]
public class UIGraph : MonoBehaviour
{
    public UDPListener source; // Information source for adding new points
    public UILineRenderer f1Line, f2Line, f3Line; // Individual lines to plot points
    public RectTransform f1Anchor, f2Anchor, f3Anchor; // Endpoints for each line
    public TextMeshProUGUI f1Text, f2Text, f3Text; // Text for each line
    public float timeRange; // Range of time the graph plots
    public float timeResolution; // Minimum amount of time between points before a point can be added

    public List<Vector4> points = new List<Vector4>();
    private Vector4 bufferedPoint;
    private readonly Object bufferedPointLock = new Object();
    private RectTransform b;

    private void Start()
    {
        b = GetComponent<RectTransform>();  // Used for positioning points/graphical elements
        source.DataReceived += BufferPoint; // Observer for receiving new points
    }

    void LateUpdate()
    {
        if (bufferedPoint != null)
        {
            Monitor.Enter(bufferedPointLock); // Lock bufferedPoint to this thread
            // Get time on main thread
            bufferedPoint.w = Time.time;

            if (points.Count == 0 || bufferedPoint.w - points[points.Count - 1].w > timeResolution) {

                // Add point to list
                points.Add(bufferedPoint);
                Monitor.Exit(bufferedPointLock); // Release bufferedPoint after it has been added to the list
                Vector4 lastPoint = points.Last(); // Use the last point in the list for future calculation

                // Adjust anchor points
                f1Anchor.SetLocalPositionAndRotation(new Vector3(b.rect.xMax, Mathf.Lerp(b.rect.yMin, b.rect.yMax, lastPoint.x)), Quaternion.identity);
                f2Anchor.SetLocalPositionAndRotation(new Vector3(b.rect.xMax, Mathf.Lerp(b.rect.yMin, b.rect.yMax, lastPoint.y)), Quaternion.identity);
                f3Anchor.SetLocalPositionAndRotation(new Vector3(b.rect.xMax, Mathf.Lerp(b.rect.yMin, b.rect.yMax, lastPoint.z)), Quaternion.identity);

                // Adjust text
                f1Text.text = string.Format("<mspace=0.6em>Neutral | {0,3:0.00}", lastPoint.x);
                f2Text.text = string.Format("<mspace=0.6em>Smile   | {0,3:0.00}", lastPoint.y);
                f3Text.text = string.Format("<mspace=0.6em>Scowl   | {0,3:0.00}", lastPoint.z);

                // Adjust text draw order
                // This is pretty awful to look at, may want to refactor
                // Possible solution: use an array of tuples to pair y-axis values to text elements, sort elements using some sorting algorithm,
                //                    then using the array index to change the sibling index (3 + array index when sorted grt to lst)
                if (lastPoint.x > lastPoint.y)
                {
                    if (lastPoint.x > lastPoint.z)
                    {
                        if (lastPoint.y > lastPoint.z)
                        {
                            f1Anchor.SetSiblingIndex(3);
                            f2Anchor.SetSiblingIndex(4);
                            f3Anchor.SetSiblingIndex(5);
                        } else
                        {
                            f1Anchor.SetSiblingIndex(3);
                            f3Anchor.SetSiblingIndex(4);
                            f2Anchor.SetSiblingIndex(5);
                        }
                    } else
                    {
                        f3Anchor.SetSiblingIndex(3);
                        f1Anchor.SetSiblingIndex(4);
                        f2Anchor.SetSiblingIndex(5);
                    }
                } else if (lastPoint.y > lastPoint.z)
                {
                    if (lastPoint.x > lastPoint.z)
                    {
                        f2Anchor.SetSiblingIndex(3);
                        f1Anchor.SetSiblingIndex(4);
                        f3Anchor.SetSiblingIndex(5);
                    } else
                    {
                        f2Anchor.SetSiblingIndex(3);
                        f3Anchor.SetSiblingIndex(4);
                        f1Anchor.SetSiblingIndex(5);
                    }
                } else
                {
                    f3Anchor.SetSiblingIndex(3);
                    f2Anchor.SetSiblingIndex(4);
                    f1Anchor.SetSiblingIndex(5);
                } 
            }
            else // Done to prevent exiting twice
            {
                Monitor.Exit(bufferedPointLock); // Release bufferedPoint so it can be updated
            }
        }

        // Trim unneccesary points
        while (points.Count > 1 && (Time.time - points[1].w) > timeRange)
        {
            points.RemoveAt(0);
        }
        
        // Generate point lists for individual lines
        List<Vector2> rPlotPoints = new List<Vector2>();
        List<Vector2> gPlotPoints = new List<Vector2>();
        List<Vector2> bPlotPoints = new List<Vector2>();
        foreach (Vector4 p in points)
        {
            float x  = Mathf.Lerp(b.rect.xMax, b.rect.xMin, (Time.time - p.w) / timeRange);
            float y1 = Mathf.Lerp(b.rect.yMin, b.rect.yMax, p.x); // Points for feature 1 line
            float y2 = Mathf.Lerp(b.rect.yMin, b.rect.yMax, p.y); // Points for feature 2 line
            float y3 = Mathf.Lerp(b.rect.yMin, b.rect.yMax, p.z); // Points for feature 3 line

            rPlotPoints.Add(new Vector2(x, y1));
            gPlotPoints.Add(new Vector2(x, y2));
            bPlotPoints.Add(new Vector2(x, y3));
        }

        // Draw lines using point lists
        f1Line.Points = rPlotPoints.ToArray();
        f3Line.Points = bPlotPoints.ToArray();
        f2Line.Points = gPlotPoints.ToArray();
    }

    // Observer changes the value of the buffered point so long as the main thread isn't using it
    void BufferPoint(object sender, Vector3 param)
    {
        if (Monitor.TryEnter(bufferedPointLock)) // Try to edit bufferedPoint, non-blocking when bufferedPoint is locked
        {
            bufferedPoint = param;
            Monitor.Exit(bufferedPointLock);
        }
    }
}
