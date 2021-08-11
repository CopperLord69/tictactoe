using Assets.Scripts;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    public int id;
    public TicTacToeFigureType FigureType { get; set; }
    public bool CanMakeTurn { get; set; }

    public FieldController FieldController { get; set; }

    public void Start()
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

    public void MakeTurn(Field field)
    {
        if (CanMakeTurn)
        {
            field.PutFigure(Instance.FigureType);
            ClientSend.SendTurn(field.name);
            FieldController.CheckWin();
        }
    }
}
