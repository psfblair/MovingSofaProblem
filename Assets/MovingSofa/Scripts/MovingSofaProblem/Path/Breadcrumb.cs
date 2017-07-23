using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;

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
    }
}
