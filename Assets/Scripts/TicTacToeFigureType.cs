using UnityEngine;

namespace Assets.Scripts
{
    public enum TicTacToeFigureType
    {
        Cross,
        Circle,
    }

    public static class FigureTypeExtensions
    {
        public static string Name(this TicTacToeFigureType type)
        {
            switch (type)
            {
                case TicTacToeFigureType.Circle:
                    {
                        return "Circle";
                    }
                case TicTacToeFigureType.Cross:
                    {
                        return "Cross";
                    }
                default:
                    {
                        return "Unknown";
                    }
            }

        }

        public static GameObject Prefab(this TicTacToeFigureType type)
        {
            switch (type)
            {
                case TicTacToeFigureType.Circle:
                    {
                        return (GameObject)Resources.Load("Prefabs/Circle");
                    }
                case TicTacToeFigureType.Cross:
                    {
                        return (GameObject)Resources.Load("Prefabs/Cross");
                    }
                default:
                    {
                        throw new MissingReferenceException("No such figure type");
                    }
            }

        }
    }




}
