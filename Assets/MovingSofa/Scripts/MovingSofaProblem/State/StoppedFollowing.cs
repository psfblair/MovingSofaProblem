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
                state => 
                    {
                        measureReleaser(state.Measure);
                        state.InitialPath.Add(state.Measure.transform.position, state.Measure.transform.rotation);
                        return state;
                    };

            // TODO Show spinner as side effect

            Func<GameState, GameState> stopSpatialMappingObserver =
                state => { spatialMappingObserverStopper(); return state; };
            Func<GameState, GameState> simplifyPath =
                state => { return PathSimplified.HasSimplifiedPath(state); };
            Func<GameState, GameState> findSolution =
                state => { return SolutionFound.HasFoundSolution(state); };

            // TODO Stop spinner as side effect

            var sideEffects = ToList(putDownMeasure, GameState.SayState, stopSpatialMappingObserver, simplifyPath, findSolution, GameState.SayState);

            return new StateTransition(newState, sideEffects);
        }
    }
}

