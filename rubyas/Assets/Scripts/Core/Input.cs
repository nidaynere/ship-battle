using UnityEngine;
using System;

namespace TurnBasedFW
{
    /// <summary>
    /// All input to game.
    /// </summary>
    /// 

    public class Input
    {
        #region Actions
        public Action OnStartGamePlay;
        public Action OnNextTurn;
        public Action<int, string, int, int> OnPlayablePositionSet;
        public Action<int, string, string> OnPlayableAttack;
        #endregion

        #region Calls
        public void PlayablePositionSet(int user, string id, int x, int y)
        {
            OnPlayablePositionSet?.Invoke(user, id, x, y);
        }
        public void PlayableAttack(int user, string id, string targetId)
        {
            OnPlayableAttack?.Invoke(user, id, targetId);
        }

        public void StartGamePlay()
        {
            OnStartGamePlay?.Invoke();
        }

        public void NextTurn()
        {
            OnNextTurn?.Invoke();
        }
        #endregion
    }

}