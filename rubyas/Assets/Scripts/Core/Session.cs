namespace TurnBasedFW
{
    /// <summary>
    /// Single game session. Multiple is possible.
    /// </summary>
    public class Session
    {
        public Input GameInput;
        public Output GameOutput;
        public Game Game;

        public Session()
        {
            GameInput = new Input();
            GameOutput = new Output();
        }

        public void Initialize (
            int mapSizeX,
                int mapSizeY,
                Playable[] playables,
                // Length means user count in the game. For now, keep it two. This is a game for 2 players game.
                int playableCount,
                int blocksCount)
        {
            Game = new Game(mapSizeX, mapSizeY, playables, playableCount, blocksCount, GameInput, GameOutput);
        }
    }
}
