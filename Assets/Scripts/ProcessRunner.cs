using System.Diagnostics;
using UnityEngine;

public class ProcessRunner : MonoBehaviour
{
    public string processPath;
    public string processArgs;

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

            // Close process by sending a close message to its main window.
            process.CloseMainWindow();
            // Free resources associated with process.
            process.Close();
        }
    }

    Process StartProcess(string processPath, string processArgs)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = processPath;
        start.Arguments = processArgs;
        start.UseShellExecute = true;
        return Process.Start(start);
    }

    public bool HasProcessExited()
    {
        return process.HasExited;
    }
}
