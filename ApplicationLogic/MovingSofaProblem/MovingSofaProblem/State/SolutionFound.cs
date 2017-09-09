using MovingSofaProblem.Path;

namespace MovingSofaProblem.State
{
    public sealed class SolutionFound : GameState
    {
        private string whatYouCanSayNow = "Say 'Replay solution' to see it.";
        override public string SayableStateDescription { get { return "Finished figuring out a solution. " + whatYouCanSayNow; } }
        override public string SayableStatus { get { return "I have figured out a solution. You can " + whatYouCanSayNow; } }

        private SolutionFound(GameState priorState) : base(GameMode.SolutionFound, priorState) { }

		// This is a degenerate transition method in that it doesn't return a state transition; 
        // only PathSimplified calls it, transitioning to it in side effects.
		internal static GameState HasFoundSolution(GameState priorState, PathHolder solution)
        {
            if(priorState.Mode != GameMode.PathSimplified) 
            {
                return priorState;
            }

            // TODO Save solution to text asset

            var newState = new SolutionFound(priorState);
            newState.PathToReplay = solution;
            return newState;
        }
    }
}