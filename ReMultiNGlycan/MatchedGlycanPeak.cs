using COL.GlycoLib;
using COL.MassLib;
using System;
using System.Collections.Generic;

namespace COL.MultiGlycan
{
	public class MatchedGlycanPeak
	{
		private MSPeak _MSPeak;
		private double _Time;
		private int _ScanNum;
		private List<MSPoint> _Points;
		private GlycanCompound _glycanComposition;

		//private double _MergedIntensity;
		private double _CorrectedIntensity = 0;

		public MatchedGlycanPeak(int argScanNum, double argTime, MSPeak argPeak, GlycanCompound argGlycanComp)
		{
			_ScanNum = argScanNum;
			_Time = argTime;
			_MSPeak = argPeak;
			_glycanComposition = argGlycanComp;
		}

		public List<MSPoint> MSPoints
		{
			get => _Points;
			set => _Points = value;
		}

		public string AdductString
		{
			get
			{
				string tmp = "";
				foreach (Tuple<string, float, int> adduct in _glycanComposition.Adducts)
				{
					tmp = tmp + adduct.Item1 + " * " + adduct.Item3 + ";";
				}
				tmp = tmp.Substring(0, tmp.Length - 1);
				return tmp;
			}
		}

		public int Charge => (int)_MSPeak.ChargeState;

		/// <summary>
		/// Sum of Intensity Value: All isotoped peaks are included.
		/// </summary>
		public double IsotopicClusterIntensity => _MSPeak.ClusterIntensity;

		public double CorrectedIntensity
		{
			get
			{
				if (_CorrectedIntensity == 0 && _Points.Count != 0) //No corrected intensity return MostIntenseIntensity
				{
					return _MSPeak.MostIntenseIntensity;
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
		public double MostIntenseIntensity => _MSPeak.MostIntenseIntensity;

		public double ScanTime => _Time;

		public MSPeak Peak => _MSPeak;

		public int ScanNum => _ScanNum;

		public GlycanCompound GlycanComposition
		{
			get => _glycanComposition;
			set => _glycanComposition = value;
		}

		public string GlycanKey => _glycanComposition.GlycanKey;
	}
}