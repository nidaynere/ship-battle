using System.Collections.Generic;

namespace TurnBasedFW
{

    public class Player
    {
        public Player(int _id)
        {
            Id = _id;
        }

        public int Id;

        public Dictionary<string, Playable> spawnedPlayables;
    }

}
