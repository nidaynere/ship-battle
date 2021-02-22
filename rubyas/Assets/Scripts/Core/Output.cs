using System;
using UnityEngine;

namespace TurnBasedFW
{
 /// <summary>
 /// All output from game.
 /// </summary>
    public class Output
    {
        public Action<Map> OnMapInitialized;

        /// <summary>
        /// Game status update.
        /// </summary>
        public Action<Game.Status> OnGameStatusUpdate;

        /// <summary>
        /// Only for visual effects.
        /// </summary>
        public Action<int, Playable, Vector3[]> OnPlayablePathfind;

        /// <summary>
        /// A playable is spawned by game.
        /// int => user
        /// </summary>
        public Action<int, Playable> OnPlayableSpawned;

        /// <summary>
        /// A playable is destroyed by game. Probably dead.
        /// </summary>
        public Action<int, Playable> OnPlayableDestroyed;

        /// <summary>
        /// Playables health update.  health, maxhealth
        /// </summary>
        public Action<int, Playable, float, float> OnHealthUpdate;

        /// <summary>
        /// Some one get hurt.
        /// </summary>
        public Action<int, Playable, int, Playable> OnDamageEffect;

        /// <summary>
        /// Game finished, int is the winner.
        /// </summary>
        public Action<int> OnGameFinished;
    }
}