using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet receivedPacket)
    {
        receivedPacket.WriteLength();
        Client.Instance.tcp.SendData(receivedPacket);
    }

    private static void SendUDPData(Packet receivedPacket)
    {
        receivedPacket.WriteLength();
        Client.Instance.udp.SendData(receivedPacket);
    }

    #region Packets

    public static void WelcomeReceived()
    {
        SendIdTcp(ClientPackets.WelcomeReceived);
    }

    public static void SendLobby()
    {
        Client.Instance.InLobby = true;
        SendIdTcp(ClientPackets.LobbyEntered);
    }

    public static void SendLobbyExit()
    {
        SendIdTcp(ClientPackets.LobbyExit);
    }

    public static void SendTurn(string fieldName)
    {
        Player.Instance.CanMakeTurn = false;
        using (Packet packetToSend = new Packet((int)ClientPackets.Turn))
        {
            int figureType = (int)Player.Instance.FigureType;
            packetToSend.Write(Client.Instance.battleId);
            packetToSend.Write(figureType);
            packetToSend.Write(fieldName);
            SendTCPData(packetToSend);
        }
    }

    public static void SendLeavedSession()
    {
        Debug.Log($"Sending leaving session");
        Player.Instance.CanMakeTurn = false;
        using (Packet packetToSend = new Packet((int)ClientPackets.LeavedSession))
        {
            packetToSend.Write(Client.Instance.battleId);
            SendTCPData(packetToSend);
        }
    }

    private static void SendIdTcp(ClientPackets packetType)
    {
        using (Packet packetToSend = new Packet((int)packetType))
        {
            packetToSend.Write(Client.Instance.myId);
            SendTCPData(packetToSend);
        }
    }

    #endregion
}
