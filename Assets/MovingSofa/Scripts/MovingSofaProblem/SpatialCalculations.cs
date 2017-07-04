﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MovingSofaProblem
{
    public class PositionAndRotation
    {
        public PositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }

        public Vector3 Position { get; private set; }
        public Quaternion Rotation { get; private set; }
    }

    public static class SpatialCalculations
    {
        public static PositionAndRotation OrientationRelativeToOneUnitInFrontOf(Transform transformToBeInFrontOf, Vector3 relativePosition)
        {
            var forward = new Vector3(transformToBeInFrontOf.forward.x, 0.0f, transformToBeInFrontOf.forward.z);
            var position = transformToBeInFrontOf.position + forward + relativePosition;
            var rotation = Quaternion.Inverse(transformToBeInFrontOf.rotation);
            return new PositionAndRotation(position, rotation);
        }

        public static PositionAndRotation PositionInFrontOf(Vector3 objectExtents, Transform transformToBeInFrontOf)
        {
            float yRelativePosition = -0.4f;
            float zRelativePosition = -0.5f;
            var relativePosition = new Vector3(0.0f, yRelativePosition + objectExtents.y, zRelativePosition + objectExtents.z);
            return OrientationRelativeToOneUnitInFrontOf(transformToBeInFrontOf, relativePosition);

            // TODO: Using smoothing, note each time the game object rotates. This will give us segmentation (?)
        }
    }
}