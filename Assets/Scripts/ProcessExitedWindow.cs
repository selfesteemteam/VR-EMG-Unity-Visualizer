using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Reflection;
using UnityEditor.Search;

public class ProcessExitedWindow : MonoBehaviour
{
    public ProcessRunner processRunner;

    private CanvasGroup cg;
    private bool hidden;

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

    private void OnGUI()
    {
        if (hidden)
        {
            Hide();
        } else
        {
            Show();
        }
    }

    void Test(float f)
    {
        Debug.Log(Thread.CurrentThread.ManagedThreadId);
    }

    public void Hide()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        hidden = true;
    }

    public void Show()
    {
        cg.alpha = 1f;
        cg.interactable = true;
        hidden = false;
    }

    void OnProcessClosed(object sender, System.EventArgs e)
    {
        hidden = true;
    }
}
