namespace Domain
{
    public class CameraLocation
    {
        public CameraLocation(Vector position, Rotation rotation, Vector forward)
        {
            this.position = position;
            this.rotation = rotation;
			this.forward = forward;
		}

        public Vector position { get; set; }
        public Rotation rotation { get; set; }
		public Vector forward { get; set; }

		public override string ToString()
		{
			return "Position: " + position.ToString() +
				   " Rotation: " + rotation.ToString() +
				   " Forward: " + forward.ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}

			var other = (CameraLocation)obj;
			return other.position == position && other.rotation == rotation && other.forward == forward;
		}

		public override int GetHashCode()
		{
			int hash = this.GetType().ToString().GetHashCode();
			hash = (13 * hash) + position.GetHashCode();
			hash = (13 * hash) + rotation.GetHashCode();
			hash = (13 * hash) + forward.GetHashCode();
			return hash;
		}

	}
}
