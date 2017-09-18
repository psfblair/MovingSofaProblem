using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Option;

#if UNIT_TESTS
using Vector = Domain.Vector;
using Rotation = Domain.Rotation;
#else
using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;
#endif

namespace MovingSofaProblem.Path
{
    public class PathHolder
    {
        internal readonly LinkedList<Breadcrumb> path;
        public PathHolder() : this(new LinkedList<Breadcrumb>())
        {
        }

        internal PathHolder(LinkedList<Breadcrumb> path)
        {
            this.path = path;
        }

        public void Add(PositionAndRotation positionAndRotation, float cameraY)
        {
            var breadcrumb = new Breadcrumb(positionAndRotation.Position, positionAndRotation.Rotation, cameraY);
            path.AddLast(breadcrumb);
        }

        public static Option<float> FinalCameraY(PathHolder pathHolder)
        {
            try
            {
                var lastElement = pathHolder.path.Last<Breadcrumb>();
                return Option.Create<float>(lastElement.CameraY);
            }
            catch (InvalidOperationException)
            {
                return Option.None;
            }
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
            if (!HasSegments(pathHolder))
            {
                return pathHolder;
            }

            var pathToSimplify = pathHolder.path;
            var accumulator = new LinkedList<Breadcrumb>();
            var newPath = pathToSimplify.Aggregate(accumulator, pathSimplifier);
            return new PathHolder(newPath);
        }

        // Keep ignoring breadcrumbs in the path while we're moving forward and:
        // * The angle between the previous segment and the proposed next segment (projected in the XZ plane / YZ plane)
        //   is not greater than n degrees -- i.e., we haven't turned significantly, and
        // * Our rotation hasn't changed by more than n% of a circle
        // This algorithm always takes the last breadcrumb, even if it is below the deviation threshold.
        static Func<LinkedList<Breadcrumb>, Breadcrumb, LinkedList<Breadcrumb>> pathSimplifier =
            (breadcrumbsSoFar, proposedNewBreadcrumb) =>
            {
                const float xTranslationAngleThreshold = 10.0f;
                const float yTranslationAngleThreshold = 10.0f;
                const float xRotationTolerance = 0.1f;
                const float yRotationTolerance = 0.1f;
                const float zRotationTolerance = 0.1f;

                var previousBreadcrumbNode = breadcrumbsSoFar.Last;

                // We need at least two segments to get the angle between them.
                if (previousBreadcrumbNode == null || previousBreadcrumbNode.Previous == null)
                {
                    // We add the first and second breadcrumbs to the simplified path
                    breadcrumbsSoFar.AddLast(proposedNewBreadcrumb);
                    return breadcrumbsSoFar;
                }

				var proposedPathSegment = new PathSegment(previousBreadcrumbNode.Value, proposedNewBreadcrumb);
				var previousPathSegment =
                    new PathSegment(previousBreadcrumbNode.Previous.Value, previousBreadcrumbNode.Value);
                var angleInXZPlaneBetweenSegments =
                    SpatialCalculations.AngleInXZPlaneBetween(previousPathSegment, proposedPathSegment);
                var angleInYZPlaneBetweenSegments =
                    SpatialCalculations.AngleInYZPlaneBetween(previousPathSegment, proposedPathSegment);

                if (angleInXZPlaneBetweenSegments <= xTranslationAngleThreshold
                    && angleInYZPlaneBetweenSegments <= yTranslationAngleThreshold
                    && previousPathSegment.XAxisRotationChange <= xRotationTolerance
                    && previousPathSegment.YAxisRotationChange <= yRotationTolerance
                    && previousPathSegment.ZAxisRotationChange <= zRotationTolerance)
                {
                    // Mutation, ugh!
                    breadcrumbsSoFar.RemoveLast();
                }
				
				breadcrumbsSoFar.AddLast(proposedNewBreadcrumb);
				return breadcrumbsSoFar;
            };


		public override string ToString()
		{
            var itemStrings = path.Select(item => item.ToString()).ToArray();
            return "Path: { " + String.Join(" ,\n\t", itemStrings) + " }\n";
		}

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = (PathHolder)obj;
            return path.SequenceEqual(other.path);
        }

        public override int GetHashCode()
        {
            int hash = this.GetType().ToString().GetHashCode();
            hash = (13 * hash) + path.GetHashCode();
            return hash;
        }
    }
}
