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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public static float Distance(Vector position1, Vector position2)
        {
            return Magnitude(position1 - position2);
        }

        public static float Dot(Vector vec1, Vector vec2)
        {
            throw new NotImplementedException();
        }

        public static Vector Normalize(Vector vectorToNormalize)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + ")"; 
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Vector)obj;
            return other.x == x && other.y == y && other.z == z;
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
