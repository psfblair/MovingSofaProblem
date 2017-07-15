using System;
using System.Collections.Generic;
using Measure = UnityEngine.GameObject;
using Vector = UnityEngine.Vector3;
using CameraLocation = UnityEngine.Transform;

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
        static readonly Vector InitialMeasurePositionRelativeToOneUnitInFrontOfCamera = new Vector(0.0f, -0.2f, 0.0f);


        public static StateTransition Start( Action<string> statusSpeaker)
        {
            var newState = new Starting(statusSpeaker);
            var sideEffects = new List<Func<GameState, GameState>>();
            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition StartMeasuring( GameState currentState
                                                    , CameraLocation cameraTransform
                                                    , Func<Measure, PositionAndRotation, Measure> measureCreator)
        {
            Func<GameState, GameState> createNewMeasure = state =>
            {
                var newPositionAndRotation = SpatialCalculations.OrientationRelativeToOneUnitInFrontOf(cameraTransform, InitialMeasurePositionRelativeToOneUnitInFrontOfCamera);
                var newMeasure = measureCreator(state.Measure, newPositionAndRotation);
                return Measuring.StartMeasuring(state, newMeasure);
            };
            
            var sideEffects = ToList(createNewMeasure, GameState.SpeakState);
            return new StateTransition(currentState, sideEffects);
        }


        public static StateTransition StartFollowing( GameState currentState
                                                    , CameraLocation cameraTransform
                                                    , Action boundingBoxDisabler
                                                    , Action spatialMappingObserverStarter
                                                    , Func<Measure, CameraLocation, PositionAndRotation> carryMeasure)
        {
            var newState = Following.StartFollowing(currentState);

            Func<GameState, GameState> disableBoundingBox = 
                state => { boundingBoxDisabler(); return state; };
            Func<GameState, GameState> startSpatialMappingObserver = 
                state => { spatialMappingObserverStarter(); return state; };
            Func<GameState, GameState> keepMeasureInFrontOfMe = 
                state =>
                    {
                        var newPositionAndRotation = carryMeasure(currentState.Measure, cameraTransform);
                        state.InitialPath.Add(newPositionAndRotation.Position, newPositionAndRotation.Rotation);
                        return state;
                    };

            var sideEffects = ToList(disableBoundingBox, startSpatialMappingObserver, keepMeasureInFrontOfMe, GameState.SpeakState);
            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition KeepFollowing( GameState currentState
                                                   , CameraLocation cameraTransform
                                                   , Func<Measure, CameraLocation, PositionAndRotation> carryMeasure)
        {
            Func<GameState, GameState> keepMeasureInFrontOfMe = state =>
            {
                var newPositionAndRotation = carryMeasure(currentState.Measure, cameraTransform);
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
            var newState = StoppedFollowing.HasStoppedFollowing(currentState);
            var sideEffects = ToList();

            if (newState.Mode == GameMode.Following)
            {
                // TODO Show spinner as side effect
                Func<GameState, GameState> stopSpatialMappingObserver = 
                    state => { spatialMappingObserverStopper(); return state; };
                Func<GameState, GameState> simplifyPath = 
                    state => { return PathSimplified.HasSimplifiedPath(currentState); };
                Func<GameState, GameState> createPlanes = 
                    state => { planeCreator(); return state; };

                sideEffects = ToList(GameState.SpeakState, stopSpatialMappingObserver, createPlanes);
            }
            else
            {
                sideEffects = ToList(GameState.Say("I can't stop following you because I'm not following you."));
            }

            return new StateTransition(newState, sideEffects);
        }


        public static StateTransition InitializeRoute( GameState currentState
                                                      , Action wallVertexRemover
                                                      , Action wallSurfaceCreator)
        {
            var newState = RouteInitialized.HasInitializedRoute(currentState);
            var sideEffects = ToList();

            if (newState.Mode == GameMode.StoppedFollowing)
            {
                // TODO Stop spinner as side effect
                Func<GameState, GameState> removeWallVertices = state => { wallVertexRemover(); return state; };
                Func<GameState, GameState> createWallSurfaces = state => { wallSurfaceCreator(); return state; };

                // TODO Do AI/search here
                // Redo the initial path with colliders against the planes, find all the places where it touches
                // Then try to solve those. Also can't be raised above the person's head too far.
                // TODO Save path to text asset

                Func<GameState, GameState> transitionToSolutionFound = state =>
                {
                    var solutionFoundState = SolutionFound.HasFoundSolution(state);
                    GameState.SpeakState(solutionFoundState);
                    return solutionFoundState;
                };

                sideEffects = ToList(GameState.SpeakState
                                    , removeWallVertices
                                    , createWallSurfaces
                                    , transitionToSolutionFound);
            }

            return new StateTransition(newState, sideEffects);
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
                    if(state.CurrentPathStep.TryGetValue(out firstStep))
                    {
                        state.Measure.transform.position = firstStep.StartNode.Value.Position;
                        state.Measure.transform.rotation = firstStep.StartNode.Value.Rotation;
                    }
                    return state;
                };

                sideEffects = ToList(positionMeasureAtStart, GameState.SpeakState);
            }
            else
            {
                sideEffects = ToList(GameState.Say("I can't replay the solution because I have no solution to replay."));
            }

            return new StateTransition(newState, sideEffects);
        }

        public static StateTransition PlayNextSegment(GameState currentState, float replayStartTime)
        {
            var newState = Replaying.IsPlayingNextSegment(currentState, replayStartTime);
            var sideEffects = ToList();

            switch(newState.Mode)
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
            var newState = Replaying.IsPlayingNextSegment(currentState, replayStartTime);
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
                                                   , float translationSpeed
                                                   , float rotationSpeed
                                                   , Func<Measure, PositionAndRotation, PositionAndRotation> moveMeasure)
        {
            var sideEffects = ToList();

            switch (currentState.Mode)
            {
                case GameMode.Replaying:
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
                                        , translationSpeed
                                        , rotationSpeed
                                        , currentPathStep.PathSegment);

                                PositionAndRotation newPositionAndRotation;
                                if (maybeNewPosition.TryGetValue(out newPositionAndRotation))
                                {
                                    moveMeasure(state.Measure, newPositionAndRotation);
                                }
                            }
                            return state;
                        };

                    sideEffects = ToList(repositionMeasure);
                    break;
            }

            return new StateTransition(currentState, sideEffects);
        }


        public static List<Func<GameState, GameState>> SayStatus(GameState currentState, string intro)
        {
            var status = intro + " Right now " + currentState.SayableStatus;
            return ToList(GameState.Say(status));
        }

        private static List<Func<GameState, GameState>> ToList(params Func<GameState, GameState>[] sideEffectFunctions)
        {
            return new List<Func<GameState, GameState>>(sideEffectFunctions);
        }
    }
}
