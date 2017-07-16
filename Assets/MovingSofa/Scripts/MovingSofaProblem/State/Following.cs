using System;

using MovingSofaProblem.Path;

using CameraLocation = UnityEngine.Transform;
using Measure = UnityEngine.GameObject;

namespace MovingSofaProblem.State
{
    public sealed class Following : GameState
    {
        private string whatYouCanSayNow = "Say 'Put it down' when you have arrived at the place where you want to move the object.";
        override public string SayableStateDescription { get { return "I'm following you. Go to where you want to move the object. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I am following you. You can " + whatYouCanSayNow; } }

        private Following(GameState priorState) : base(GameMode.Following, priorState)
        {
            this.InitialPath = new PathHolder();
            this.InitialPath.Add(priorState.Measure.transform.position, priorState.Measure.transform.rotation);
            this.PathToReplay = new PathHolder();
        }

        public static StateTransition StartFollowing(GameState currentState
                                                    , CameraLocation cameraTransform
                                                    , Action boundingBoxDisabler
                                                    , Action spatialMappingObserverStarter
                                                    , Func<Measure, CameraLocation, PositionAndRotation> carryMeasure)
        {
            var newState = new Following(currentState);

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

            var sideEffects = ToList(disableBoundingBox, startSpatialMappingObserver, keepMeasureInFrontOfMe, GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }

        public static StateTransition KeepFollowing(GameState currentState
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
    }
}
