using Assets.Scripts;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet receivedPacket)
    {
        string msg = receivedPacket.ReadString();
        int myId = receivedPacket.ReadInt();

        Client.Instance.myId = myId;
        ClientSend.WelcomeReceived();

        Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
        DebugOutput.Instance.PutMessage("Connected to server");
        DebugOutput.Instance.PutMessage($"Your IP is {IPManager.GetIP(ADDRESSFAM.IPv4)}");
        ClientSend.SendLobby();
    }

    public static void LoadMap(Packet receivedPacket)
    {
        int mapId = receivedPacket.ReadInt();
        Client.Instance.InLobby = false;
        Client.Instance.battleId = receivedPacket.ReadInt();
        int figureType = receivedPacket.ReadInt();
        Player.Instance.CanMakeTurn = receivedPacket.ReadBool();
        Player.Instance.FigureType = (TicTacToeFigureType)figureType;
        SceneManager.LoadScene(mapId);
    }

    public static void EnteredLobby(Packet receivedPacket)
    {
        Client.Instance.InLobby = true;
        DebugOutput.Instance.PutMessage("Entered lobby");
    }

    public static void ExitLobby(Packet receivedPacket)
    {
        Client.Instance.InLobby = false;
        DebugOutput.Instance.PutMessage("Exit lobby");

    }

    public static void OtherPlayerTurn(Packet receivedPacket)
    {
        Player.Instance.CanMakeTurn = true;
        Debug.Log("other player turn");
        var type = receivedPacket.ReadInt();
        var fieldName = receivedPacket.ReadString();
        Player.Instance.FieldController.PutFigureInField(fieldName, type);
    }

    public static void OtherPlayerLeaved(Packet packet)
    {
        UIManager.Instance.ShowOtherPlayerLeavedMessage();
    }

    internal static void OtherPlayerPlayAgain(Packet packet)
    {
        UIManager.Instance.ShowOtherPlayerWantsToPlayAgainMessage();
    }
}
