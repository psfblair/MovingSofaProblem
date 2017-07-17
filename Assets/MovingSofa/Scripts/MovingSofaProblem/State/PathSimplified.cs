using System;

using MovingSofaProblem.Path;

namespace MovingSofaProblem.State
{
    public sealed class PathSimplified : GameState
    {
        override public string SayableStateDescription { get { return "Route simplified. Finding solution."; } }
        override public string SayableStatus { get { return "I have simplified the route and am figuring out a solution."; } }

        private PathSimplified(GameState priorState, PathHolder simplifiedPath) : base(GameMode.PathSimplified, priorState)
        {
            this.InitialPath = simplifiedPath;
            this.PathToReplay = simplifiedPath;
        }

        internal static StateTransition SimplifyPath(GameState priorState)
        {
            // Make sure the final point where the object is dropped is in the path.
            priorState.InitialPath.Add(priorState.Measure.transform.position, priorState.Measure.transform.rotation);
            var simplifiedPath = PathHolder.Simplify(priorState.InitialPath);
            var newState = new PathSimplified(priorState, simplifiedPath);

            Func<GameState, GameState> findSolution = state => { return SolutionFound.HasFoundSolution(state); };

            // TODO Stop spinner as side effect

            var sideEffects = ToList(findSolution, GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }
    }
}
