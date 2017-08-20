#if UNIT_TESTS
using Vector = Domain.Vector;
using Rotation = Domain.Rotation;
#else
using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;
#endif

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

        public override string ToString()
        {
            return "Position: " + Position.ToString() + 
                " Rotation: " + Rotation.ToString();
        }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (PositionAndRotation)obj;
            return other.Position == Position && other.Rotation == Rotation;
        }

        public override int GetHashCode()
        {
            int hash = this.GetType().ToString().GetHashCode();
            hash = (13 * hash) + Position.GetHashCode();
            hash = (13 * hash) + Rotation.GetHashCode();
            return hash;
        }
    }
}
