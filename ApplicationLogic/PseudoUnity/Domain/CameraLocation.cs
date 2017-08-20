namespace Domain
{
    public class CameraLocation
    {
        public CameraLocation(Vector position, Rotation rotation, Vector forward)
        {
            this.position = position;
            this.forward = forward;
            this.rotation = rotation;
        }

        public Vector position { get; set; }
        public Vector forward { get; set; }
        public Rotation rotation { get; set; }
    }
}
