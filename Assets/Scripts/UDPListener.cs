using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class UDPListener : MonoBehaviour
{

    public const int listenPort = 11000; // Server port to listen to
    public const int clientPort = 11001; // Client port to listen from, must not be equal to server port if running locally
    public event EventHandler<Vector3> DataReceived; // Event to broadcast received data

    private UdpClient udpClient; // Client handler

    void Start()
    {
        // Connect to socket and begin listening for messages from server
        udpClient = ConnectTo("localhost", listenPort, clientPort);
        Task.Factory.StartNew(() => ListenForMessages(udpClient));
    }

    // Connect to the server process
    static UdpClient ConnectTo(string hostname, int listenPort, int clientPort)
    {
        var connection = new UdpClient(clientPort);
        connection.Connect(hostname, listenPort);
        return connection;
    }

    // Struct for received message
    public struct Received
    {
        public IPEndPoint Sender;
        public byte[] Message;
    }

    // When message is received, construct message struct
    async Task<Received> Receive(UdpClient client)
    {
        var result = await client.ReceiveAsync();
        return new Received()
        {
            Message = result.Buffer,
            Sender = result.RemoteEndPoint
        };
    }
    
    // Convert message data to floats for use
    Vector3 MessageToVector3(byte[] message)
    {
        float messageX = BitConverter.ToSingle(message, 0);
        float messageY = BitConverter.ToSingle(message, 4);
        float messageZ = BitConverter.ToSingle(message, 8);

        return new Vector3(messageX, messageY, messageZ);
    }

    // Listen for messages
    async void ListenForMessages(UdpClient client)
    {
        while (true)
        {
            try
            {
                var received = await Receive(udpClient);
                if (received.Message.Length == 12) // Message is a data packet
                {
                    Vector3 v = MessageToVector3(received.Message); // Convert to vector3
                    OnDataReceived(v); // Broadcast vector3
                }
            }
            catch (SocketException ex)
            {
                // Stop task if socket has been forcibly closed
                UnityEngine.Debug.LogError(ex);
                return;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(ex);
                return;
            }
        }
    }

    // Broadcasts vector3 to observing classes
    protected virtual void OnDataReceived(Vector3 v)
    {
        DataReceived?.Invoke(this, v);
    }
}
