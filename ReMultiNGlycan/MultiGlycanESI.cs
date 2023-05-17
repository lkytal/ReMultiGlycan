using COL.GlycoLib;
using COL.MassLib;
using Facet.Combinatorics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

//using ZedGraph;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using Chart = System.Windows.Forms.DataVisualization.Charting.Chart;

namespace COL.MultiGlycan
{
	//Mass Spectrometry Adduct Calculator http://fiehnlab.ucdavis.edu/staff/kind/Metabolomics/MS-Adduct-Calculator/
	[Serializable]
	public class MultiGlycanESI
	{
		private ManualResetEvent _doneEvent;
		private string _rawFile;

		//private List<ClusteredPeak> _cluPeaks;

		private List<MatchedGlycanPeak> _2ndPassedPeaksInScan;
		private List<ClusteredPeak> _MergedResultListAfterApplyLCOrder;
		private List<GlycanCompound> _GlycanList;
		private bool _MergeDifferentCharge = true;
		private int _MaxCharge = 5;
		private bool _FindClusterUseList = true;
		private string _ExportFilePath;
		private List<float> _adductMass;
		private Dictionary<float, string> _adductLabel;
		private float _maxLCBackMin = 5.0f;
		private float _maxLCFrontMin = 5.0f;
		private double _minAbundance = 10 ^ 6;
		private float _minLengthOfLC = 10;
		private bool _IncludeMZMatch = false;
		private float _totalLCTime = 0;
		private float _LCTimeTolerance = 0;
		private bool _ApplyLinearRegLC = false;
		private float _minPeakHeightPrecentage = 5;
		private bool _forceProtonatedGlycan = true;
		private Dictionary<string, List<MatchedGlycanPeak>> _OnePeakTwoGlycan;
		private ThermoRawReader rawReader;
		private List<int> MSScanList;
		//List<CandidatePeak> _lstCandidatePeak; //Store candidate glycan m/z

		private List<float> _candidateMzList;

		//Dictionary<float, List<CandidatePeak>> _dicCandidatePeak;
		private Dictionary<float, List<GlycanCompound>> _dicCandidateGlycan;

		private List<GlycanCompound> _identifiedGlycan;
		private float _IsotopePPM = 8.5f;  //6 PPM suggested by Shiyue
		private float _PeakSNRatio = 2;
		private int _MinIsotopePeakCount = 3;
		private Dictionary<int, string> _GlycanLCodrer;
		private List<Color> LstColor = new List<Color>() { Color.DarkCyan, Color.DarkGoldenrod, Color.DarkGray, Color.DarkGreen, Color.DarkKhaki, Color.DarkMagenta, Color.DarkOliveGreen, Color.DarkOrchid, Color.DarkRed, Color.DarkSalmon, Color.DarkSeaGreen, Color.DarkSlateBlue, Color.DarkSlateGray, Color.DarkTurquoise, Color.DarkViolet, Color.DeepPink, Color.DeepSkyBlue };

		//List<ZedGraph.SymbolType> LstSymbol= new List<ZedGraph.SymbolType>(){SymbolType.Circle,SymbolType.Diamond,SymbolType.HDash,SymbolType.Plus,SymbolType.Square,SymbolType.Star,SymbolType.Triangle,SymbolType.TriangleDown,SymbolType.VDash,SymbolType.XCross};
		private Dictionary<enumLabelingTag, float> _LabelingRatio;

		private enumGlycanLabelingMethod _LabelingMethod;
		private bool _individualImgs = true;
		private bool _quantImgs = true;
		private bool _isMatchMonoPeakOnly = true;
		private Dictionary<enumLabelingTag, List<Tuple<string, int, float>>> dictEstimedPurity;
		private Dictionary<enumLabelingTag, float> PurityRatio;
		private Dictionary<string, Dictionary<enumLabelingTag, double>> _NormalizedFactor;
		private Dictionary<string, Dictionary<enumLabelingTag, List<double>>> MergeIntensities;
		private bool _PositiveChargeMode = true;

		public MultiGlycanESI(string argRawFile, int argStartScan, int argEndScan, string argGlycanList, double argMassPPM, double argGlycanMass, bool argPermenthylated, bool argReducedReducingEnd, int argSia, Dictionary<enumLabelingTag, float> argLabelingRatio, Dictionary<float, string> argAdductLabel, List<float> argAdductMass, bool argLog, bool argPositiveChargeMode = true)
		{
			DoLOG = argLog;
			_rawFile = argRawFile;
			//_cluPeaks = new List<ClusteredPeak>();
			MatchedPeakInScan = new List<MatchedGlycanPeak>();
			_OnePeakTwoGlycan = new Dictionary<string, List<MatchedGlycanPeak>>();
			MassPPM = argMassPPM;
			GlycanFile = argGlycanList;
			IsPermethylated = argPermenthylated;
			IsReducedReducingEnd = argReducedReducingEnd;
			GlycanPPM = argGlycanMass;
			StartScan = argStartScan;
			EndScan = argEndScan;
			_adductMass = new List<float>();
			_identifiedGlycan = new List<GlycanCompound>();
			if (Path.GetExtension(argRawFile) == ".raw")
			{
				rawReader = new ThermoRawReader(argRawFile);
			}
			_LabelingRatio = argLabelingRatio;
			_adductLabel = argAdductLabel;
			_adductMass = argAdductMass;
			SiaType = argSia;
			//Read Glycan list
			if (DoLOG)
			{
				Logger.WriteLog("Start Reading glycan list");
			}
			_PositiveChargeMode = argPositiveChargeMode;
			ReadGlycanList();
			if (DoLOG)
			{
				Logger.WriteLog("Finish Reading glycan list");
			}
		}

		public MultiGlycanESI(string argRawFile, int argStartScan, int argEndScan, string argGlycanList, double argMassPPM, double argGlycanMass, bool argPermenthylated, bool argReducedReducingEnd, int argSia, bool argLog, bool argPositiveChargeMode = true)
			: this(argRawFile, argStartScan, argEndScan, argGlycanList, argMassPPM, argGlycanMass, argPermenthylated, argReducedReducingEnd, argSia, null, null, null, argLog, argPositiveChargeMode)
		{
		}

		public bool HasEstimatePurity(enumLabelingTag argTag)
		{
			if (dictEstimedPurity.ContainsKey(argTag))
			{
				return true;
			}
			return false;
		}

		public string RawFile
		{
			set
			{
				if (_rawFile != value)
				{
					rawReader.Dispose();
					rawReader = null;

					rawReader = new ThermoRawReader(value);
					_rawFile = value;
				}
			}
			get => _rawFile;
		}

		public bool PositiveChargeMode
		{
			get => _PositiveChargeMode;
			set => _PositiveChargeMode = value;
		}

		public int SiaType { get; }

		public float TotalLCTime
		{
			get => _totalLCTime;
			set => _totalLCTime = value;
		}

		public float LCTimeTolerance
		{
			get => _LCTimeTolerance;
			set => _LCTimeTolerance = value;
		}

		public bool ApplyLinearRegLC
		{
			get => _ApplyLinearRegLC;
			set => _ApplyLinearRegLC = value;
		}

		public bool ForceProtonatedGlycan
		{
			get => _forceProtonatedGlycan;
			set => _forceProtonatedGlycan = value;
		}

		public bool IsReducedReducingEnd { get; }

		public List<float> CandidateMzList
		{
			get => _candidateMzList;
			set => _candidateMzList = value;
		}

		public bool IsPermethylated { get; }

		public string GlycanFile { get; }

		public List<GlycanCompound> IdentifiedGlycanCompounds
		{
			get => _identifiedGlycan;
			set => _identifiedGlycan = value;
		}

		public float MinPeakHeightPrecentage
		{
			get => _minPeakHeightPrecentage;
			set => _minPeakHeightPrecentage = value;
		}

		public double MassPPM { get; }

		public double GlycanPPM { get; }

		public Dictionary<enumLabelingTag, float> LabelingRatio
		{
			get => _LabelingRatio;
			set => _LabelingRatio = value;
		}

		public bool IndividualImgs
		{
			get => _individualImgs;
			set => _individualImgs = value;
		}

		public bool QuantificationImgs
		{
			get => _quantImgs;
			set => _quantImgs = value;
		}

		public enumGlycanLabelingMethod LabelingMethod
		{
			get => _LabelingMethod;
			set => _LabelingMethod = value;
		}

		public float PeakSNRatio
		{
			get => _PeakSNRatio;
			set => _PeakSNRatio = value;
		}

		public float IsotopePPM
		{
			get => _IsotopePPM;
			set => _IsotopePPM = value;
		}

		public int MininumIsotopePeakCount
		{
			get => _MinIsotopePeakCount;
			set => _MinIsotopePeakCount = value;
		}

		public bool IsMatchMonoisotopicOnly
		{
			get => _isMatchMonoPeakOnly;
			set => _isMatchMonoPeakOnly = value;
		}

		public List<GlycanCompound> GlycanList
		{
			get => _GlycanList;
			set => _GlycanList = value;
		}

		public bool GlycanLCorderExist { get; private set; } = false;

		public bool IncludeMZMatch
		{
			get => _IncludeMZMatch;
			set => _IncludeMZMatch = value;
		}

		public float MinLengthOfLC
		{
			set => _minLengthOfLC = value;
			get => _minLengthOfLC;
		}

		public float MaxLCFrontMin
		{
			set => _maxLCFrontMin = value;
			get => _maxLCFrontMin;
		}

		public float MaxLCBackMin
		{
			set => _maxLCBackMin = value;
			get => _maxLCBackMin;
		}

		public double MinAbundance
		{
			set => _minAbundance = value;
			get => _minAbundance;
		}

		public IRawFileReader RawReader => rawReader;

		public Dictionary<string, Dictionary<enumLabelingTag, double>> NormalizedAbundance { get; private set; }

		//public List<ClusteredPeak> ClustedPeak
		//{
		//    get { return _cluPeaks; }
		//}
		public List<MatchedGlycanPeak> MatchedPeakInScan { get; private set; }

		public List<ClusteredPeak> MergedResultList { get; private set; }

		public List<ClusteredPeak> Merged2ndPassedResult { get; }

		public string ExportFilePath
		{
			set => _ExportFilePath = value;
			get => _ExportFilePath;
		}

		public int StartScan { get; }

		public int EndScan { get; }

		public int MaxGlycanCharge
		{
			set => _MaxCharge = value;
			get => _MaxCharge;
		}

		public bool MergeDifferentChargeIntoOne
		{
			set => _MergeDifferentCharge = value;
			get => _MergeDifferentCharge;
		}

		public List<float> AdductMass
		{
			get => _adductMass;
			set => _adductMass = value;
		}

		public Dictionary<float, string> AdductMassToLabel
		{
			get => _adductLabel;
			set => _adductLabel = value;
		}

		public Dictionary<int, string> GlycanLCodrer
		{
			get => _GlycanLCodrer;
			set => _GlycanLCodrer = value;
		}

		public bool DoLOG { get; } = false;

		public void ProcessSingleScan(int argScanNo)
		{
			//rawReader.SetPeakProcessorParameter(_peakParameter);
			//rawReader.SetTransformParameter(_transformParameters);
			if (DoLOG)
			{
				Logger.WriteLog("Start process scan:" + argScanNo);
			}
			if (rawReader.GetMsLevel(argScanNo) == 1)
			{
				if (DoLOG)
				{
					Logger.WriteLog("\tStart read raw file: " + argScanNo);
				}
				//Get MS Scan
				MSScan GMSScan = rawReader.ReadScan(argScanNo);
				if (DoLOG)
				{
					Logger.WriteLog("\tEnd read raw file: " + argScanNo);
				}
				//Get Peaks
				//List<MSPeak> deIsotopedPeaks = GMSScan.;

				//Convert to Float List
				//List<float> mzList = new List<float>();
				//foreach (MSPeak Peak in GMSScan.MSPeaks)
				//{
				//    mzList.Add(Peak.MonoisotopicMZ);
				//}
				//mzList.Sort();

				//Glycan Cluster in this scan
				List<MatchedGlycanPeak> Cluster;
				if (_FindClusterUseList)
				{
					if (_candidateMzList == null) // Generate Candidate Peak
					{
						if (DoLOG)
						{
							Logger.WriteLog("Start generate candidate peak");
						}
						_candidateMzList = GenerateCandidateGlycanMZList(_GlycanList);
						if (DoLOG)
						{
							Logger.WriteLog("End generate candidate peak");
						}
					}
					if (DoLOG)
					{
						Logger.WriteLog("\tStart find cluster use default list:" + argScanNo);
					}
					//Cluster = FindClusterWGlycanList(deIsotopedPeaks, argScanNo, GMSScan.Time);
					Cluster = FindClusterWGlycanList(GMSScan);
					foreach (MatchedGlycanPeak MatchedPeak in Cluster)
					{
						if (!_identifiedGlycan.Contains(MatchedPeak.GlycanComposition))
						{
							_identifiedGlycan.Add(MatchedPeak.GlycanComposition);
						}
					}
					MatchedPeakInScan.AddRange(Cluster);
					//_cluPeaks.AddRange(Cluster);
					if (DoLOG)
					{
						Logger.WriteLog("\tEnd find cluster use default list:" + argScanNo);
					}

					if (_IncludeMZMatch)
					{
						if (_2ndPassedPeaksInScan == null)
						{
							_2ndPassedPeaksInScan = new List<MatchedGlycanPeak>();
						}
						if (DoLOG)
						{
							Logger.WriteLog("Start process 2nd passed scan:" + argScanNo);
						}

						for (int i = 0; i < GMSScan.MZs.Length; i++)
						{
							float targetMZ = GMSScan.MZs[i];
							int ClosedPeaksIdx = MassLib.MassUtility.GetClosestMassIdx(_candidateMzList, targetMZ);
							float PPM = Math.Abs(Convert.ToSingle(((targetMZ - _candidateMzList[ClosedPeaksIdx]) / _candidateMzList[ClosedPeaksIdx]) * Math.Pow(10.0, 6.0)));
							if (PPM > MassPPM)
							{
								continue;
							}
							// List<CandidatePeak> ClosedGlycans = _dicCandidatePeak[_candidateMzList[ClosedPeaksIdx]];
							List<GlycanCompound> ClosedGlycans = _dicCandidateGlycan[_candidateMzList[ClosedPeaksIdx]];
							int MaxIntIdx = i;
							float MAXInt = 0;
							int StartIdx = i - 10;
							int EndIdx = i + 10;
							if (StartIdx < 0)
							{
								StartIdx = 0;
							}
							if (EndIdx > GMSScan.Intensities.Length - 1)
							{
								EndIdx = GMSScan.Intensities.Length - 1;
							}
							for (int j = StartIdx; j <= EndIdx; j++)
							{
								if (GMSScan.Intensities[j] > MAXInt)
								{
									MAXInt = GMSScan.Intensities[j];
									MaxIntIdx = j;
								}
							}
							MSPeak Peak = new MSPeak(targetMZ, GMSScan.Intensities[MaxIntIdx]);
							//############SaveMatchedGlycanCompound

							foreach (GlycanCompound CP in ClosedGlycans)
							{
								_2ndPassedPeaksInScan.Add(new MatchedGlycanPeak(GMSScan.ScanNo, GMSScan.Time, Peak, CP));
							}
						}
					}
				}
				else //Not use the list
				{
					//FIX
					//if (DoLog)
					//{
					//    Logger.WriteLog("\tStart find cluster without list:" + argScanNo.ToString());
					//}
					//Cluster = FindClusterWOGlycanList(deIsotopedPeaks, argScanNo, GMSScan.Time);
					//List<MSPeak> UsedPeakList = new List<MSPeak>();

					////ConvertGlycanListMz into MSPoint
					//List<MSPoint> MSPs = new List<MSPoint>();
					//foreach (GlycanCompound comp in _GlycanList)
					//{
					//    MSPs.Add(new MSPoint(Convert.ToSingle(comp.MonoMass), 0.0f));
					//}
					////Find Composition for each Cluster
					//foreach (ClusteredPeak cls in Cluster)
					//{
					//    int Idx = MassLib.MassUtility.GetClosestMassIdx(MSPs, Convert.ToSingle(cls.ClusterMono));
					//    if (GetMassPPM(_GlycanList[Idx].MonoMass, cls.ClusterMono) < _glycanPPM)
					//    {
					//        cls.GlycanComposition = _GlycanList[Idx];
					//    }
					//    UsedPeakList.AddRange(cls.Peaks);
					//    _cluPeaks.Add(cls);
					//}
					////Find Composition for single peak
					//foreach (MSPeak peak in deIsotopedPeaks)
					//{
					//    if (!UsedPeakList.Contains(peak))
					//    {
					//        int Idx = MassLib.MassUtility.GetClosestMassIdx(MSPs, peak.MonoMass);
					//        if (GetMassPPM(_GlycanList[Idx].MonoMass, peak.MonoMass) < _glycanPPM)
					//        {
					//            ClusteredPeak cls = new ClusteredPeak(argScanNo);
					//            cls.StartTime = GMSScan.Time;
					//            cls.EndTime = GMSScan.Time;
					//            cls.Charge = peak.ChargeState;
					//            cls.Peaks.Add(peak);
					//            cls.GlycanComposition = _GlycanList[Idx];
					//            _cluPeaks.Add(cls);
					//            UsedPeakList.Add(peak);
					//        }
					//    }
					//}
					//if (DoLog)
					//{
					//    Logger.WriteLog("\tEnd find cluster without list:" + argScanNo.ToString());
					//}
				}// Don't use glycan list;
				if (DoLOG)
				{
					Logger.WriteLog("\tEnd find cluster:" + argScanNo);
				}
				GMSScan = null;
			} //MS scan only
		}

		//Merged to ProcessSingleScan(int argScanNo)
		public void ProcessSingleScanTwoPassID1(int argScanNo)
		{
			if (_2ndPassedPeaksInScan == null)
			{
				_2ndPassedPeaksInScan = new List<MatchedGlycanPeak>();
			}

			if (DoLOG)
			{
				Logger.WriteLog("Start process 2nd passed scan:" + argScanNo);
			}
			if (rawReader.GetMsLevel(argScanNo) == 1)
			{
				if (DoLOG)
				{
					Logger.WriteLog("\tStart read raw file: " + argScanNo);
				}
				//Get MS Scan
				MSScan MSScan = rawReader.ReadScan(argScanNo);
				if (DoLOG)
				{
					Logger.WriteLog("\tEnd read raw file: " + argScanNo);
				}
				//Use identifed glycan
				_candidateMzList = GenerateCandidateGlycanMZList(_identifiedGlycan);

				foreach (float targetMZ in _candidateMzList)
				{
					int ClosedPeaksIdx = MassLib.MassUtility.GetClosestMassIdx(MSScan.MZs, targetMZ);
					if (MassLib.MassUtility.GetMassPPM(MSScan.MZs[ClosedPeaksIdx], targetMZ) > MassPPM)
					{
						continue;
					}

					//#################Find Peak##############
					//float[] Intensities = MSScan.Intensities;
					////Left bound
					//int LBound = ClosedPeaksIdx -1;
					//if (LBound < 0)
					//    LBound = 0;
					//do
					//{
					//    LBound = LBound - 1;
					//    if (LBound < 0)
					//    {
					//        LBound = 0;
					//        break;
					//    }
					//} while (LBound>0 && Intensities[LBound - 1] != 0.0f);

					////Right Bound
					//int RBound = ClosedPeaksIdx + 1;
					//if (RBound >=Intensities.Length)
					//    RBound = Intensities.Length -1;
					//do
					//{
					//    RBound = RBound + 1;
					//    if (RBound+1 >= Intensities.Length)
					//    {
					//        RBound = Intensities.Length - 1;
					//        break;
					//    }
					//} while (RBound < Intensities.Length &&Intensities[RBound + 1] != 0.0);
					//FindMax Intensity
					int MaxIntIdx = ClosedPeaksIdx;
					float MAXInt = 0;
					int StartIdx = ClosedPeaksIdx - 10;
					int EndIdx = ClosedPeaksIdx + 10;
					if (StartIdx < 0)
					{
						StartIdx = 0;
					}
					if (EndIdx > MSScan.Intensities.Length - 1)
					{
						EndIdx = MSScan.Intensities.Length - 1;
					}
					for (int i = StartIdx; i <= EndIdx; i++)
					{
						if (MSScan.Intensities[i] > MAXInt)
						{
							MAXInt = MSScan.Intensities[i];
							MaxIntIdx = i;
						}
					}
					MSPeak Peak = new MSPeak(MSScan.MZs[MaxIntIdx], MSScan.Intensities[MaxIntIdx]);
					//############SaveMatchedGlycanCompound
					//List<CandidatePeak> ClosedGlycans = _dicCandidatePeak[targetMZ];
					List<GlycanCompound> ClosedGlycans = _dicCandidateGlycan[targetMZ];
					foreach (GlycanCompound CP in ClosedGlycans)
					{
						_2ndPassedPeaksInScan.Add(new MatchedGlycanPeak(MSScan.ScanNo, MSScan.Time, Peak, CP));
					}
				}
			}
		}

		public void EstimatePurity()
		{
			dictEstimedPurity = new Dictionary<enumLabelingTag, List<Tuple<string, int, float>>>(); //Dict Key: Labeling Tag, Tuple1 = Glycan Key;Tuple3 = Scan Num; Tuple3 = Purity
			Dictionary<enumLabelingTag, float> SumPurity = new Dictionary<enumLabelingTag, float>();
			foreach (MatchedGlycanPeak MPeak in MatchedPeakInScan)
			{
				if (COL.MassLib.MassUtility.GetMassPPM(MPeak.GlycanComposition.MZ, MPeak.MSPoints[0].Mass) < MassPPM) // If the Theoretical Monoisotopic Peak = First peak
				{
					continue;
				}
				if (!dictEstimedPurity.ContainsKey(MPeak.GlycanComposition.LabelingTag))
				{
					dictEstimedPurity.Add(MPeak.GlycanComposition.LabelingTag, new List<Tuple<string, int, float>>());
					SumPurity.Add(MPeak.GlycanComposition.LabelingTag, 0);
				}
				//Find Theoratical Mono
				int TheoratiocalMonoIdx = 0;
				for (int i = 0; i < MPeak.MSPoints.Count; i++)
				{
					if (MassLib.MassUtility.GetMassPPM(MPeak.MSPoints[i].Mass, MPeak.GlycanComposition.MZ) <= MassPPM)
					{
						TheoratiocalMonoIdx = i;
						break;
					}
				}

				float[] Intensities = new float[TheoratiocalMonoIdx + 1];
				for (int i = 0; i <= TheoratiocalMonoIdx; i++)
				{
					Intensities[i] = MPeak.MSPoints[i].Intensity;
				}
				float purity = PurityEstimater.Estimater(MPeak.GlycanComposition.NumOfPermethlationSites, 0.8f, Intensities);
				if (purity == 0.8f)
				{
					continue;
				}
				dictEstimedPurity[MPeak.GlycanComposition.LabelingTag].Add(new Tuple<string, int, float>(MPeak.GlycanComposition.GlycanKey, MPeak.ScanNum, purity));
				SumPurity[MPeak.GlycanComposition.LabelingTag] = SumPurity[MPeak.GlycanComposition.LabelingTag] + purity;
			}
			PurityRatio = new Dictionary<enumLabelingTag, float>();
			foreach (enumLabelingTag tag in SumPurity.Keys)
				PurityRatio.Add(tag, SumPurity[tag] / dictEstimedPurity[tag].Count);
		}

		//public void CorrectIntensityByIsotope()
		//{
		//    foreach (MatchedGlycanPeak MPeak in _MatchedPeaksInScan)
		//    {
		//        if (COL.MassLib.MassUtility.GetMassPPM(MPeak.GlycanComposition.MZ, MPeak.MSPoints[0].Mass) < _massPPM) // If the Theoretical Monoisotopic Peak = First peak
		//        {
		//            continue;
		//        }

		//        int TheoratiocalMonoIdx = 0;
		//        for (int i = 0; i < MPeak.MSPoints.Count; i++)
		//        {
		//            if (MassLib.MassUtility.GetMassPPM(MPeak.MSPoints[i].Mass, MPeak.GlycanComposition.MZ) <= _massPPM)
		//            {
		//                TheoratiocalMonoIdx = i;
		//                break;
		//            }
		//        }
		//        int IntensityCount = TheoratiocalMonoIdx + 3;
		//        if (TheoratiocalMonoIdx + 3 > MPeak.MSPoints.Count)
		//        {
		//            IntensityCount = MPeak.MSPoints.Count;
		//        }
		//        float[] Intensities = new float[IntensityCount];
		//        for (int i = 0; i < IntensityCount; i++)
		//        {
		//            Intensities[i] = MPeak.MSPoints[i].Intensity;
		//        }
		//        if (MPeak.GlycanComposition.LabelingTag == enumLabelingTag.MP_CH3)
		//        {
		//            MPeak.CorrectedIntensity = IntensityNormalization.IntensityCorrectionByIsotope(MPeak.GlycanComposition, TheoratiocalMonoIdx, Intensities, 1.0f);
		//        }
		//        else
		//        {
		//            MPeak.CorrectedIntensity = IntensityNormalization.IntensityCorrectionByIsotope(MPeak.GlycanComposition, TheoratiocalMonoIdx, Intensities, PurityRatio[MPeak.GlycanComposition.LabelingTag]);
		//        }
		//        Console.WriteLine(MPeak.CorrectedIntensity);
		//    }
		//}
		public void ApplyLCordrer()
		{
			Dictionary<string, int> GlycanOrder = new Dictionary<string, int>();
			//Get LC Order
			foreach (GlycanCompound GlycanC in _GlycanList)
			{
				if (GlycanC.GlycanLCorder != 0)
				{
					GlycanOrder.Add(GlycanC.GlycanKey, GlycanC.GlycanLCorder);
				}
			}
			List<ClusteredPeak> MergeWithLCOrder = new List<ClusteredPeak>();

			foreach (ClusteredPeak MCluster in MergedResultList)
			{
				if (GlycanOrder.ContainsKey(MCluster.GlycanKey))
				{
					MergeWithLCOrder.Add(MCluster);
				}
			}
			List<int> IdentifiedOrder = new List<int>();
			foreach (ClusteredPeak g in MergeWithLCOrder)
			{
				IdentifiedOrder.Add(GlycanOrder[g.GlycanKey]);
			}
			//LIC
			List<int> Length = new List<int>();
			List<int> Prev = new List<int>();

			for (int i = 0; i < IdentifiedOrder.Count; i++)
			{
				Length.Add(1);
				Prev.Add(-1);
			}
			for (int i = 0; i < IdentifiedOrder.Count; i++)
			{
				for (int j = i + 1; j < IdentifiedOrder.Count; j++)
				{
					if (IdentifiedOrder[i] < IdentifiedOrder[j])
					{
						if (Length[i] + 1 > Length[j])
						{
							Length[j] = Length[i] + 1;
							Prev[j] = i;
						}
					}
				}
			}
			int n = 0, pos = 0;
			for (int i = 0; i < IdentifiedOrder.Count; i++)
			{
				if (Length[i] > n)
				{
					n = Length[i];
					pos = i;
				}
			}
			List<int> LIC = new List<int>();
			for (; Prev[pos] != -1; pos = Prev[pos])
			{
				LIC.Add(IdentifiedOrder[pos]);
			}
			LIC.Add(IdentifiedOrder[pos]);
			LIC.Reverse();
			//insert glycan not in LIC within tolerence
			for (int i = 0; i < IdentifiedOrder.Count; i++)
			{
				if (LIC.Contains(IdentifiedOrder[i]))
				{
					continue;
				}
				int PrvLICIdx = 0;
				int NxtLICIdx = IdentifiedOrder.Count;

				for (int j = i - 1; j >= 0; j--)
				{
					if (LIC.Contains(IdentifiedOrder[j]))
					{
						PrvLICIdx = j;
						break;
					}
				}
				for (int j = i + 1; j < IdentifiedOrder.Count; j++)
				{
					if (LIC.Contains(IdentifiedOrder[j]))
					{
						NxtLICIdx = j;
						break;
					}
				}

				if (Math.Abs(IdentifiedOrder[i] - IdentifiedOrder[PrvLICIdx]) <= 3)
				{
					LIC.Insert(LIC.IndexOf(IdentifiedOrder[PrvLICIdx]) + 1, IdentifiedOrder[i]);
					continue;
				}
				if (i < IdentifiedOrder.Count - 1 && Math.Abs(IdentifiedOrder[NxtLICIdx] - IdentifiedOrder[i]) <= 3)
				{
					LIC.Insert(LIC.IndexOf(IdentifiedOrder[NxtLICIdx]), IdentifiedOrder[i]);
					continue;
				}
			}
			_MergedResultListAfterApplyLCOrder = new List<ClusteredPeak>();
			for (int i = 0; i < MergedResultList.Count; i++)
			{
				if (!GlycanOrder.ContainsKey(MergedResultList[i].GlycanKey)) // Glycan no LC order
				{
					_MergedResultListAfterApplyLCOrder.Add(MergedResultList[i]);
				}
				else
				{
					if (LIC.Contains(GlycanOrder[MergedResultList[i].GlycanKey]))  //Glycan in LIC
					{
						_MergedResultListAfterApplyLCOrder.Add(MergedResultList[i]);
					}
				}
			}
		}

		public void Process(Object threadContext)
		{
			if (MSScanList == null)
			{
				MSScanList = new List<int>();
				for (int i = StartScan; i <= EndScan; i++)
				{
					if (rawReader.GetMsLevel(i) == 1)
					{
						MSScanList.Add(i);
					}
				}
			}
			foreach (int ScanNo in MSScanList)
			{
				ProcessSingleScan(ScanNo);
			}
			_doneEvent.Set();
		}

		public void NormalizeQuantitaionAbundance()
		{
			Dictionary<string, GlycanCompound> DictGlycan = new Dictionary<string, GlycanCompound>();
			MergeIntensities = new Dictionary<string, Dictionary<enumLabelingTag, List<double>>>();
			foreach (MatchedGlycanPeak matchP in MatchedPeakInScan)
			{
				if (!MergeIntensities.ContainsKey(matchP.GlycanKey))
				{
					MergeIntensities.Add(matchP.GlycanKey, new Dictionary<enumLabelingTag, List<double>>());
					DictGlycan.Add(matchP.GlycanKey, matchP.GlycanComposition);
				}
				if (!MergeIntensities[matchP.GlycanKey].ContainsKey(matchP.GlycanComposition.LabelingTag))
				{
					MergeIntensities[matchP.GlycanKey].Add(matchP.GlycanComposition.LabelingTag, new List<double>());
					for (int i = 0; i < 7; i++)
					{
						MergeIntensities[matchP.GlycanKey][matchP.GlycanComposition.LabelingTag].Add(0);
					}
				}
				int MonoIdx = 0;

				for (int i = 0; i < matchP.MSPoints.Count; i++)
				{
					if (Math.Abs(matchP.GlycanComposition.MZ - matchP.MSPoints[i].Mass) < 0.1)
					{
						MonoIdx = i;
						break;
					}
				}
				int StartFillInListIdx = 0;
				int StartFillInIntensityIdx = 0;
				if (MonoIdx <= 3)
				{
					StartFillInListIdx = 3 - MonoIdx;
					StartFillInIntensityIdx = 0;
				}
				else
				{
					StartFillInListIdx = 0;
					StartFillInIntensityIdx = MonoIdx - 3;
				}
				for (int i = StartFillInIntensityIdx; i < matchP.MSPoints.Count; i++)
				{
					MergeIntensities[matchP.GlycanKey][matchP.GlycanComposition.LabelingTag][StartFillInListIdx] =
						MergeIntensities[matchP.GlycanKey][matchP.GlycanComposition.LabelingTag][StartFillInListIdx] +
						matchP.MSPoints[i].Intensity;
					StartFillInListIdx++;
					if (StartFillInListIdx == 7)
					{
						break;
					}
				}
			}
			NormalizedAbundance = new Dictionary<string, Dictionary<enumLabelingTag, double>>();
			_NormalizedFactor = new Dictionary<string, Dictionary<enumLabelingTag, double>>();
			foreach (string GlycanKey in MergeIntensities.Keys)
			{
				_NormalizedFactor.Add(GlycanKey, new Dictionary<enumLabelingTag, double>());
				foreach (enumLabelingTag Tag in MergeIntensities[GlycanKey].Keys)
				{
					float correctedRatio = LabelingRatio.ContainsKey(Tag) ? LabelingRatio[Tag] : 1.0f;

					correctedRatio = PurityRatio.ContainsKey(Tag) ? PurityRatio[Tag] : correctedRatio;

					double NormalizatedFactor = IntensityNormalization.IntensityNormalizationFactorByIsotope(DictGlycan[GlycanKey],
						DictGlycan[GlycanKey].NumOfPermethlationSites, MergeIntensities[GlycanKey][Tag].ToArray(), correctedRatio);

					_NormalizedFactor[GlycanKey].Add(Tag, NormalizatedFactor);
				}
			}
			foreach (string GlycanKey in _NormalizedFactor.Keys)
			{
				NormalizedAbundance.Add(GlycanKey, new Dictionary<enumLabelingTag, double>());
				foreach (enumLabelingTag Tag in MergeIntensities[GlycanKey].Keys)
				{
					double estimedPurity = LabelingRatio.ContainsKey(Tag) ? LabelingRatio[Tag] : 1.0;
					if (PurityRatio.ContainsKey(Tag))
					{
						estimedPurity = PurityRatio[Tag];
					}
					double NormalizatedAbundance = MergeIntensities[GlycanKey][Tag][3] * _NormalizedFactor[GlycanKey][Tag] /
												   Math.Pow(estimedPurity, DictGlycan[GlycanKey].NumOfPermethlationSites);
					NormalizedAbundance[GlycanKey].Add(Tag, NormalizatedAbundance);
				}
			}
		}

		public void ExportToCSV()
		{
			if (!Directory.Exists(_ExportFilePath))
			{
				Directory.CreateDirectory(_ExportFilePath);
			}

			#region Merge report

			//Merged Cluster
			StreamWriter sw = null;

			sw = new StreamWriter(_ExportFilePath + "\\" + Path.GetFileName(_ExportFilePath) + ".csv");
			//parameters
			string version = Assembly.GetEntryAssembly().GetName().Version.ToString();
			sw.WriteLine("Program Version:" + version);
			sw.WriteLine("----------Parameters----------");
			sw.WriteLine("Raw Files:" + _rawFile);
			sw.WriteLine("Range:" + StartScan + "~" + EndScan);
			sw.WriteLine("Glycan List:" + GlycanFile);
			sw.WriteLine("Reduced Reducing End:" + IsReducedReducingEnd);
			sw.WriteLine("Permethylated:" + IsPermethylated);
			string adduct = "";
			foreach (float add in _adductLabel.Keys)
			{
				adduct = adduct + add + "(" + _adductLabel[add] + ");";
			}
			sw.WriteLine("Adduct:" + adduct);
			string SearchSia = "NeuAc";
			if (SiaType == 1)
			{
				SearchSia = "NeuGC";
			}
			else if (SiaType == 2)
			{
				SearchSia = SearchSia + "; NeuGc";
			}
			sw.WriteLine("Sia:" + SearchSia);
			sw.WriteLine("----------Peak Processing----------");
			sw.WriteLine("Isotope envelop tolerance:" + _IsotopePPM + "PPM");
			sw.WriteLine("Mininum peak count for isotope envelope:" + _MinIsotopePeakCount);
			sw.WriteLine("Single / Noise tolerance" + _PeakSNRatio);
			sw.WriteLine("----------Search Threshold----------");
			sw.WriteLine("Mass tolerance (PPM):" + MassPPM);
			sw.WriteLine("Min peak height:" + _minPeakHeightPrecentage + "%");
			sw.WriteLine("Only monoisotopic peak:" + _isMatchMonoPeakOnly);
			sw.WriteLine("Must have protonated adduct:" + _forceProtonatedGlycan);
			sw.WriteLine("----------Filter and export----------");
			sw.WriteLine("Max minute in front of LC apex  (a):" + _maxLCFrontMin);
			sw.WriteLine("Max minute in back of LC apex  (b):" + _maxLCBackMin);
			sw.WriteLine("Merge different charge glycan:" + _MergeDifferentCharge);
			sw.WriteLine("Min length of LC Peak in minute (c):" + _minLengthOfLC);
			sw.WriteLine("Minimum abundance:" + _minAbundance);

			//sw.WriteLine("Signal to noise ratio" + _peakParameter.SignalToNoiseThreshold.ToString());
			//sw.WriteLine("Peak background ratio" + _peakParameter.PeakBackgroundRatio.ToString());
			//sw.WriteLine("Use absolute peptide intensity" + _transformParameters.UseAbsolutePeptideIntensity.ToString());
			//if (_transformParameters.UseAbsolutePeptideIntensity)
			//{
			//    sw.WriteLine("Absolute peptide intensity:" + _transformParameters.AbsolutePeptideIntensity.ToString());
			//}
			//else
			//{
			//    sw.WriteLine("Peptide intensity ratio:" + _transformParameters.PeptideMinBackgroundRatio.ToString());
			//}
			try
			{
				switch (_LabelingMethod)
				{
					case enumGlycanLabelingMethod.None:
						sw.WriteLine(
							"Start Time,End Time,Start Scan Num,End Scan Num,Peak Intensity,LC Peak Area,HexNac-Hex-deHex-NeuAc-NeuGc,Composition mono,GU");
						//Sort
						MergedResultList =
							MergedResultList.OrderBy(x => x.GlycanComposition.NoOfHexNAc)
								.ThenBy(x => x.GlycanComposition.NoOfHex)
								.ThenBy(x => x.GlycanComposition.NoOfDeHex)
								.ThenBy(x => x.GlycanComposition.NoOfSia)
								.ThenByDescending(x => x.MonoIntensity)
								.ToList();
						break;

					case enumGlycanLabelingMethod.DRAG:
					case enumGlycanLabelingMethod.MultiplexPermethylated:
						string tmp = "";
						foreach (enumLabelingTag tag in _LabelingRatio.Keys)
						{
							tmp = tmp + tag + "(" + _LabelingRatio[tag].ToString("0.00") + ")" + ";";
						}
						sw.WriteLine("Labeling Tag:" + tmp);
						if (_LabelingMethod == enumGlycanLabelingMethod.MultiplexPermethylated)
						{
							tmp = "";
							foreach (enumLabelingTag tag in _LabelingRatio.Keys)
							{
								if (PurityRatio.ContainsKey(tag))
								{
									tmp = tmp + tag + "(" + PurityRatio[tag].ToString("0.00") + ")" + ";";
								}
							}
							sw.WriteLine("Estimated Purity:" + tmp);
						}
						sw.WriteLine(
							"Start Time,End Time,Start Scan Num,End Scan Num,Peak Intensity,Adjusted Intensity,LC Peak Area,Adjusted Intensity,HexNac-Hex-deHex-NeuAc-NeuGc,Label Tag,Composition mono");
						//Sort
						MergedResultList =
							MergedResultList.OrderBy(x => x.GlycanComposition.NoOfHexNAc)
								.ThenBy(x => x.GlycanComposition.NoOfHex)
								.ThenBy(x => x.GlycanComposition.NoOfDeHex)
								.ThenBy(x => x.GlycanComposition.NoOfSia)
								.ThenBy(x => x.MonoIntensity)
								.ThenByDescending(x => x.LabelingTag)
								.ToList();
						break;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			try
			{
				foreach (ClusteredPeak cls in MergedResultList)
				{
					if (_minLengthOfLC > (cls.EndTime - cls.StartTime) || cls.MonoIntensity < _minAbundance)
					{
						continue;
					}

					string export = cls.StartTime + ",";
					if (cls.EndTime == 0)
					{
						export = export + cls.StartTime + ",";
					}
					else
					{
						export = export + cls.EndTime + ",";
					}
					export = export + cls.StartScan + ","
									 + cls.EndScan + ","
									 + cls.MonoIntensity + ",";
					if (_LabelingMethod != enumGlycanLabelingMethod.None)
					{
						export = export + (cls.MonoIntensity * _LabelingRatio[cls.LabelingTag]) + ",";
					}
					export = export + cls.PeakArea + ",";
					if (_LabelingMethod != enumGlycanLabelingMethod.None)
					{
						export = export + (cls.PeakArea * _LabelingRatio[cls.LabelingTag]) + ",";
					}

					if (cls.GlycanComposition != null)
					{
						string Composition = cls.GlycanKey;//cls.GlycanComposition.NoOfHexNAc + "-" + cls.GlycanComposition.NoOfHex + "-" + cls.GlycanComposition.NoOfDeHex + "-" + cls.GlycanComposition.NoOfSia;
						if (_LabelingMethod != enumGlycanLabelingMethod.None)
						{
							export = export + Composition + "," + cls.GlycanComposition.LabelingTag + "," + cls.GlycanComposition.MonoMass;
						}
						else
						{
							export = export + Composition + "," + cls.GlycanComposition.MonoMass;
						}
					}
					else
					{
						export = export + ",-,-";
					}
					export += "," + cls.GU;
					sw.WriteLine(export);
				}
				sw.Flush();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (sw != null)
				{
					sw.Close();
				}
			}

			#endregion Merge report

			#region LC order Report

			string FullFilename;
			if (GlycanLCorderExist)
			{
				try
				{
					//With LC Order Result
					FullFilename = _ExportFilePath + "\\" + Path.GetFileName(_ExportFilePath) + "_wLCorder.csv";
					sw = new StreamWriter(FullFilename);
					sw.WriteLine("Start Time,End Time,Start Scan Num,End Scan Num,Abundance(All Isotoped Clustered),Abundance(Most Intense Peak),HexNac-Hex-deHex-Sia,Composition mono");
					foreach (ClusteredPeak cls in _MergedResultListAfterApplyLCOrder)
					{
						string export = cls.StartTime + ",";
						if (cls.EndTime == 0)
						{
							export = export + cls.StartTime + ",";
						}
						else
						{
							export = export + cls.EndTime + ",";
						}
						export = export + cls.StartScan + ","
										+ cls.EndScan + ","
									 + cls.IsotopicClusterIntensity + ","
									 + cls.MostIntenseIntensity + ",";

						if (cls.GlycanComposition != null)
						{
							string Composition = cls.GlycanComposition.NoOfHexNAc + "-" + cls.GlycanComposition.NoOfHex + "-" + cls.GlycanComposition.NoOfDeHex + "-" + cls.GlycanComposition.NoOfSia;
							export = export + Composition + "," + cls.GlycanComposition.MonoMass;
						}
						else
						{
							export = export + ",-,-";
						}

						sw.WriteLine(export);
					}
					sw.Flush();
				}
				catch (Exception ex)
				{
					throw ex;
				}
				finally
				{
					if (sw != null)
					{
						sw.Close();
					}
				}
			}

			#endregion LC order Report

			#region Full list

			Dictionary<string, List<MatchedGlycanPeak>> dictGlycans = new Dictionary<string, List<MatchedGlycanPeak>>();
			//Single Cluster in each scan
			try
			{
				FullFilename = _ExportFilePath + "\\" + Path.GetFileName(_ExportFilePath) + "_FullList.csv";
				sw = new StreamWriter(FullFilename);
				if (_LabelingMethod != enumGlycanLabelingMethod.None)
				{
					sw.WriteLine("Time,Scan Num,Abundance, Adjusted abundance,m/z,HexNac-Hex-deHex-NeuAc-NeuGc,Adduct,Label Tag,Composition mono");
				}
				else
				{
					sw.WriteLine("Time,Scan Num,Abundance,m/z,HexNac-Hex-deHex-NeuAc-NeuGc,Adduct,Composition mono");
				}

				//Sort by Glycan than Time

				MatchedPeakInScan.Sort(delegate (MatchedGlycanPeak M1, MatchedGlycanPeak M2)
				{
					int r = M1.GlycanKey.CompareTo(M2.GlycanKey);
					if (r == 0) r = M1.ScanTime.CompareTo(M2.ScanTime);
					if (r == 0) r = M1.Peak.MonoIntensity.CompareTo(M2.Peak.MonoIntensity);
					if (_LabelingMethod != enumGlycanLabelingMethod.None) r = M1.GlycanComposition.LabelingTag.CompareTo(M2.GlycanComposition.LabelingTag);
					return r;
				});
				MatchedPeakInScan = MatchedPeakInScan.OrderBy(x => x.GlycanComposition.NoOfHexNAc).ThenBy(x => x.GlycanComposition.NoOfHex).ThenBy(x => x.GlycanComposition.NoOfDeHex).ThenBy(x => x.GlycanComposition.NoOfSia).ThenBy(x => x.GlycanComposition.MonoMass).ThenByDescending(x => x.GlycanComposition.LabelingTag).ToList();

				foreach (MatchedGlycanPeak cls in MatchedPeakInScan)
				{
					string key = cls.GlycanKey;
					if (_LabelingMethod != enumGlycanLabelingMethod.None)
					{
						key = cls.GlycanKey + "-" + cls.GlycanComposition.LabelingTag;
					}
					if (!dictGlycans.ContainsKey(key))
					{
						dictGlycans[key] = new List<MatchedGlycanPeak>();
					}
					dictGlycans[key].Add(cls);
					//if (cls.IsotopicClusterIntensity == 0 || cls.Peak.MonoIntensity==0)
					//{
					//    continue;
					//}
					string export = cls.ScanTime.ToString("0.00") + ","
									+ cls.ScanNum + ",";

					export = export + cls.Peak.MonoIntensity + ",";
					if (_LabelingMethod != enumGlycanLabelingMethod.None)
					{
						// export = export + (cls.Peak.MonoIntensity * _LabelingRatio[cls.GlycanComposition.LabelingTag]).ToString() + ",";
						//2014.09.30 change to export correct intensity
						export = export + (cls.CorrectedIntensity) + ",";
					}
					export = export + cls.Peak.DeisotopeMz + ",";

					if (cls.GlycanComposition != null)
					{
						string Composition = cls.GlycanKey;// cls.GlycanComposition.NoOfHexNAc + "-" + cls.GlycanComposition.NoOfHex + "-" + cls.GlycanComposition.NoOfDeHex + "-" + cls.GlycanComposition.NoOfSia;
						export = export + Composition + ",";
						string strAdduct = "";
						foreach (Tuple<string, float, int> Aadduct in cls.GlycanComposition.Adducts)
						{
							strAdduct = strAdduct + Aadduct.Item1 + " * " + Aadduct.Item3 + "; ";
						}
						export = export + strAdduct + ",";
						//if (cls.Adduct == "H")
						//{
						//    export = export + cls.Charge + "*" + cls.Adduct + ",";
						//}
						//else
						//{
						//    if (cls.AdductCount == cls.Charge)
						//    {
						//        export = export + cls.AdductCount.ToString() + "*" + cls.Adduct +",";
						//    }
						//    else
						//    {
						//        export = export + cls.AdductCount.ToString() + "*" + cls.Adduct + " H*" + (cls.Charge - cls.AdductCount).ToString() + ",";
						//    }
						//}
						if (_LabelingMethod != enumGlycanLabelingMethod.None)
						{
							export = export + cls.GlycanComposition.LabelingTag + ",";
						}
						export = export + cls.GlycanComposition.MonoMass;
					}
					else
					{
						export = export + ",-,-";
					}

					sw.WriteLine(export);
				}
				sw.Flush();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (sw != null)
				{
					sw.Close();
				}
			}

			#endregion Full list

			#region Quantification

			//Quant Result
			if (_LabelingMethod != enumGlycanLabelingMethod.None)
			{
				if (_LabelingMethod == enumGlycanLabelingMethod.MultiplexPermethylated)
				{
					try
					{
						NormalizeQuantitaionAbundance();
						FullFilename = _ExportFilePath + "\\" + Path.GetFileName(_ExportFilePath) + "_Quant.csv";
						sw = new StreamWriter(FullFilename);
						string tmp = "Glycan,";
						List<enumLabelingTag> Tags = new List<enumLabelingTag>();
						foreach (enumLabelingTag tag in _LabelingRatio.Keys)
						{
							Tags.Add(tag);
						}
						Tags.Sort();
						foreach (enumLabelingTag tag in Tags)
						{
							tmp = tmp + tag + ",Normalization Factor, Estimated Purity,Normalizted and Adjusted Abundance (Adjusted Factor=" + _LabelingRatio[tag] + "),";
						}
						sw.WriteLine(tmp);
						foreach (string GKey in MergeIntensities.Keys)
						{
							tmp = GKey + ",";
							foreach (enumLabelingTag tag in Tags)
							{
								double Purity = 1.0;
								if (PurityRatio.ContainsKey(tag))
								{
									Purity = PurityRatio[tag];
								}
								if (MergeIntensities[GKey].ContainsKey(tag))
								{
									tmp = tmp + MergeIntensities[GKey][tag][3] + "," +
										  _NormalizedFactor[GKey][tag].ToString("0.00") + "," +
										  Purity.ToString("0.000") + "," +
										  (NormalizedAbundance[GKey][tag] * _LabelingRatio[tag]) + ",";
								}
								else
								{
									tmp = tmp + "N/A,N/A,N/A,N/A,";
								}
							}
							sw.WriteLine(tmp);
						}
					}
					catch (Exception ex)
					{
						throw ex;
					}
					finally
					{
						if (sw != null)
						{
							sw.Close();
						}
					}
				}
				else if (_LabelingMethod == enumGlycanLabelingMethod.DRAG)
				{
					FullFilename = _ExportFilePath + "\\" + Path.GetFileName(_ExportFilePath) + "_Quant.csv";
					try
					{
						sw = new StreamWriter(FullFilename);
						string tmp = "Glycan,";
						List<enumLabelingTag> Tags = new List<enumLabelingTag>();
						foreach (enumLabelingTag tag in _LabelingRatio.Keys)
						{
							Tags.Add(tag);
						}
						Tags.Sort();
						foreach (enumLabelingTag tag in Tags)
						{
							tmp = tmp + tag + "," + tag + "(Adjusted " + _LabelingRatio[tag] + "),";
						}
						sw.WriteLine(tmp);
						Dictionary<string, List<MatchedGlycanPeak>> GlycanPeaks = new Dictionary<string, List<MatchedGlycanPeak>>();
						foreach (string Gkey in dictGlycans.Keys)
						{
							string key = Gkey.Substring(0, Gkey.LastIndexOf('-'));
							if (!GlycanPeaks.ContainsKey(key))
							{
								GlycanPeaks.Add(key, new List<MatchedGlycanPeak>());
							}
							GlycanPeaks[key].AddRange(dictGlycans[Gkey]);
						}
						foreach (string GKey in GlycanPeaks.Keys)
						{
							List<MatchedGlycanPeak> psks = GlycanPeaks[GKey];
							Dictionary<enumLabelingTag, double> dictLabelIntensity = new Dictionary<enumLabelingTag, double>();
							foreach (MatchedGlycanPeak p in psks)
							{
								if (!dictLabelIntensity.ContainsKey(p.GlycanComposition.LabelingTag))
								{
									dictLabelIntensity.Add(p.GlycanComposition.LabelingTag, 0.0);
								}
								dictLabelIntensity[p.GlycanComposition.LabelingTag] = (p.MostIntenseIntensity) + dictLabelIntensity[p.GlycanComposition.LabelingTag];
							}
							tmp = GKey + ",";
							foreach (enumLabelingTag tag in Tags)
							{
								if (dictLabelIntensity.ContainsKey(tag))
								{
									tmp = tmp + dictLabelIntensity[tag] + "," +
										  (dictLabelIntensity[tag] * _LabelingRatio[tag]) + ",";
								}
								else
								{
									tmp = tmp + "0,0,";
								}
							}
							sw.WriteLine(tmp);
						}
					}
					catch (Exception ex)
					{
						throw ex;
					}
					finally
					{
						if (sw != null)
						{
							sw.Close();
						}
					}
				}
			}

			#endregion Quantification

			// Use GenerateImages Class 2015-03-09
			//#region Image
			////Generate images.
			////CreateFolder
			//if (_individualImgs || _quantImgs)
			//{
			//    try
			//    {
			//        string Dir = _ExportFilePath + "\\Pic";
			//        if (!Directory.Exists(Dir))
			//        {
			//            Directory.CreateDirectory(Dir);
			//        }
			//        if (_individualImgs)
			//        {
			//            foreach (string key in dictGlycans.Keys)
			//            {
			//                GetLCImage(ref zedControl, key, dictGlycans[key]).Save(Dir + "\\" + key + ".png", System.Drawing.Imaging.ImageFormat.Png);
			//            }
			//        }
			//        //Generate Quantitation figures
			//        if (_LabelingMethod != enumGlycanLabelingMethod.None && _quantImgs && _LabelingRatio.Count >= 2)
			//        {
			//            if (_LabelingMethod == enumGlycanLabelingMethod.MultiplexPermethylated)
			//            {
			//                foreach (string Gkey in dictGlycans.Keys)
			//                {
			//                    string key = Gkey.Substring(0, Gkey.LastIndexOf('-'));
			//                    GetQuantitionImage(ref zedControl, key, _NormalizedAbundance[key]).Save(Dir + "\\Quant-" + key + ".png", System.Drawing.Imaging.ImageFormat.Png);
			//                }
			//            }
			//            else if (_LabelingMethod == enumGlycanLabelingMethod.DRAG)
			//            {
			//                Dictionary<string, List<MatchedGlycanPeak>> GlycanPeaks =
			//                    new Dictionary<string, List<MatchedGlycanPeak>>();
			//                foreach (string Gkey in dictGlycans.Keys)
			//                {
			//                    string key = Gkey.Substring(0, Gkey.LastIndexOf('-'));
			//                    if (!GlycanPeaks.ContainsKey(key))
			//                    {
			//                        GlycanPeaks.Add(key, new List<MatchedGlycanPeak>());
			//                    }
			//                    GlycanPeaks[key].AddRange(dictGlycans[Gkey]);
			//                }
			//                foreach (string GKey in GlycanPeaks.Keys)
			//                {
			//                    GetQuantitionImage(ref zedControl, GKey, GlycanPeaks[GKey])
			//                        .Save(Dir + "\\Quant-" + GKey + ".png", System.Drawing.Imaging.ImageFormat.Png);
			//                }
			//            }
			//        }
			//    }
			//    catch (Exception ex)
			//    {
			//        throw ex;
			//    }
			//}
			//#endregion

			//#region Test
			//FullFilename = _ExportFilePath + "\\" + Path.GetFileName(_ExportFilePath) + "_5-6-0-3.csv";
			//sw = new StreamWriter(FullFilename);
			//Dictionary<string,Dictionary<float, float>> _intensityCount = new Dictionary<string,Dictionary<float, float>>();
			//sw.WriteLine("mz,0mz,idx,-10,-9,-8,-7,-6,-5,-4,-3,-2,-1,0,1,2,3,4,5,6,7,8");
			//foreach (MatchedGlycanPeak MatchPeak in MatchedPeakInScan)
			//{
			//    if (MatchPeak.GlycanComposition.LabelingTag == enumLabelingTag.MP_CH2D)
			//    {
			//        //find index
			//        int MonoIdx = 0;

			//        for (int i = 0; i < MatchPeak.MSPoints.Count; i++)
			//        {
			//            if (Math.Abs(MatchPeak.GlycanComposition.MZ - MatchPeak.MSPoints[i].Mass) < 0.1)
			//            {
			//                MonoIdx = i;
			//                break;
			//            }
			//        }
			//        string tmpOpt =MatchPeak.GlycanComposition.MZ +"," +MatchPeak.MSPoints[MonoIdx].Mass+","+MonoIdx+",";

			//        for (int i = 0; i < 10 - MonoIdx; i++)
			//        {
			//            tmpOpt = tmpOpt + "0,";
			//        }
			//        for (int i = 0; i < MatchPeak.MSPoints.Count; i++)
			//        {
			//            tmpOpt = tmpOpt + MatchPeak.MSPoints[i].Intensity + ",";
			//        }
			//        sw.WriteLine(tmpOpt.Substring(0,tmpOpt.Length-1));
			//    }
			//}

			//sw.Close();
			//#endregion
		}

		public void ExportParametersToExcel()
		{
			FileInfo NewFile = new FileInfo(_ExportFilePath);
			OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage(NewFile);

			OfficeOpenXml.ExcelWorksheet CurrentSheet = pck.Workbook.Worksheets.Add("Parameters");
			CurrentSheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
			CurrentSheet.Column(1).Width = 35;
			CurrentSheet.Cells[1, 1].Value = "Raw File:";
			CurrentSheet.Cells[1, 2].Value = _rawFile;
			CurrentSheet.Cells[2, 1].Value = "Range:";
			CurrentSheet.Cells[2, 2].Value = StartScan + "~" + EndScan;
			CurrentSheet.Cells[3, 1].Value = "Glycan List:";
			CurrentSheet.Cells[3, 2].Value = GlycanFile;
			CurrentSheet.Cells[5, 1].Value = "Experiment Section";
			CurrentSheet.Cells[6, 1].Value = "Reduced Reducing End:";
			CurrentSheet.Cells[6, 2].Value = IsReducedReducingEnd.ToString();
			CurrentSheet.Cells[7, 1].Value = "Permethylated:";
			CurrentSheet.Cells[7, 2].Value = IsPermethylated.ToString();
			CurrentSheet.Cells[8, 1].Value = "Adduct:";

			string adduct = "";
			foreach (float add in _adductLabel.Keys)
			{
				adduct = adduct + add + "(" + _adductLabel[add] + ");";
			}
			CurrentSheet.Cells[8, 2].Value = adduct;

			CurrentSheet.Cells[9, 1].Value = "Mass tolerance (PPM):";
			CurrentSheet.Cells[9, 2].Value = MassPPM.ToString();

			CurrentSheet.Cells[10, 1].Value = "Include m/z match only peak:";
			CurrentSheet.Cells[10, 2].Value = _IncludeMZMatch.ToString();

			CurrentSheet.Cells[12, 1].Value = "Merge Section";

			CurrentSheet.Cells[13, 1].Value = "Max minute in front of LC apex  (a):";
			CurrentSheet.Cells[13, 2].Value = _maxLCFrontMin.ToString();

			CurrentSheet.Cells[14, 1].Value = "Max minute in back of LC apex  (b):";
			CurrentSheet.Cells[14, 2].Value = _maxLCBackMin.ToString();

			CurrentSheet.Cells[15, 1].Value = "Merge different charge glycan:";
			CurrentSheet.Cells[15, 2].Value = _MergeDifferentCharge.ToString();

			CurrentSheet.Cells[16, 1].Value = "Min length of LC Peak in minute (c):";
			CurrentSheet.Cells[16, 2].Value = _minLengthOfLC.ToString();

			CurrentSheet.Cells[17, 1].Value = "Minimum abundance:";
			CurrentSheet.Cells[17, 2].Value = _minAbundance.ToString();

			//CurrentSheet.Cells[19, 1].Value = "Peak processing parameters";
			//CurrentSheet.Cells[20, 1].Value = "Signal to noise ratio:";
			//CurrentSheet.Cells[20, 2].Value = _peakParameter.SignalToNoiseThreshold.ToString();
			//CurrentSheet.Cells[21, 1].Value = "Peak background ratio:";
			//CurrentSheet.Cells[21, 2].Value = _peakParameter.PeakBackgroundRatio.ToString();
			//CurrentSheet.Cells[22, 1].Value = "Use absolute peptide intensity:";
			//CurrentSheet.Cells[22, 2].Value = _transformParameters.UseAbsolutePeptideIntensity.ToString();
			//if (_transformParameters.UseAbsolutePeptideIntensity)
			//{
			//    CurrentSheet.Cells[23, 1].Value = "Absolute peptide intensity:";
			//    CurrentSheet.Cells[23, 2].Value = _transformParameters.AbsolutePeptideIntensity.ToString();
			//}
			//else
			//{
			//    CurrentSheet.Cells[23, 1].Value = "Peptide intensity ratio:";
			//    CurrentSheet.Cells[23, 2].Value = _transformParameters.PeptideMinBackgroundRatio.ToString();
			//}

			//CurrentSheet.Cells[24, 1].Value = "Max charge";
			//CurrentSheet.Cells[24, 2].Value = _transformParameters.MaxCharge;

			System.Reflection.Assembly assm = typeof(frmMainESI).Assembly;
			System.Reflection.AssemblyName assmName = assm.GetName();
			Version ver = assmName.Version;
			CurrentSheet.Cells[27, 1].Value = "Program Version:";
			CurrentSheet.Cells[27, 2].Value = ver.ToString();

			CurrentSheet.Cells[28, 1].Value = "Process Time:";
			CurrentSheet.Cells[28, 2].Value = DateTime.Now.ToString();

			pck.Save();
		}

		//public void ExportGlycanToExcel(string argGkey, List<MatchedGlycanPeak> argScanRecord, List<ClusteredPeak> argMergedRecord)
		//{
		//    string Gkey = argGkey;
		//    int OutputRowCount = 0;
		//    FileInfo NewFile = new FileInfo(_ExportFilePath);
		//    OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage(NewFile);
		//    OfficeOpenXml.ExcelWorksheet CurrentSheet = pck.Workbook.Worksheets.Add(Gkey);
		//    ZedGraphControl zefControl = new ZedGraphControl();
		//    var picture = CurrentSheet.Drawings.AddPicture(Gkey, GetLCImage(ref zefControl, Gkey, argScanRecord));
		//    picture.SetPosition(0, 0, 9, 0);
		//    picture.SetSize(1320, 660);
		//    //CurrentSheet.Row(1).Height = 400;
		//    CurrentSheet.DefaultRowHeight = 50;

		//    OutputRowCount = 1;
		//    CurrentSheet.Cells[OutputRowCount, 1].Value = "Start Time";
		//    CurrentSheet.Cells[OutputRowCount, 2].Value = "End  Time";
		//    CurrentSheet.Cells[OutputRowCount, 3].Value = "Start Scan Num";
		//    CurrentSheet.Cells[OutputRowCount, 4].Value = "End Scan Num";
		//    CurrentSheet.Cells[OutputRowCount, 5].Value = "Sum Intensity";
		//    CurrentSheet.Cells[OutputRowCount, 6].Value = "Peak Area";
		//    CurrentSheet.Cells[OutputRowCount, 7].Value = "HexNac-Hex-deHex-Sia";
		//    OutputRowCount++;
		//    //Export Merge Result
		//    foreach (ClusteredPeak cls in argMergedRecord)
		//    {
		//        if (_minLengthOfLC > (cls.EndTime - cls.StartTime) || cls.MonoIntensity < _minAbundance)
		//        {
		//            continue;
		//        }
		//        CurrentSheet.Cells[OutputRowCount, 1].Value = cls.StartTime;
		//        CurrentSheet.Cells[OutputRowCount, 2].Value = cls.EndTime;
		//        CurrentSheet.Cells[OutputRowCount, 3].Value = cls.StartScan;
		//        CurrentSheet.Cells[OutputRowCount, 4].Value = cls.EndScan;
		//        CurrentSheet.Cells[OutputRowCount, 5].Value = cls.MonoIntensity;
		//        CurrentSheet.Cells[OutputRowCount, 6].Value = cls.PeakArea;

		//        if (cls.GlycanComposition != null)
		//        {
		//            CurrentSheet.Cells[OutputRowCount, 7].Value = cls.GlycanComposition.NoOfHexNAc + "-" + cls.GlycanComposition.NoOfHex + "-" + cls.GlycanComposition.NoOfDeHex + "-" + cls.GlycanComposition.NoOfSia;

		//        }
		//        else
		//        {
		//            CurrentSheet.Cells[OutputRowCount, 7].Value = "-";
		//        }
		//        OutputRowCount++;
		//    }
		//    CurrentSheet.Row(OutputRowCount).Height = 30; //Empty Row
		//    OutputRowCount++;
		//    CurrentSheet.Cells[OutputRowCount, 1].Value = "Time";
		//    CurrentSheet.Cells[OutputRowCount, 2].Value = "Scan Num";
		//    CurrentSheet.Cells[OutputRowCount, 3].Value = "Abundance";
		//    CurrentSheet.Cells[OutputRowCount, 4].Value = "m/z";
		//    CurrentSheet.Cells[OutputRowCount, 5].Value = "HexNac-Hex-deHex-Sia";
		//    CurrentSheet.Cells[OutputRowCount, 6].Value = "Adduct";
		//    OutputRowCount++;

		//    //Detail
		//    //argScanRecord.Sort(delegate(MatchedGlycanPeak M1, MatchedGlycanPeak M2)
		//    //{
		//    //    int r = M1.ScanTime.CompareTo(M2.ScanTime);
		//    //    if (r == 0) r = M1.Adduct.CompareTo(M2.Adduct);
		//    //    if (r == 0) r = M1.AdductCount.CompareTo(M2.AdductCount);
		//    //    return r;
		//    //});

		//    //foreach (MatchedGlycanPeak cls in sortedScanRecords[Gkey])
		//    //{
		//    //    CurrentSheet.Cells[OutputRowCount, 1].Value = cls.ScanTime;
		//    //    CurrentSheet.Cells[OutputRowCount, 2].Value = cls.ScanNum;
		//    //    CurrentSheet.Cells[OutputRowCount, 3].Value = cls.Peak.MonoIntensity;
		//    //    CurrentSheet.Cells[OutputRowCount, 4].Value = cls.Peak.MonoisotopicMZ;
		//    //    if (cls.GlycanComposition != null)
		//    //    {
		//    //        CurrentSheet.Cells[OutputRowCount, 5].Value = cls.GlycanComposition.NoOfHexNAc + "-" + cls.GlycanComposition.NoOfHex + "-" + cls.GlycanComposition.NoOfDeHex + "-" + cls.GlycanComposition.NoOfSia;
		//    //        CurrentSheet.Cells[OutputRowCount, 6].Value = cls.Adduct + "*" + cls.AdductCount;
		//    //    }
		//    //    else
		//    //    {
		//    //        CurrentSheet.Cells[OutputRowCount, 5].Value = "-";
		//    //        CurrentSheet.Cells[OutputRowCount, 6].Value = "-";
		//    //    }
		//    //    OutputRowCount++;
		//    //}
		//    pck.Save();
		//    //pck.Dispose();
		//    CurrentSheet.Dispose();
		//    GC.Collect();
		//}
		public void GetPurityEstimateImage(enumLabelingTag argTag, string argSaveFileName)
		{
			Chart cht = new Chart();
			cht.Size = new Size(2400, 1200);

			cht.ChartAreas.Add("Default");
			cht.ChartAreas["Default"].AxisX.Title = "Scan Number";
			cht.ChartAreas["Default"].AxisX.TitleFont = new Font("Arial", 24, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisX.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			cht.ChartAreas["Default"].AxisX.IsMarginVisible = true;
			cht.ChartAreas["Default"].AxisY.Title = "Abundance";
			cht.ChartAreas["Default"].AxisY.TitleFont = new Font("Arial", 24, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisY.LabelStyle.Font = new Font("Arial", 24, FontStyle.Bold);
			cht.ChartAreas["Default"].AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
			cht.ChartAreas["Default"].AxisY.IsMarginVisible = true;

			cht.Titles.Add("Default");
			cht.Titles[0].Text = "Purity Estimate " + argTag + " - " + PurityRatio[argTag].ToString("0.000000");
			cht.Titles[0].Font = new Font("Arial", 24, FontStyle.Bold);

			cht.Legends.Add("Default");
			cht.Legends["Default"].Docking = Docking.Top;
			cht.Legends["Default"].Alignment = StringAlignment.Center;
			cht.Legends["Default"].LegendStyle = LegendStyle.Row;
			cht.Legends["Default"].Font = new Font("Arial", 24, FontStyle.Bold);

			List<Tuple<string, int, float>> dataTag = dictEstimedPurity[argTag];

			Series series = cht.Series.Add("Default");
			series.ChartType = SeriesChartType.Point;
			series.MarkerSize = 15;
			series.MarkerStyle = MarkerStyle.Circle; ;
			series.Color = Color.Blue;
			foreach (Tuple<string, int, float> data in dataTag)
			{
				series.Points.AddXY(Convert.ToSingle(data.Item2), data.Item3);
			}
			cht.SaveImage(argSaveFileName, ChartImageFormat.Png);
		}

		//public Image GetLCImage(ref ZedGraph.ZedGraphControl argZedGraphControl, string argGKey, List<MatchedGlycanPeak> argMatchedScan)
		//{
		//    try
		//    {
		//        string Gkey = argGKey;
		//        ZedGraph.ZedGraphControl zgcGlycan = argZedGraphControl;
		//        zgcGlycan.Width = 2400;
		//        zgcGlycan.Height = 1200;

		//        GraphPane GP = zgcGlycan.GraphPane;
		//        GP.Title.Text = "Glycan: " + Gkey;
		//        GP.XAxis.Title.Text = "Scan time (min)";
		//        GP.YAxis.Title.Text = "Abundance";
		//        GP.CurveList.Clear();
		//        Dictionary<string, PointPairList> dictAdductPoints = new Dictionary<string, PointPairList>();
		//        Dictionary<float, float> MergeIntensity = new Dictionary<float, float>();
		//        List<float> Time = new List<float>();
		//        foreach (MatchedGlycanPeak MPeak in argMatchedScan)
		//        {
		//            string adduct = "";
		//            foreach (Tuple<string, float, int> addct in MPeak.GlycanComposition.Adducts)
		//            {
		//                adduct = adduct + addct.Item1 + "+";
		//            }
		//            adduct = adduct.Substring(0, adduct.Length - 1);
		//            if (!dictAdductPoints.ContainsKey(adduct))
		//            {
		//                dictAdductPoints.Add(adduct, new PointPairList());
		//            }
		//            dictAdductPoints[adduct].Add(MPeak.ScanTime, MPeak.Peak.MonoIntensity);

		//            if (!MergeIntensity.ContainsKey(Convert.ToSingle(MPeak.ScanTime)))
		//            {
		//                MergeIntensity.Add(Convert.ToSingle(MPeak.ScanTime), 0);
		//            }
		//            MergeIntensity[Convert.ToSingle(MPeak.ScanTime)] = MergeIntensity[Convert.ToSingle(MPeak.ScanTime)] + MPeak.Peak.MonoIntensity;

		//            if (!Time.Contains(Convert.ToSingle(MPeak.ScanTime)))
		//            {
		//                Time.Add(Convert.ToSingle(MPeak.ScanTime));
		//            }
		//        }
		//        int ColorIdx = 0;
		//        int SymbolIdx = 0;
		//        foreach (string Adduct in dictAdductPoints.Keys)
		//        {
		//            dictAdductPoints[Adduct].Sort(delegate(PointPair M1, PointPair M2)
		//            {
		//                return M1.X.CompareTo(M2.X);
		//            }
		//            );
		//            List<double> Mzs = new List<double>();
		//            List<double> Intensities = new List<double>();
		//            foreach (PointPair pp in dictAdductPoints[Adduct])
		//            {
		//                if (Mzs.Contains(pp.X))
		//                {
		//                    int idx = Mzs.IndexOf(pp.X);
		//                    Intensities[idx] = Intensities[idx] + pp.Y;
		//                }
		//                else
		//                {
		//                    Mzs.Add(pp.X);
		//                    Intensities.Add(pp.Y);
		//                }
		//            }
		//            LineItem Lne = GP.AddCurve(Adduct, Mzs.ToArray(), Intensities.ToArray(), LstColor[ColorIdx % 17],LstSymbol[SymbolIdx%10]);
		//            Lne.Line.IsSmooth = true;
		//            Lne.Line.SmoothTension = 0.15f;
		//            Lne.Symbol.Size = 2.0f;
		//            ColorIdx = ColorIdx + 1;
		//            SymbolIdx = SymbolIdx +1;
		//        }
		//        //Merge Intensity
		//        Time.Sort();
		//        PointPairList PPLMerge = new PointPairList();
		//        foreach (float tim in Time)
		//        {
		//            PPLMerge.Add(Convert.ToSingle(tim.ToString("0.00")), MergeIntensity[tim]);
		//        }
		//        LineItem Merge = GP.AddCurve("Merge", PPLMerge, Color.Black);
		//        Merge.Symbol.Size = 2.0f;
		//        Merge.Line.Width = 3.0f;
		//        Merge.Line.IsSmooth = true;
		//        Merge.Line.SmoothTension = 0.15f;
		//        zgcGlycan.AxisChange();
		//        zgcGlycan.Refresh();
		//        return (Image)zgcGlycan.MasterPane.GetImage();
		//    }
		//    catch (Exception ex)
		//    {
		//        throw new Exception("GetLC Pic failed " + argGKey + "  Err Msg:" + ex.ToString());
		//    }

		//}

		//public Image GetQuantitionImage(ref ZedGraph.ZedGraphControl argZedGraphControl, string argGKey, Dictionary<enumLabelingTag, double> argData)
		//{
		//    string Gkey = argGKey;
		//    ZedGraph.ZedGraphControl zgcGlycan = argZedGraphControl;
		//    zgcGlycan.Width = 2400;
		//    zgcGlycan.Height = 1200;

		//    GraphPane GP = zgcGlycan.GraphPane;
		//    GP.Title.Text = "Glycan: " + Gkey;
		//    GP.XAxis.Title.Text = "Labeling";
		//    GP.YAxis.Title.Text = "Abundance(%)";
		//    GP.CurveList.Clear();
		//    double YMax = 0;
		//    foreach (enumLabelingTag tag in argData.Keys)
		//    {
		//        if (argData[tag] > YMax)
		//        {
		//            YMax = argData[tag];
		//        }
		//    }
		//    List<string> labels = new List<string>();
		//    PointPairList ppl = new PointPairList();
		//    int x = 0;
		//    foreach (enumLabelingTag tag in argData.Keys)
		//    {
		//        labels.Add(tag.ToString());
		//        ppl.Add(x, argData[tag] / YMax * 100);
		//        x++;
		//    }

		//    BarItem myBar = GP.AddBar("Data", ppl, Color.Red);
		//    myBar.Bar.Fill.Type = FillType.Solid;
		//    for (int i = 0; i < myBar.Points.Count; i++)
		//    {
		//        TextObj barLabel = new TextObj(myBar.Points[i].Y.ToString("0.00"), myBar.Points[i].X + 1, myBar.Points[i].Y + 5);
		//        barLabel.FontSpec.Border.IsVisible = false;
		//        GP.GraphObjList.Add(barLabel);
		//    }
		//    myBar.Label.IsVisible = true;
		//    GP.Legend.IsVisible = false;
		//    GP.XAxis.Type = AxisType.Text;
		//    GP.XAxis.Scale.TextLabels = labels.ToArray();
		//    GP.XAxis.MajorTic.IsAllTics = false;
		//    zgcGlycan.AxisChange();
		//    zgcGlycan.Refresh();
		//    return (Image)zgcGlycan.MasterPane.GetImage();
		//}
		//public Image GetQuantitionImage(ref ZedGraph.ZedGraphControl argZedGraphControl, string argGKey, List<MatchedGlycanPeak> argMatchedScan)
		//{
		//    string Gkey = argGKey;
		//    ZedGraph.ZedGraphControl zgcGlycan = argZedGraphControl;
		//    zgcGlycan.Width = 2400;
		//    zgcGlycan.Height = 1200;

		//    GraphPane GP = zgcGlycan.GraphPane;
		//    GP.Title.Text = "Glycan: " + Gkey;
		//    GP.XAxis.Title.Text = "Labeling";
		//    GP.YAxis.Title.Text = "Abundance(%)";
		//    GP.CurveList.Clear();
		//    Dictionary<enumLabelingTag, double> dictLabelIntensity = new Dictionary<enumLabelingTag, double>();
		//    double YMax = 0;
		//    foreach (MatchedGlycanPeak p in argMatchedScan)
		//    {
		//        if (!dictLabelIntensity.ContainsKey(p.GlycanComposition.LabelingTag))
		//        {
		//            dictLabelIntensity.Add(p.GlycanComposition.LabelingTag, 0.0);
		//        }
		//        dictLabelIntensity[p.GlycanComposition.LabelingTag] = (p.CorrectedIntensity * _LabelingRatio[p.GlycanComposition.LabelingTag]) + dictLabelIntensity[p.GlycanComposition.LabelingTag];
		//        if (dictLabelIntensity[p.GlycanComposition.LabelingTag] > YMax)
		//        {
		//            YMax = dictLabelIntensity[p.GlycanComposition.LabelingTag];
		//        }
		//    }
		//    List<string> labels = new List<string>();
		//    PointPairList ppl = new PointPairList();
		//    if (_LabelingMethod == enumGlycanLabelingMethod.MultiplexPermethylated)
		//    {
		//        int i = 0;
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.MP_CH3) && dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CH3))
		//        {
		//            labels.Add("CH3");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CH3))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.MP_CH3] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//            i++;
		//        }
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.MP_CH2D) && dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CH2D))
		//        {
		//            labels.Add("CH2D");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CH2D))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.MP_CH2D] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//            i++;
		//        }
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.MP_CHD2) && dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CHD2))
		//        {
		//            labels.Add("CHD2");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CHD2))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.MP_CHD2] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//            i++;
		//        }
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.MP_CD3) && dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CD3))
		//        {
		//            labels.Add("CD3");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.MP_CD3))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.MP_CD3] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//            i++;
		//        }
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.MP_13CH3) && dictLabelIntensity.ContainsKey(enumLabelingTag.MP_13CH3))
		//        {
		//            labels.Add("13CH3");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.MP_13CH3))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.MP_13CH3] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//            i++;
		//        }
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.MP_13CHD2) && dictLabelIntensity.ContainsKey(enumLabelingTag.MP_13CHD2))
		//        {
		//            labels.Add("13CHD2");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.MP_13CHD2))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.MP_13CHD2] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//            i++;
		//        }
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.MP_13CD3) && dictLabelIntensity.ContainsKey(enumLabelingTag.MP_13CD3))
		//        {
		//            labels.Add("13CD3");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.MP_13CD3))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.MP_13CD3] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//        }
		//    }
		//    else if (_LabelingMethod == enumGlycanLabelingMethod.DRAG)
		//    {
		//        int i = 0;
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.DRAG_Light))
		//        {
		//            labels.Add("Light");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.DRAG_Light))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.DRAG_Light] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//            i++;
		//        }
		//        if (_LabelingRatio.ContainsKey(enumLabelingTag.DRAG_Heavy))
		//        {
		//            labels.Add("Heavy");
		//            if (dictLabelIntensity.ContainsKey(enumLabelingTag.DRAG_Heavy))
		//            {
		//                ppl.Add(i, dictLabelIntensity[enumLabelingTag.DRAG_Heavy] / YMax * 100);
		//            }
		//            else
		//            {
		//                ppl.Add(i, 0);
		//            }
		//        }
		//    }

		//    BarItem myBar = GP.AddBar("Data", ppl, Color.Red);
		//    myBar.Bar.Fill.Type = FillType.Solid;
		//    for (int i = 0; i < myBar.Points.Count; i++)
		//    {
		//        TextObj barLabel = new TextObj(myBar.Points[i].Y.ToString("0.00"), myBar.Points[i].X + 1, myBar.Points[i].Y + 5);
		//        barLabel.FontSpec.Border.IsVisible = false;
		//        GP.GraphObjList.Add(barLabel);
		//    }
		//    myBar.Label.IsVisible = true;
		//    GP.Legend.IsVisible = false;
		//    GP.XAxis.Type = AxisType.Text;
		//    GP.XAxis.Scale.TextLabels = labels.ToArray();
		//    GP.XAxis.MajorTic.IsAllTics = false;
		//    zgcGlycan.AxisChange();
		//    zgcGlycan.Refresh();
		//    return (Image)zgcGlycan.MasterPane.GetImage();
		//}
		//public void ExportToExcel()
		//{
		//    try
		//    {
		//        _MatchedPeaksInScan.Sort(delegate(MatchedGlycanPeak M1, MatchedGlycanPeak M2)
		//        {
		//            int r = M1.GlycanKey.CompareTo(M2.GlycanKey);
		//            if (r == 0) r = M1.ScanTime.CompareTo(M2.ScanTime);
		//            if (r == 0) r = M1.Peak.MonoIntensity.CompareTo(M2.Peak.MonoIntensity);
		//            return r;
		//        });

		//        Dictionary<string, List<MatchedGlycanPeak>> sortedScanRecords = new Dictionary<string, List<MatchedGlycanPeak>>();
		//        foreach (MatchedGlycanPeak MPeak in _MatchedPeaksInScan)
		//        {
		//            if (!sortedScanRecords.ContainsKey(MPeak.GlycanKey))
		//            {
		//                sortedScanRecords.Add(MPeak.GlycanKey, new List<MatchedGlycanPeak>());
		//            }
		//            sortedScanRecords[MPeak.GlycanKey].Add(MPeak);
		//        }

		//        _MergedResultList.Sort(delegate(ClusteredPeak CPeak1, ClusteredPeak CPeak2)
		//        {
		//            int r = CPeak1.GlycanKey.CompareTo(CPeak2.GlycanKey);
		//            if (r == 0) r = CPeak1.StartTime.CompareTo(CPeak2.StartTime);
		//            if (r == 0) r = CPeak1.MonoIntensity.CompareTo(CPeak2.MonoIntensity);
		//            return r = 0;
		//        });
		//        Dictionary<string, List<ClusteredPeak>> sortedMergeRecords = new Dictionary<string, List<ClusteredPeak>>();
		//        foreach (ClusteredPeak CPeak in _MergedResultList)
		//        {
		//            if (!sortedMergeRecords.ContainsKey(CPeak.GlycanKey))
		//            {
		//                sortedMergeRecords.Add(CPeak.GlycanKey, new List<ClusteredPeak>());
		//            }
		//            sortedMergeRecords[CPeak.GlycanKey].Add(CPeak);
		//        }

		//        int ColorIdx = 0;

		//        FileInfo NewFile = new FileInfo(_ExportFilePath);
		//        if (NewFile.Exists)
		//        {
		//            File.Delete(NewFile.FullName);
		//        }
		//        OfficeOpenXml.ExcelPackage pck = new OfficeOpenXml.ExcelPackage(NewFile);

		//        OfficeOpenXml.ExcelWorksheet CurrentSheet = pck.Workbook.Worksheets.Add("Parameters");
		//        CurrentSheet.Column(1).Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
		//        CurrentSheet.Column(1).Width = 35;
		//        CurrentSheet.Cells[1, 1].Value = "Raw File:";
		//        CurrentSheet.Cells[1, 2].Value = _rawFile;
		//        CurrentSheet.Cells[2, 1].Value = "Range:";
		//        CurrentSheet.Cells[2, 2].Value = _StartScan + "~" + _EndScan;
		//        CurrentSheet.Cells[3, 1].Value = "Glycan List:";
		//        CurrentSheet.Cells[3, 2].Value = _glycanFile;
		//        CurrentSheet.Cells[5, 1].Value = "Experiment Section";
		//        CurrentSheet.Cells[6, 1].Value = "Reduced Reducing End:";
		//        CurrentSheet.Cells[6, 2].Value = _isReducedReducingEnd.ToString();
		//        CurrentSheet.Cells[7, 1].Value = "Permethylated:";
		//        CurrentSheet.Cells[7, 2].Value = _isPermethylated.ToString();
		//        CurrentSheet.Cells[8, 1].Value = "Adduct:";

		//        string adduct = "";
		//        foreach (float add in _adductLabel.Keys)
		//        {
		//            adduct = adduct + add + "(" + _adductLabel[add] + ");";
		//        }
		//        CurrentSheet.Cells[8, 2].Value = adduct;

		//        CurrentSheet.Cells[9, 1].Value = "Mass tolerance (PPM):";
		//        CurrentSheet.Cells[9, 2].Value = _massPPM.ToString();

		//        CurrentSheet.Cells[10, 1].Value = "Include m/z match only peak:";
		//        CurrentSheet.Cells[10, 2].Value = _IncludeMZMatch.ToString();

		//        CurrentSheet.Cells[12, 1].Value = "Merge Section";

		//        CurrentSheet.Cells[13, 1].Value = "Max minute in front of LC apex  (a):";
		//        CurrentSheet.Cells[13, 2].Value = _maxLCFrontMin.ToString();

		//        CurrentSheet.Cells[14, 1].Value = "Max minute in back of LC apex  (b):";
		//        CurrentSheet.Cells[14, 2].Value = _maxLCBackMin.ToString();

		//        CurrentSheet.Cells[15, 1].Value = "Merge different charge glycan:";
		//        CurrentSheet.Cells[15, 2].Value = _MergeDifferentCharge.ToString();

		//        CurrentSheet.Cells[16, 1].Value = "Min length of LC Peak in minute (c):";
		//        CurrentSheet.Cells[16, 2].Value = _minLengthOfLC.ToString();

		//        CurrentSheet.Cells[17, 1].Value = "Minimum abundance:";
		//        CurrentSheet.Cells[17, 2].Value = _minAbundance.ToString();

		//        //CurrentSheet.Cells[19, 1].Value = "Peak processing parameters";
		//        //CurrentSheet.Cells[20, 1].Value = "Signal to noise ratio";
		//        //CurrentSheet.Cells[20, 2].Value = _peakParameter.SignalToNoiseThreshold.ToString();
		//        //CurrentSheet.Cells[21, 1].Value = "Peak background ratio";
		//        //CurrentSheet.Cells[21, 2].Value = _peakParameter.PeakBackgroundRatio.ToString();
		//        //CurrentSheet.Cells[22, 1].Value = "Use absolute peptide intensity";
		//        //CurrentSheet.Cells[22, 2].Value = _transformParameters.UseAbsolutePeptideIntensity.ToString();
		//        //if (_transformParameters.UseAbsolutePeptideIntensity)
		//        //{
		//        //    CurrentSheet.Cells[23, 1].Value = "Absolute peptide intensity";
		//        //    CurrentSheet.Cells[23, 2].Value = _transformParameters.AbsolutePeptideIntensity.ToString();
		//        //}
		//        //else
		//        //{
		//        //    CurrentSheet.Cells[23, 1].Value = "Peptide intensity ratio";
		//        //    CurrentSheet.Cells[23, 2].Value = _transformParameters.PeptideMinBackgroundRatio.ToString();
		//        //}

		//        //CurrentSheet.Cells[24, 1].Value = "Max charge";
		//        //CurrentSheet.Cells[24, 2].Value = _transformParameters.MaxCharge;

		//        System.Reflection.Assembly assm = typeof(frmMainESI).Assembly;
		//        System.Reflection.AssemblyName assmName = assm.GetName();
		//        Version ver = assmName.Version;
		//        CurrentSheet.Cells[27, 1].Value = "Program Version:";
		//        CurrentSheet.Cells[27, 2].Value = ver.ToString();

		//        CurrentSheet.Cells[28, 1].Value = "Process Time:";
		//        CurrentSheet.Cells[28, 2].Value = DateTime.Now.ToString();

		//        int OutputRowCount = 0;
		//        pck.Save();
		//        CurrentSheet.Dispose();
		//        ZedGraphControl zefControl = new ZedGraphControl();
		//        foreach (string Gkey in sortedMergeRecords.Keys)
		//        {
		//            OutputRowCount = 0;
		//            pck = new OfficeOpenXml.ExcelPackage(NewFile);
		//            CurrentSheet = pck.Workbook.Worksheets.Add(Gkey);

		//            var picture = CurrentSheet.Drawings.AddPicture(Gkey, GetLCImage(ref zefControl, Gkey, sortedScanRecords[Gkey]));
		//            picture.SetPosition(0, 0, 9, 0);
		//            picture.SetSize(1320, 660);
		//            CurrentSheet.Row(1).Height = 400;
		//            CurrentSheet.DefaultRowHeight = 50;

		//            OutputRowCount = 1;
		//            CurrentSheet.Cells[OutputRowCount, 1].Value = "Start Time";
		//            CurrentSheet.Cells[OutputRowCount, 2].Value = "End  Time";
		//            CurrentSheet.Cells[OutputRowCount, 3].Value = "Start Scan Num";
		//            CurrentSheet.Cells[OutputRowCount, 4].Value = "End Scan Num";
		//            CurrentSheet.Cells[OutputRowCount, 5].Value = "Sum Intensity";
		//            CurrentSheet.Cells[OutputRowCount, 6].Value = "Peak Area";
		//            CurrentSheet.Cells[OutputRowCount, 7].Value = "HexNac-Hex-deHex-Sia";
		//            OutputRowCount++;
		//            //Export Merge Result
		//            foreach (ClusteredPeak cls in sortedMergeRecords[Gkey])
		//            {
		//                if (_minLengthOfLC > (cls.EndTime - cls.StartTime) || cls.MonoIntensity < _minAbundance)
		//                {
		//                    continue;
		//                }
		//                CurrentSheet.Cells[OutputRowCount, 1].Value = cls.StartTime;
		//                CurrentSheet.Cells[OutputRowCount, 2].Value = cls.EndTime;
		//                CurrentSheet.Cells[OutputRowCount, 3].Value = cls.StartScan;
		//                CurrentSheet.Cells[OutputRowCount, 4].Value = cls.EndScan;
		//                CurrentSheet.Cells[OutputRowCount, 5].Value = cls.MonoIntensity;
		//                CurrentSheet.Cells[OutputRowCount, 6].Value = cls.PeakArea;

		//                if (cls.GlycanComposition != null)
		//                {
		//                    CurrentSheet.Cells[OutputRowCount, 7].Value = cls.GlycanComposition.NoOfHexNAc + "-" + cls.GlycanComposition.NoOfHex + "-" + cls.GlycanComposition.NoOfDeHex + "-" + cls.GlycanComposition.NoOfSia;

		//                }
		//                else
		//                {
		//                    CurrentSheet.Cells[OutputRowCount, 7].Value = "-";
		//                }
		//                OutputRowCount++;
		//            }

		//            //CurrentSheet.Row(OutputRowCount).Height = 30; //Empty Row
		//            //OutputRowCount++;
		//            //CurrentSheet.Cells[OutputRowCount, 1].Value = "Time";
		//            //CurrentSheet.Cells[OutputRowCount, 2].Value = "Scan Num";
		//            //CurrentSheet.Cells[OutputRowCount, 3].Value = "Abundance";
		//            //CurrentSheet.Cells[OutputRowCount, 4].Value = "m/z";
		//            //CurrentSheet.Cells[OutputRowCount, 5].Value = "HexNac-Hex-deHex-Sia";
		//            //CurrentSheet.Cells[OutputRowCount, 6].Value = "Adduct";
		//            //OutputRowCount++;
		//            //sortedScanRecords[Gkey].Sort(delegate(MatchedGlycanPeak M1, MatchedGlycanPeak M2)
		//            //{
		//            //    int r = M1.ScanTime.CompareTo(M2.ScanTime);
		//            //    if (r == 0) r = M1.Adduct.CompareTo(M2.Adduct);
		//            //    if (r == 0) r = M1.AdductCount.CompareTo(M2.AdductCount);
		//            //    return r;
		//            //});

		//            //foreach (MatchedGlycanPeak cls in sortedScanRecords[Gkey])
		//            //{
		//            //    CurrentSheet.Cells[OutputRowCount, 1].Value = cls.ScanTime;
		//            //    CurrentSheet.Cells[OutputRowCount, 2].Value = cls.ScanNum;
		//            //    CurrentSheet.Cells[OutputRowCount, 3].Value = cls.Peak.MonoIntensity;
		//            //    CurrentSheet.Cells[OutputRowCount, 4].Value = cls.Peak.MonoisotopicMZ;
		//            //    if (cls.GlycanComposition != null)
		//            //    {
		//            //        CurrentSheet.Cells[OutputRowCount, 5].Value = cls.GlycanComposition.NoOfHexNAc + "-" + cls.GlycanComposition.NoOfHex + "-" + cls.GlycanComposition.NoOfDeHex + "-" + cls.GlycanComposition.NoOfSia;
		//            //        CurrentSheet.Cells[OutputRowCount, 6].Value = cls.Adduct + "*" + cls.AdductCount;
		//            //    }
		//            //    else
		//            //    {
		//            //        CurrentSheet.Cells[OutputRowCount, 5].Value = "-";
		//            //        CurrentSheet.Cells[OutputRowCount, 6].Value = "-";
		//            //    }
		//            //    OutputRowCount++;
		//            //}
		//            pck.Save();
		//            pck.Stream.Close();
		//            CurrentSheet.Dispose();
		//            pck.Dispose();
		//        }

		//    }
		//    catch (Exception ex)
		//    {
		//        throw ex;
		//    }
		//}
		public List<float> GenerateCandidateGlycanMZList(List<GlycanCompound> argGlycanComList)
		{
			List<float> CandidateMzList = new List<float>();
			_dicCandidateGlycan = new Dictionary<float, List<GlycanCompound>>();
			foreach (GlycanCompound comp in argGlycanComList)
			{
				if (!_dicCandidateGlycan.ContainsKey(Convert.ToSingle(comp.MZ)))
				{
					_dicCandidateGlycan.Add(Convert.ToSingle(comp.MZ), new List<GlycanCompound>());
				}
				_dicCandidateGlycan[Convert.ToSingle(comp.MZ)].Add(comp);
				if (!CandidateMzList.Contains(Convert.ToSingle(comp.MZ)))
				{
					CandidateMzList.Add(Convert.ToSingle(comp.MZ));
				}
			}
			CandidateMzList.Sort();
			return CandidateMzList;
		}

		/// <summary>
		/// Old retired function
		/// </summary>
		/// <param name="argGlycanComList"></param>
		/// <returns></returns>
		//private List<float> GenerateCandidatePeakList(List<GlycanCompound> argGlycanComList)
		//{
		//    _lstCandidatePeak = new List<CandidatePeak>();
		//    List<float> CandidateMzList = new List<float>();
		//    _dicCandidatePeak = new Dictionary<float, List<CandidatePeak>>(); //CandidateMZ to Glycan
		//    foreach (GlycanCompound comp in argGlycanComList)
		//    {
		//        for (int i = 1; i <= _MaxCharge; i++) //Charge
		//        {
		//            foreach (float adductMass in _adductMass)
		//            {
		//                for (int j = 0; j <= i; j++) //Adduct Number
		//                {
		//                    string AdductLabel = _adductLabel[adductMass];
		//                    float AdductMass = adductMass;
		//                    CandidatePeak tmpCandidate = new CandidatePeak(comp, i, AdductMass, j, AdductLabel);
		//                    bool FoundSameGlycanKey;
		//                    //Add labeling glycan
		//                    if (_LabelingMethod != enumGlycanLabelingMethod.None)
		//                    {
		//                        foreach (enumLabelingTag LabelKey in _LabelingRatio.Keys)
		//                        {
		//                            if (j == 0)
		//                            {
		//                                AdductLabel = "H";
		//                                AdductMass = Atoms.ProtonMass;
		//                                tmpCandidate = new CandidatePeak(comp, i, AdductMass, 1, AdductLabel);
		//                            }
		//                            else
		//                            {
		//                                tmpCandidate = new CandidatePeak(comp, i, AdductMass, j, AdductLabel);
		//                            }
		//                            tmpCandidate.GlycanComposition.LabelingTag = LabelKey;
		//                            //If candidateMZ has the same value don't add into list;
		//                            if (!CandidateMzList.Contains(tmpCandidate.TotalMZ))
		//                            {
		//                                CandidateMzList.Add(tmpCandidate.TotalMZ);
		//                            }

		//                            //Inseet to dictionary <mz, List<candidates>>
		//                            if (!_dicCandidatePeak.ContainsKey(tmpCandidate.TotalMZ))
		//                            {
		//                                _dicCandidatePeak.Add(tmpCandidate.TotalMZ, new List<CandidatePeak>());
		//                            }
		//                            FoundSameGlycanKey = false;
		//                            foreach (CandidatePeak CP in _dicCandidatePeak[tmpCandidate.TotalMZ])
		//                            {
		//                                if (CP.GlycanKey == tmpCandidate.GlycanKey && CP.LabelingTag == tmpCandidate.LabelingTag) //Current Glycan already in List
		//                                {
		//                                    FoundSameGlycanKey = true;
		//                                    break;
		//                                }
		//                            }
		//                            if (!FoundSameGlycanKey)
		//                            {
		//                                ((List<CandidatePeak>)(_dicCandidatePeak[tmpCandidate.TotalMZ])).Add(tmpCandidate);
		//                                _lstCandidatePeak.Add(tmpCandidate);
		//                            }
		//                        }
		//                    }
		//                    else
		//                    {
		//                        if (j == 0)
		//                        {
		//                            AdductLabel = "H";
		//                            AdductMass = Atoms.ProtonMass;
		//                            tmpCandidate = new CandidatePeak(comp, i, AdductMass, 1, AdductLabel);
		//                        }

		//                        //If candidateMZ has the same value don't add into list;
		//                        if (!CandidateMzList.Contains(tmpCandidate.TotalMZ))
		//                        {
		//                            CandidateMzList.Add(tmpCandidate.TotalMZ);
		//                        }

		//                        //Inseet to dictionary <mz, List<candidates>>
		//                        if (!_dicCandidatePeak.ContainsKey(tmpCandidate.TotalMZ))
		//                        {
		//                            _dicCandidatePeak.Add(tmpCandidate.TotalMZ, new List<CandidatePeak>());
		//                        }
		//                        FoundSameGlycanKey = false;
		//                        foreach (CandidatePeak CP in _dicCandidatePeak[tmpCandidate.TotalMZ])
		//                        {
		//                            if (CP.GlycanKey == tmpCandidate.GlycanKey) //Current Glycan already in List
		//                            {
		//                                FoundSameGlycanKey = true;
		//                                break;
		//                            }
		//                        }
		//                        if (!FoundSameGlycanKey)
		//                        {
		//                            ((List<CandidatePeak>)(_dicCandidatePeak[tmpCandidate.TotalMZ])).Add(tmpCandidate);
		//                            _lstCandidatePeak.Add(tmpCandidate);
		//                        }
		//                    }

		//                }
		//            }
		//        }
		//    }
		//    //_lstCandidatePeak.Sort();
		//    CandidateMzList.Sort();
		//    return CandidateMzList;
		//}
		private List<MatchedGlycanPeak> FindClusterWGlycanList(MSScan argScan)
		{
			List<MatchedGlycanPeak> MatchedPeaks = new List<MatchedGlycanPeak>();
			List<string> MatchedGlycanKey = new List<string>();
			List<GlycanCompound> FoundGlycanCompounds = new List<GlycanCompound>();
			//First round search in High confidence peak list
			Dictionary<float, List<MatchedGlycanPeak>> tmpDuplicateGlycan = new Dictionary<float, List<MatchedGlycanPeak>>();
			foreach (GlycanCompound Candidate in _GlycanList)
			{
				if (argScan.MZs.Length == 0)
				{
					continue;
				}

				int ClosedIdx = MassLib.MassUtility.GetClosestMassIdx(argScan.MZs, Convert.ToSingle(Candidate.MZ));
				if (ClosedIdx == -1 ||
					MassLib.MassUtility.GetMassPPM(argScan.MZs[ClosedIdx], Candidate.MZ) > MassPPM ||
					(argScan.Intensities[ClosedIdx] / argScan.MaxIntensity) * 100 < _minPeakHeightPrecentage)
				{
					continue;
				}
				//Find in m/z
				List<int> Peak = FindPeakIdx(argScan.MZs, ClosedIdx, Candidate.Charge);
				if (Peak.Count < _MinIsotopePeakCount) //Peak Count is not enough
				{
					continue;
				}
				if (Peak[0] != ClosedIdx) //Not First Mono
				{
					continue;
				}
				if (_isMatchMonoPeakOnly && ClosedIdx != Peak[0])
				{
					continue;
				}
				MSPeak msp;
				float maxIntensity = 0;
				int maxIntensityIdx = 0;
				double TotalIntensity = 0;
				List<MSPoint> MSPs = new List<MSPoint>();
				for (int j = 0; j < Peak.Count; j++)
				{
					TotalIntensity = TotalIntensity + argScan.Intensities[Peak[j]];
					MSPs.Add(new MSPoint(argScan.MZs[Peak[j]], argScan.Intensities[Peak[j]]));
					if (argScan.Intensities[Peak[j]] > maxIntensity)
					{
						maxIntensity = argScan.Intensities[Peak[j]];
						maxIntensityIdx = Peak[j];
					}
				}
				float Mass = (argScan.MZs[Peak[0]] - MassLib.Atoms.ProtonMass) * Candidate.Charge;
				msp = new MSPeak(Mass, argScan.Intensities[Peak[0]], Candidate.Charge, argScan.MZs[ClosedIdx], 0, argScan.MZs[maxIntensityIdx], argScan.Intensities[maxIntensityIdx], TotalIntensity);
				MatchedGlycanPeak newMatchedPeak = new MatchedGlycanPeak(argScan.ScanNo, argScan.Time, msp, Candidate);
				newMatchedPeak.MSPoints = MSPs;
				MatchedPeaks.Add(newMatchedPeak);
				FoundGlycanCompounds.Add(Candidate);
				if (!tmpDuplicateGlycan.ContainsKey(argScan.MZs[ClosedIdx]))
				{
					tmpDuplicateGlycan.Add(argScan.MZs[ClosedIdx], new List<MatchedGlycanPeak>());
				}
				tmpDuplicateGlycan[argScan.MZs[ClosedIdx]].Add(newMatchedPeak);
				if (!MatchedGlycanKey.Contains(Candidate.GlycanKey))
				{
					MatchedGlycanKey.Add(Candidate.GlycanKey);
				}
			}
			foreach (float mzKey in tmpDuplicateGlycan.Keys)
			{
				if (tmpDuplicateGlycan[mzKey].Count > 1)
				{
					if (!_OnePeakTwoGlycan.ContainsKey(tmpDuplicateGlycan[mzKey][0].GlycanKey))
					{
						_OnePeakTwoGlycan.Add(tmpDuplicateGlycan[mzKey][0].GlycanKey, new List<MatchedGlycanPeak>());
					}
					_OnePeakTwoGlycan[tmpDuplicateGlycan[mzKey][0].GlycanKey].AddRange(tmpDuplicateGlycan[mzKey]);
				}
			}
			//Second round glycan in low raw mz list with already match glycan
			//foreach (GlycanCompound Candidate in _GlycanList)
			//{
			//    if (!MatchedGlycanKey.Contains(Candidate.GlycanKey) || FoundGlycanCompounds.Contains(Candidate))
			//    {
			//        continue;
			//    }
			//    int ClosedIdx = MassLib.MassUtility.GetClosestMassIdx(argScan.RawMZs, Convert.ToSingle(Candidate.MZ));
			//    if (MassLib.MassUtility.GetMassPPM(argScan.RawMZs[ClosedIdx], Candidate.MZ) > _massPPM)
			//    {
			//        continue;
			//    }

			//    List<int> Peak = FindPeakIdx(argScan.RawMZs, ClosedIdx, Candidate.Charge);

			//    if (_isMatchMonoPeakOnly && ClosedIdx != Peak[0])
			//    {
			//        continue;
			//    }
			//    if (Candidate.GlycanKey == "4-4-0-1-0")
			//    {
			//        Console.WriteLine("aaa");
			//    }
			//    MSPeak msp;
			//    float maxIntensity = 0;
			//    int maxIntensityIdx = 0;
			//    double TotalIntensity = 0;
			//    List<MSPoint> MSPs = new List<MSPoint>();
			//    for (int j = 0; j < Peak.Count; j++)
			//    {
			//        TotalIntensity = TotalIntensity + argScan.RawIntensities[Peak[j]];
			//        MSPs.Add(new MSPoint(argScan.RawMZs[Peak[j]], argScan.RawIntensities[Peak[j]]));
			//        if (argScan.RawIntensities[Peak[j]] > maxIntensity)
			//        {
			//            maxIntensity = argScan.RawIntensities[Peak[j]];
			//            maxIntensityIdx = Peak[j];
			//        }
			//    }
			//    if (MSPs.Count < _MinPeakCount)
			//    {
			//        continue;
			//    }
			//    float Mass = (argScan.RawMZs[Peak[0]] - MassLib.Atoms.ProtonMass) * Candidate.Charge;
			//    msp = new MSPeak(Mass, argScan.RawIntensities[Peak[0]], Candidate.Charge, argScan.RawMZs[ClosedIdx], 0, argScan.RawMZs[maxIntensityIdx], argScan.RawIntensities[maxIntensityIdx], TotalIntensity);
			//    MatchedGlycanPeak newMatchedPeak = new MatchedGlycanPeak(argScan.ScanNo, argScan.Time, msp, Candidate);
			//    newMatchedPeak.MSPoints = MSPs;
			//    MatchedPeaks.Add(newMatchedPeak);
			//}
			return MatchedPeaks;
		}

		private MSPeak FindPeak(List<MSPeak> argMSPeaks, double argMZ)
		{
			double closedDistance = (argMSPeaks.Min(n => Math.Abs(argMZ - n.MonoisotopicMZ)));
			return argMSPeaks.First(n => Math.Abs(argMZ - n.MonoisotopicMZ) == closedDistance);
		}

		private List<int> FindPeakIdx(float[] argMZAry, int argTargetIdx, int argCharge)
		{
			List<int> Peak = new List<int>();
			float Interval = 1 / (float)argCharge;
			float FirstMZ = argMZAry[argTargetIdx];
			int CurrentIdx = argTargetIdx;
			Peak.Add(argTargetIdx);
			//Forward  Peak
			for (int i = argTargetIdx - 1; i >= 0; i--)
			{
				if (argMZAry[argTargetIdx] - argMZAry[i] >= Interval * 10)
				{
					break;
				}
				List<int> ClosedPeaks = MassLib.MassUtility.GetClosestMassIdxsWithinPPM(argMZAry, argMZAry[CurrentIdx] - Interval, _IsotopePPM);
				if (ClosedPeaks.Count == 1)
				{
					CurrentIdx = ClosedPeaks[0];
					Peak.Insert(0, ClosedPeaks[0]);
				}
				else if (ClosedPeaks.Count > 1)
				{
					double minPPM = 100;
					int minPPMIdx = 0;
					for (int j = 0; j < ClosedPeaks.Count; j++)
					{
						if (MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] - Interval) < minPPM)
						{
							minPPMIdx = ClosedPeaks[j];
							minPPM = MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval);
						}
					}
					CurrentIdx = minPPMIdx;
					Peak.Insert(0, CurrentIdx);
				}
			}
			//Backward  Peak
			CurrentIdx = argTargetIdx;
			for (int i = argTargetIdx + 1; i < argMZAry.Length; i++)
			{
				if (argMZAry[i] - argMZAry[argTargetIdx] >= Interval * 10)
				{
					break;
				}
				List<int> ClosedPeaks = MassLib.MassUtility.GetClosestMassIdxsWithinPPM(argMZAry, argMZAry[CurrentIdx] + Interval, _IsotopePPM);
				if (ClosedPeaks.Count == 1)
				{
					CurrentIdx = ClosedPeaks[0];
					Peak.Add(ClosedPeaks[0]);
				}
				else if (ClosedPeaks.Count > 1)
				{
					double minPPM = 100;
					int minPPMIdx = 0;
					for (int j = 0; j < ClosedPeaks.Count; j++)
					{
						if (MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval) < minPPM)
						{
							minPPMIdx = ClosedPeaks[j];
							minPPM = MassLib.MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval);
						}
					}
					CurrentIdx = minPPMIdx;
					Peak.Add(CurrentIdx);
				}
			}

			return Peak;
		}

		private List<MatchedGlycanPeak> FindClusterWGlycanList(List<MSPeak> argPeaks, int argScanNum, double argTime)
		{
			//List<ClusteredPeak> ClsPeaks = new List<ClusteredPeak>(); //Store all cluster in this scan
			List<MatchedGlycanPeak> MatchedPeaks = new List<MatchedGlycanPeak>();
			List<MSPeak> SortedPeaks = argPeaks;
			SortedPeaks.Sort(delegate (MSPeak P1, MSPeak P2) { return Comparer<double>.Default.Compare(P1.MonoisotopicMZ, P2.MonoisotopicMZ); });

			foreach (MSPeak p in SortedPeaks)
			{
				//PeakMZ.Add(p.MonoisotopicMZ);
				//int ClosedPeakIdx = MassLib.MassUtility.GetClosestMassIdx(_candidateMzList,p.MonoisotopicMZ);
				//List<CandidatePeak> ClosedPeaks = _dicCandidatePeak[_candidateMzList[ClosedPeakIdx]];
				//foreach (CandidatePeak ClosedPeak in ClosedPeaks)
				List<int> ClosedPeaksIdxs = MassLib.MassUtility.GetClosestMassIdxsWithinPPM(_candidateMzList, p.MonoisotopicMZ, (float)MassPPM);
				foreach (int ClosedPeakIdx in ClosedPeaksIdxs)
				{
					List<GlycanCompound> ClosedGlycans = _dicCandidateGlycan[_candidateMzList[ClosedPeakIdx]];
					foreach (GlycanCompound ClosedGlycan in ClosedGlycans)
					{
						if (p.ChargeState == ClosedGlycan.Charge &&
							Math.Abs(MassLib.MassUtility.GetMassPPM(ClosedGlycan.MZ, p.MonoisotopicMZ)) <= MassPPM)
						{
							MatchedGlycanPeak MatchedGlycanP = new MatchedGlycanPeak(argScanNum, argTime, p, ClosedGlycan);
							MatchedPeaks.Add(MatchedGlycanP);
							/*ClusteredPeak tmpPeak = new ClusteredPeak(argScanNum);
                            tmpPeak.EndScan = argScanNum;
                            tmpPeak.StartTime = argTime;
                            tmpPeak.EndTime = argTime;
                            tmpPeak.Charge = ClosedPeak.Charge;
                            tmpPeak.GlycanComposition = ClosedPeak.GlycanComposition;
                            tmpPeak.Peaks.Add(p);
                            tmpPeak.Adduct = ClosedPeak.AdductLabel;
                            ClsPeaks.Add(tmpPeak);*/
						}
					}
				}
			}

			//foreach (GlycanCompound comp in _GlycanList)
			//{
			//    float[] GlycanMZ = new float[_MaxCharge + 1]; // GlycanMZ[1] = charge 1; GlycanMZ[2] = charge 2
			//    for (int i = 1; i <= _MaxCharge; i++)
			//    {
			//        GlycanMZ[i] = (float)(comp.MonoMass + MassLib.Atoms.ProtonMass * i) / (float)i;
			//    }
			//    for (int i = 1; i <= _MaxCharge; i++)
			//    {
			//        int ClosedPeak = MassLib.MassUtility.GetClosestMassIdx(PeakMZ, GlycanMZ[i]);
			//        int ChargeState = Convert.ToInt32(SortedPeaks[ClosedPeak].ChargeState);
			//        if (ChargeState == 0 || ChargeState != i ||
			//            (MassLib.MassUtility.GetClosestMassIdx(PeakMZ, GlycanMZ[i]) == 0 && PeakMZ[0] - GlycanMZ[i] > 10.0f) ||
			//            (MassLib.MassUtility.GetClosestMassIdx(PeakMZ, GlycanMZ[i]) == PeakMZ.Count - 1 && GlycanMZ[i] - PeakMZ[PeakMZ.Count - 1] > 10.0f))
			//        {
			//            continue;
			//        }
			//        else
			//        {
			//            //GetMassPPM(SortedPeaks[ClosedPeak].MonoisotopicMZ,GlycanMZ[i])> _glycanPPM
			//            /// Cluster of glycan
			//            /// Z = 1 [M+H]     [M+NH4]
			//            /// Z = 2 [M+2H]   [M+NH4+H]	    [M+2NH4]
			//            /// Z = 3 [M+3H]	[M+NH4+2H]	[M+2NH4+H] 	[M+3NH4]
			//            /// Z = 4 [M+4H]	[M+NH4+3H]	[M+2NH4+2H]	[M+3NH4+H]	[M+4NH4]
			//            if (_adductMass.Count == 0)
			//            {
			//                _adductMass.Add(0.0f);
			//            }
			//            foreach (float adductMass in _adductMass)
			//            {
			//                float[] Step = new float[ChargeState + 1];
			//                //Step[0] = GlycanMZ[i];
			//                for (int j = 0; j <= ChargeState; j++)
			//                {
			//                    Step[j] = (GlycanMZ[1] + adductMass * j) / ChargeState;
			//                }
			//                int[] PeakIdx = new int[Step.Length];
			//                for (int j = 0; j < PeakIdx.Length; j++)
			//                {
			//                    PeakIdx[j] = -1;
			//                }
			//                for (int j = 0; j < PeakIdx.Length; j++)
			//                {
			//                    int ClosedPeak2 = Convert.ToInt32(MassLib.MassUtility.GetClosestMassIdx(PeakMZ, Step[j]));
			//                    if (GetMassPPM(PeakMZ[ClosedPeak2], Step[j]) < _massPPM)
			//                    {
			//                        PeakIdx[j] = ClosedPeak2;
			//                    }
			//                }
			//                ClusteredPeak Cls = new ClusteredPeak(argScanNum);
			//                for (int j = 0; j < PeakIdx.Length; j++)
			//                {
			//                    if (PeakIdx[j] != -1)
			//                    {
			//                        Cls.Peaks.Add(SortedPeaks[PeakIdx[j]]);
			//                    }
			//                }
			//                if (Cls.Peaks.Count > 0)
			//                {
			//                    Cls.StartTime = argTime;
			//                    Cls.EndTime = argTime;
			//                    Cls.Charge = i;
			//                    Cls.GlycanCompostion = comp;
			//                    Cls.AdductMass = adductMass;
			//                    if (!ClsPeaks.Contains(Cls))
			//                    {
			//                        ClsPeaks.Add(Cls);
			//                    }
			//                }
			//            }
			//        }
			//    }
			//}
			return MatchedPeaks;
		}

		//private List<MatchedGlycanPeak> FindClusterWOGlycanList(List<MSPeak> argPeaks, int argScanNum, double argTime)
		//{
		//    List<MatchedGlycanPeak> ClsPeaks = new List<MatchedGlycanPeak>();
		//    List<MSPeak> SortedPeaks = argPeaks;
		//    SortedPeaks.Sort(delegate(MSPeak P1, MSPeak P2) { return Comparer<double>.Default.Compare(P1.MonoisotopicMZ, P2.MonoisotopicMZ); });

		//    if (_adductMass.Count == 0)
		//    {
		//        _adductMass.Add(0.0f);
		//    }
		//    for (int i = 0; i < SortedPeaks.Count; i++)
		//    {
		//        /// Cluster of glycan
		//        /// Z = 1 [M+H]     [M+NH4]
		//        /// Z = 2 [M+2H]   [M+NH4+H]	    [M+2NH4]
		//        /// Z = 3 [M+3H]	[M+NH4+2H]	[M+2NH4+H] 	[M+3NH4]
		//        /// Z = 4 [M+4H]	[M+NH4+3H]	[M+2NH4+2H]	[M+3NH4+H]	[M+4NH4]
		//        //Create cluster interval
		//        foreach (float adductMass in _adductMass)
		//        {
		//            double[] Step = new double[Convert.ToInt32(SortedPeaks[i].ChargeState) + 1];
		//            //double NH3 = MassLib.Atoms.NitrogenMass + 3 * MassLib.Atoms.HydrogenMass;
		//            Step[0] = SortedPeaks[i].MonoisotopicMZ;
		//            for (int j = 1; j <= SortedPeaks[i].ChargeState; j++)
		//            {
		//                Step[j] = Step[j - 1] + (adductMass) / SortedPeaks[i].ChargeState;
		//            }
		//            int[] PeakIdx = new int[Step.Length];
		//            PeakIdx[0] = i;
		//            for (int j = 1; j < PeakIdx.Length; j++)
		//            {
		//                PeakIdx[j] = -1;
		//            }
		//            int CurrentMatchIdx = 1;
		//            for (int j = i + 1; j < SortedPeaks.Count; j++)
		//            {
		//                if (SortedPeaks[i].ChargeState != SortedPeaks[j].ChargeState)
		//                {
		//                    continue;
		//                }
		//                for (int k = CurrentMatchIdx; k < Step.Length; k++)
		//                {
		//                    if (GetMassPPM(Step[k], SortedPeaks[j].MonoisotopicMZ) < _massPPM)
		//                    {
		//                        PeakIdx[k] = j;
		//                        CurrentMatchIdx = k + 1;
		//                        break;
		//                    }
		//                }
		//            }
		//            //FIX
		//            //Cluster status check
		//            //ClusteredPeak Cls = new ClusteredPeak(argScanNum);
		//            //for (int j = 0; j < PeakIdx.Length; j++)
		//            //{
		//            //    if (PeakIdx[j] != -1)
		//            //    {
		//            //        Cls.Peaks.Add(SortedPeaks[PeakIdx[j]]);
		//            //    }
		//            //}
		//            //Cls.StartTime = argTime;
		//            //Cls.EndTime = argTime;
		//            //Cls.Charge = SortedPeaks[i].ChargeState;
		//            //Cls.Adduct = _adductLabel[adductMass];
		//            //if (!ClsPeaks.Contains(Cls))
		//            //{
		//            //    ClsPeaks.Add(Cls);
		//            //}
		//        }
		//    }
		//    return ClsPeaks;
		//}
		/// <summary>
		/// Merge Multiple scan into one cluser by glycan composition and charge
		/// </summary>
		/// <param name="argDurationMin"></param>
		/// <returns></returns>
		public void MergeCluster()
		{
			//List<ClusteredPeak> MergedClusterForAllKeys = new List<ClusteredPeak>();
			MergedResultList = new List<ClusteredPeak>();  //Store Result
			Dictionary<string, List<MatchedGlycanPeak>> dictAllPeak = new Dictionary<string, List<MatchedGlycanPeak>>();  //KEY: GlycanKey;GlycanKey+Charge;,GlycanKey+LabelTag; GlycanKey+charge + LabelTag
																														  //List<string> GlycanWProton = new List<string>(); //Store Glycan with Proton adduct
			for (int i = 0; i < MatchedPeakInScan.Count; i++)
			{
				string key = "";
				if (_MergeDifferentCharge)
				{
					if (_LabelingMethod != enumGlycanLabelingMethod.None)
					{
						key = MatchedPeakInScan[i].GlycanKey + "-" + MatchedPeakInScan[i].GlycanComposition.LabelingTag;
					}
					else
					{
						key = MatchedPeakInScan[i].GlycanKey;
					}
				}
				else
				{
					if (_LabelingMethod != enumGlycanLabelingMethod.None)
					{
						key = MatchedPeakInScan[i].GlycanKey + "-" + MatchedPeakInScan[i].Charge + "-" + MatchedPeakInScan[i].GlycanComposition.LabelingTag;
					}
					else
					{
						key = MatchedPeakInScan[i].GlycanKey + "-" +
										 MatchedPeakInScan[i].Charge;
					}
				}
				if (!dictAllPeak.ContainsKey(key))
				{
					dictAllPeak.Add(key, new List<MatchedGlycanPeak>());
				}
				dictAllPeak[key].Add(MatchedPeakInScan[i]);
				//if (_MatchedPeaksInScan[i].Adduct == "H" && !GlycanWProton.Contains(key))
				//{
				//    GlycanWProton.Add(key);
				//}
			}
			foreach (string KEY in dictAllPeak.Keys)
			{
				//if (!GlycanWProton.Contains(KEY))  //Skip identified glycans without Proton adduct;
				//{
				//    continue;
				//}
				List<MatchedGlycanPeak> AllPeaksWithSameGlycan = dictAllPeak[KEY];

				//if (AllPeaksWithSameGlycan[AllPeaksWithSameGlycan.Count - 1].ScanTime - AllPeaksWithSameGlycan[0].ScanTime <= _maxLCFrontMin)
				//{   //All peaks within duration
				//mergedPeak = (ClusteredPeak)CLSPeaks[0].Clone();

				//Sum up intensity
				Dictionary<double, double> SumIntensity = new Dictionary<double, double>();
				foreach (MatchedGlycanPeak MatchedPeak in AllPeaksWithSameGlycan)
				{
					if (!SumIntensity.ContainsKey(MatchedPeak.ScanTime))
					{
						SumIntensity.Add(MatchedPeak.ScanTime, 0.0f);
					}
					SumIntensity[MatchedPeak.ScanTime] = SumIntensity[MatchedPeak.ScanTime] + MatchedPeak.MostIntenseIntensity;
				}
				List<MSPoint> lstMSPs = new List<MSPoint>();
				foreach (double time in SumIntensity.Keys)
				{
					lstMSPs.Add(new MSPoint(Convert.ToSingle(time), Convert.ToSingle(SumIntensity[time])));
				}
				lstMSPs.Sort(delegate (MSPoint p1, MSPoint p2) { return p1.Mass.CompareTo(p2.Mass); });

				//Smooth
				List<MassLib.MSPoint> lstSmoothPnts = new List<MassLib.MSPoint>();
				lstSmoothPnts = MassLib.Smooth.SavitzkyGolay.Smooth(lstMSPs, MassLib.Smooth.SavitzkyGolay.FILTER_WIDTH.FILTER_WIDTH_7);

				//Peak Finding
				List<MassLib.LCPeak> lcPk = null;
				lcPk = MassLib.LCPeakDetection.PeakFinding(lstSmoothPnts, 0.1f, 0.01f);

				//Create Result Peak
				for (int i = 0; i < lcPk.Count; i++)
				{
					ClusteredPeak MergedPeak = new ClusteredPeak();
					MergedPeak.LCPeak = lcPk[i];
					foreach (MatchedGlycanPeak ClusterPeak in AllPeaksWithSameGlycan)
					{
						if (ClusterPeak.ScanTime >= lcPk[i].StartTime && ClusterPeak.ScanTime <= lcPk[i].EndTime)
						{
							MergedPeak.MatchedPeaksInScan.Add(ClusterPeak);
						}
					}
					if (_ApplyLinearRegLC)
					{
						float expectedLCTime = 0.0f;
						bool LCTimeWithinTolerance = false;
						if (MergedPeak.GlycanComposition.HasLinearRegressionParameters)
						{
							expectedLCTime = (float)MergedPeak.GlycanComposition.MonoMass * MergedPeak.GlycanComposition.LinearRegSlope +
											 MergedPeak.GlycanComposition.LinearRegIntercept;
							if (Math.Abs(expectedLCTime - MergedPeak.LCPeak.Apex.Mass) <= _LCTimeTolerance * _totalLCTime)
							{
								LCTimeWithinTolerance = true;
							}
						}
						else
						{
							//No LC parameters
							LCTimeWithinTolerance = true;
						}

						if (MergedPeak.IsotopicClusterIntensity > _minAbundance &&
						MergedPeak.TimeInterval >= _maxLCBackMin &&
						 MergedPeak.TimeInterval < _maxLCFrontMin &&
						  MergedPeak.MatchedPeaksInScan.Count >= _minLengthOfLC &&
						   LCTimeWithinTolerance)
						{
							MergedResultList.Add(MergedPeak);
						}
					}
					else
					{
						if (MergedPeak.IsotopicClusterIntensity > _minAbundance &&
						MergedPeak.TimeInterval >= _maxLCBackMin &&
						 MergedPeak.TimeInterval < _maxLCFrontMin &&
						  MergedPeak.MatchedPeaksInScan.Count >= _minLengthOfLC)
						{
							MergedResultList.Add(MergedPeak);
						}
					}
				}
				//}
				//else //Split into multiple clusters because exceed Max LC time
				//{
				//    //int ScanCount = 0;

				//    List<string> ScanInterval = new List<string>();
				//    int StartScanIdx = 0;

				//    for (int i = 1; i < AllPeaksWithSameGlycan.Count; i++)
				//    {
				//        if (AllPeaksWithSameGlycan[i].ScanTime - AllPeaksWithSameGlycan[i-1].ScanTime > 0.1)  //Merge Scan within 0.1 min
				//        {
				//            ScanInterval.Add(StartScanIdx.ToString() + "-" + (i - 1).ToString());
				//            StartScanIdx = i;
				//        }
				//        //if (MergedPeak == null)
				//        //{
				//        //    //mergedPeak = (ClusteredPeak)CLSPeaks[i].Clone();
				//        //    MergedPeak = new ClusteredPeak();

				//        //    ScanCount = 1;
				//        //    continue;
				//        //}
				//        //if (CLSPeaks[i].ScanTime - mergedPeak.EndTime < 1.0)
				//        //{
				//        //    mergedPeak.EndTime = CLSPeaks[i].ScanTime;
				//        //    mergedPeak.EndScan = CLSPeaks[i].ScanNum;
				//        //    ScanCount++;
				//        //}
				//        //else //New Cluster
				//        //{
				//        //    double timeinterval = mergedPeak.EndTime - mergedPeak.StartTime;
				//        //    if (mergedPeak.IsotopicClusterIntensity > _minAbundance &&
				//        //        timeinterval > _maxLCBackMin &&
				//        //        timeinterval < _maxLCFrontMin &&
				//        //        ScanCount > _minLengthOfLC
				//        //        )
				//        //    {
				//        //        _MergedResultList.Add(mergedPeak);
				//        //    }
				//        //    //mergedPeak = (ClusteredPeak)CLSPeaks[i].Clone();
				//        //    mergedPeak.Adduct = CLSPeaks[i].Adduct;
				//        //    ScanCount = 1;
				//        //}
				//    }
				//    ScanInterval.Add(StartScanIdx.ToString() + "-" + (AllPeaksWithSameGlycan.Count - 1).ToString());
				//    foreach (string str in ScanInterval)
				//    {
				//        int StrScan = Convert.ToInt32(str.Split('-')[0]);
				//        int EndScan = Convert.ToInt32(str.Split('-')[1]);
				//        ClusteredPeak MergedPeak = new ClusteredPeak();
				//        for (int i = StrScan; i <= EndScan; i++)
				//        {
				//            MergedPeak.MatchedPeaksInScan.Add(AllPeaksWithSameGlycan[i]);
				//        }
				//        if (MergedPeak.IsotopicClusterIntensity > _minAbundance &&
				//             MergedPeak.TimeInterval >= _maxLCBackMin &&
				//              MergedPeak.TimeInterval < _maxLCFrontMin &&
				//              MergedPeak.MatchedPeaksInScan.Count >= _minLengthOfLC)
				//               {
				//                                    _MergedResultList.Add(MergedPeak);
				//               }
				//    }
				//    //if (_MergedResultList.Count > 1 && _MergedResultList[_MergedResultList.Count - 1] != mergedPeak) //Add last Cluster into result
				//    //{
				//    //    double timeinterval = mergedPeak.EndTime - mergedPeak.StartTime;
				//    //    if (mergedPeak.IsotopicClusterIntensity > _minAbundance &&
				//    //        timeinterval > _maxLCBackMin &&
				//    //         timeinterval < _maxLCFrontMin &&
				//    //         ScanCount > _minLengthOfLC)
				//    //    {
				//    //        _MergedResultList.Add(mergedPeak);
				//    //    }
				//    //}
				//}
			}
			//_MergedResultList = MergedCluster;
		}

		public void MergeSingleScanResultToPeak()
		{
			if (_2ndPassedPeaksInScan != null)
			{
				MatchedPeakInScan.AddRange(_2ndPassedPeaksInScan);
			}
			MergedResultList = new List<ClusteredPeak>();  //Store Result
			Dictionary<string, List<MatchedGlycanPeak>> dictAllPeak = new Dictionary<string, List<MatchedGlycanPeak>>();  //KEY: GlycanKey ot GlycanKey+Charge
			List<string> GlycanWProton = new List<string>(); //Store Glycan key with Proton adduct
			MatchedPeakInScan.RemoveAll(item => item == null);
			int error_i = 0;
			try
			{
				for (int i = 0; i < MatchedPeakInScan.Count; i++)
				{
					error_i = i;
					string key = "";
					if (_MergeDifferentCharge)
					{
						if (_LabelingMethod != enumGlycanLabelingMethod.None)
						{
							key = MatchedPeakInScan[i].GlycanKey + "-" + MatchedPeakInScan[i].GlycanComposition.LabelingTag;
						}
						else
						{
							key = MatchedPeakInScan[i].GlycanKey;
						}
					}
					else
					{
						if (_LabelingMethod != enumGlycanLabelingMethod.None)
						{
							key = MatchedPeakInScan[i].GlycanKey + "-" + MatchedPeakInScan[i].Charge + "-" + MatchedPeakInScan[i].GlycanComposition.LabelingTag;
						}
						else
						{
							key = MatchedPeakInScan[i].GlycanKey + "-" +
											 MatchedPeakInScan[i].Charge;
						}
					}
					if (!dictAllPeak.ContainsKey(key))
					{
						dictAllPeak.Add(key, new List<MatchedGlycanPeak>());
					}
					dictAllPeak[key].Add(MatchedPeakInScan[i]);
					if (MatchedPeakInScan[i].AdductString.Contains("H "))
					{
						GlycanWProton.Add(key);
					}
				}
				foreach (string KEY in dictAllPeak.Keys)
				{
					if (_forceProtonatedGlycan && !GlycanWProton.Contains(KEY))  //Skip identified glycans without Proton adduct;
					{
						continue;
					}
					List<MatchedGlycanPeak> AllPeaksWithSameGlycan = dictAllPeak[KEY];
					AllPeaksWithSameGlycan.Sort(delegate (MatchedGlycanPeak Peak1, MatchedGlycanPeak Peak2)
					{
						return Peak1.ScanTime.CompareTo(Peak2.ScanTime);
					});
					Dictionary<double, double> MergeIntensity = new Dictionary<double, double>();
					List<double> Time = new List<double>();

					//Merge Intensity
					foreach (MatchedGlycanPeak MGlycanPeak in AllPeaksWithSameGlycan)
					{
						if (!MergeIntensity.ContainsKey(MGlycanPeak.ScanTime))
						{
							MergeIntensity.Add(MGlycanPeak.ScanTime, 0);
						}
						MergeIntensity[MGlycanPeak.ScanTime] = MergeIntensity[MGlycanPeak.ScanTime] + MGlycanPeak.Peak.MonoIntensity;

						if (!Time.Contains(MGlycanPeak.ScanTime))
						{
							Time.Add(MGlycanPeak.ScanTime);
						}
					}

					Time.Sort();
					double[] ArryIntesity = new double[Time.Count];
					double[] ArryTime = Time.ToArray();
					for (int i = 0; i < Time.Count; i++)
					{
						ArryIntesity[i] = MergeIntensity[Time[i]];
					}

					List<double[]> PeaksTime = new List<double[]>();
					List<double[]> PeaksIntensity = new List<double[]>();

					do
					{
						//Iter to find peak
						int MaxIdx = FindMaxIdx(ArryIntesity);
						int PeakStart = MaxIdx;
						int PeakEnd = MaxIdx;
						//PeakStartPoint
						while (PeakStart > 0)
						{
							//0.5  Two MS scan Max Interval
							if (ArryTime[PeakStart] - ArryTime[PeakStart - 1] < 0.5 && ArryTime[MaxIdx] - ArryTime[PeakStart] < _maxLCFrontMin)
							{
								PeakStart = PeakStart - 1;
							}
							else
							{
								break;
							}
						}

						//PeakEndPoint
						while (PeakEnd < ArryTime.Length - 1)
						{
							if (ArryTime[PeakEnd + 1] - ArryTime[PeakEnd] < 0.5 && ArryTime[PeakEnd] - ArryTime[MaxIdx] < _maxLCBackMin)
							{
								PeakEnd = PeakEnd + 1;
							}
							else
							{
								break;
							}
						}

						//Peak Array
						double[] PeakTime = new double[PeakEnd - PeakStart + 1];
						double[] PeakInt = new double[PeakEnd - PeakStart + 1];
						Array.Copy(ArryTime, PeakStart, PeakTime, 0, PeakEnd - PeakStart + 1);
						Array.Copy(ArryIntesity, PeakStart, PeakInt, 0, PeakEnd - PeakStart + 1);
						//Store Peaks
						PeaksTime.Add(PeakTime);
						PeaksIntensity.Add(PeakInt);
						//MergeRest
						int SizeOfRestArray = ArryTime.Length - PeakEnd + PeakStart - 1;
						double[] NewArryTime = new double[SizeOfRestArray];
						double[] NewArryIntensity = new double[SizeOfRestArray];
						Array.Copy(ArryTime, 0, NewArryTime, 0, PeakStart);
						Array.Copy(ArryTime, PeakEnd + 1, NewArryTime, PeakStart, ArryTime.Length - 1 - PeakEnd);
						Array.Copy(ArryIntesity, 0, NewArryIntensity, 0, PeakStart);
						Array.Copy(ArryIntesity, PeakEnd + 1, NewArryIntensity, PeakStart, ArryTime.Length - 1 - PeakEnd);

						ArryTime = NewArryTime;
						ArryIntesity = NewArryIntensity;
					} while (ArryTime.Length != 0);

					List<ClusteredPeak> MergedPeaks = new List<ClusteredPeak>();
					for (int i = 0; i < PeaksTime.Count; i++)
					{
						ClusteredPeak tmpMergedPeak = new ClusteredPeak();

						List<double> PeakTimeCount = new List<double>();
						for (int j = 0; j < PeaksTime[i].Length; j++)
						{
							tmpMergedPeak.MatchedPeaksInScan.AddRange(FindGlycanIdxInGlycanList(AllPeaksWithSameGlycan, PeaksTime[i][j]));
							if (!PeakTimeCount.Contains(PeaksTime[i][j]))
							{
								PeakTimeCount.Add(PeaksTime[i][j]);
							}
						}
						if (tmpMergedPeak.MatchedPeaksInScan.Count < 3 || PeakTimeCount.Count < 3)
						{
							continue;
						}
						if (_ApplyLinearRegLC)
						{
							float expectedLCTime = 0.0f;
							bool LCTimeWithinTolerance = false;
							if (tmpMergedPeak.GlycanComposition.HasLinearRegressionParameters)
							{
								expectedLCTime = (float)tmpMergedPeak.GlycanComposition.MonoMass * tmpMergedPeak.GlycanComposition.LinearRegSlope +
												 tmpMergedPeak.GlycanComposition.LinearRegIntercept;
								tmpMergedPeak.CalcLCPeak();
								if (Math.Abs(expectedLCTime - tmpMergedPeak.LCPeak.Apex.Mass) <= _LCTimeTolerance * _totalLCTime)
								{
									LCTimeWithinTolerance = true;
								}
							}
							else
							{
								//No LC parameters
								LCTimeWithinTolerance = true;
							}

							if (tmpMergedPeak.MonoIntensity >= _minAbundance &&
								tmpMergedPeak.TimeInterval >= _minLengthOfLC &&
							   LCTimeWithinTolerance)
							{
								MergedPeaks.Add(tmpMergedPeak);
							}
						}
						else
						{
							if (tmpMergedPeak.MonoIntensity >= _minAbundance &&
						   tmpMergedPeak.TimeInterval >= _minLengthOfLC)
							{
								MergedPeaks.Add(tmpMergedPeak);
							}
						}
					}
					//Add peaks without proton adduct but within range
					//foreach (MatchedGlycanPeak MGlycanPeak in AllPeaksWithSameGlycan)
					//{
					//    ProtonAdductPeak = MGlycanPeak.AdductString.Contains("H ");
					//    if (!ProtonAdductPeak)
					//    {
					//        foreach (ClusteredPeak MPeak in MergedPeaks)
					//        {
					//            if (MPeak.StartTime <= MGlycanPeak.ScanTime &&
					//                MGlycanPeak.ScanTime <= MPeak.EndTime &&
					//                MPeak.GlycanKey == MGlycanPeak.GlycanKey)
					//            {
					//                MPeak.MatchedPeaksInScan.Add(MGlycanPeak);
					//            }
					//        }
					//    }
					//}

					if (_forceProtonatedGlycan)
					{
						foreach (ClusteredPeak gPeak in MergedPeaks)
						{
							foreach (MatchedGlycanPeak g in gPeak.MatchedPeaksInScan)
							{
								if (g.AdductString.Contains("H ") && g.GlycanComposition.Adducts.Count == 1 &&
									!MergedResultList.Contains(gPeak))
								{
									MergedResultList.Add(gPeak);
									break;
								}
							}
						}
					}
					else
					{
						MergedResultList.AddRange(MergedPeaks);
					}

					//ExportGlycanToExcel(KEY, AllPeaksWithSameGlycan, MergedPeaks);
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Module:MergeToSingle  " + error_i + "  " + ex);
			}
		}

		public void SolveDuplicateAssignment()
		{
			//StreamWriter sw = new StreamWriter(@"D:\duplicateGlycan.csv");
			//foreach (string GKey in _OnePeakTwoGlycan.Keys)
			//{
			//    sw.WriteLine(GKey);
			//    foreach (MatchedGlycanPeak gpeak in _OnePeakTwoGlycan[GKey])
			//    {
			//        sw.WriteLine("," + gpeak.ScanNum + "," + gpeak.ScanTime + "," + gpeak.GlycanKey + "," + gpeak.AdductString + "," + gpeak.MSPoints[0].Mass+","+ gpeak.Charge);
			//    }
			//}
			//sw.Close();
			foreach (string GKey in _OnePeakTwoGlycan.Keys)
			{
				List<MatchedGlycanPeak> lstGlycan = _OnePeakTwoGlycan[GKey];
				List<MatchedGlycanPeak> duplicateGlycan = new List<MatchedGlycanPeak>();
				duplicateGlycan.Add(lstGlycan[0]);
				bool isPeakCollected = false;
				for (int i = 1; i < lstGlycan.Count; i++)
				{
					if (duplicateGlycan[0].ScanNum != lstGlycan[i].ScanNum || duplicateGlycan[0].MSPoints[0].Mass != lstGlycan[i].MSPoints[0].Mass)
					{
						isPeakCollected = true;
					}
					else
					{
						duplicateGlycan.Add(lstGlycan[i]);
						if (i == lstGlycan.Count - 1) //For last cluster
						{
							isPeakCollected = true;
						}
					}

					if (isPeakCollected)
					{
						List<double> DistanceToProtonPeak = new List<double>();
						for (int j = 0; j < duplicateGlycan.Count; j++)
						{
							List<ClusteredPeak> clsPeaks = MergedResultList.Where(a => a.GlycanKey == duplicateGlycan[j].GlycanKey).ToList();

							double ClosedProtonTime = 100;
							foreach (ClusteredPeak cls in clsPeaks)
							{
								foreach (MatchedGlycanPeak gPeak in cls.MatchedPeaksInScan.Where(a => a.GlycanComposition.Adducts.Any(g => g.Item1.Equals("H")) && a.GlycanComposition.Adducts.Count == 1).ToList())
								{
									if (ClosedProtonTime > Math.Abs(gPeak.ScanTime - duplicateGlycan[j].ScanTime))
									{
										ClosedProtonTime = Math.Abs(gPeak.ScanTime - duplicateGlycan[j].ScanTime);
									}
								}
							}
							DistanceToProtonPeak.Add(ClosedProtonTime);
						}
						double SmallestDistance = DistanceToProtonPeak.Min();
						for (int k = 0; k < DistanceToProtonPeak.Count; k++)
						{
							if (DistanceToProtonPeak[k] != SmallestDistance)
							{
								MatchedPeakInScan.Remove(duplicateGlycan[k]);
								List<ClusteredPeak> clsPeaks = MergedResultList.Where(a => a.GlycanKey == duplicateGlycan[k].GlycanKey).ToList();
								foreach (ClusteredPeak cls in clsPeaks)
								{
									cls.MatchedPeaksInScan.Remove(duplicateGlycan[k]);
									if (cls.MatchedPeaksInScan.Count == 0)
									{
										MergedResultList.Remove(cls);
									}
								}
							}
						}
						DistanceToProtonPeak.Clear();
						duplicateGlycan.Clear();
						duplicateGlycan.Add(lstGlycan[i]);
						isPeakCollected = false;
					}
				}
			}
		}

		private List<MatchedGlycanPeak> FindGlycanIdxInGlycanList(List<MatchedGlycanPeak> argMatchGlycans, double argTime)
		{
			List<MatchedGlycanPeak> MatchedPeaks = new List<MatchedGlycanPeak>();
			for (int i = 0; i < argMatchGlycans.Count; i++)
			{
				if (argMatchGlycans[i].ScanTime == argTime)
				{
					MatchedPeaks.Add(argMatchGlycans[i]);
				}
			}
			return MatchedPeaks;
		}

		private int FindMaxIdx(double[] argArry)
		{
			int MaxIdx = 0;
			for (int i = 1; i < argArry.Length; i++)
			{
				if (argArry[i] > argArry[MaxIdx])
				{
					MaxIdx = i;
				}
			}
			return MaxIdx;
		}

		public void Merge2PassedCluster()
		{
			if (_2ndPassedPeaksInScan == null)
			{
				return;
			}
			MergedResultList = new List<ClusteredPeak>();  //Store Result
			Dictionary<string, List<MatchedGlycanPeak>> dictAllPeak = new Dictionary<string, List<MatchedGlycanPeak>>();  //KEY: GlycanKey ot GlycanKey+Charge
			List<string> GlycanWProton = new List<string>(); //Store Glycan with Proton adduct
			MatchedPeakInScan.AddRange(_2ndPassedPeaksInScan);

			for (int i = 0; i < MatchedPeakInScan.Count; i++)
			{
				string key = "";
				if (_MergeDifferentCharge)
				{
					key = MatchedPeakInScan[i].GlycanKey;
				}
				else
				{
					key = MatchedPeakInScan[i].GlycanKey + "-" +
									 MatchedPeakInScan[i].Charge;
				}
				if (!dictAllPeak.ContainsKey(key))
				{
					dictAllPeak.Add(key, new List<MatchedGlycanPeak>());
				}
				dictAllPeak[key].Add(MatchedPeakInScan[i]);
				//if (_MatchedPeaksInScan[i].Adduct == "H" && !GlycanWProton.Contains(key))
				//{
				//    GlycanWProton.Add(key);
				//}
			}
			foreach (string KEY in dictAllPeak.Keys)
			{
				//if (!GlycanWProton.Contains(KEY))  //Skip identified glycans without Proton adduct;
				//{
				//    continue;
				//}
				List<MatchedGlycanPeak> AllPeaksWithSameGlycan = dictAllPeak[KEY];

				//if (AllPeaksWithSameGlycan[AllPeaksWithSameGlycan.Count - 1].ScanTime - AllPeaksWithSameGlycan[0].ScanTime <= _maxLCFrontMin)
				//{   //All peaks within duration
				//mergedPeak = (ClusteredPeak)CLSPeaks[0].Clone();

				//Sum up intensity
				Dictionary<double, double> SumIntensity = new Dictionary<double, double>();
				foreach (MatchedGlycanPeak MatchedPeak in AllPeaksWithSameGlycan)
				{
					if (!SumIntensity.ContainsKey(MatchedPeak.ScanTime))
					{
						SumIntensity.Add(MatchedPeak.ScanTime, 0.0f);
					}
					SumIntensity[MatchedPeak.ScanTime] = SumIntensity[MatchedPeak.ScanTime] + MatchedPeak.MostIntenseIntensity;
				}
				List<MSPoint> lstMSPs = new List<MSPoint>();
				foreach (double time in SumIntensity.Keys)
				{
					if (Convert.ToSingle(SumIntensity[time]) != 0)
					{
						lstMSPs.Add(new MSPoint(Convert.ToSingle(time), Convert.ToSingle(SumIntensity[time])));
					}
				}
				lstMSPs.Sort(delegate (MSPoint p1, MSPoint p2) { return p1.Mass.CompareTo(p2.Mass); });

				//Smooth
				List<MassLib.MSPoint> lstSmoothPnts = new List<MassLib.MSPoint>();
				lstSmoothPnts = MassLib.Smooth.SavitzkyGolay.Smooth(lstMSPs, MassLib.Smooth.SavitzkyGolay.FILTER_WIDTH.FILTER_WIDTH_7);

				//Peak Finding
				List<MassLib.LCPeak> lcPk = null;
				lcPk = MassLib.LCPeakDetection.PeakFinding(lstSmoothPnts, 0.1f, 0.01f);

				//Create Result Peak
				for (int i = 0; i < lcPk.Count; i++)
				{
					ClusteredPeak MergedPeak = new ClusteredPeak();
					MergedPeak.LCPeak = lcPk[i];
					foreach (MatchedGlycanPeak ClusterPeak in AllPeaksWithSameGlycan)
					{
						if (ClusterPeak.ScanTime >= lcPk[i].StartTime && ClusterPeak.ScanTime <= lcPk[i].EndTime)
						{
							MergedPeak.MatchedPeaksInScan.Add(ClusterPeak);
						}
					}
					if (MergedPeak.IsotopicClusterIntensity > _minAbundance &&
						MergedPeak.TimeInterval >= _maxLCBackMin &&
						 MergedPeak.TimeInterval < _maxLCFrontMin &&
						  MergedPeak.MatchedPeaksInScan.Count >= _minLengthOfLC)
					{
						MergedResultList.Add(MergedPeak);
					}
				}
			}

			//_Merged2ndPassedResultList = new List<ClusteredPeak>();
			//Dictionary<string, List<ClusteredPeak>> dictClusteredPeak = new Dictionary<string, List<ClusteredPeak>>();
			////Create Key
			//foreach (ClusteredPeak ClusteredPeak in _MergedResultList)
			//{
			//    string key = ClusteredPeak.GlycanKey;
			//    if (!_MergeDifferentCharge)
			//    {
			//        key = ClusteredPeak.GlycanKey + "-" +
			//                         ClusteredPeak.MatchedPeaksInScan[0].Charge.ToString();
			//    }
			//    if (!dictClusteredPeak.ContainsKey(key))
			//    {
			//        dictClusteredPeak.Add(key, new List<ClusteredPeak>());
			//    }
			//    dictClusteredPeak[key].Add(ClusteredPeak);
			//}
			//Dictionary<string, List<MatchedGlycanPeak>> dict2ndPassedPeaks = new Dictionary<string, List<MatchedGlycanPeak>>();

			//foreach (MatchedGlycanPeak MatchedPeaks in _2ndPassedPeaksInScan)
			//{
			//    string key = MatchedPeaks.GlycanKey;
			//    if (!_MergeDifferentCharge)
			//    {
			//        key = MatchedPeaks.GlycanKey + "-" +
			//                         MatchedPeaks.Charge.ToString();
			//    }
			//    if (!dict2ndPassedPeaks.ContainsKey(key))
			//    {
			//        dict2ndPassedPeaks.Add(key, new List<MatchedGlycanPeak>());
			//    }
			//    dict2ndPassedPeaks[key].Add(MatchedPeaks);
			//}
			//foreach (string key in dict2ndPassedPeaks.Keys)
			//{
			//    List<MatchedGlycanPeak> MGPeaks = dict2ndPassedPeaks[key];

			//    if (!dictClusteredPeak.ContainsKey(key))
			//    {
			//        //New Glycan
			//        Dictionary<double, double> SumIntensity = new Dictionary<double, double>();
			//        foreach (MatchedGlycanPeak MatchedPeak in MGPeaks)
			//        {
			//            if (!SumIntensity.ContainsKey(MatchedPeak.ScanTime))
			//            {
			//                SumIntensity.Add(MatchedPeak.ScanTime, 0.0f);
			//            }
			//            SumIntensity[MatchedPeak.ScanTime] = SumIntensity[MatchedPeak.ScanTime] + MatchedPeak.Peak.MonoIntensity;
			//        }
			//        List<MSPoint> lstMSPs = new List<MSPoint>();
			//        foreach (double time in SumIntensity.Keys)
			//        {
			//            lstMSPs.Add(new MSPoint(Convert.ToSingle(time), Convert.ToSingle(SumIntensity[time])));
			//        }
			//        lstMSPs.Sort(delegate(MSPoint p1, MSPoint p2) { return p1.Mass.CompareTo(p2.Mass); });

			//        //Smooth
			//        List<MassLib.MSPoint> lstSmoothPnts = new List<MassLib.MSPoint>();
			//        lstSmoothPnts = MassLib.Smooth.SavitzkyGolay.Smooth(lstMSPs, MassLib.Smooth.SavitzkyGolay.FILTER_WIDTH.FILTER_WIDTH_7);

			//        //Peak Finding
			//        List<MassLib.LCPeak> lcPk = null;
			//        lcPk = MassLib.LCPeakDetection.PeakFinding(lstSmoothPnts, 0.1f, 0.01f);
			//        lcPk.Sort(delegate(LCPeak p1, LCPeak p2) { return p1.StartTime.CompareTo(p2.StartTime); });

			//        ClusteredPeak tmpMergedPeak = new ClusteredPeak(MGPeaks[0]);
			//        if (lcPk.Count > 1)
			//        {
			//            tmpMergedPeak.LCPeak = lcPk[0];
			//            for (int i = 1; i < lcPk.Count; i++)
			//            {
			//                if (lcPk[i].StartTime - tmpMergedPeak.LCPeak.EndTime <= 2.5)
			//                {
			//                    tmpMergedPeak.LCPeak = MergeLCPeak(tmpMergedPeak.LCPeak, lcPk[i]);
			//                }
			//                else
			//                {
			//                    foreach (MatchedGlycanPeak ClusterPeak in MGPeaks)
			//                    {
			//                        if (ClusterPeak.ScanTime >= tmpMergedPeak.StartTime && ClusterPeak.ScanTime <= tmpMergedPeak.EndTime)
			//                        {
			//                            tmpMergedPeak.MatchedPeaksInScan.Add(ClusterPeak);
			//                        }
			//                    }

			//                    _MergedResultList.Add(tmpMergedPeak);

			//                    tmpMergedPeak = new ClusteredPeak();
			//                    tmpMergedPeak.LCPeak = lcPk[i];

			//                }

			//            }
			//            foreach (MatchedGlycanPeak ClusterPeak in MGPeaks)
			//            {
			//                if (ClusterPeak.ScanTime >= tmpMergedPeak.StartTime && ClusterPeak.ScanTime <= tmpMergedPeak.EndTime)
			//                {
			//                    tmpMergedPeak.MatchedPeaksInScan.Add(ClusterPeak);
			//                }
			//            }
			//            _MergedResultList.Add(tmpMergedPeak);
			//        }
			//        ////Create Result Peak
			//        //for (int i = 0; i < lcPk.Count; i++)
			//        //{
			//        //    ClusteredPeak MergedPeak = new ClusteredPeak();
			//        //    MergedPeak.LCPeak = lcPk[i];
			//        //    foreach (MatchedGlycanPeak ClusterPeak in MGPeaks)
			//        //    {
			//        //        if (ClusterPeak.ScanTime >= lcPk[i].StartTime && ClusterPeak.ScanTime <= lcPk[i].EndTime)
			//        //        {
			//        //            MergedPeak.MatchedPeaksInScan.Add(ClusterPeak);
			//        //        }
			//        //    }
			//        //    if (MergedPeak.IsotopicClusterIntensity > _minAbundance &&
			//        //        MergedPeak.TimeInterval >= _maxLCBackMin &&
			//        //         MergedPeak.TimeInterval < _maxLCFrontMin &&
			//        //          MergedPeak.MatchedPeaksInScan.Count >= _minLengthOfLC)
			//        //    {
			//        //        _MergedResultList.Add(MergedPeak);
			//        //    }
			//        //}

			//    }
			//    else
			//    {
			//        //List<int> MergeStartAndEndIdx = new List<int>();
			//        List<float> MGPeaksTime = new List<float>();
			//        List<int> MGPeaksFirstIdx = new List<int>();
			//        for (int i = 0; i < MGPeaks.Count; i++)
			//        {
			//            if (MGPeaksTime.Count != 0)
			//            {
			//                if (Convert.ToSingle(Math.Round(MGPeaks[i].ScanTime, 5)) != MGPeaksTime[MGPeaksTime.Count - 1])
			//                {
			//                    MGPeaksTime.Add(Convert.ToSingle(Math.Round(MGPeaks[i].ScanTime, 5)));
			//                    MGPeaksFirstIdx.Add(i);
			//                }
			//            }
			//            else
			//            {
			//                MGPeaksTime.Add(Convert.ToSingle(Math.Round(MGPeaks[i].ScanTime, 5)));
			//                MGPeaksFirstIdx.Add(i);
			//            }
			//        }

			//        List<ClusteredPeak> TargetPeaks = dictClusteredPeak[key];
			//        List<ClusteredPeak> MergedPeaks = new List<ClusteredPeak>();
			//        for (int i = 0; i < TargetPeaks.Count; i++)
			//        {
			//            double StartTime = TargetPeaks[i].StartTime;
			//            //ExtendFront
			//            int ExtendFrontEndIdx = 0;
			//            for (int j = 0; j < MGPeaksTime.Count; j++)
			//            {
			//                if (StartTime - MGPeaksTime[j] < 0)
			//                {
			//                    ExtendFrontEndIdx = j-1;
			//                    break;
			//                }
			//            }
			//            int ExtendFrontStartIdx = ExtendFrontEndIdx;
			//            if (ExtendFrontEndIdx <= 0)
			//            {
			//                ExtendFrontEndIdx = 0;
			//                ExtendFrontStartIdx = 0;
			//            }
			//            else
			//            {
			//                while (true)
			//                {
			//                    if (ExtendFrontStartIdx == 0)
			//                    {
			//                        ExtendFrontStartIdx = 0;
			//                        break;
			//                    }
			//                    if (MGPeaksTime[ExtendFrontStartIdx] - MGPeaksTime[ExtendFrontStartIdx - 1] < 0.25)
			//                    {
			//                        ExtendFrontStartIdx = ExtendFrontStartIdx - 1;
			//                    }
			//                    else
			//                    {
			//                        break;
			//                    }
			//                }
			//            }
			//            //ExtendBack
			//            double EndTime = TargetPeaks[i].EndTime;
			//            int ExtendBackStartIdx = 0;
			//            for (int j = 0; j < MGPeaksTime.Count; j++)
			//            {
			//                if (EndTime - MGPeaksTime[j] <= 0)
			//                {
			//                    ExtendBackStartIdx = j;
			//                    break;
			//                }
			//            }
			//            int ExtendBackEndIdx = ExtendBackStartIdx;
			//            while (ExtendBackEndIdx<MGPeaksTime.Count)
			//            {
			//                if (ExtendBackEndIdx == MGPeaksTime.Count-1)
			//                {
			//                    ExtendBackEndIdx = MGPeaksTime.Count - 1;
			//                    break;
			//                }
			//                if (MGPeaksTime[ExtendBackEndIdx + 1] - MGPeaksTime[ExtendBackEndIdx] < 0.25)
			//                {
			//                    ExtendBackEndIdx = ExtendBackEndIdx + 1;

			//                }
			//                else
			//                {
			//                    break;
			//                }
			//            }
			//            //Check
			//            if (i > 0) //Check Front
			//            {
			//                double ExtendStartTime = MGPeaksTime[ExtendFrontStartIdx];
			//                if (MergedPeaks[i- 1].EndTime - ExtendStartTime >= 0)
			//                {
			//                    //Revice ExtendStartIndex;
			//                    do
			//                    {
			//                        ExtendFrontStartIdx = ExtendFrontStartIdx + 1;
			//                        if (MGPeaksTime[ExtendFrontStartIdx] - MergedPeaks[i - 1].EndTime > 0)
			//                        {
			//                            break;
			//                        }
			//                    } while (true);
			//                }
			//            }
			//            if (i != TargetPeaks.Count - 1) //Check Back
			//            {
			//                double ExtendEndTime = MGPeaksTime[ExtendBackEndIdx];
			//                if (ExtendEndTime - TargetPeaks[i + 1].StartTime >= 0)
			//                {
			//                    //Revice ExtendBackEndIndex;
			//                    do
			//                    {
			//                        ExtendBackEndIdx = ExtendBackEndIdx - 1;
			//                        if (TargetPeaks[i + 1].StartTime - MGPeaksTime[ExtendBackEndIdx] > 0)
			//                        {
			//                            break;
			//                        }
			//                    } while (true);
			//                }
			//            }
			//            //CreateNewPeaks
			//            ClusteredPeak NewClustered = new ClusteredPeak();
			//            //Add front peak
			//            for (int j = 0; j < MGPeaks.Count; j++)
			//            {
			//                if (MGPeaksTime[ExtendFrontStartIdx] < MGPeaks[j].ScanTime && MGPeaks[j].ScanTime < MGPeaksTime[ExtendFrontEndIdx])
			//                {
			//                    NewClustered.MatchedPeaksInScan.Add(MGPeaks[j]);
			//                }
			//                if (MGPeaks[j].ScanTime > MGPeaksTime[ExtendFrontEndIdx])
			//                {
			//                    break;
			//                }
			//            }
			//            //Add original peak
			//            NewClustered.MatchedPeaksInScan.AddRange(TargetPeaks[i].MatchedPeaksInScan);
			//            //Add back peak
			//            for (int j = 0; j < MGPeaks.Count; j++)
			//            {
			//                if (MGPeaksTime[ExtendBackStartIdx] <= MGPeaks[j].ScanTime && MGPeaks[j].ScanTime <= MGPeaksTime[ExtendBackEndIdx])
			//                {
			//                    NewClustered.MatchedPeaksInScan.Add(MGPeaks[j]);
			//                }
			//                if (MGPeaks[j].ScanTime > MGPeaksTime[ExtendBackEndIdx])
			//                {
			//                    break;
			//                }
			//            }
			//            MergedPeaks.Add(NewClustered);
			//        }
			//        //Merger Clusted Peak
			//        List<ClusteredPeak> MergedMergedPeak = new List<ClusteredPeak>();
			//        while(MergedPeaks.Count!=0)
			//        {
			//            int LastIdx = MergedPeaks.Count - 1;

			//            if (MergedPeaks.Count > 1)
			//            {
			//                if (MergedPeaks[LastIdx].StartTime - MergedPeaks[LastIdx - 1].EndTime < 0.25)
			//                {
			//                    //Merge
			//                    MergedPeaks[LastIdx - 1].MatchedPeaksInScan.AddRange(MergedPeaks[LastIdx].MatchedPeaksInScan);
			//                    MergedPeaks.RemoveAt(LastIdx);
			//                }
			//                else //move to previous clusterpeak
			//                {
			//                    MergedMergedPeak.Add(MergedPeaks[LastIdx]);
			//                    MergedPeaks.RemoveAt(LastIdx);
			//                }
			//            }
			//            else
			//            {
			//                MergedMergedPeak.Add(MergedPeaks[0]);
			//                MergedPeaks.RemoveAt(0);
			//            }

			//        }
			//        MergedMergedPeak.Reverse();
			//        _Merged2ndPassedResultList.AddRange(MergedMergedPeak);
			//    }
			//}
		}

		private LCPeak MergeLCPeak(LCPeak Pk1, LCPeak PK2)
		{
			LCPeak First = Pk1;
			LCPeak Second = PK2;
			if (Pk1.StartTime > PK2.StartTime)
			{
				First = PK2;
				Second = Pk1;
			}
			LCPeak Merged = First;
			Merged.RawPoint.AddRange(Second.RawPoint);
			return Merged;
		}

		private void UpdateLCPeak(ref ClusteredPeak argClusterPeak)
		{
			Dictionary<double, double> dicMSPs = new Dictionary<double, double>();
			List<MSPoint> lstMSPs = new List<MSPoint>();
			foreach (MatchedGlycanPeak MatchedPeak in argClusterPeak.MatchedPeaksInScan)
			{
				if (dicMSPs.ContainsKey(MatchedPeak.ScanTime))
				{
					dicMSPs[MatchedPeak.ScanTime] = dicMSPs[MatchedPeak.ScanTime] + MatchedPeak.Peak.MonoIntensity;
				}
				else
				{
					dicMSPs.Add(MatchedPeak.ScanTime, MatchedPeak.Peak.MonoIntensity);
				}
			}

			foreach (double key in dicMSPs.Keys)
			{
				lstMSPs.Add(new MSPoint(Convert.ToSingle(key), Convert.ToSingle(dicMSPs[key])));
			}
			lstMSPs.Sort(delegate (MSPoint p1, MSPoint p2) { return p1.Mass.CompareTo(p2.Mass); });

			//Smooth
			List<MassLib.MSPoint> lstSmoothPnts = new List<MassLib.MSPoint>();
			lstSmoothPnts = MassLib.Smooth.SavitzkyGolay.Smooth(lstMSPs, MassLib.Smooth.SavitzkyGolay.FILTER_WIDTH.FILTER_WIDTH_7);

			//Peak Finding
			List<MassLib.LCPeak> lcPk = null;
			lcPk = MassLib.LCPeakDetection.PeakFinding(lstSmoothPnts, 0.1f, 0.01f);
			MassLib.LCPeak MergedLCPeak = new LCPeak(lcPk[0].StartTime, lcPk[lcPk.Count - 1].EndTime, lcPk[0].RawPoint);
			if (lcPk.Count != 1)
			{
				for (int i = 1; i < lcPk.Count; i++)
				{
					MergedLCPeak.RawPoint.AddRange(lcPk[i].RawPoint);
				}
			}
			argClusterPeak.LCPeak = MergedLCPeak;

			//for (int i = 0; i < lcPk.Count; i++)
			//{
			//    ClusteredPeak MergedPeak = new ClusteredPeak();
			//    MergedPeak.LCPeak = lcPk[i];

			//    if (MergedPeak.IsotopicClusterIntensity > _minAbundance &&
			//        MergedPeak.TimeInterval >= _maxLCBackMin &&
			//         MergedPeak.TimeInterval < _maxLCFrontMin &&
			//          MergedPeak.MatchedPeaksInScan.Count >= _minLengthOfLC)
			//    {
			//        argClusterPeak = MergedPeak;
			//    }
			//}
		}

		///// <summary>
		/////
		///// </summary>
		///// <param name="argDurationMin"></param>
		///// <returns></returns>
		//public static List<ClusteredPeak> MergeCluster(List<ClusteredPeak> argCLU, double argDurationMin)
		//{
		//    List<ClusteredPeak> MergedCluster = new List<ClusteredPeak>();
		//    List<ClusteredPeak> _cluPeaks = argCLU;
		//    Dictionary<string, List<ClusteredPeak>> dictAllPeak = new Dictionary<string, List<ClusteredPeak>>();
		//    Dictionary<string, double> dictPeakIntensityMax = new Dictionary<string, double>();
		//    for (int i = 0; i < _cluPeaks.Count; i++)
		//    {
		//        string key = _cluPeaks[i].GlycanCompostion.NoOfHexNAc.ToString() +"-"+
		//                            _cluPeaks[i].GlycanCompostion.NoOfHex.ToString() + "-" +
		//                            _cluPeaks[i].GlycanCompostion.NoOfDeHex.ToString() + "-" +
		//                            _cluPeaks[i].GlycanCompostion.NoOfSia.ToString() + "-" +
		//                            _cluPeaks[i].Charge.ToString();
		//        if (!dictAllPeak.ContainsKey(key))
		//        {
		//            dictAllPeak.Add(key, new List<ClusteredPeak>());
		//            dictPeakIntensityMax.Add(key, _cluPeaks[i].Intensity);
		//        }
		//        dictAllPeak[key].Add(_cluPeaks[i]);
		//        if (_cluPeaks[i].Intensity > dictPeakIntensityMax[key])
		//        {
		//            dictPeakIntensityMax[key] = _cluPeaks[i].Intensity;
		//        }
		//    }

		//    foreach (string KEY in dictAllPeak.Keys)
		//    {
		//        List<ClusteredPeak> CLSPeaks = dictAllPeak[KEY];
		//        double threshold = Math.Sqrt(dictPeakIntensityMax[KEY]);
		//        ClusteredPeak mergedPeak =null;
		//        for(int i =0 ; i< CLSPeaks.Count;i++)
		//        {
		//            if (CLSPeaks[i].Intensity < threshold)
		//            {
		//                continue;
		//            }
		//            if (mergedPeak == null)
		//            {
		//                mergedPeak = (ClusteredPeak)CLSPeaks[i].Clone();
		//                mergedPeak.MergedIntensity = CLSPeaks[i].Intensity;
		//                continue;
		//            }
		//            if (CLSPeaks[i].StartTime - mergedPeak.EndTime < 1.0)
		//            {
		//                mergedPeak.EndTime = CLSPeaks[i].StartTime;
		//                mergedPeak.EndScan = CLSPeaks[i].StartScan;
		//                mergedPeak.MergedIntensity = mergedPeak.MergedIntensity + CLSPeaks[i].Intensity;
		//            }
		//            else
		//            {
		//                MergedCluster.Add(mergedPeak);
		//                mergedPeak = (ClusteredPeak)CLSPeaks[i].Clone();
		//                mergedPeak.MergedIntensity = CLSPeaks[i].Intensity;
		//            }
		//        }
		//        if (MergedCluster[MergedCluster.Count - 1] != mergedPeak)
		//        {
		//            MergedCluster.Add(mergedPeak);
		//        }
		//    }
		//    return MergedCluster;
		//}
		public static double GetMassPPM(double argExactMass, double argMeasureMass)
		{
			return Math.Abs(Convert.ToDouble(((argMeasureMass - argExactMass) / argExactMass) * Math.Pow(10.0, 6.0)));
		}

		public void ReadGlycanList()
		{
			_GlycanList = new List<GlycanCompound>();
			List<GlycanCompound> tmps = new List<GlycanCompound>();
			tmps = COL.GlycoLib.ReadGlycanListFromFile.ReadGlycanList(GlycanFile,
																															   IsPermethylated,
																																true,
																																IsReducedReducingEnd);
			bool isHuman = true;
			if (SiaType == 1)
			{
				isHuman = false;
			}
			foreach (GlycanCompound gCompound in tmps)
			{
				foreach (enumLabelingTag Tag in _LabelingRatio.Keys)
				{
					for (int i = 1; i <= 5; i++) //charge
					{
						foreach (List<Tuple<int, int>> combin in GetCombinations(_adductLabel.Count, i))
						{
							List<Tuple<string, float, int>> LstAdducts = new List<Tuple<string, float, int>>();
							for (int j = 0; j < combin.Count; j++)
							{
								LstAdducts.Add(new Tuple<string, float, int>(_adductLabel[_adductMass[combin[j].Item1]], _adductMass[combin[j].Item1], combin[j].Item2));
							}
							_GlycanList.Add(new GlycanCompound(gCompound.NoOfHexNAc,
														 gCompound.NoOfHex,
														 gCompound.NoOfDeHex,
														 gCompound.NoOfSia,
														 IsPermethylated,
														 false,
														 IsReducedReducingEnd,
														 false,
														 isHuman,
														 LstAdducts,
														 Tag));
							_GlycanList[_GlycanList.Count - 1].PositiveCharge = _PositiveChargeMode;
							if (gCompound.HasLinearRegressionParameters)
							{
								_GlycanList[_GlycanList.Count - 1].LinearRegSlope = gCompound.LinearRegSlope;
								_GlycanList[_GlycanList.Count - 1].LinearRegIntercept = gCompound.LinearRegIntercept;
							}
							if (SiaType == 2)
							{
								_GlycanList.Add(new GlycanCompound(gCompound.NoOfHexNAc,
							   gCompound.NoOfHex,
							   gCompound.NoOfDeHex,
							   gCompound.NoOfSia,
							   IsPermethylated,
							   false,
							   IsReducedReducingEnd,
							   false,
							   false,
							   LstAdducts,
							   Tag));
								if (gCompound.HasLinearRegressionParameters)
								{
									_GlycanList[_GlycanList.Count - 1].LinearRegSlope = gCompound.LinearRegSlope;
									_GlycanList[_GlycanList.Count - 1].LinearRegIntercept = gCompound.LinearRegIntercept;
								}
							}
						}
					}
				}
			}
		}

		public void ReadGlycanListOld()
		{
			_GlycanList = new List<GlycanCompound>();
			StreamReader sr;
			//Assembly assembly = Assembly.GetExecutingAssembly();
			//sr = new StreamReader(assembly.GetManifestResourceStream( "MutliNGlycanFitControls.Properties.Resources.combinations.txt"));
			int LineNumber = 0;
			sr = new StreamReader(GlycanFile);

			string tmp; // temp line for processing
			tmp = sr.ReadLine();
			LineNumber++;
			Hashtable compindex = new Hashtable(); //Glycan Type index.

			//Read the title
			string[] splittmp = tmp.Trim().Split(',');
			if (tmp.ToLower().Contains("order"))
			{
				GlycanLCorderExist = true;
			}
			try
			{
				for (int i = 0; i < splittmp.Length; i++)
				{
					if (splittmp[i].ToLower() == "neunac" || splittmp[i].ToLower() == "neungc" || splittmp[i].ToLower() == "sialic")
					{
						compindex.Add("sia", i);
						continue;
					}
					if (splittmp[i].ToLower() != "hexnac" && splittmp[i].ToLower() != "hex" && splittmp[i].ToLower() != "dehex" && splittmp[i].ToLower() != "sia" && splittmp[i].ToLower() != "order")
					{
						throw new Exception("Glycan list file title error. (Use:HexNAc,Hex,DeHex,Sia,NeuNAc,NeuNGc,Order)");
					}
					compindex.Add(splittmp[i].ToLower(), i);
				}
			}
			catch (Exception ex)
			{
				sr.Close();
				throw ex;
			}
			int processed_count = 0;

			//Read the list
			try
			{
				do
				{
					tmp = sr.ReadLine();
					LineNumber++;
					splittmp = tmp.Trim().Split(',');
					GlycanCompound GC = new GlycanCompound(Convert.ToInt32(splittmp[(int)compindex["hexnac"]]),
											 Convert.ToInt32(splittmp[(int)compindex["hex"]]),
											 Convert.ToInt32(splittmp[(int)compindex["dehex"]]),
											 Convert.ToInt32(splittmp[(int)compindex["sia"]]),
											 IsPermethylated,
											 false,
											 IsReducedReducingEnd,
											 false,
											 true
											 );
					_GlycanList.Add(GC);
					if (splittmp.Length == 5 && splittmp[(int)compindex["order"]] != "") // Contain order
					{
						_GlycanList[_GlycanList.Count - 1].GlycanLCorder = Convert.ToInt32(splittmp[(int)compindex["order"]]);
					}
					processed_count++;
				} while (!sr.EndOfStream);
			}
			catch (Exception ex)
			{
				throw new Exception("Glycan list file reading error on Line:" + LineNumber + ". Please check input file. (" + ex.Message + ")");
			}
			finally
			{
				sr.Close();
			}

			if (_GlycanList.Count == 0)
			{
				throw new Exception("Glycan list file reading error. Please check input file.");
			}
			_GlycanList.Sort();
		}

		private IEnumerable<List<Tuple<int, int>>> GetCombinations(int argAdductNumber, int argMaxCharge)
		{
			//Generate Adduct Combination
			int TotalAdductNumber = argAdductNumber;
			char[] inputset = new char[TotalAdductNumber];
			for (int i = 0; i < TotalAdductNumber; i++)
			{
				inputset[i] = Convert.ToChar(i + 48);
			}
			Combinations<char> combinations = new Combinations<char>(inputset, argMaxCharge, GenerateOption.WithRepetition);
			Dictionary<char, int> DictAdductCount = new Dictionary<char, int>();
			foreach (IList<char> c in combinations)
			{
				DictAdductCount.Clear();
				for (int i = 0; i < c.Count; i++)
				{
					if (!DictAdductCount.ContainsKey(c[i]))
					{
						DictAdductCount[c[i]] = 0;
					}
					DictAdductCount[c[i]] = DictAdductCount[c[i]] + 1;
				}
				List<Tuple<int, int>> ReturnValues = new List<Tuple<int, int>>();
				for (int j = 0; j < TotalAdductNumber; j++)
				{
					if (DictAdductCount.ContainsKey(Convert.ToChar(j + 48)))
					{
						ReturnValues.Add(new Tuple<int, int>(j, DictAdductCount[Convert.ToChar(j + 48)]));
					}
				}
				yield return ReturnValues;
			}
		}

		/// <summary>
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T DeepClone<T>(T source)
		{
			if (!typeof(T).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", "source");
			}

			// Don't serialize a null object, simply return the default for that object
			if (Object.ReferenceEquals(source, null))
			{
				return default(T);
			}

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(stream);
			}
		}
	}
}