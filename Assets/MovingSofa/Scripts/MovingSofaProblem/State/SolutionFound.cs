namespace MovingSofaProblem.State
{
    public sealed class SolutionFound : GameState
    {
        override public string SayableStateDescription { get { return "Finished figuring out a solution.  Say 'replay solution' to see it."; } }
        override public string SayableStatus { get { return "I have figured out a solution. Say 'replay solution' to see it."; } }

        private SolutionFound(GameState priorState) : base(GameMode.SolutionFound, priorState) { }

        internal static SolutionFound HasFoundSolution(GameState priorState)
        {
            // TODO Do AI/search here
            // Redo the initial path with colliders against the planes, find all the places where it touches
            // Then try to solve those. Also can't be raised above the person's head too far.
            // TODO Save path to text asset

            return new SolutionFound(priorState);
        }
    }
}