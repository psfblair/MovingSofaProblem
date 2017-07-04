using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using HUX.Interaction;

namespace MovingSofaProblem
{
    public class StateTransition
    {
        public StateTransition(GameState newState, List<Func<GameState, GameState>> sideEffects)
        {
            this.NewState = newState;
            this.SideEffects = sideEffects;
        }

        public GameState NewState { get; private set; }
        public List<Func<GameState, GameState>> SideEffects { get; private set; }
    }

    public static class StateTransitions
    {
        static readonly Vector3 InitialMeasurePositionRelativeToOneUnitInFrontOfCamera = new Vector3(0.0f, -0.2f, 0.0f);


        public static StateTransition Start( Action<string> statusSpeaker)
        {
            var newState = new Starting(statusSpeaker);
            var sideEffects = new List<Func<GameState, GameState>>();
            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition StartMeasuring( GameState currentState
                                                    , Transform cameraTransform
                                                    , Func<GameObject, PositionAndRotation, GameObject> measureCreator)
        {
            var newState = new Measuring(currentState);
            
            Func<GameState, GameState> createNewMeasure = state =>
            {
                var newPositionAndRotation = SpatialCalculations.OrientationRelativeToOneUnitInFrontOf(cameraTransform, InitialMeasurePositionRelativeToOneUnitInFrontOfCamera);
                var newMeasure = measureCreator(state.Measure, newPositionAndRotation);
                return new Measuring(state, newMeasure);
            };
            
            var sideEffects = ToList(createNewMeasure, newState.SpeakState);
            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition StartFollowing( GameState currentState
                                                    , Transform cameraTransform
                                                    , Action boundingBoxDisabler
                                                    , Action spatialMappingObserverStarter
                                                    , Func<GameObject, Transform, PositionAndRotation> moveMeasure)
        {
            var newState = new Following(currentState);

            Func<GameState, GameState> disableBoundingBox          = state => { boundingBoxDisabler();           return state; };
            Func<GameState, GameState> startSpatialMappingObserver = state => { spatialMappingObserverStarter(); return state; };
            Func<GameState, GameState> keepMeasureInFrontOfMe = state =>
            {
                var newPositionAndRotation = moveMeasure(currentState.Measure, cameraTransform);
                state.InitialPath.Add(newPositionAndRotation.Position, newPositionAndRotation.Rotation);
                return state;
            };

            var sideEffects = ToList(disableBoundingBox, startSpatialMappingObserver, keepMeasureInFrontOfMe, newState.SpeakState);
            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition KeepFollowing( GameState currentState
                                                   , Transform cameraTransform
                                                   , Func<GameObject, Transform, PositionAndRotation> moveMeasure)
        {
            Func<GameState, GameState> keepMeasureInFrontOfMe = state =>
            {
                var newPositionAndRotation = moveMeasure(currentState.Measure, cameraTransform);
                state.InitialPath.Add(newPositionAndRotation.Position, newPositionAndRotation.Rotation);
                return state;
            };

            var sideEffects = ToList(keepMeasureInFrontOfMe);
            return new StateTransition(currentState, sideEffects);
        }


        public static StateTransition StopFollowing( GameState currentState
                                                   , Action spatialMappingObserverStopper
                                                   , Action planeCreator)
        {
            if (currentState.Mode != GameMode.Following)
            {
                var errorSideEffects = ToList(currentState.Say("I can't stop following you because I'm not following you."));
                return new StateTransition(currentState, errorSideEffects);
            }

            var newState = new StoppedFollowing(currentState);
            // TODO Show spinner as side effect
            Func<GameState, GameState> stopSpatialMappingObserver = state => { spatialMappingObserverStopper(); return state; };
            Func<GameState, GameState> createPlanes = state => { planeCreator(); return state; };

            var sideEffects = ToList(newState.SpeakState, stopSpatialMappingObserver, createPlanes);
            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition RouteInitialized( GameState currentState
                                                      , Action wallVertexRemover
                                                      , Action wallSurfaceCreator)
        {
            if (currentState.Mode != GameMode.StoppedFollowing)
            {
                // Were already doing something else, so ignore the call to RouteInitialized.
                return new StateTransition(currentState, new List<Func<GameState, GameState>>());
            }

            var newState = new RouteInitialized(currentState);

            // TODO Stop spinner as side effect
            Func<GameState, GameState> removeWallVertices = state => { wallVertexRemover(); return state; };
            Func<GameState, GameState> createWallSurfaces = state => { wallSurfaceCreator(); return state; };

            // TODO Do AI/search here
            // Redo the initial path with colliders against the planes, find all the places where it touches
            // Then try to solve those. Also can't be raised above the person's head too far.
            // TODO Save path to text asset

            Func<GameState, GameState> transitionToSolutionFound = state =>
            {
                var solutionFoundState = new SolutionFound(state);
                solutionFoundState.SpeakState(solutionFoundState);
                return solutionFoundState;
            };

            var sideEffects = ToList(newState.SpeakState, removeWallVertices, createWallSurfaces, transitionToSolutionFound);
            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition StartReplaying(GameState currentState)
        {
            if(currentState.PathToReplay.Count < 2)
            {
                var errorSideEffects = ToList(currentState.Say("I can't replay the solution because I have no solution to replay."));
                return new StateTransition(currentState, errorSideEffects);
            }
            var newState = new WaitingToReplay(currentState);
            var sideEffects = ToList(newState.SpeakState);
            return new StateTransition(newState, sideEffects);
        }


        public static List<Func<GameState, GameState>> SayStatus(GameState currentState, string intro)
        {
            var status = intro + " Right now " + currentState.SayableStatus;
            return ToList(currentState.Say(status));
        }

        /*
            private void PlayNextSegment()
            {
                if (pathToReplay.MoveNext())
                {
                    startOfCurrentSegment = endOfCurrentSegment;
                    endOfCurrentSegment = pathToReplay.Current;
                    currentGameMode = GameMode.Replaying;
                }
                else
                {
                    textToSpeechManager.SpeakText("You're at the end of the path.");
                    currentGameMode = GameMode.FinishedReplaying;
                }
            }

            private void ReplayCurrentSegment()
            {
                // TODO: Reset current position back to startOfCurrentSegment
                currentGameMode = GameMode.Replaying;
            }
        */


        private static List<Func<GameState, GameState>> ToList(params Func<GameState, GameState>[] sideEffectFunctions)
        {
            return new List<Func<GameState, GameState>>(sideEffectFunctions);
        }
    }
}
