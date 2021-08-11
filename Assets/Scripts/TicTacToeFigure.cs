using Assets.Scripts;
using UnityEngine;

public class TicTacToeFigure : MonoBehaviour
{
    public TicTacToeFigureType Type => type;

    [SerializeField]
    private TicTacToeFigureType type;
}
