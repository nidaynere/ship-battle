using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TurnBasedFW
{
    public class Game
    {
        public enum Status
        {
            Pending,
            User1Turn,
            User2Turn,
            Finished
        }

        public Map GetMap { get; }

        private Status _gameStatus;
        private Status gameStatus
        {
            get
            {
                return _gameStatus;
            }

            set
            {
                _gameStatus = value;
                output.OnGameStatusUpdate?.Invoke(value);
            }
        }

        /// <summary>
        /// users in game.
        /// </summary>
        private Dictionary<int, Player> players;

        private Output output;

        public Game
            (
                int mapSizeX,
                int mapSizeY,
                Playable[] playables,
                // Length means user count in the game. For now, keep it two. This is a game for 2 players game.
                int playableCount,
                int randomBlocksCount,
                Input gameInput,
                Output gameOutput
            )
        {
            #region create input
            output = gameOutput;
            // register game to input.
            gameInput.OnPlayablePositionSet += SetPlayablePosition;
            gameInput.OnPlayableAttack += PlayableAttack;
            gameInput.OnStartGamePlay += StartGamePlay;
            gameInput.OnNextTurn += NextTurn;
            #endregion

            GetMap = new Map(mapSizeX, mapSizeY, randomBlocksCount);

            // random blocks on the map.

            output.OnMapInitialized?.Invoke(GetMap);

            #region players definition
            players = new Dictionary<int, Player>();

            int userId = 0;
            foreach (var playable in playables)
            {
                if (playable.Attributes == null)
                {
                    Debug.LogError("Attributes of playable is null.");
                    continue;
                }

                ///define user.
                var newPlayer = new Player(userId);
                newPlayer.spawnedPlayables = new Dictionary<string, Playable>();
                players.Add(userId, newPlayer);

                //spawn.
                SpawnPlayable(playable, userId, ref playableCount);

                userId++;
            }

            #endregion

            ///User 2 will start.
            gameStatus = Status.Pending;
        }

        /// <summary>
        /// Spawn playables on map.
        /// </summary>
        /// <param name="playable"></param>
        /// <param name="user"></param>
        /// <param name="playableCount"></param>
        private Playable[] SpawnPlayable(Playable playable, int user, ref int playableCount)
        {
            for (int i = 0; i < playableCount; i++)
            {
                int[] position = GetMap.GetRandomPosition(user);
                GetMap.SetWalkable(position[0], position[1], false);
                // set the available position as non walkable.

                var newPlayable = playable.Clone<Playable>();
                newPlayable.ID = System.Guid.NewGuid().ToString();
                newPlayable.PosX = position[0];
                newPlayable.PosY = position[1];
                newPlayable.Attributes.Initialize();

                players[user].spawnedPlayables.Add(newPlayable.ID, newPlayable);

                // Create health listener.
                newPlayable.Attributes.RegisterListener("health", (value) => {

                    float maxHealth = newPlayable.Attributes.GetAttribute("maxhealth");

                    if (value <= 0)
                    {
                        //
                        Debug.Log("playable " + newPlayable.ID + " is dead.");
                        players[user].spawnedPlayables.Remove(newPlayable.ID);

                        output.OnPlayableDestroyed?.Invoke(user, newPlayable);
                    }
                    else
                    {
                        output.OnHealthUpdate?.Invoke(user, newPlayable, value, maxHealth);
                    }
                });
                //

                output.OnPlayableSpawned?.Invoke(user, newPlayable);
            }

            return players[user].spawnedPlayables.Values.ToArray();
        }

        #region user functions (called by input) use input class.

        private void StartGamePlay ()
        {
            if (gameStatus == Status.Pending)
            {
                gameStatus = Status.User2Turn;
            }
        }

        private void NextTurn()
        {
            if (gameStatus == Status.User1Turn)
                gameStatus = Status.User2Turn;
            else if (gameStatus == Status.User2Turn)
                gameStatus = Status.User1Turn;
        }

        private void SetPlayablePosition(int user, string id, int x, int y)
        {
            var playable = players[user].spawnedPlayables[id];

            // unblock current pos.
            GetMap.SetWalkable(playable.PosX, playable.PosY, true);

            playable.SetPosition(x, y);

            // block current pos.
            GetMap.SetWalkable(x, y, false);
        }

        private void PlayableAttack(int user, string id, string targetId)
        {
            var playable = players[user].spawnedPlayables[id];
            var tPlayable = players[user == 0? 1: 0].spawnedPlayables[targetId];

            Vector3 tPosition = tPlayable.GetPosition();

            var canSee = GetMap.CanSee (playable.GetPosition(), tPosition);

            if (canSee)
            { // we can see the target.
                // damage effect.
                output.OnDamageEffect?.Invoke(user, playable, user == 0 ? 1 : 0, tPlayable);

                float damage = playable.Attributes.GetAttribute("damage");

                float tHealth = tPlayable.Attributes.GetAttribute("health");
                tHealth -= damage;

                tPlayable.Attributes.SetAttribute("health", tHealth);

                // Check end game.
                int whoWon = 1;
                bool gameEnded = false;
                foreach (var p in players)
                {
                    if (p.Value.spawnedPlayables.Count == 0)
                    {
                        gameEnded = true;
                        if (p.Key == 1)
                            whoWon = 0;
                    }
                }

                if (gameEnded)
                {
                    output.OnGameFinished?.Invoke(whoWon);
                }
                else
                    NextTurn();
            }
            else
            {
                // move to tPosition, the point which at we can see the target
                var path = GetMap.GetPath(playable.GetPosition(), tPosition);
                
                int pathLength = path.Length;
                for (int i = 0; i < pathLength; i++)
                {
                    if (GetMap.CanSee(path[i], tPosition))
                    {
                        // cut the path.
                        path = path.Take(i+1).ToArray ();
                        break;
                    }
                }

                var newPos = path[path.Length - 1];

                output.OnPlayablePathfind?.Invoke(user, playable, path);

                SetPlayablePosition(user, id, Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.z));
            }
        }

        #endregion
    }

}

