using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;

namespace MovingSofaProblem.Path
{
    public class PositionAndRotation
    {
        public PositionAndRotation(Vector position, Rotation rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }

        public Vector Position { get; private set; }
        public Rotation Rotation { get; private set; }
    }
}
