using System;
using System.Collections.Generic;

#if UNIT_TESTS
using Situation = Domain.Situation;
#else
using Situation = UnityEngine.Transform;
#endif

namespace MovingSofaProblem.State
{
    public sealed class Starting : GameState
    {
        override public string SayableStateDescription { get { return "Starting."; } }
        override public string SayableStatus { get { return "I am starting."; } }

		private Starting(Situation cameraSituation
                        , float carryYRelativeToCamera
                        , Action<string> statusSpeaker
                        )  : base(GameMode.Starting, cameraSituation, carryYRelativeToCamera, statusSpeaker) { }

		public static StateTransition Start(Situation cameraSituation
                    					   , float yCarryPositionRelativeToCamera
                                           , Action<string> statusSpeaker
                                           )
        {
            var newState = new Starting(cameraSituation, yCarryPositionRelativeToCamera, statusSpeaker);
            var sideEffects = ToList(GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }
    }
}