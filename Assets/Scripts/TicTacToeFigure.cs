using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToeFigure : MonoBehaviour
{
    public TicTacToeFigureType Type => type;

    [SerializeField]
    private TicTacToeFigureType type;
}
