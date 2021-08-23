using Assets.Scripts.Networking;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int fromClient, Packet p)
    {
        int clientIdCheck = p.ReadInt();

        Debug.Log(Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint + " connected and now is player " + fromClient);
        if (fromClient != clientIdCheck)
        {
            Debug.Log("Player (ID: " + fromClient + ") has assumed the wrong client ID " + clientIdCheck);
        }
    }

    public static void PlayerLobbyEnter(int fromClient, Packet receivedPacket)
    {
        Debug.Log($"Player {fromClient} entered lobby");
        Matchmaker.AddPlayerToLobby(fromClient);
        ServerSend.ClientEnterLobby(fromClient);
        Matchmaker.SendPlayersToMap();
    }

    public static void PlayerLobbyExit(int fromClient, Packet receivedPacket)
    {
        Debug.Log($"Player {fromClient} exit lobby");
        Matchmaker.RemovePlayerFromLobby(fromClient);
        ServerSend.ClientExitLobby(fromClient);
    }

    public static void PlayerTurn(int fromClient, Packet receivedPacket)
    {
        Debug.Log($"Player {fromClient} made turn");
        var battleId = receivedPacket.ReadInt();
        using(Packet packetToSend = new Packet((int)ServerPackets.Turn))
        {
            packetToSend.Write(receivedPacket.ReadInt());
            packetToSend.Write(receivedPacket.ReadString());
            ServerSend.SendTcpDataToAllInBattleExcept(fromClient, packetToSend, battleId);
        }
    }

    public static void PlayerLeavedSession(int fromClient, Packet p)
    {
        Debug.Log($"Player {fromClient} leaved session");
        var battleId = p.ReadInt(); 
        using(Packet packetToSend = new Packet((int)ServerPackets.OtherPlayerLeaved))
        {
            ServerSend.SendTcpDataToAllInBattleExcept(fromClient, packetToSend, battleId);
        }
    }

    internal static void PlayerPlayAgain(int fromClient, Packet p)
    {
        var battleId = p.ReadInt();
        using(Packet packetToSend = new Packet((int)ServerPackets.PlayAgain))
        {
            ServerSend.SendTcpDataToAllInBattleExcept(fromClient, packetToSend, battleId);
        }
    }
}
