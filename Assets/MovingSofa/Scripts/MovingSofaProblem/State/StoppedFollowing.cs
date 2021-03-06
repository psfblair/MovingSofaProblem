using System;

using Measure = UnityEngine.GameObject;

namespace MovingSofaProblem.State
{
    public sealed class StoppedFollowing : GameState
    {
        override public string SayableStateDescription { get { return "Simplifying the route."; } }
        override public string SayableStatus { get { return "I have stopped following you and am simplifying the route."; } }

        private StoppedFollowing(GameState priorState) : base(GameMode.StoppedFollowing, priorState)
        {
            // Make sure we get the last spot before we stop following.
            priorState.InitialPath.Add(priorState.Measure.transform.position, priorState.Measure.transform.rotation);
        }

        public static StateTransition StopFollowing(GameState currentState
                                                   , Action<Measure> measureReleaser
                                                   , Action spatialMappingObserverStopper)
        {
            if (currentState.Mode != GameMode.Following)
            {
                var errorSideEffects = ToList(GameState.Say("I can't put it down because I'm not carrying anything right now."));
                return new StateTransition(currentState, errorSideEffects);
            }

            var newState = new StoppedFollowing(currentState);

            Func<GameState, GameState> putDownMeasure =
                state => { measureReleaser(state.Measure); return state; };

            // TODO Show spinner as side effect

            Func<GameState, GameState> stopSpatialMappingObserver =
                state => { spatialMappingObserverStopper(); return state; };

            // We get to the PathSimplified state from an event triggered by BreakFall indicating the measure
            // has reached its final resting point.
            var sideEffects = ToList(putDownMeasure, GameState.SayState, stopSpatialMappingObserver);

            return new StateTransition(newState, sideEffects);
        }
    }
}

