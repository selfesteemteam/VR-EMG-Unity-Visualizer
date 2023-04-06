using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerClientRestarter : MonoBehaviour
{
    public ProcessRunner server;
    public UDPListener client;

    int serverPort = 11000;
    int clientPort = 11001;


    public void SetServerPort(string str)
    {
        try
        {
            int port = int.Parse(str);
            serverPort = port;
        } catch (FormatException)
        {
            Debug.LogErrorFormat("Unable to parse string {0}", str);
        }
    }
    
    public void SetClientPort(string str)
    {
        try
        {
            int port = int.Parse(str);
            clientPort = port;
        }
        catch (FormatException)
        {
            Debug.LogErrorFormat("Unable to parse string {0}", str);
        }
    }

    public void Restart()
    {
        client.CloseClient();
        client.clientPort = clientPort;
        client.listenPort = serverPort;
        client.Connect();

        server.processArgs = string.Format("\"Assets/Scripts/Python/PythonServer.py\" {0} {1}", serverPort, clientPort);
        server.RestartProcess();
    }
}
