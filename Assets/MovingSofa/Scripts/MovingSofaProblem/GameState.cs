using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HoloToolkit.Unity;
using HUX.Interaction;

namespace MovingSofaProblem
{
    public enum GameMode
    { Starting
    , Measuring
    , Following
    , StoppedFollowing
    , RouteInitialized
    , SolutionFound
    , WaitingToReplay
    , Replaying
    , FinishedReplaying
    };

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
            this.CurrentPathSegment = 0;
            this.Measure = new GameObject();
            this.statusSpeaker = statusSpeaker;
        }

        // Initialization based on a prior state, then modified by subclass constructor if necessary.
        protected GameState(GameMode gameMode, GameState priorState)
        {
            this.Mode = gameMode;

            this.InitialPath = priorState.InitialPath;
            this.PathToReplay = priorState.PathToReplay;
            this.CurrentPathSegment = priorState.CurrentPathSegment;
            this.Measure = priorState.Measure;
            this.statusSpeaker = priorState.statusSpeaker;
        }

        public GameMode   Mode         { get; protected set; }
        public PathHolder InitialPath  { get; protected set; }
        public PathHolder PathToReplay { get; protected set; }
        public int CurrentPathSegment  { get; protected set; }
        public GameObject Measure      { get; protected set; }

        public abstract string SayableStateDescription { get;  }
        public abstract string SayableStatus { get; }

        public Func<GameState, GameState> SpeakState = state =>
        {
            state.statusSpeaker(state.SayableStateDescription);
            return state;
        };

        // Currying in C#!
        public Func<string, Func<GameState, GameState>> Say = 
            somethingToSay => state => { state.statusSpeaker(somethingToSay); return state; };
    }

    public sealed class Starting : GameState
    {
        override public string SayableStateDescription { get { return "Starting."; } }
        override public string SayableStatus { get { return "I am starting."; } }

        public Starting(Action<string> statusSpeaker) : base(GameMode.Starting, statusSpeaker) { }
    }


    public sealed class Measuring : GameState
    {
        override public string SayableStateDescription { get { return "Measuring object."; } }
        override public string SayableStatus { get { return "I am measuring."; } }

        public Measuring(GameState priorState): base(GameMode.Measuring, priorState) {}

        public Measuring(GameState priorState, GameObject newMeasure) : base(GameMode.Measuring, priorState)
        {
            this.Measure = newMeasure;
        }
    }

    public sealed class Following : GameState
    {
        override public string SayableStateDescription { get { return "I'm following you."; } }
        override public string SayableStatus { get { return "I am following you."; } }

        public Following(GameState priorState) : base(GameMode.Following, priorState)
        {
            this.InitialPath = new PathHolder();
            this.PathToReplay = new PathHolder();
        }
    }

    public sealed class StoppedFollowing : GameState
    {
        override public string SayableStateDescription { get { return "Simplifying the route."; } }
        override public string SayableStatus { get { return "I have stopped following you and am simplifying the route."; } }

        public StoppedFollowing(GameState priorState) : base(GameMode.StoppedFollowing, priorState)
        {
        }
    }

    public sealed class RouteInitialized : GameState
    {
        override public string SayableStateDescription { get { return "Figuring out a solution."; } }
        override public string SayableStatus { get { return "I am figuring out a solution."; } }

        public RouteInitialized(GameState priorState) : base(GameMode.RouteInitialized, priorState)
        {
            this.PathToReplay = this.InitialPath;
        }
    }

    public sealed class SolutionFound : GameState
    {
        override public string SayableStateDescription { get { return "Finished figuring out a solution.  Say 'replay solution' to see it."; } }
        override public string SayableStatus { get { return "I have figured out a solution. Say 'replay solution' to see it."; } }

        public SolutionFound(GameState priorState) : base(GameMode.SolutionFound, priorState) {}
    }

    public sealed class WaitingToReplay : GameState
    {
        override public string SayableStateDescription { get { return "Replaying the solution. Say 'next' to replay the next step."; } }
        override public string SayableStatus { get { return "I am starting to replay the solution. Say 'next' to replay the next step."; } }

        public WaitingToReplay(GameState priorState) : base(GameMode.WaitingToReplay, priorState)
        {
            this.CurrentPathSegment = 0;
        }
    }

    public sealed class Replaying : GameState
    {
        override public string SayableStateDescription { get { return "Replaying the solution. Say 'next' to replay the next step, 'again' to replay the current step, or 'replay solution' to start over from the beginning."; } }
        override public string SayableStatus { get { return "I am in the middle of replaying the solution. Say 'next' to replay the next step, 'again' to replay the current step, or 'replay solution' to start over from the beginning."; } }

        public Replaying(GameState priorState) : base(GameMode.Replaying, priorState)
        {
        }
    }

    public sealed class FinishedReplaying : GameState
    {
        override public string SayableStateDescription { get { return "Finished replaying the solution. Say 'replay solution' to replay again from the beginning."; } }
        override public string SayableStatus { get { return "I have finished replaying the solution. Say 'replay solution' to replay again from the beginning."; } }

        public FinishedReplaying(GameState priorState) : base(GameMode.FinishedReplaying, priorState)
        {
        }
    }

}
