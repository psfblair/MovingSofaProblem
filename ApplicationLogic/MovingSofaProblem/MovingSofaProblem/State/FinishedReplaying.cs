[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MovingSofaProblemTests")]
namespace MovingSofaProblem.State
{
    public sealed class FinishedReplaying : GameState
    {
        private string whatYouCanSayNow = "Say 'Replay solution' to replay again from the beginning..";
        override public string SayableStateDescription { get { return "Finished replaying the solution. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I have finished replaying the solution. You can " + whatYouCanSayNow; } }

        private FinishedReplaying(GameState priorState) : base(GameMode.FinishedReplaying, priorState)
        {
        }

        internal static FinishedReplaying IsFinishedReplaying(GameState priorState)
        {
            return new FinishedReplaying(priorState);
        }
    }
}
