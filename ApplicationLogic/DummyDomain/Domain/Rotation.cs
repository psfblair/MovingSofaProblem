﻿using System;

namespace Domain
{
    public class Rotation
    {
        public Rotation(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float x { get; }
        public float y { get; }
        public float z { get; }
        public float w { get; }

		// Dummy value for ease of testing: Euler angles are the same as the
		// x, y, z of the rotation times 360 degrees.
		public Vector eulerAngles
        {
            get
            {
                return new Vector(x * 360.0f, y* 360.0f, z* 360.0f); // Dummy value
            }
        }

        public static Rotation operator +(Rotation r1, Rotation r2)
        {
            return new Rotation(r1.x + r2.x, r1.y + r2.y, r1.z + r2.z, r1.w + r2.w);
        }

		// Dummy value for ease of testing: we just multiply each component.
		public static Rotation operator *(Rotation r1, Rotation r2)
        {
            return new Rotation(r1.x * r2.x, r1.y * r2.y, r1.z * r2.z, r1.w * r2.w);
        }

        // Dummy values for ease of testing: 
		public static float Angle(Rotation rotation1, Rotation rotation2)
        {
            var noRotation = new Rotation(0.0f, 0.0f, 0.0f, 0.0f);
            var ninetyDegreesAboutZ = new Rotation(0.0f, 0.0f, 1.0f, 0.25f);
            if(rotation1 == rotation2)
            {
                return 0.0f;
            }
            if(rotation1 == noRotation && rotation2 == ninetyDegreesAboutZ)
            {
                return 90.0f;
            }
            throw new NotSupportedException("Only specific rotations have a dummy calculation. Rotations given were: " +
                                            rotation1 + " and " + rotation2);
        }

		// Dummy value for ease of testing: Euler uses the passed-in values for
        // x,y,z and gives a w of 0
		public static Rotation Euler(float heading, float attitude, float bank)
        {
            return new Rotation(heading, attitude, bank, 0.0f);
        }

		// Dummy value for ease of testing: Take proportion of differences of x,y,z,w
		public static Rotation Lerp(Rotation initialRotation, Rotation finalRotation, float proportionOfRotationComplete)
        {
            Func<float, float, float, float> proportionateDistance = (initial, final, proportion) => (final - initial) * proportion;
            return new Rotation(
                proportionateDistance(initialRotation.x, finalRotation.x, proportionOfRotationComplete),
                proportionateDistance(initialRotation.y, finalRotation.y, proportionOfRotationComplete),
                proportionateDistance(initialRotation.z, finalRotation.z, proportionOfRotationComplete),
                proportionateDistance(initialRotation.w, finalRotation.w, proportionOfRotationComplete)
            );
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + "," + w + ")";
        }

		private const float EPSILON = 0.0001f;

		public override bool Equals(object obj)
        {
            return this == (Rotation)obj;
		}

		public static bool operator ==(Rotation r1, Rotation r2)
		{
			if (((object)r1 == null) || ((object)r2 == null))
			{
				return false;
			}

			return Math.Abs(r1.x - r2.x) < EPSILON &&
				   Math.Abs(r1.y - r2.y) < EPSILON &&
				   Math.Abs(r1.z - r2.z) < EPSILON &&
				   Math.Abs(r1.w - r2.w) < EPSILON;
		}

		public static bool operator !=(Rotation r1, Rotation r2)
		{
			return !(r1 == r2);
		}

        public override int GetHashCode()
        {
            int hash = this.GetType().ToString().GetHashCode();
            hash = (13 * hash) + x.GetHashCode();
            hash = (13 * hash) + y.GetHashCode();
            hash = (13 * hash) + z.GetHashCode();
            hash = (13 * hash) + w.GetHashCode();
            return hash;
        }
    }
}