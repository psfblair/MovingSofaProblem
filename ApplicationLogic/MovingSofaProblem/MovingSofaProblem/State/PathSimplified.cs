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

		public static StateTransition SimplifyPath(GameState priorState
                                                    , PositionAndRotation positionAndRotationAfterDropped
                                                    , Func<PathHolder, PathHolder> solutionFinder)
        {
            // We now use the final Y of the object, since it's been put down
            var finalY = positionAndRotationAfterDropped.Position.y;
            priorState.InitialPath.Add(positionAndRotationAfterDropped, finalY);
            var simplifiedPath = PathHolder.Simplify(priorState.InitialPath);
            var newState = new PathSimplified(priorState, simplifiedPath);

            Func<GameState, GameState> findSolution = state => {
				// TODO Do AI/search here
				var solution = solutionFinder(state.InitialPath);
                return SolutionFound.HasFoundSolution(state, solution);
            };

			// TODO Stop spinner as side effect

			var sideEffects = ToList(GameState.SayState, findSolution, GameState.SayState);
            return new StateTransition(newState, sideEffects);
        }
    }
}
