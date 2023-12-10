using COL.GlycoLib;
using COL.MassLib;
using System;
using System.Collections.Generic;

namespace COL.ReMultiGlycan
{
	public class MatchedGlycanPeak
	{
		//private double _MergedIntensity;
		private double _CorrectedIntensity = 0;

		public MatchedGlycanPeak(int argScanNum, double argTime, MSPeak argPeak, GlycanCompound argGlycanComp)
		{
			ScanNum = argScanNum;
			ScanTime = argTime;
			Peak = argPeak;
			GlycanComposition = argGlycanComp;
		}

		public List<MSPoint> MSPoints { get; set; }

		public string AdductString
		{
			get
			{
				string tmp = "";
				foreach (Tuple<string, float, int> adduct in GlycanComposition.Adducts)
				{
					tmp = tmp + adduct.Item1 + " * " + adduct.Item3 + ";";
				}
				tmp = tmp.Substring(0, tmp.Length - 1);
				return tmp;
			}
		}

		public int Charge => (int)Peak.ChargeState;

		/// <summary>
		/// Sum of Intensity Value: All isotoped peaks are included.
		/// </summary>
		public double IsotopicClusterIntensity => Peak.ClusterIntensity;

		public double CorrectedIntensity
		{
			get
			{
				if (_CorrectedIntensity == 0 && MSPoints.Count != 0) //No corrected intensity return MostIntenseIntensity
				{
					return Peak.MostIntenseIntensity;
				}
				else
				{
					return _CorrectedIntensity;
				}
			}
			set => _CorrectedIntensity = value;
		}

		/// <summary>
		/// Sum of Intensity Value: The most intens isotoped peak is included only.
		/// </summary>
		public double MostIntenseIntensity => Peak.MostIntenseIntensity;

		public double ScanTime { get; }

		public MSPeak Peak { get; }

		public int ScanNum { get; }

		public GlycanCompound GlycanComposition { get; set; }

		public string GlycanKey => GlycanComposition.GlycanKey;
	}
}