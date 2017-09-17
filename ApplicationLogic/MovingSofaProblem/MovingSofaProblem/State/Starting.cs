using System;
using System.Collections.Generic;

#if UNIT_TESTS
using Situation = Domain.Situation;
using Vector = Domain.Vector;
#else
using Situation = UnityEngine.Transform;
using Vector = UnityEngine.Vector3;
#endif

// replayingTranslationSpeed = 0.7f; // units/sec
// replayingRotationSpeed = 20.0f; // degrees/sec

namespace MovingSofaProblem.State
{
    public sealed class Starting : GameState
    {
        override public string SayableStateDescription { get { return "Starting."; } }
        override public string SayableStatus { get { return "I am starting."; } }

		private Starting(Situation cameraSituation
                        , Vector carryPositionRelativeToCamera
                        , float replayingTranslationSpeed
						, float replayingRotationSpeed
						, Action<string> statusSpeaker
                        )  : base(GameMode.Starting
                                  , cameraSituation
                                  , carryPositionRelativeToCamera
            					  , replayingTranslationSpeed
            					  , replayingRotationSpeed
								  , statusSpeaker) { }

		public static StateTransition Start(Situation cameraSituation
										   , Vector carryPositionRelativeToCamera
                    					   , float replayingTranslationSpeedUnitsPerSec
                    					   , float replayingRotationSpeedDegreesPerSec
                                           , Action<string> statusSpeaker
                                           )
        {
            var newState = new Starting(
                cameraSituation
				, carryPositionRelativeToCamera
				, replayingTranslationSpeedUnitsPerSec
			    , replayingRotationSpeedDegreesPerSec
                , statusSpeaker
            );
            var sideEffects = ToList(GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }
    }
}