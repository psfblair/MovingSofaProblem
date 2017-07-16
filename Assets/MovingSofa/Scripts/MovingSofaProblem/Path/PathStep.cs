using System.Collections.Generic;
using Functional.Option;

namespace MovingSofaProblem.Path
{
    public class PathStep
    {
        private LinkedListNode<Breadcrumb> start;
        private LinkedListNode<Breadcrumb> end;
        private PathSegment pathSegment;

        public PathStep(LinkedListNode<Breadcrumb> start, LinkedListNode<Breadcrumb> end)
        {
            this.start = start;
            this.end = end;
            this.pathSegment = new PathSegment(start.Value, end.Value);
        }

        public LinkedListNode<Breadcrumb> StartNode { get { return start; } }
        public LinkedListNode<Breadcrumb> EndNode { get { return end; } }
        public PathSegment PathSegment { get { return pathSegment; } }

        public static Option<PathStep> NextStep(PathStep step)
        {
            return step.EndNode.Next.ToOption().Select(
                nextBreadcrumbNode => new PathStep(step.EndNode, nextBreadcrumbNode)
            );
        }

        public static Option<PathStep> PreviousStep(PathStep step)
        {
            return step.StartNode.Previous.ToOption().Select(
                previousBreadcrumbNode => new PathStep(previousBreadcrumbNode, step.StartNode)
            );
        }

    }
}
