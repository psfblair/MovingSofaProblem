using System;

using MovingSofaProblem.Path;

using Measure = UnityEngine.GameObject;
using Vector = UnityEngine.Vector3;
using CameraLocation = UnityEngine.Transform;

namespace MovingSofaProblem.State
{
    public sealed class Measuring : GameState
    {
        static readonly Vector InitialMeasurePositionRelativeToOneUnitInFrontOfCamera = new Vector(0.0f, -0.2f, 0.0f);

        private string whatYouCanSayNow = "Say 'Come with me' when you have finished and are ready to indicate where you want to move the object.";
        override public string SayableStateDescription { get { return "You can measure an object now. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I can measure an object that you select. You can " + whatYouCanSayNow; } }

        private Measuring(GameState priorState, Measure newMeasure) : base(GameMode.Measuring, priorState)
        {
            this.Measure = newMeasure;
        }

        public static StateTransition StartMeasuring(GameState currentState
                                                    , CameraLocation cameraTransform
                                                    , Func<Measure, PositionAndRotation, Measure> measureCreator)
        {
            Func<GameState, GameState> createNewMeasure = state =>
            {
                var newPositionAndRotation = SpatialCalculations.OrientationRelativeToOneUnitInFrontOf(cameraTransform, InitialMeasurePositionRelativeToOneUnitInFrontOfCamera);
                var newMeasure = measureCreator(state.Measure, newPositionAndRotation);
                return new Measuring(state, newMeasure);
            };

            var sideEffects = ToList(createNewMeasure, GameState.SayState);
            return new StateTransition(currentState, sideEffects);
        }
    }
}