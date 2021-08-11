using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using TMPro;

public class Client : MonoBehaviour
{
    public static Client Instance;
    public static int dataBufferSize = 4096;

    public string Ip { get; private set; }
    public bool InLobby { get; set; }
    public int port = 26950;
    public int myId = 0;
    public int battleId = 0;
    public string username = "";
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet packet);
    private static Dictionary<ServerPackets, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            tcp.socket.Close();
            udp.socket.Close();
            StopAllCoroutines();
            Debug.Log("Disconnected from server");
        }
    }

    public void ConnectToServer(TMP_InputField ip)
    {
        Ip = ip.text;
        tcp = new TCP();
        udp = new UDP();
        DontDestroyOnLoad(gameObject);
        InitializeClientData();
        tcp.Connect();
    }

    public void BecomeAHost()
    {
        Ip = "127.0.0.1";
        tcp = new TCP();
        udp = new UDP();
        DontDestroyOnLoad(gameObject);
        InitializeClientData();
        tcp.Connect();
        isConnected = true;
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedPacket;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient()
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(Instance.Ip, Instance.port, ConnectCallback, null);
        }

        public void SendData(Packet receivedPacket)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(receivedPacket.ToArray(), 0, receivedPacket.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error sending data:" + e.Message);
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                socket.EndConnect(result);

                if (!socket.Connected)
                {
                    return;
                }

                stream = socket.GetStream();

                receivedPacket = new Packet();

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (SocketException e)
            {
                DebugOutput.Instance.PutMessage(e.Message);
            }

        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Instance.Disconnect();
                    return;

                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedPacket.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception exception)
            {
                Console.WriteLine("error: " + exception.Message);

                Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;
            receivedPacket.SetBytes(data);
            if (receivedPacket.UnreadLength() >= 4)
            {
                packetLength = receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }
            while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
            {
                byte[] packetBytes = receivedPacket.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet p = new Packet(packetBytes))
                    {
                        ServerPackets packetId = (ServerPackets)p.ReadInt();
                        Debug.Log($"Received a packet {packetId}");
                        packetHandlers[packetId](p);
                    }
                });

                packetLength = 0;

                if (receivedPacket.UnreadLength() >= 4)
                {
                    packetLength = receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            if (Instance.InLobby)
            {
                ClientSend.SendLobbyExit();
            }
            Instance.Disconnect();
            stream = null;
            receiveBuffer = null;
            receivedPacket = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            Debug.Log($"127.0.0.1 |{Instance.Ip}|");
            var ip = IPAddress.Parse(Instance.Ip);
            endPoint = new IPEndPoint(ip, Instance.port);
        }


        public void Connect(int localPort)
        {
            socket = new UdpClient(localPort);
            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet packetToSend = new Packet())
            {
                SendData(packetToSend);
            }
        }


        public void SendData(Packet packetToSend)
        {
            try
            {
                packetToSend.InsertInt(Instance.myId);

                if (socket != null)
                {
                    socket.BeginSend(packetToSend.ToArray(), packetToSend.Length(), null, null);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                byte[] data = socket.EndReceive(result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (data.Length < 4)
                {
                    Instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using (Packet receivedPacket = new Packet(data))
            {
                int packetLength = receivedPacket.ReadInt();
                data = receivedPacket.ReadBytes(packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet receivedPacket = new Packet(data))
                {
                    var packetId = (ServerPackets)receivedPacket.ReadInt();
                    Debug.Log(packetId);
                    packetHandlers[packetId](receivedPacket);
                }
            });
        }

        private void Disconnect()
        {
            Instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<ServerPackets, PacketHandler>()
        {
            { ServerPackets.Welcome, ClientHandle.Welcome },
            { ServerPackets.LoadMap, ClientHandle.LoadMap },
            { ServerPackets.EnterLobby, ClientHandle.EnteredLobby },
            { ServerPackets.ExitLobby, ClientHandle.ExitLobby },
            { ServerPackets.Turn, ClientHandle.OtherPlayerTurn },
            {ServerPackets.OtherPlayerLeaved, ClientHandle.OtherPlayerLeaved }
        };
        Debug.Log("initialized packets");
    }
}
