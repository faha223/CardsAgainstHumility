using System;
using System.Collections.Generic;
using CardsAgainstHumility.GameClasses;

namespace CardsAgainstHumility.Events
{
    public sealed class GameAddedEventArgs : EventArgs
    {
        public List<GameInstance> Games { get; private set; }
        public GameAddedEventArgs(List<GameInstance> games)
        {
            Games = games;
        }
    }
}