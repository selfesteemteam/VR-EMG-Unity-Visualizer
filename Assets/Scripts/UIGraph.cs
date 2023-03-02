using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(UILineRendererList))]
public class UIGraph : MonoBehaviour
{
    public UDPListener listener;
    public UILineRendererList uiLineList;
    public float timeRange;

    public Color smileColor, scowlColor, idleColor;

    private List<Vector4> points = new List<Vector4>();
    private Vector4 bufferPoint = Vector4.zero;

    private void Start()
    {
        
    }
}
