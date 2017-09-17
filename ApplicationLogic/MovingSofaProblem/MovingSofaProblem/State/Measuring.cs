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
    public sealed class Measuring : GameState
    {
        private string whatYouCanSayNow = "Say 'Come with me' when you have finished and are ready to indicate where you want to move the object.";
        override public string SayableStateDescription { get { return "You can measure an object now. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I can measure an object that you select. You can " + whatYouCanSayNow; } }

        private Measuring(GameState priorState, PositionAndRotation newMeasureLocation) : 
            base(GameMode.Measuring, priorState)
        {
            this.MeasureLocation = newMeasureLocation;
        }

        public static StateTransition StartMeasuring(GameState currentState
                                                    , Situation cameraSituation
                                                    , Action<PositionAndRotation> measureCreator)
        {
            Func<GameState, GameState> createNewMeasure = state =>
            {
                var newPositionAndRotation = 
                    SpatialCalculations.ReorientRelativeToOneUnitForwardFrom(
                        cameraSituation, 
                        currentState.CarryPositionRelativeToCamera
                    );
                measureCreator(state.MeasureLocation);
                return new Measuring(state, newPositionAndRotation);
            };

            var sideEffects = ToList(createNewMeasure, SayState);
            return new StateTransition(currentState, sideEffects);
        }
    }
}