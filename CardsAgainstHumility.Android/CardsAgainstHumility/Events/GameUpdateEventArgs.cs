using System;
using CardsAgainstHumility.SocketComm;

namespace CardsAgainstHumility.Events
{
    public sealed class GameUpdateEventArgs : EventArgs
    {
        public GameState GameState { get; private set; }
        public GameUpdateEventArgs(GameState gameState)
        {
            GameState = gameState;
        }
    }
}