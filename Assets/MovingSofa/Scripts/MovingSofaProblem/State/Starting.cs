using System;
using System.Collections.Generic;

namespace MovingSofaProblem.State
{
    public sealed class Starting : GameState
    {
        override public string SayableStateDescription { get { return "Starting."; } }
        override public string SayableStatus { get { return "I am starting."; } }

        private Starting(Action<string> statusSpeaker) : base(GameMode.Starting, statusSpeaker) { }

        public static StateTransition Start(Action<string> statusSpeaker)
        {
            var newState = new Starting(statusSpeaker);
            var sideEffects = ToList(GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }
    }
}