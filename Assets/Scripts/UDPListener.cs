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

    public const int listenPort = 11000;
    public const int clientPort = 11001;
    public event EventHandler<Vector3> DataReceived;

    private UdpClient udpClient;

    void Start()
    {
        udpClient = ConnectTo("localhost", listenPort, clientPort);

        Task.Factory.StartNew(() => ListenForMessages(udpClient));
    }

    static UdpClient ConnectTo(string hostname, int listenPort, int clientPort)
    {
        var connection = new UdpClient(clientPort);
        connection.Connect(hostname, listenPort);
        return connection;
    }

    public struct Received
    {
        public IPEndPoint Sender;
        public byte[] Message;
    }

    async Task<Received> Receive(UdpClient client)
    {
        var result = await client.ReceiveAsync();
        return new Received()
        {
            Message = result.Buffer,
            Sender = result.RemoteEndPoint
        };
    }
    Vector3 MessageToVector3(byte[] message)
    {
        float messageX = BitConverter.ToSingle(message, 0);
        float messageY = BitConverter.ToSingle(message, 4);
        float messageZ = BitConverter.ToSingle(message, 8);

        return new Vector3(messageX, messageY, messageZ);
    }

    async void ListenForMessages(UdpClient client)
    {
        while (true)
        {
            try
            {
                var received = await Receive(udpClient);
                if (received.Message.Length == 12) // Message is a data packet
                {
                    Vector3 v = MessageToVector3(received.Message);
                    OnDataReceived(v);
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

    protected virtual void OnDataReceived(Vector3 v)
    {
        DataReceived?.Invoke(this, v);
    }
}
