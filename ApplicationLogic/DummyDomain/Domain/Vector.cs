using System;

namespace Domain
{
    public class Vector
    {
        public Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x { get; }
        public float y { get; }
        public float z { get; }

        public static Vector operator +(Vector vector1, Vector vector2)
        {
            return new Vector(vector1.x + vector2.x, vector1.y + vector2.y, vector1.z + vector2.z);
        }

        public static Vector operator -(Vector vector1, Vector vector2)
        {
            return new Vector(vector1.x - vector2.x, vector1.y - vector2.y, vector1.z - vector2.z);
        }

        public static float Magnitude(Vector vector)
        {
            var sum = (vector.x * vector.x) + (vector.y * vector.y) + (vector.z * vector.z);
            return Utilities.DoubleToFloat(Math.Sqrt(sum));
        }

        public static Vector Lerp(Vector initialPosition, Vector finalPosition, float proportionOfTranslationComplete)
        {
            Func<float, float, float, float> proportionateDistance = (initial, final, proportion) => (final - initial) * proportion;
            return new Vector(proportionateDistance(initialPosition.x, finalPosition.x, proportionOfTranslationComplete),
                              proportionateDistance(initialPosition.y, finalPosition.y, proportionOfTranslationComplete),
                              proportionateDistance(initialPosition.z, finalPosition.z, proportionOfTranslationComplete));
        }

        public static float Distance(Vector position1, Vector position2)
        {
            return Magnitude(position1 - position2);
        }

        public static float Dot(Vector vec1, Vector vec2)
        {
            return (vec1.x * vec2.x) + (vec1.y * vec2.y) + (vec1.z * vec2.z);
        }

        public static Vector Normalize(Vector vectorToNormalize)
        {
            var magnitude = Magnitude(vectorToNormalize);
            if(magnitude < EPSILON) {
                // Dummy - simple way of handling degenerate case
                return new Vector(0.0f, 0.0f, 0.0f);
            } else {
				return new Vector(vectorToNormalize.x / magnitude,
								  vectorToNormalize.y / magnitude,
								  vectorToNormalize.z / magnitude);
			}
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")"; 
        }

        private const float EPSILON = 0.0001f;

		public override bool Equals(object obj)
		{
			return this == (Vector)obj;
		}

        public static bool operator ==(Vector v1, Vector v2)
		{
			if (((object)v1 == null) || ((object)v2 == null))
			{
				return false;
			}

            return Math.Abs(v1.x - v2.x) < EPSILON &&
                   Math.Abs(v1.y - v2.y) < EPSILON &&
                   Math.Abs(v1.z - v2.z) < EPSILON;
		}

        public static bool operator !=(Vector v1, Vector v2)
		{
			return !(v1 == v2);
		}
		
        public override int GetHashCode()
        {
            int hash = this.GetType().ToString().GetHashCode();
            hash = (13 * hash) + x.GetHashCode();
            hash = (13 * hash) + y.GetHashCode();
            hash = (13 * hash) + z.GetHashCode();
            return hash;
        }
    }
}
