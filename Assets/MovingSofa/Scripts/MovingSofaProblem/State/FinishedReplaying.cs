namespace MovingSofaProblem.State
{
    public sealed class FinishedReplaying : GameState
    {
        override public string SayableStateDescription { get { return "Finished replaying the solution. Say 'replay solution' to replay again from the beginning."; } }
        override public string SayableStatus { get { return "I have finished replaying the solution. Say 'replay solution' to replay again from the beginning."; } }

        private FinishedReplaying(GameState priorState) : base(GameMode.FinishedReplaying, priorState)
        {
        }

        internal static FinishedReplaying IsFinishedReplaying(GameState priorState)
        {
            return new FinishedReplaying(priorState);
        }
    }
}
