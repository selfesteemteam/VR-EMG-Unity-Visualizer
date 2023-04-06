using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class ProcessRunner : MonoBehaviour
{
    public string processPath; // Path to process to be ran
    public string processArgs; // Arguments to run process with
    public event EventHandler ProcessExited; // Raised when the process exits, used for UI to restart process if it ends early

    private Process process;

    // An attempt to ensure that processes are started before
    // components that need them initialize
    void Awake()
    {
        process = StartProcess(processPath, processArgs);
    }

    private void OnDestroy()
    {
        CloseProcess();
    }

    // Starts a process inside of a shell
    private Process StartProcess(string processPath, string processArgs)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = processPath;
        start.Arguments = processArgs;
        start.UseShellExecute = true;
        process = Process.Start(start);
        process.EnableRaisingEvents = true;
        process.Exited += OnProcessExited;
        return process;
    }

    private void CloseProcess()
    {
        process.Exited -= ProcessExited; // Process is exiting normally, so remove event before it goes off
        if (!process.HasExited)
        {
            UnityEngine.Debug.Log("Closing python server.");

            process.CloseMainWindow(); // Close process by sending a close message to its main window
            process.Close(); // Free resources associated with process
        } else
        {
            UnityEngine.Debug.Log("Attempted to close process that has already exited.");
        }
    }

    public void RestartProcess()
    {
        CloseProcess();
        process = StartProcess(processPath, processArgs);
        process.EnableRaisingEvents = true;
        process.Exited += OnProcessExited;
    }

    protected virtual void OnProcessExited(object sender, System.EventArgs e) 
    {
        ProcessExited?.Invoke(this, EventArgs.Empty);
        process.Exited -= ProcessExited; // Remove event handler from object
    }
}
