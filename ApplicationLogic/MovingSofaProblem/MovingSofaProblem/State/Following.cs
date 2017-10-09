using System;

using MovingSofaProblem.Path;

#if UNIT_TESTS
using Situation = Domain.Situation;
using Vector = Domain.Vector;
#else
using Situation = UnityEngine.Transform;
using Vector = UnityEngine.Vector3;
#endif

namespace MovingSofaProblem.State
{
    public sealed class Following : GameState
    {
        private string whatYouCanSayNow = "Say 'Put it down' when you have arrived at the place where you want to move the object.";
        override public string SayableStateDescription { get { return "I'm with you. Go to where you want to move the object. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I am following you. You can " + whatYouCanSayNow; } }

        private Following(GameState priorState
                          , PositionAndRotation measurePositionAndRotation
                          , Situation cameraSituation) : base(GameMode.Following, priorState)
        {
            this.MeasureLocation = measurePositionAndRotation;
            this.InitialPath = new PathHolder();
            this.InitialPath.Add(measurePositionAndRotation, cameraSituation.position.y);
            this.PathToReplay = new PathHolder();
        }

        public static StateTransition StartFollowing(GameState currentState
                                                    , PositionAndRotation currentMeasurePosition
                                                    , Situation cameraSituation
                                                    , Action boundingBoxDisabler
                                                    , Action spatialMappingObserverStarter
                                                    , Action<PositionAndRotation> moveMeasure)
        {
            var newState = new Following(currentState, currentMeasurePosition, cameraSituation);

            Func<GameState, GameState> disableBoundingBox =
                state => { boundingBoxDisabler(); return state; };
            Func<GameState, GameState> startSpatialMappingObserver =
                state => { spatialMappingObserverStarter(); return state; };
            Func<GameState, GameState> keepMeasureInFrontOfMe = 
                KeepMeasureInFrontOfMe(cameraSituation, moveMeasure);

            var sideEffects = ToList(disableBoundingBox, startSpatialMappingObserver, keepMeasureInFrontOfMe, GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }

        // We position the measure relative to the camera as we move.
        public static StateTransition KeepFollowing(GameState currentState
                                                   , Situation cameraLocation
												   , Action<PositionAndRotation> moveMeasure)
		{
            var sideEffects = ToList(KeepMeasureInFrontOfMe(cameraLocation, moveMeasure));
            return new StateTransition(currentState, sideEffects);
        }

		private static Func<GameState, GameState> KeepMeasureInFrontOfMe(Situation cameraLocation
												                        , Action<PositionAndRotation> moveMeasure)
        {
			return state =>
    			{
    				var newPositionAndRotation =
    					SpatialCalculations.ReorientRelativeToOneUnitForwardFrom(
                            cameraLocation, 
                            state.CarryPositionRelativeToCamera
                        );
    				state.MeasureLocation = newPositionAndRotation;
    				state.InitialPath.Add(newPositionAndRotation, cameraLocation.position.y);
    				moveMeasure(newPositionAndRotation);
    				return state;
    			};
		}
    }
}
