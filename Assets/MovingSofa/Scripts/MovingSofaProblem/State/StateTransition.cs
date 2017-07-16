using System;
using System.Collections.Generic;

namespace MovingSofaProblem.State
{
    public class StateTransition
    {
        public StateTransition(GameState newState, List<Func<GameState, GameState>> sideEffects)
        {
            this.NewState = newState;
            this.SideEffects = sideEffects;
        }

        public GameState NewState { get; private set; }
        public List<Func<GameState, GameState>> SideEffects { get; private set; }
    }
}
