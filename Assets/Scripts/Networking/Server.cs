using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    public static int MaxPlayers { get; private set; }

    public static Dictionary<int, ClientObject> clients = new Dictionary<int, ClientObject>();

    public static int Port { get; private set; }

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public delegate void PacketHandler(int fromClient, Packet p);
    public static Dictionary<int, PacketHandler> packetHandlers;

    public static void Start(int maxPlayers, int port)
    {
        MaxPlayers = maxPlayers;
        Port = port;

        Debug.Log("Starting server...");
        InitializeServerData();
        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(port);
        udpListener.BeginReceive(UDPReceiveCallback, null);
        Debug.Log("Server started on port " + Port);
    }

    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log("Incoming new connection from " + client.Client.RemoteEndPoint);
        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);
                return;
            }
        }

        Debug.Log(client.Client.RemoteEndPoint + " failed to connect to server: server is full; ");
    }

    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clientPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;

            }

            using (Packet p = new Packet(data))
            {
                int clientId = p.ReadInt();

                if (clientId == 0)
                {
                    return;
                }

                if (clients[clientId].udp.endPoint == null)
                {
                    clients[clientId].udp.Connect(clientPoint);
                    return;
                }

                if (clients[clientId].udp.endPoint.ToString() == clientPoint.ToString())
                {
                    clients[clientId].udp.HandleData(p);
                }
            }

        }
        catch (Exception e)
        {
            Debug.Log("error UDP receiving: " + e.Message);
        }
    }

    public static void SendUDPData(IPEndPoint endPoint, Packet p)
    {
        try
        {
            if (endPoint != null)
            {
                udpListener.BeginSend(p.ToArray(), p.Length(), endPoint, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log("error sending udp data to " + endPoint + ": " + e.Message);
        }
    }



    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            clients.Add(i, new ClientObject(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.WelcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.LobbyEntered, ServerHandle.PlayerLobbyEnter },
                { (int)ClientPackets.LobbyExit, ServerHandle.PlayerLobbyExit },
                { (int)ClientPackets.Turn, ServerHandle.PlayerTurn },
                { (int)ClientPackets.LeavedSession, ServerHandle.PlayerLeavedSession },
            };
        Debug.Log("Initialized packets");
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }
}
