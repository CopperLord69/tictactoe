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
}
