using System;

using MovingSofaProblem.Path;

namespace MovingSofaProblem.State
{
    public sealed class PathSimplified : GameState
    {
        override public string SayableStateDescription { get { return "Path simplified. Finding solution."; } }
        override public string SayableStatus { get { return "I have simplified the path and am figuring out a solution."; } }
		
		private PathSimplified(GameState priorState, PathHolder simplifiedPath) : base(GameMode.PathSimplified, priorState)
        {
            this.InitialPath = simplifiedPath;
            this.PathToReplay = simplifiedPath;
        }

		internal static StateTransition SimplifyPath(GameState priorState
                                                    , Func<PathHolder, PathHolder> solutionFinder)
        {
            var priorLocation = priorState.Measure.transform;
            // Make sure the final point where the object is dropped is in the path.
            // We will use the cameraY of the previous breadcrumb before we dropped the object.
            var finalY = PathHolder.FinalCameraY(priorState.InitialPath).ValueOr(priorLocation.position.y);

            priorState.InitialPath.Add(priorLocation.position, priorLocation.rotation, finalY);
            var simplifiedPath = PathHolder.Simplify(priorState.InitialPath);
            var newState = new PathSimplified(priorState, simplifiedPath);

            Func<GameState, GameState> findSolution = state => {
				// TODO Do AI/search here
				var solution = solutionFinder(state.InitialPath);
                return SolutionFound.HasFoundSolution(state, solution);
            };

			// TODO Stop spinner as side effect

			var sideEffects = ToList(GameState.SayState, findSolution);
            return new StateTransition(newState, sideEffects);
        }
    }
}
