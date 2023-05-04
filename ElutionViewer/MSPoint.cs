using System;

namespace COL.ElutionViewer
{
	[Serializable]
	public class MSPoint : IComparable
	{
		private float _mass;
		private float _intensity;

		public MSPoint()
		{
		}

		public MSPoint(float mass, float intensity)
		{
			_mass = mass;
			_intensity = intensity;
		}

		public float Mass
		{
			get => _mass;
			set => _mass = value;
		}

		public float Intensity
		{
			get => _intensity;
			set => _intensity = value;
		}

		public int CompareTo(object obj)
		{
			if (obj is MSPoint)
			{
				MSPoint p2 = (MSPoint)obj;
				return _mass.CompareTo(p2.Mass);
			}
			else
				throw new ArgumentException("Object is not a MSPoint.");
		}
	}
}