using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Networking;

public class ServerSend : MonoBehaviour
{
    #region Packets
    public static void Welcome(int tcpClient, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.Welcome))
        {
            packet.Write(msg);
            packet.Write(tcpClient);
            Debug.Log("sending welcome");
            SendTcpData(tcpClient, packet);
        }
    }

    public static void ClientEnterLobby(int fromClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.EnterLobby))
        {
            SendTcpData(fromClient, packet);
        }
    }

    public static void ClientExitLobby(int fromClient)
    {
        using (Packet packet = new Packet((int)ServerPackets.ExitLobby))
        {
            SendTcpData(fromClient, packet);
        }
    }

    public static void LoadMap(int toClient, int mapId, int battleId, int figureType, bool canMove)
    {
        using (Packet p = new Packet((int)ServerPackets.LoadMap))
        {
            p.Write(mapId);
            p.Write(battleId);
            p.Write(figureType);
            p.Write(canMove);
            Debug.Log("Sending loadmap to client " + toClient);
            SendTcpData(toClient, p);
        }
    }

    #endregion

    #region TCP
    private static void SendTcpData(int tcpClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[tcpClient].tcp.SendData(packet);
    }



    private static void SendTcpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    public static void SendTcpDataToAllExcept(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
    }

    public static void SendTcpDataToAllInBattle(int battleId, Packet p)
    {
        p.WriteLength();
        foreach (var client in Matchmaker.GetPlayersInBattle(battleId))
        {
            Server.clients[client].tcp.SendData(p);
        }
    }

    public static void SendTcpDataToAllInBattleExcept(int except, Packet p, int battleId)
    {
        p.WriteLength();
        foreach (var client in Matchmaker.GetPlayersInBattle(battleId))
        {
            if (client != except)
            {
                Server.clients[client].tcp.SendData(p);
            }
        }
    }

    #endregion

    #region UDP
    private static void SendUdpData(int client, Packet p)
    {
        p.WriteLength();
        Server.clients[client].udp.SendData(p);
    }

    private static void SendUdpDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    public static void SendUdpDataToAllExcept(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i < Server.MaxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].udp.SendData(packet);
            }
        }
    }

    public static void SendUdpDataToAllInBattle(int battleId, Packet p)
    {
        p.WriteLength();
        foreach (var client in Matchmaker.GetPlayersInBattle(battleId))
        {
            Server.clients[client].udp.SendData(p);
        }
    }

    public static void SendUdpDataToAllInBattleExcept(int battleId, int except, Packet p)
    {
        p.WriteLength();
        foreach (var client in Matchmaker.GetPlayersInBattle(battleId))
        {
            if (client != except)
            {
                Server.clients[client].udp.SendData(p);
            }
        }
    }
    #endregion
}
