using System;

using Functional.Option;

#if UNIT_TESTS
using Situation = Domain.Situation;
using Vector = Domain.Vector;
using Rotation = Domain.Rotation;
#else
using CameraLocation = UnityEngine.Transform;
using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("MovingSofaProblemTests")]
namespace MovingSofaProblem.Path
{
    public static class SpatialCalculations
    {
        internal static PositionAndRotation ReorientRelativeToOneUnitForwardFrom(Situation locationToBeInFrontOf, Vector relativePosition)
        {
            // The camera transform's forward is a unit vector in world space pointing forward from the camera.
            // We just want the x and z components. So it's not exactly one unit in front (z), depending on x and y.
            var forwardCircaOneUnit = new Vector(locationToBeInFrontOf.forward.x, 0.0f, locationToBeInFrontOf.forward.z);
            var position = locationToBeInFrontOf.position + forwardCircaOneUnit + relativePosition;

            // We want to rotate the object around the y axis only so that it keeps looking at the wearer.
            Vector locationRotation = locationToBeInFrontOf.rotation.eulerAngles;
            var newYRotation = locationRotation.y - 180 > -360 ? locationRotation.y - 180 : locationRotation.y + 180;
            Rotation rotation = Rotation.Euler(0, newYRotation, 0);
            return new PositionAndRotation(position, rotation);
        }

        internal static float AngleInXZPlaneBetween(PathSegment previousPathSegment, PathSegment proposedPathSegment)
        {
            Func<PathSegment, Vector> xzProjectionGenerator =
                pathSegment => new Vector(pathSegment.XDisplacement, 0.0f, pathSegment.ZDisplacement);
            return AngleBetween(previousPathSegment, proposedPathSegment, xzProjectionGenerator);
        }

        internal static float AngleInYZPlaneBetween(PathSegment previousPathSegment, PathSegment proposedPathSegment)
        {
            Func<PathSegment, Vector> yzProjectionGenerator =
                pathSegment => new Vector(0.0f, pathSegment.YDisplacement, pathSegment.ZDisplacement);
            return AngleBetween(previousPathSegment, proposedPathSegment, yzProjectionGenerator);
        }

        private static float AngleBetween(PathSegment previousPathSegment
                                         , PathSegment nextPathSegment
                                         , Func<PathSegment, Vector> planeProjector)
        {
            // The projector functions just take displacements, so both projections start at the origin and terminate at the result.
            var projectionOfPrevious = planeProjector(previousPathSegment);
            var projectionOfNext = planeProjector(nextPathSegment);
            var dotProduct = Vector.Dot(Vector.Normalize(projectionOfPrevious), Vector.Normalize(projectionOfNext));

            return (float) (Math.Acos(dotProduct) * 180 / Math.PI);
        }

		internal static bool IsMovementComplete(PathSegment pathSegment, Vector currentPosition, Rotation currentRotation) {
            return pathSegment.End.Position == currentPosition && pathSegment.End.Rotation == currentRotation;
        }

        internal static PositionAndRotation InterpolatedPositionAndRotation (
            float currentTime
            , float replayStartTime
            , float translationSpeed
            , float rotationSpeed
            , PathSegment pathSegment
            )
        {
            var distanceToTravel = pathSegment.TranslationDistance;
            var proportionOfTranslationComplete = 
                ProportionOfTranslationComplete(currentTime, replayStartTime, translationSpeed, distanceToTravel);

            var angleToRotate = pathSegment.RotationAngle;
            var proportionOfRotationComplete =
                ProportionOfRotationComplete(currentTime, replayStartTime, rotationSpeed, angleToRotate);

            var newPosition = InterpolatePosition(pathSegment.Start, pathSegment.End, proportionOfTranslationComplete);
            var newRotation = InterpolateRotation(pathSegment.Start, pathSegment.End, proportionOfRotationComplete);

            return new PositionAndRotation(newPosition, newRotation);
        }

		// May be greater than 1.0; Lerp is clamped.
		private static float ProportionOfTranslationComplete(float currentTime
                                                             , float replayStartTime
                                                             , float translationSpeed
                                                             , float distanceToTravel)
        {
            if (Math.Abs(distanceToTravel) < 0.0001f) 
            {
                return 1.0f;
            }
            var distanceTraveled = (currentTime - replayStartTime) * translationSpeed;
            var proportionOfTranslationComplete = distanceTraveled / distanceToTravel;
            return proportionOfTranslationComplete;
        }

        // May be greater than 1.0; Lerp is clamped.
        private static float ProportionOfRotationComplete(float currentTime
                                                          , float replayStartTime
                                                          , float rotationSpeed
                                                          , float angleToRotateInDegrees)
        {
			if (Math.Abs(angleToRotateInDegrees) < 0.0001f)
			{
				return 1.0f;
			}
			var rotationTraveled = (currentTime - replayStartTime) * rotationSpeed;
            var proportionOfRotationComplete = rotationTraveled / angleToRotateInDegrees;
            return proportionOfRotationComplete;
        }

        private static Vector InterpolatePosition(Breadcrumb startBreadcrumb
                                                 , Breadcrumb endBreadcrumb
                                                 , float proportionOfTranslationComplete)
        {
            var initialPosition = startBreadcrumb.Position;
            var finalPosition = endBreadcrumb.Position;
            return Vector.Lerp(initialPosition, finalPosition, proportionOfTranslationComplete);
        }

        private static Rotation InterpolateRotation(Breadcrumb startBreadcrumb
                                                   , Breadcrumb endBreadcrumb
                                                   , float proportionOfRotationComplete)
        {
            var initialRotation = startBreadcrumb.Rotation;
            var finalRotation = endBreadcrumb.Rotation;
            return Rotation.Lerp(initialRotation, finalRotation, proportionOfRotationComplete);
        }
    }
}
