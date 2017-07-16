using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Option;

// Type aliases for some insulation from Unity
using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;

namespace MovingSofaProblem.Path
{
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
            var firstElement = pathHolder.path.First<Breadcrumb>();
            var seed = new LinkedList<Breadcrumb>();
            seed.AddFirst(firstElement);

            var newPath = pathToSimplify.Aggregate(seed, pathSimplifier);

            // Make sure the endpoint is the last point in the original path
            var newPathLast = newPath.Last<Breadcrumb>();
            var originalPathLast = pathToSimplify.Last<Breadcrumb>();
            if (newPathLast != originalPathLast)
            {
                newPath.AddLast(originalPathLast);
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
