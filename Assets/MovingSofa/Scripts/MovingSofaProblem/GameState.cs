using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HoloToolkit.Unity;
using HUX.Interaction;
using Functional.Option;

namespace MovingSofaProblem
{
    public enum GameMode
    { Starting
    , Measuring
    , Following
    , StoppedFollowing
    , PathSimplified
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
            this.CurrentPathStep = Option.None;
            this.SegmentReplayStartTime = 0;
            this.Measure = new GameObject();
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
        public GameObject Measure               { get; protected set; }


        public abstract string SayableStateDescription { get;  }
        public abstract string SayableStatus { get; }

        protected PathHolder PathToReplay { get; set; }
        protected static Option<PathStep> FirstStep(GameState state)
        {
            return PathHolder.FirstStep(state.PathToReplay);
        }

        static public Func<GameState, GameState> SpeakState = state =>
        {
            state.statusSpeaker(state.SayableStateDescription);
            return state;
        };

        // Currying in C#!
        static public Func<string, Func<GameState, GameState>> Say = 
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
        override public string SayableStatus { get { return "I am measuring the object you select."; } }

        private Measuring(GameState priorState, GameObject newMeasure) : base(GameMode.Measuring, priorState)
        {
            this.Measure = newMeasure;
        }

        internal static Measuring StartMeasuring(GameState priorState, GameObject newMeasure)
        {
            return new Measuring(priorState, newMeasure);
        }
    }

    public sealed class Following : GameState
    {
        override public string SayableStateDescription { get { return "I'm following you."; } }
        override public string SayableStatus { get { return "I am following you."; } }

        private Following(GameState priorState) : base(GameMode.Following, priorState)
        {
            this.InitialPath = new PathHolder();
            this.PathToReplay = new PathHolder();
        }

        internal static Following StartFollowing(GameState priorState)
        {
            return new Following(priorState);
        }
    }

    public sealed class StoppedFollowing : GameState
    {
        override public string SayableStateDescription { get { return "Simplifying the route."; } }
        override public string SayableStatus { get { return "I have stopped following you and am simplifying the route."; } }

        private StoppedFollowing(GameState priorState) : base(GameMode.StoppedFollowing, priorState)
        {
        }

        internal static GameState HasStoppedFollowing(GameState priorState)
        {
            if (priorState.Mode == GameMode.Following)
            {
                return new StoppedFollowing(priorState);
            }
            else
            { 
                return priorState;
            }
        }
    }

    public sealed class PathSimplified : GameState
    {
        override public string SayableStateDescription { get { return "Route simplified."; } }
        override public string SayableStatus { get { return "I have simplified the route."; } }

        private PathSimplified(GameState priorState, PathHolder simplifiedPath) : base(GameMode.PathSimplified, priorState)
        {
            this.InitialPath = simplifiedPath;
            this.PathToReplay = simplifiedPath;
        }

        internal static PathSimplified HasSimplifiedPath(GameState priorState)
        {
            var simplifiedPath = PathHolder.Simplify(priorState.InitialPath);
            return new PathSimplified(priorState, simplifiedPath);
        }
    }

    public sealed class RouteInitialized : GameState
    {
        override public string SayableStateDescription { get { return "Figuring out a solution."; } }
        override public string SayableStatus { get { return "I am figuring out a solution."; } }

        private RouteInitialized(GameState priorState) : base(GameMode.RouteInitialized, priorState)
        {
            this.PathToReplay = this.InitialPath;
        }

        internal static GameState HasInitializedRoute(GameState priorState)
        {
            if (priorState.Mode == GameMode.StoppedFollowing)
            {
                // Were already doing something else, so ignore the call to RouteInitialized.
                return new RouteInitialized(priorState);
            }
            else
            {
                return priorState;
            }
        }
    }

    public sealed class SolutionFound : GameState
    {
        override public string SayableStateDescription { get { return "Finished figuring out a solution.  Say 'replay solution' to see it."; } }
        override public string SayableStatus { get { return "I have figured out a solution. Say 'replay solution' to see it."; } }

        private SolutionFound(GameState priorState) : base(GameMode.SolutionFound, priorState) {}

        internal static SolutionFound HasFoundSolution(GameState priorState)
        {
            return new SolutionFound(priorState);
        }
    }

    public sealed class WaitingToReplay : GameState
    {
        override public string SayableStateDescription { get { return "Replaying the solution. Say 'next' to replay the next step."; } }
        override public string SayableStatus { get { return "I am ready to replay the solution. Say 'next' to replay the next step."; } }

        private WaitingToReplay(GameState priorState, PathStep firstStep) : base(GameMode.WaitingToReplay, priorState)
        {
            this.CurrentPathStep = firstStep;

        }

        internal static GameState IsWaitingToReplay(GameState priorState)
        {
            PathStep firstStep;
            if (GameState.FirstStep(priorState).TryGetValue(out firstStep))
            {
                return new WaitingToReplay(priorState, firstStep);
            }
            else
            {
                return priorState;
            }
        }
    }

    public sealed class Replaying : GameState
    {
        override public string SayableStateDescription { get { return "Replaying the solution. Say 'next' to replay the next step, 'again' to replay the current step, or 'replay solution' to start over from the beginning."; } }
        override public string SayableStatus { get { return "I am in the middle of replaying the solution. Say 'next' to replay the next step, 'again' to replay the current step, or 'replay solution' to start over from the beginning."; } }

        private Replaying(GameState priorState, PathStep currentStep, float replayStartTime) : base(GameMode.Replaying, priorState)
        {
            this.CurrentPathStep = currentStep;
            this.SegmentReplayStartTime = replayStartTime;
        }

        internal static GameState IsPlayingNextSegment(GameState priorState, float replayStartTime)
        {
            if (priorState.Mode != GameMode.WaitingToReplay &&
                priorState.Mode != GameMode.Replaying)
            {
                return priorState;
            }
           
            // Most Performant way of using Option type
            PathStep pathStep;
            if (!priorState.CurrentPathStep.TryGetValue(out pathStep))
            {
                return priorState;
            }

            PathStep nextStep;
            if (PathStep.NextStep(pathStep).TryGetValue(out nextStep))
            {
                return new Replaying(priorState, nextStep, replayStartTime);
            }
            else
            {
                return FinishedReplaying.IsFinishedReplaying(priorState);
            }
        }

        static internal GameState IsReplayingCurrentSegment(GameState priorState, float replayStartTime)
        {
            if (priorState.Mode == GameMode.WaitingToReplay ||
                priorState.Mode == GameMode.Replaying ||
                priorState.Mode == GameMode.FinishedReplaying)
            {
                // Most Performant way of using Option type
                PathStep pathStep;
                if (priorState.CurrentPathStep.TryGetValue(out pathStep))
                {
                    return new Replaying(priorState, pathStep, replayStartTime);
                }
            }

            return priorState;
        }
    }

    public sealed class FinishedReplaying : GameState
    {
        override public string SayableStateDescription { get { return "Finished replaying the solution. Say 'replay solution' to replay again from the beginning."; } }
        override public string SayableStatus { get { return "I have finished replaying the solution. Say 'replay solution' to replay again from the beginning."; } }

        private FinishedReplaying(GameState priorState) : base(GameMode.FinishedReplaying, priorState)
        {
        }

        internal static FinishedReplaying IsFinishedReplaying(GameState priorState)
        {
            return new FinishedReplaying(priorState);
        }
    }

}
