using System;

using MovingSofaProblem.Path;

namespace MovingSofaProblem.State
{
    public sealed class WaitingToReplay : GameState
    {
        private string whatYouCanSayNow = "Say 'Next' to replay the next step.";
        override public string SayableStateDescription { get { return "Replaying the solution. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I am ready to replay the solution. You can " + whatYouCanSayNow; } }

        private WaitingToReplay(GameState priorState, PathStep firstStep) : base(GameMode.WaitingToReplay, priorState)
        {
            this.CurrentPathStep = firstStep;
        }

        public static StateTransition StartReplaying(GameState currentState)
        {
            PathStep firstStep;
            if (! GameState.FirstStep(currentState).TryGetValue(out firstStep))
            {
                var errorSideEffects = ToList(GameState.Say("I can't replay the solution because I have no solution to replay."));
                return new StateTransition(currentState, errorSideEffects);
            }

            var newState = new WaitingToReplay(currentState, firstStep);

            Func<GameState, GameState> placeMeasureAtStart = state =>
            {
                state.Measure.transform.position = firstStep.StartNode.Value.Position;
                state.Measure.transform.rotation = firstStep.StartNode.Value.Rotation;
                return state;
            };

            var sideEffects = ToList(placeMeasureAtStart, GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }
    }
}