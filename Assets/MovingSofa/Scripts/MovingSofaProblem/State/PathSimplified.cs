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

        internal static PathSimplified HasSimplifiedPath(GameState priorState)
        {
            var simplifiedPath = PathHolder.Simplify(priorState.InitialPath);
            return new PathSimplified(priorState, simplifiedPath);
        }
    }
}
