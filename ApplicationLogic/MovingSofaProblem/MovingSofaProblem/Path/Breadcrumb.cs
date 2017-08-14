#if UNIT_TESTS
using Vector = Domain.Vector;
using Rotation = Domain.Rotation;
#else
using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;
#endif

namespace MovingSofaProblem.Path
{
    public class Breadcrumb
    {
        // We save the CameraY of breadcrumbs so that we know the absolute position
        // in world space of how high the user's eyes are. This will keep us from
        // including paths that are too high for the user to reach.
        public Breadcrumb(Vector position, Rotation rotation, float cameraY)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.CameraY = cameraY;
        }

        public Vector Position { get; private set; }
        public Rotation Rotation { get; private set; }
        public float CameraY { get; private set; }

        public override string ToString()
        {
            return "Position: " + Position.ToString() +
                    " Rotation: " + Rotation.ToString() +
                    " CameraY: " + CameraY.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            var other = (Breadcrumb)obj;
            return (other.Position == Position && other.Rotation == Rotation && other.CameraY == CameraY);
        }

        public override int GetHashCode()
        {
            int hash = this.GetType().ToString().GetHashCode();
            hash = (13 * hash) + Position.GetHashCode();
            hash = (13 * hash) + Rotation.GetHashCode();
            hash = (13 * hash) + CameraY.GetHashCode();
            return hash;
        }
    }
}
