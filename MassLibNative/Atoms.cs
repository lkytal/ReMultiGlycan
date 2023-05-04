namespace COL.MassLib
{
	public class Atoms
	{
		//Monoisotopic mass
		//H	Hydrogen	1.00783
		//C	Carbon	12.000000
		//N	Nitrogen	14.003072
		//O	Oxygen	15.9949141
		//Na	Sodium	22.989767

		private const float C = 12.000000f;
		private const float C_AVG = 12.011007f;
		private const float C13 = 13.0034f;
		private const float C13_AVG = 13.0034f;
		private const float H = 1.0078f;
		private const float H_AVG = 1.0078f;
		private const float O = 15.9949f;
		private const float O_AVG = 15.9988f;
		private const float N = 14.0031f;
		private const float N_AVG = 14.0068f;
		private const float Na = 22.9898f;
		private const float D = 2.0141f;
		private const float D_AVG = 2.0141f;
		private const float Proton = 1.0073f;
		private const float _Potassium = 39.0983f;

		public static float Potassium => _Potassium;

		public static float ProtonMass => Proton;

		public static float DeuteriumMass => D;

		public static float CarbonMass => C;

		public static float CarbonAVGMass => C_AVG;

		public static float HydrogenMass => H;

		public static float HydrogenAVGMass => H_AVG;

		public static float OxygenMass => O;

		public static float OxygenAVGMass => O_AVG;

		public static float NitrogenMass => N;

		public static float NitrogenAVGMass => N_AVG;

		public static float SodiumMass => Na;

		public static float Carbon13Mass => C13;

		public static float Carbon13AVGMass => C13_AVG;
	}
}