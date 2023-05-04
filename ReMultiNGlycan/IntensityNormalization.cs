using CSMSL.Chemistry;
using CSMSL.Spectral;
using System.Linq;
using IsotopeDistribution;

namespace COL.MultiGlycan
{
	public static class IntensityNormalization
	{
		/// <summary>
		///
		/// </summary>
		/// <param name="argCompond"></param>
		/// <param name="argTheoreticalMonoIdx"></param>
		/// <param name="argIntensities"></param>
		/// <returns></returns>
		public static double IntensityNormalizationFactorByIsotope(COL.GlycoLib.GlycanCompound argCompond, int argNumOfLabelingSite, double[] argIntensities, float argPurity)
		{
			ChemicalFormula MonoChemFormula = new ChemicalFormula();
			MonoChemFormula.Add("C", argCompond.Carbon + argCompond.Carbon13);
			MonoChemFormula.Add("H", argCompond.Hydrogen + argCompond.Deuterium);
			MonoChemFormula.Add("O", argCompond.Oxygen);
			//if (argCompond.Carbon13 != 0)
			//{
			//    MonoChemFormula.Add("C{13}", argCompond.Carbon13);
			//}
			//if (argCompond.Deuterium != 0)
			//{
			//    MonoChemFormula.Add("D", argCompond.Deuterium);
			//}
			if (argCompond.Sodium != 0)
			{
				MonoChemFormula.Add("Na", argCompond.Sodium);
			}
			if (argCompond.Nitrogen != 0)
			{
				MonoChemFormula.Add("N", argCompond.Nitrogen);
			}
			
			var ID = new IsotopicDistribution.Normalization();

			MZPeak[] Peaks = ID.CalculateDistribuition(MonoChemFormula, 7, IsotopicDistribution.Normalization.BasePeak).GetPeaks().ToArray();
			double[] isotopeRatio = new double[7];
			for (int i = 0; i < 7; i++)
			{
				isotopeRatio[i] = Peaks[i].Intensity;
			}
			double[] CorrectedIntensities = (double[])argIntensities.Clone();

			//Isotope Correction
			for (int i = 0; i <= 2; i++)
			{
				double Ratio = CorrectedIntensities[i] / isotopeRatio[0];
				for (int j = i; j < 7; j++)
				{
					CorrectedIntensities[j] = CorrectedIntensities[j] - (isotopeRatio[j - i] * Ratio);
				}
			}

			double isotopeCorrectionFactor = CorrectedIntensities[3] / argIntensities[3];

			// Purity Correction
			//double PurityCorrection = Math.Pow(argPurity, argNumOfLabelingSite);

			return isotopeCorrectionFactor;
		}
	}
}