﻿using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;
using CameraLocation = UnityEngine.Transform;
using Functional.Option;
using System;

namespace MovingSofaProblem.Path
{
    public static class SpatialCalculations
    {
        internal static PositionAndRotation OrientationRelativeToOneUnitInFrontOf(CameraLocation locationToBeInFrontOf, Vector relativePosition)
        {
            var forward = new Vector(locationToBeInFrontOf.forward.x, 0.0f, locationToBeInFrontOf.forward.z);
            var position = locationToBeInFrontOf.position + forward + relativePosition;
            // TODO? This doesn't seem to keep the rotation happening. Or does it?
            var rotation = Rotation.Inverse(locationToBeInFrontOf.rotation);
            return new PositionAndRotation(position, rotation);
        }

        internal static PositionAndRotation PositionInFrontOf(Vector objectExtents, CameraLocation locationToBeInFrontOf)
        {
            const float yRelativePosition = -0.4f;
            const float zRelativePosition = -0.5f;
            var relativePosition = new Vector(0.0f, yRelativePosition + objectExtents.y, zRelativePosition + objectExtents.z);
            return OrientationRelativeToOneUnitInFrontOf(locationToBeInFrontOf, relativePosition);
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
