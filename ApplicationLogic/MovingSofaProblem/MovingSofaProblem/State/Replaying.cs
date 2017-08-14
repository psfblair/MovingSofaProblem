using System;

using MovingSofaProblem.Path;

#if UNIT_TESTS
using Measure = Domain.Measure;
#else
using Measure = UnityEngine.GameObject;
#endif

namespace MovingSofaProblem.State
{
    public sealed class Replaying : GameState
    {
        private static float replayingTranslationSpeed = 0.7f; // units/sec
        private static float replayingRotationSpeed = 20.0f; // degrees/sec

        private string whatYouCanSayNow = "Say 'Next' to replay the next step, 'Again' to replay the current step, or 'Replay solution' to start over from the beginning.";
        override public string SayableStateDescription { get { return "Replaying the solution. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I am in the middle of replaying the solution. You can " + whatYouCanSayNow; } }

        private Replaying(GameState priorState, PathStep currentStep, float replayStartTime) : base(GameMode.Replaying, priorState)
        {
            this.CurrentPathStep = currentStep;
            this.SegmentReplayStartTime = replayStartTime;
        }

        public static StateTransition PlayNextSegment(GameState currentState, float replayStartTime)
        {
            var newState = Replaying.MaybeReplayingNextSegmentState(currentState, replayStartTime);
            var sideEffects = ToList();

            switch (newState.Mode)
            {
                case GameMode.Replaying:
                    break;
                case GameMode.FinishedReplaying:
                    sideEffects = ToList(GameState.Say("You're at the end of the path."));
                    break;
                default:
                    sideEffects = ToList(GameState.Say("I can't replay the solution because I have no solution to replay."));
                    break;
            }

            return new StateTransition(newState, sideEffects);
        }

        public static StateTransition ReplayCurrentSegment(GameState currentState, float replayStartTime)
        {
            var newState = Replaying.MaybeReplayingCurrentSegmentState(currentState, replayStartTime);
            var sideEffects = ToList();

            switch (newState.Mode)
            {
                case GameMode.Replaying:
                    break;
                default:
                    sideEffects = ToList(GameState.Say("I can't replay the current step because I currently don't have a step to replay."));
                    break;
            }

            return new StateTransition(newState, sideEffects);
        }

        public static StateTransition KeepReplaying(GameState currentState
                                                   , float currentTime
                                                   , Func<Measure, PositionAndRotation, PositionAndRotation> moveMeasure)
        {
            if(currentState.Mode != GameMode.Replaying)
            {
                return new StateTransition(currentState, ToList());
            }

            Func<GameState, GameState> repositionMeasure = state =>
            {
                // Using if statements with out parameters for performance
                PathStep currentPathStep;
                if (state.CurrentPathStep.TryGetValue(out currentPathStep))
                {
                    var maybeNewPosition =
                        SpatialCalculations.MaybeNewInterpolatedPosition(
                            currentTime
                            , state.SegmentReplayStartTime
                            , replayingTranslationSpeed
                            , replayingRotationSpeed
                            , currentPathStep.PathSegment);

                    PositionAndRotation newPositionAndRotation;
                    if (maybeNewPosition.TryGetValue(out newPositionAndRotation))
                    {
                        moveMeasure(state.Measure, newPositionAndRotation);
                    }
                }
                return state;
            };

            var sideEffects = ToList(repositionMeasure);
            return new StateTransition(currentState, sideEffects);
        }

        private static GameState MaybeReplayingNextSegmentState(GameState priorState, float replayStartTime)
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

            if (priorState.Mode == GameMode.WaitingToReplay)
            {
                return new Replaying(priorState, pathStep, replayStartTime);
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

        static private GameState MaybeReplayingCurrentSegmentState(GameState priorState, float replayStartTime)
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
}