using CSMSL.Chemistry;
using System;

namespace COL.MultiGlycan
{
	public static class PurityEstimater
	{
		/// <summary>
		/// Return estimate purity base on Binomial distrubution
		/// </summary>
		/// <param name="argNumberPermethlationSite"></param>
		/// <param name="argStartPurity"></param>
		/// <param name="argIntensities">Last peak is the thoretical mono</param>
		/// <returns></returns>
		public static float Estimater(int argNumberPermethlationSite, float argStartPurity, float[] argIntensities)
		{
			float _Purity = argStartPurity;
			double _MaxCorrelation = 0;
			//Convert argIntensity to Ratio;
			int MaxIdx = 0;
			for (int i = 1; i < argIntensities.Length; i++)
			{
				if (argIntensities[i] > argIntensities[MaxIdx])
				{
					MaxIdx = i;
				}
			}
			double[] MeasuredRatios = new double[argIntensities.Length];
			for (int i = 0; i < argIntensities.Length; i++)
			{
				MeasuredRatios[i] = argIntensities[i] / argIntensities[MaxIdx];
			}
			for (int i = Convert.ToInt32(argStartPurity * 100); i <= 100; i++)
			{
				double[] CalcedRatios = new double[argIntensities.Length];
				for (int j = 0; j < argIntensities.Length; j++)
				{
					CalcedRatios[j] = Binomial(argNumberPermethlationSite, argNumberPermethlationSite - argIntensities.Length + j + 1, Convert.ToSingle(i) / 100);
				}
				double PPCCM = Correlation(CalcedRatios, MeasuredRatios);
				if (PPCCM > _MaxCorrelation)
				{
					_MaxCorrelation = PPCCM;
					_Purity = Convert.ToSingle(i) / 100;
				}
			}
			return _Purity;
		}

		/// <summary>
		///  Use only Theoritcal mono and the peak before it
		/// </summary>
		/// <param name="argNumberPermethlationSite"></param>
		/// <param name="argStartPurity"></param>
		/// <param name="argIntensities"></param>
		/// <returns></returns>
		public static float Estimater2(int argNumberPermethlationSite, float argStartPurity, float[] argIntensities)
		{
			float ObservedRatio = argIntensities[1] / argIntensities[0];
			float minDistance = 100;
			float minRatio = argStartPurity;
			for (int i = Convert.ToInt32(argStartPurity * 100); i <= 100; i++)
			{
				float CalcedRatio = (Convert.ToSingle(i) / 100) / ((1 - (Convert.ToSingle(i) / 100)) * argNumberPermethlationSite);
				if (Math.Abs(CalcedRatio - ObservedRatio) < minDistance)
				{
					minDistance = Math.Abs(CalcedRatio - ObservedRatio);
					minRatio = Convert.ToSingle(i) / 100;
				}
			}
			return minRatio;
		}

		/// <summary>
		/// Combine isotope pattern
		/// </summary>
		/// <param name="argNumberPermethlationSite"></param>
		/// <param name="argStartPurity"></param>
		/// <param name="argIntensities"></param>
		/// <returns></returns>
		public static float Estimater3(COL.GlycoLib.GlycanCompound argCompond, int argTheoreticalMonoIdx, float[] argIntensities)
		{
			double[] isotopeRatio = new double[argIntensities.Length];
			for (int i = 0; i < isotopeRatio.Length; i++)
			{
				isotopeRatio[i] = 0;
			}
			ChemicalFormula MonoChemFormula = new ChemicalFormula();
			MonoChemFormula.Add("C", argCompond.Carbon);
			MonoChemFormula.Add("H", argCompond.Hydrogen);
			MonoChemFormula.Add("O", argCompond.Oxygen);
			if (argCompond.Carbon13 != 0)
			{
				MonoChemFormula.Add("C{13}", argCompond.Carbon13);
			}
			if (argCompond.Deuterium != 0)
			{
				MonoChemFormula.Add("D", argCompond.Deuterium);
			}
			if (argCompond.Sodium != 0)
			{
				MonoChemFormula.Add("Na", argCompond.Sodium);
			}
			if (argCompond.Nitrogen != 0)
			{
				MonoChemFormula.Add("N", argCompond.Nitrogen);
			}
			double[] IsotopeDist = MonoChemFormula.GetIsotopicDistribution(10);

			return 0.0f;
		}

		public static double Correlation(double[] Xs, double[] Ys)
		{
			double sumX = 0;
			double sumX2 = 0;
			double sumY = 0;
			double sumY2 = 0;
			double sumXY = 0;

			int n = Xs.Length < Ys.Length ? Xs.Length : Ys.Length;

			for (int i = 0; i < n; ++i)
			{
				double x = Xs[i];
				double y = Ys[i];

				sumX += x;
				sumX2 += x * x;
				sumY += y;
				sumY2 += y * y;
				sumXY += x * y;
			}

			double stdX = Math.Sqrt(sumX2 / n - sumX * sumX / n / n);
			double stdY = Math.Sqrt(sumY2 / n - sumY * sumY / n / n);
			double covariance = (sumXY / n - sumX * sumY / n / n);

			return covariance / stdX / stdY;
		}

		private static double Binomial(int argN, int argK, float argProperity)
		{
			int tmpStop = argK;
			if (argN - argK > argN - (argN - argK))
			{
				tmpStop = argK;
			}
			else
			{
				tmpStop = argN - argK;
			}
			float numerator = 1.0f;
			float denominator = 1.0f;
			int CurrentNum = argN;
			int CurrentDenum = 1;
			for (int i = 1; i <= tmpStop; i++)
			{
				numerator = numerator * CurrentNum;
				denominator = denominator * CurrentDenum;
				CurrentNum--;
				CurrentDenum++;
			}
			double Total = Convert.ToDouble(numerator / denominator);
			Total = Total * Math.Pow((double)argProperity, (double)argK) * Math.Pow((double)(1 - argProperity), (double)(argN - argK));
			return Total;
		}
	}
}