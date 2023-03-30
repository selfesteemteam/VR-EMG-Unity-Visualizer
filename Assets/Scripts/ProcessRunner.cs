using System;
using System.Diagnostics;
using UnityEngine;

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
        if (!process.HasExited)
        {
            UnityEngine.Debug.Log("Closing python server.");

            
            process.CloseMainWindow(); // Close process by sending a close message to its main window
            process.Close(); // Free resources associated with process
        }
    }

    // Starts a process inside of a shell
    public Process StartProcess(string processPath, string processArgs)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = processPath;
        start.Arguments = processArgs;
        start.UseShellExecute = true;
        process = Process.Start(start);
        process.Exited += ProcessExited;
        return Process.Start(start);
    }

    protected virtual void OnProcessExited() 
    {
        ProcessExited?.Invoke(this, EventArgs.Empty);
    }
}
