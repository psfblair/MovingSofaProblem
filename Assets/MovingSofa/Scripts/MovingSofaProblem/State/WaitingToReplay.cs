using System;

using MovingSofaProblem.Path;

namespace MovingSofaProblem.State
{
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

        public static StateTransition StartReplaying(GameState currentState)
        {
            var newState = WaitingToReplay.IsWaitingToReplay(currentState);
            var sideEffects = ToList();

            if (newState.Mode == GameMode.WaitingToReplay)
            {
                Func<GameState, GameState> positionMeasureAtStart = state =>
                {
                    PathStep firstStep;
                    if (state.CurrentPathStep.TryGetValue(out firstStep))
                    {
                        state.Measure.transform.position = firstStep.StartNode.Value.Position;
                        state.Measure.transform.rotation = firstStep.StartNode.Value.Rotation;
                    }
                    return state;
                };

                sideEffects = ToList(positionMeasureAtStart, GameState.SayState);
            }
            else
            {
                sideEffects = ToList(GameState.Say("I can't replay the solution because I have no solution to replay."));
            }

            return new StateTransition(newState, sideEffects);
        }
    }
}