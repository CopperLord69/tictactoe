using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    private static Dictionary<TicTacToeFigureType, GameObject> figures;

    public bool IsBusy { get; private set; }

    private void Start()
    {
        if (figures == null)
        {
            figures = new Dictionary<TicTacToeFigureType, GameObject>
                {
                    { TicTacToeFigureType.Circle, TicTacToeFigureType.Circle.Prefab() },
                    { TicTacToeFigureType.Cross, TicTacToeFigureType.Cross.Prefab()},
                };
        }
    }

    public void PutFigure(TicTacToeFigureType type)
    {
        IsBusy = true;
        Instantiate(figures[type], transform);
    }
}
