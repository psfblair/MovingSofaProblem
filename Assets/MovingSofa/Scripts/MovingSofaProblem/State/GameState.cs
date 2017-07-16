using System;
using System.Collections.Generic;

using Functional.Option;
using MovingSofaProblem.Path;

using Measure = UnityEngine.GameObject;

namespace MovingSofaProblem.State
{
    public abstract class GameState
    {
        private readonly Action<string> statusSpeaker;

        // For initialization from zero
        protected GameState(GameMode gameMode, 
                            Action<string> statusSpeaker)
        {
            this.Mode = gameMode;

            this.InitialPath = new PathHolder();
            this.PathToReplay = new PathHolder();
            this.CurrentPathStep = Option.None;
            this.SegmentReplayStartTime = 0;
            this.Measure = new Measure();
            this.statusSpeaker = statusSpeaker;
        }

        // Initialization based on a prior state, then modified by subclass constructor if necessary.
        protected GameState(GameMode gameMode, GameState priorState)
        {
            this.Mode = gameMode;

            this.InitialPath = priorState.InitialPath;
            this.PathToReplay = priorState.PathToReplay;
            this.CurrentPathStep = priorState.CurrentPathStep;
            this.SegmentReplayStartTime = priorState.SegmentReplayStartTime;
            this.Measure = priorState.Measure;
            this.statusSpeaker = priorState.statusSpeaker;
        }

        public GameMode   Mode                  { get; protected set; }
        public PathHolder InitialPath           { get; protected set; }
        public Option<PathStep> CurrentPathStep { get; protected set; }
        public float SegmentReplayStartTime     { get; protected set; }
        public Measure Measure               { get; protected set; }


        public abstract string SayableStateDescription { get;  }
        public abstract string SayableStatus { get; }

        protected PathHolder PathToReplay { get; set; }
        protected static Option<PathStep> FirstStep(GameState state)
        {
            return PathHolder.FirstStep(state.PathToReplay);
        }

        static public Func<GameState, GameState> SayState = state =>
        {
            state.statusSpeaker(state.SayableStateDescription);
            return state;
        };

        // Currying in C#!
        static public Func<string, Func<GameState, GameState>> Say = 
            somethingToSay => state => { state.statusSpeaker(somethingToSay); return state; };

        public static List<Func<GameState, GameState>> SayStatus(GameState currentState)
        {
            var status = "Right now " + currentState.SayableStatus;
            return ToList(GameState.Say(status));
        }

        protected static List<Func<GameState, GameState>> ToList(params Func<GameState, GameState>[] sideEffectFunctions)
        {
            return new List<Func<GameState, GameState>>(sideEffectFunctions);
        }
    }
}
