using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Option;

// Type aliases for some insulation from Unity
using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;

namespace MovingSofaProblem
{
    public class Breadcrumb
    {
        public Breadcrumb(Vector position, Rotation rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }

        public Vector Position { get; private set; }
        public Rotation Rotation { get; private set; }
    }

    public class PathSegment
    {
        private Breadcrumb start;
        private Breadcrumb end;

        public PathSegment(Breadcrumb start, Breadcrumb end)
        {
            this.start = start;
            this.end = end;

            var startPosition = start.Position;
            var endPosition = end.Position;
            XDisplacement = endPosition.x - startPosition.x;
            YDisplacement = endPosition.y - startPosition.y;
            ZDisplacement = endPosition.z - startPosition.z;

            var startRotation = start.Rotation.eulerAngles;
            var endRotation = end.Rotation.eulerAngles;
            XAxisChange = (endRotation.x - startRotation.x) / 360;
            YAxisChange = (endRotation.y - startRotation.y) / 360;
            ZAxisChange = (endRotation.z - startRotation.z) / 360;
        }

        public Breadcrumb Start { get { return start; } }
        public Breadcrumb End { get { return end; } }

        public float XDisplacement { get; private set; }

        public float YDisplacement { get; private set; }

        public float ZDisplacement { get; private set; }

        public float TranslationDistance
        {
            get { return Vector.Distance(Start.Position, End.Position); }
        }

        public float XAxisChange { get; private set; }

        public float YAxisChange { get; private set; }

        public float ZAxisChange { get; private set; }

        public float RotationAngle
        {
            get { return Rotation.Angle(Start.Rotation, End.Rotation); }
        }
    }

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

    public class PathHolder
    {
        LinkedList<Breadcrumb> path;
        public PathHolder (): this(new LinkedList<Breadcrumb>())
        { 
        }

        private PathHolder(LinkedList<Breadcrumb> path)
        {
            this.path = path;
        }

        public void Add(Vector position, Rotation rotation)
        {
            var breadcrumb = new Breadcrumb(position, rotation);
            path.AddLast(breadcrumb);
        }
        
        public static bool HasSegments(PathHolder pathHolder)
        {
            return pathHolder.path.First != null && pathHolder.path.First.Next != null;
        }

        public static Option<PathStep> FirstStep(PathHolder pathHolder)
        {
            if (HasSegments(pathHolder))
            {
                var firstSegment = pathHolder.path.First;
                return Option.Some(new PathStep(firstSegment, firstSegment.Next));
            }
            else
            {
                return Option.None;
            }
        }

        public static PathHolder Simplify(PathHolder pathHolder)
        {
            if (! HasSegments(pathHolder))
            {
                return pathHolder;
            }

            var pathToSimplify = pathHolder.path;
            var seed = new LinkedList<Breadcrumb>();
            seed.AddFirst(pathToSimplify.First);

            var newPath = pathToSimplify.Aggregate(seed, pathSimplifier);

            // Make sure the endpoint is the last point in the original path
            if (newPath.Last != pathToSimplify.Last)
            {
                newPath.AddLast(pathToSimplify.Last);
            }

            return new PathHolder(newPath);
        }

        // Keep ignoring breadcrumbs in the path while we're moving forward and:
        // * Our x,y position hasn't changed by more than n meters
        // * Our rotation hasn't changed by more than n% of a circle
        static Func<LinkedList<Breadcrumb>, Breadcrumb, LinkedList<Breadcrumb>> pathSimplifier =
            (breadcrumbsSoFar, proposedNewBreadcrumb) =>
            {
                const float xTranslationTolerance = 0.1f;
                const float yTranslationTolerance = 0.1f;
                const float xRotationTolerance = 0.05f;
                const float yRotationTolerance = 0.05f;
                const float zRotationTolerance = 0.05f;

                var proposedPathSegment = new PathSegment(breadcrumbsSoFar.Last.Value, proposedNewBreadcrumb);

                if (proposedPathSegment.XDisplacement > xTranslationTolerance
                    || proposedPathSegment.YDisplacement > yTranslationTolerance
                    || proposedPathSegment.XAxisChange > xRotationTolerance
                    || proposedPathSegment.YAxisChange > yRotationTolerance
                    || proposedPathSegment.ZAxisChange > zRotationTolerance)
                {
                    // Mutation, ugh!
                    breadcrumbsSoFar.AddLast(proposedNewBreadcrumb);
                    return breadcrumbsSoFar;
                }
                else
                {
                    return breadcrumbsSoFar;
                }
            };
    }
}
