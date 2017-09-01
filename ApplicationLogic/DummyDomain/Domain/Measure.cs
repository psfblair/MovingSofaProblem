using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Domain
{
    public class Measure
    {
        public readonly Situation transform;

        public Measure() : this(new Situation(new Vector(0.0f, 0.0f, 0.0f), new Rotation(0.0f, 0.0f, 0.0f, 0.0f), new Vector(0.0f, 0.0f, 0.0f)))
		{
		}

		public Measure(Situation situation)
		{
			this.transform = situation;
		}
	}
}
