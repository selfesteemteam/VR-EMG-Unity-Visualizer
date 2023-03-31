using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Reflection;
using UnityEditor.Search;
using UnityEngine.UI.Extensions;

public class ProcessExitedWindow : MonoBehaviour
{
    public ProcessRunner processRunner;

    private CanvasGroup cg;

    private void Start()
    {
        cg = GetComponent<CanvasGroup>();

        try
        {
            processRunner.ProcessExited += OnProcessClosed; // Listen for data from the server, use it to adjust animation rig
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void Hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
    }

    public IEnumerator Show()
    {
        cg.alpha = 1f;
        cg.interactable = true;
        yield return null;
    }

    void OnProcessClosed(object sender, System.EventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(Show()); // Throw event onto main thread for handling
    }
}
