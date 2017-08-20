using System;

using Functional.Option;

#if UNIT_TESTS
using CameraLocation = Domain.CameraLocation;
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
        internal static PositionAndRotation OrientationRelativeToOneUnitForward(CameraLocation locationToBeInFrontOf, Vector relativePosition)
        {
            // The camera transform's forward is a unit vector in world space pointing forward from the camera.
            // We just want the x and z components. So it's not exactly one unit in front (z), depending on x and y.
            var forward = new Vector(locationToBeInFrontOf.forward.x, 0.0f, locationToBeInFrontOf.forward.z);
            var position = locationToBeInFrontOf.position + forward + relativePosition;

            // We want to rotate the object around the y axis only so that it keeps looking at the wearer.
            Vector locationRotation = locationToBeInFrontOf.rotation.eulerAngles;
            Rotation rotation = Rotation.Euler(0, -locationRotation.y, 0);
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

        // This calculation uses unit vectors, so they both start at the origin. Thus we are dealing with the 
        // acute side of the angle between the vectors
        private static float AngleBetween(PathSegment previousPathSegment
                                         , PathSegment proposedPathSegment
                                         , Func<PathSegment, Vector> projectionVectorGenerator)
        {
            var previousVector = projectionVectorGenerator(previousPathSegment);
            var proposedVector = projectionVectorGenerator(proposedPathSegment);
            var dotProduct = Vector.Dot(Vector.Normalize(previousVector), Vector.Normalize(proposedVector));

            // Acos never gets above 7, so this will never go to plus or minus infinity
            return (float) (Math.Acos(dotProduct) * 180 / Math.PI);
        }

        internal static Option<PositionAndRotation> MaybeNewInterpolatedPosition (
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

            // If we have covered the entire distance, we don't have an update to the position
            if (proportionOfTranslationComplete > 1.0 && proportionOfRotationComplete > 1.0)
            {
                return Option.None;
            }

            var newPosition = InterpolatePosition(pathSegment.Start, pathSegment.End, proportionOfTranslationComplete);
            var newRotation = InterpolateRotation(pathSegment.Start, pathSegment.End, proportionOfRotationComplete);

            return Option.Create(new PositionAndRotation(newPosition, newRotation));
        }

        private static float ProportionOfTranslationComplete(float currentTime
                                                             , float replayStartTime
                                                             , float translationSpeed
                                                             , float distanceToTravel)
        {
            var distanceTraveled = (currentTime - replayStartTime) * translationSpeed;
            var proportionOfTranslationComplete = distanceTraveled / distanceToTravel;
            return proportionOfTranslationComplete > 1.0 ? 1.0f : proportionOfTranslationComplete;
        }

        private static float ProportionOfRotationComplete(float currentTime
                                                          , float replayStartTime
                                                          , float rotationSpeed
                                                          , float angleToRotate)
        {
            var rotationTraveled = (currentTime - replayStartTime) * rotationSpeed;
            var proportionOfRotationComplete = rotationTraveled / angleToRotate;
            return proportionOfRotationComplete > 1.0 ? 1.0f : proportionOfRotationComplete;
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
