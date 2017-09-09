[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MovingSofaProblemTests")]
namespace MovingSofaProblem.State
{
    public sealed class FinishedReplaying : GameState
    {
        private string whatYouCanSayNow = "Say 'Replay solution' to replay again from the beginning.";
        override public string SayableStateDescription { get { return "Finished replaying my solution. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I have finished replaying my solution. You can " + whatYouCanSayNow; } }

		private FinishedReplaying(GameState priorState) : base(GameMode.FinishedReplaying, priorState)
        {
        }

		// This is a degenerate transition method in that it doesn't return a state transition; 
		// only Replaying calls it.We are permissive in what state we take because we don't do
        // anything here, so we are not constrained or worried about getting into a bad state.
		internal static FinishedReplaying IsFinishedReplaying(GameState priorState)
        {
            return new FinishedReplaying(priorState);
        }
    }
}
