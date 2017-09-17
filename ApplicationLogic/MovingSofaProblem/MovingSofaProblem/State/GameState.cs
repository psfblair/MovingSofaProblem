using System;
using System.Collections.Generic;
using Functional.Option;

#if UNIT_TESTS
using Situation = Domain.Situation;
using Vector = Domain.Vector;
#else
using Situation = UnityEngine.Transform;
using Vector = UnityEngine.Vector3;
#endif

using MovingSofaProblem.Path;

namespace MovingSofaProblem.State
{
    public abstract class GameState
    {
        private readonly Action<string> statusSpeaker;

        // For initialization from zero
        protected GameState(GameMode gameMode
                            , Situation cameraSituation
                            , Vector carryPositionRelativeToCamera
            				, float replayingTranslationSpeedUnitsPerSec
            				, float replayingRotationSpeedDegreesPerSec
							, Action<string> statusSpeaker)
        {
            this.Mode = gameMode;

            this.InitialPath = new PathHolder();
            this.PathToReplay = new PathHolder();
            this.CurrentPathStep = Option.None;
            this.SegmentReplayStartTime = 0;

			this.CarryPositionRelativeToCamera = carryPositionRelativeToCamera;
			this.ReplayingTranslationSpeedUnitsPerSec = replayingTranslationSpeedUnitsPerSec;
			this.ReplayingRotationSpeedDegreesPerSec = replayingRotationSpeedDegreesPerSec;

			this.statusSpeaker = statusSpeaker;

			this.MeasureLocation = SpatialCalculations.ReorientRelativeToOneUnitForwardFrom(
				cameraSituation,
                CarryPositionRelativeToCamera
			);
		}

        // Initialization based on a prior state, then modified by subclass constructor if necessary.
        protected GameState(GameMode gameMode, GameState priorState)
        {
            this.Mode = gameMode;
			this.MeasureLocation = priorState.MeasureLocation;

			this.InitialPath = priorState.InitialPath;
            this.PathToReplay = priorState.PathToReplay;
            this.CurrentPathStep = priorState.CurrentPathStep;
            this.SegmentReplayStartTime = priorState.SegmentReplayStartTime;

            this.CarryPositionRelativeToCamera = priorState.CarryPositionRelativeToCamera;
			this.ReplayingTranslationSpeedUnitsPerSec = priorState.ReplayingTranslationSpeedUnitsPerSec;
			this.ReplayingRotationSpeedDegreesPerSec = priorState.ReplayingRotationSpeedDegreesPerSec;

			this.statusSpeaker = priorState.statusSpeaker;
        }

        public GameMode   Mode                      { get; protected set; }
		public PositionAndRotation MeasureLocation { get; internal set; }

		public PathHolder InitialPath               { get; internal set; }
        public Option<PathStep> CurrentPathStep     { get; protected set; }
        public float SegmentReplayStartTime         { get; protected set; }

		public Vector CarryPositionRelativeToCamera { get; private set; }
		public float ReplayingTranslationSpeedUnitsPerSec { get; private set; }
		public float ReplayingRotationSpeedDegreesPerSec  { get; private set; }

        public abstract string SayableStateDescription { get;  }
        public abstract string SayableStatus { get; }

        internal PathHolder PathToReplay { get; set; }
        internal static Option<PathStep> FirstStep(GameState state)
        {
            return PathHolder.FirstStep(state.PathToReplay);
        }

        static public Func<GameState, GameState> SayState = state =>
        {
            state.statusSpeaker(state.SayableStateDescription);
            return state;
        };

        static public Func<string, Func<GameState, GameState>> Say = 
            somethingToSay => state => { state.statusSpeaker(somethingToSay); return state; };

        public static List<Func<GameState, GameState>> SayStatus(GameState currentState)
        {
            var status = "Right now " + currentState.SayableStatus;
            return ToList(GameState.Say(status));
        }

        protected static List<Func<GameState, GameState>> ToList(params Func<GameState, GameState>[] sideEffectFunctions)
        {
            return new List<Func<GameState, GameState>>(sideEffectFunctions);
        }
    }
}
