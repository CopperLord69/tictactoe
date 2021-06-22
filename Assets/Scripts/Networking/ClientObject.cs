using Assets.Scripts.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ClientObject
{
    public static int dataBufferSize = 4096;
    public int id;
    public string username;
    public TCP tcp;
    public UDP udp;
    public Player player;

    public ClientObject(int _id)
    {
        id = _id;
        tcp = new TCP(id);
        udp = new UDP(id);
    }

    private void Disconnect()
    {
        Matchmaker.RemovePlayerFromLobby(id);
        Debug.Log($"{tcp.socket.Client.RemoteEndPoint} disconnected");
        MonoBehaviour.Destroy(player.gameObject);
        tcp.Disconnect();
        udp.Disconnect();
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private byte[] receiveBuffer;
        private Packet receivedPacket;
        public TCP(int id)
        {
            this.id = id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = dataBufferSize;
            socket.SendBufferSize = dataBufferSize;

            stream = _socket.GetStream();
            receivedPacket = new Packet();

            receiveBuffer = new byte[dataBufferSize];

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            Debug.Log("Welcome player " + id);
            ServerSend.Welcome(id, "Welcome player " + id);
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.Log("error sending data to player: " + e.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;

                }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);
                receivedPacket.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception e)
            {
                Debug.Log("error: " + e.Message);

                Server.clients[id].Disconnect();
            }
        }

        public void Disconnect()
        {
            socket.Close();
            stream = null;
            receiveBuffer = null;
            receivedPacket = null;
            socket = null;
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
                        int packetId = p.ReadInt();
                        Debug.Log($"received {(ClientPackets)packetId}");
                        Server.packetHandlers[packetId](id, p);
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

    }

    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }


        public void Connect(IPEndPoint point)
        {
            endPoint = point;
        }

        public void SendData(Packet p)
        {
            Server.SendUDPData(endPoint, p);
        }

        public void HandleData(Packet p)
        {
            int packetLength = p.ReadInt();
            byte[] packetBytes = p.ReadBytes(packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Debug.Log($"Received packet {(ClientPackets)packetId}");
                    Server.packetHandlers[packetId](id, packet);
                }
            });
        }

        public void Disconnect()
        {
            endPoint = null;
        }
    }


}