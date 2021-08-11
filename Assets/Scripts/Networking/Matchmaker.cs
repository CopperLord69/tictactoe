using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Networking
{
    static class Matchmaker
    {
        private static readonly int[] mapIds = new int[] { 1 };
        private static readonly int minPlayersPerSession = 2;

        private static List<int> lobby = new List<int>();

        private static Dictionary<int, List<int>> battles = new Dictionary<int, List<int>>();

        private static int GenerateBattleId()
        {
            var id = Random.Range(-25565, 25565);

            while (battles.ContainsKey(id) || id == 0)
            {
                id = Random.Range(-25565, 25565);
            }
            return id;
        }

        public static void AddPlayerToLobby(int id)
        {
            lobby.Add(id);
            Debug.Log("Player " + id + " entered lobby");
        }

        public static List<int> GetPlayersInBattle(int battleId)
        {
            return battles[battleId];
        }

        public static void RemovePlayerFromLobby(int id)
        {
            lobby.Remove(id);
            Debug.Log("Player " + id + " exit lobby");
        }

        public static void SendPlayersToMap()
        {
            if (lobby.Count >= minPlayersPerSession)
            {
                List<TicTacToeFigureType> figureTypes = new List<TicTacToeFigureType>()
                {
                    TicTacToeFigureType.Cross,
                    TicTacToeFigureType.Circle,
                };
                List<bool> playersCanMove = new List<bool>();
                for(int i = 0; i < minPlayersPerSession; i++)
                {
                    playersCanMove.Add(false);
                }
                playersCanMove[Random.Range(0, minPlayersPerSession - 1)] = true;
                var battleId = GenerateBattleId();
                battles.Add(battleId, new List<int>());
                var mapId = Random.Range(mapIds[0], mapIds[mapIds.Length - 1]);
                for(int count = 0; count < minPlayersPerSession; count++)
                {
                    var figureType = figureTypes[Random.Range(0, figureTypes.Count)];
                    figureTypes.Remove(figureType);
                    var client = lobby.First();
                    var canMove = playersCanMove[count];
                    ServerSend.LoadMap(client, mapId, battleId, (int)figureType, canMove);
                    battles[battleId].Add(client);
                    lobby.Remove(lobby.First());
                }
            }
        }
    }
}
