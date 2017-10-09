using System;

using MovingSofaProblem.Path;

namespace MovingSofaProblem.State
{
    public sealed class WaitingToReplay : GameState
    {
        private string whatYouCanSayNow = "Say 'Next step' to replay the next step.";
        override public string SayableStateDescription { get { return "Replaying the solution. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I am ready to replay the solution. You can " + whatYouCanSayNow; } }

        private WaitingToReplay(GameState priorState, PathStep firstStep) : base(GameMode.WaitingToReplay, priorState)
        {
            this.CurrentPathStep = firstStep;
			this.MeasureLocation = new PositionAndRotation(
				firstStep.StartNode.Value.Position,
				firstStep.StartNode.Value.Rotation
			);
		}

        public static StateTransition StartReplaying(GameState currentState
                                                    , Action<PositionAndRotation> measureMover)
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
                measureMover(state.MeasureLocation);
                return state;
            };

            var sideEffects = ToList(placeMeasureAtStart, GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }
    }
}