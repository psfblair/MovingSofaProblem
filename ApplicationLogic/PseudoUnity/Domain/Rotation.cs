using System;

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

        public Vector eulerAngles
        {
            get
            {
                var t0 = +2.0 * (w * x + y * z);
                var t1 = +1.0 - 2.0 * (x * x + y * y);

                var X = RadianToDegree(Math.Atan2(t0, t1));

                var t2 = +2.0 * (w * y - z * x);
                t2 = t2 > 1.0 ? 1.0 : t2;
                t2 = t2 < -1.0 ? -1.0 : t2;

                var Y = RadianToDegree(Math.Asin(t2));

                var t3 = +2.0 * (w * z + x * y);
                var t4 = +1.0 - 2.0 * (y * y + z * z);

                var Z = RadianToDegree(Math.Atan2(t3, t4));

                return new Vector(Utilities.DoubleToFloat(X), Utilities.DoubleToFloat(Y), Utilities.DoubleToFloat(Z));
            }
        }

        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static Rotation operator +(Rotation r1, Rotation r2)
        {
            return new Rotation(r1.x + r2.x, r1.y + r2.y, r1.z + r2.z, r1.w + r2.w);
        }

        public static Rotation operator *(Rotation r1, Rotation r2)
        {
            var x = (r1.x * r2.x) - (r1.y * r2.y) - (r1.z * r2.z) - (r1.w * r2.w);
            var y = (r1.x * r2.y) + (r1.y * r2.x) + (r1.z * r2.w) - (r1.w * r2.z);
            var z = (r1.x * r2.z) - (r1.y * r2.w) + (r1.z * r2.x) + (r1.w * r2.y);
            var w = (r1.x * r2.w) + (r1.y * r2.z) - (r1.z * r2.y) + (r1.w * r2.x);
            return new Rotation(x, y, z, w);
        }

        public static float Angle(Rotation rotation1, Rotation rotation2)
        {
            var res = rotation2 * Inverse(rotation1);
            var degrees = RadianToDegree(Math.Acos(res.w) * 2.0f);
            var asFloat = Utilities.DoubleToFloat(degrees);
            return asFloat > 180.0f ? 360.0f - asFloat : asFloat;
        }

        private static Rotation Inverse(Rotation r)
        {
            var modulusSquaredFactor = new Rotation(ModulusSquared(r), 0, 0, 0);
            return Conjugate(r) * modulusSquaredFactor;
        }

        private static Rotation Conjugate(Rotation r)
        {
            var multiplicand = new Rotation(-0.5f, 0f, 0f, 0f);
            var unitY = new Rotation(0f, 1.0f, 0f, 0f);
            var unitZ = new Rotation(0f, 0f, 1.0f, 0f);
            var unitW = new Rotation(0f, 0f, 0f, 1.0f);
            return multiplicand * (r + (unitY * r * unitY) + (unitZ * r * unitZ) + (unitW * r * unitW));
        }

        private static float ModulusSquared(Rotation r)
        {
            return (r.x * r.x) + (r.y * r.y) + (r.z * r.z) + (r.w * r.w);
        }

        public static Rotation Euler(float heading, float attitude, float bank)
        {
            var cosx = Math.Cos(heading);
            var sinx = Math.Sin(heading);
            var cosy = Math.Cos(attitude);
            var siny = Math.Sin(attitude);
            var cosz = Math.Cos(bank);
            var sinz = Math.Sin(bank);

            var w = Math.Sqrt(1.0 + (cosx * cosy) + (cosx * cosz) - (sinx * siny * sinz) + (cosy * cosz)) / 2.0;

            var w4 = (4.0 * w);
            var x = (( cosy * sinz) + (cosx * sinz) + (sinx * siny * cosz)) / w4;
            var y = (( sinx * cosy) + (sinx * cosz) + (cosx * siny * sinz)) / w4;
            var z = ((-sinx * sinz) + (cosx * siny * cosz) + siny) / w4;

            return new Rotation(
                Utilities.DoubleToFloat(x),
                Utilities.DoubleToFloat(y),
                Utilities.DoubleToFloat(z),
                Utilities.DoubleToFloat(w)
            );
        }

        public static Rotation Lerp(Rotation initialRotation, Rotation finalRotation, float proportionOfRotationComplete)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "(" + x + "," + y + "," + z + "," + w + ")";
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Rotation other = (Rotation)obj;
            return other.x == x && other.y == y && other.z == z && other.w == w;
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
