namespace COL.MassLib.MascotParser
{
	public class Summary
	{
		private int _QueryNo;
		private float _Mass;
		private float _MZ;
		private int _Charge;
		private int _Match;

		public Summary(int argQueryNo, float argMass, float argMZ, int argCharge, int argMatch)
		{
			_QueryNo = argQueryNo;
			_Mass = argMass;
			_MZ = argMZ;
			_Charge = argCharge;
			_Match = argMatch;
		}

		public int QueryNo => _QueryNo;

		public float Mass => _Match;

		public float MZ => _MZ;

		public int Charge => _Charge;

		public int Match => _Match;
	}
}