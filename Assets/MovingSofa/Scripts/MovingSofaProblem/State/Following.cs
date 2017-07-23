using System;

using MovingSofaProblem.Path;

using CameraLocation = UnityEngine.Transform;
using Measure = UnityEngine.GameObject;
using Vector = UnityEngine.Vector3;

namespace MovingSofaProblem.State
{
    public sealed class Following : GameState
    {
        static readonly Vector CarryPositionRelativeToOneUnitInFrontOfCamera = new Vector(0.0f, -0.2f, 0.0f);

        private string whatYouCanSayNow = "Say 'Put it down' when you have arrived at the place where you want to move the object.";
        override public string SayableStateDescription { get { return "I'm following you. Go to where you want to move the object. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I am following you. You can " + whatYouCanSayNow; } }

        private Following(GameState priorState, float cameraY) : base(GameMode.Following, priorState)
        {
            this.InitialPath = new PathHolder();
            this.InitialPath.Add(priorState.Measure.transform.position
                                 , priorState.Measure.transform.rotation
                                 , cameraY);
            this.PathToReplay = new PathHolder();
        }

        public static StateTransition StartFollowing(GameState currentState
                                                    , CameraLocation cameraTransform
                                                    , Action boundingBoxDisabler
                                                    , Action spatialMappingObserverStarter
                                                    , Func<Vector, Func<Measure, CameraLocation, PositionAndRotation>> carryMeasure)
        {
            var newState = new Following(currentState, cameraTransform.position.y);

            Func<GameState, GameState> disableBoundingBox =
                state => { boundingBoxDisabler(); return state; };
            Func<GameState, GameState> startSpatialMappingObserver =
                state => { spatialMappingObserverStarter(); return state; };
            Func<GameState, GameState> keepMeasureInFrontOfMe =
                state =>
                {
                    var newPositionAndRotation = 
                        carryMeasure(CarryPositionRelativeToOneUnitInFrontOfCamera)(currentState.Measure, cameraTransform);
                    state.InitialPath.Add(newPositionAndRotation.Position
                                         , newPositionAndRotation.Rotation
                                         , cameraTransform.position.y);
                    return state;
                };

            var sideEffects = ToList(disableBoundingBox, startSpatialMappingObserver, keepMeasureInFrontOfMe, GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }

        public static StateTransition KeepFollowing(GameState currentState
                                                   , CameraLocation cameraTransform
                                                   , Func<Vector, Func<Measure, CameraLocation, PositionAndRotation>> carryMeasure)
        {
            Func<GameState, GameState> keepMeasureInFrontOfMe = state =>
            {
                var newPositionAndRotation = 
                    carryMeasure(CarryPositionRelativeToOneUnitInFrontOfCamera)(currentState.Measure, cameraTransform);
                state.InitialPath.Add(newPositionAndRotation.Position
                                     , newPositionAndRotation.Rotation
                                     , cameraTransform.position.y);
                return state;
            };

            var sideEffects = ToList(keepMeasureInFrontOfMe);
            return new StateTransition(currentState, sideEffects);
        }
    }
}
