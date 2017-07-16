// Type aliases for some insulation from Unity
using Vector = UnityEngine.Vector3;
using Rotation = UnityEngine.Quaternion;

namespace MovingSofaProblem.Path
{

    public class PathSegment
    {
        private Breadcrumb start;
        private Breadcrumb end;

        public PathSegment(Breadcrumb start, Breadcrumb end)
        {
            this.start = start;
            this.end = end;

            var startPosition = start.Position;
            var endPosition = end.Position;
            XDisplacement = endPosition.x - startPosition.x;
            YDisplacement = endPosition.y - startPosition.y;
            ZDisplacement = endPosition.z - startPosition.z;

            var startRotation = start.Rotation.eulerAngles;
            var endRotation = end.Rotation.eulerAngles;
            XAxisChange = (endRotation.x - startRotation.x) / 360;
            YAxisChange = (endRotation.y - startRotation.y) / 360;
            ZAxisChange = (endRotation.z - startRotation.z) / 360;
        }

        public Breadcrumb Start { get { return start; } }
        public Breadcrumb End { get { return end; } }

        public float XDisplacement { get; private set; }

        public float YDisplacement { get; private set; }

        public float ZDisplacement { get; private set; }

        public float TranslationDistance
        {
            get { return Vector.Distance(Start.Position, End.Position); }
        }

        public float XAxisChange { get; private set; }

        public float YAxisChange { get; private set; }

        public float ZAxisChange { get; private set; }

        public float RotationAngle
        {
            get { return Rotation.Angle(Start.Rotation, End.Rotation); }
        }
    }
}
