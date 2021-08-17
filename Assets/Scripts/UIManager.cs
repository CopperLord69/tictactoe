using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Assets.Scripts;
using UnityEngine.Events;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public UnityEvent<TicTacToeFigureType> OnWin;

    [SerializeField]
    private TextMeshProUGUI PlayerFigureTypeLabel;



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            OnWin.AddListener(ShowWinLabel);
        }
        else
        {
            Destroy(this);
        }
    }

    public void DisconnectPlayer()
    {
        ClientSend.SendLeavedSession();
        Client.Instance.Disconnect();
        Application.Quit();
    }

    public void EnterLobby()
    {
        if (!Client.Instance.InLobby)
        {
            ClientSend.SendPlayAgain();
            ClientSend.SendLobby();
        }
    }

    public void SetPlayerType(TicTacToeFigureType type)
    {
        PlayerFigureTypeLabel.text = "You are the " + type.Name();
    }

    public void ShowWinLabel(TicTacToeFigureType type)
    {
        PlayerFigureTypeLabel.text = type.Name() + " Won!";
    }

    public void ShowOtherPlayerLeavedMessage()
    {
        PlayerFigureTypeLabel.text = "Other player leaved game";
    }

    internal void ShowOtherPlayerWantsToPlayAgainMessage()
    {
        PlayerFigureTypeLabel.text = "Other player wants to play again";
    }
}
