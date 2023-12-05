using System;
using System.Collections.Generic;

namespace COL.MassLib
{
	public class RawReader : IRawFileReader, IDisposable
	{
		private bool disposed = false;
		private string _fileType;
		private double _peptideMinBackgroundRatio = 5.0;
		private double _singleToNoiseRatio = 3.0;

		//private int _scanNo = 1;
		private int _numOfScans = 0;

		private GlypID.Readers.clsRawData Raw;

		/// <summary>
		///
		/// </summary>
		/// <param name="argFullPath"></param>
		/// <param name="argFileType">raw or mzxml</param>
		public RawReader(string argFullPath, string argFileType)
		{
			RawFilePath = argFullPath;
			_fileType = argFileType;
			if (_fileType.ToLower() == "raw")
			{
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.FINNIGAN);
			}
			else
			{
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.MZXMLRAWDATA);
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="argFullPath"></param>
		/// <param name="argFileType"></param>
		public RawReader(string argFullPath, enumRawDataType argRawType)
		{
			RawFilePath = argFullPath;

			if (argRawType == enumRawDataType.raw)
			{
				_fileType = "raw";
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.FINNIGAN);
			}
			else
			{
				_fileType = "mzxml";
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.MZXMLRAWDATA);
			}
		}

		public RawReader(string argFullPath, string argFileType, double argSingleToNoise, double argPeakBackground, double argPeptideBackground, short argMaxCharge)
		{
			RawFilePath = argFullPath;
			_fileType = argFullPath;
			_singleToNoiseRatio = argSingleToNoise;
			PeptideMinBackgroundRatio = argPeakBackground;
			_peptideMinBackgroundRatio = argPeptideBackground;
			MaxCharge = argMaxCharge;
			if (_fileType.ToLower() == "raw")
			{
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.FINNIGAN);
			}
			else
			{
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.MZXMLRAWDATA);
			}
		}

		public RawReader(string argFullPath, enumRawDataType argRawType, double argSingleToNoise, double argPeakBackground, double argPeptideBackground, short argMaxCharge)
		{
			RawFilePath = argFullPath;
			_singleToNoiseRatio = argSingleToNoise;
			PeptideMinBackgroundRatio = argPeakBackground;
			_peptideMinBackgroundRatio = argPeptideBackground;
			MaxCharge = argMaxCharge;
			if (argRawType == enumRawDataType.raw)
			{
				_fileType = "raw";
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.FINNIGAN);
			}
			else
			{
				_fileType = "mzxml";
				Raw = new GlypID.Readers.clsRawData(RawFilePath, GlypID.Readers.FileType.MZXMLRAWDATA);
			}
		}

		public void SetPeakProcessorParameter(float argSignalToNoiseThreshold, float argPeakBackgroundRatio)
		{
			_singleToNoiseRatio = argSignalToNoiseThreshold;
			PeptideMinBackgroundRatio = argPeakBackgroundRatio;
		}

		public void SetPeakProcessorParameter(GlypID.Peaks.clsPeakProcessorParameters argPeakProcessorParameter)
		{
			_singleToNoiseRatio = argPeakProcessorParameter.SignalToNoiseThreshold;
			PeptideMinBackgroundRatio = argPeakProcessorParameter.PeakBackgroundRatio;
		}

		public void SetTransformParameter(GlypID.HornTransform.clsHornTransformParameters argTransformParameter)
		{
			_peptideMinBackgroundRatio = argTransformParameter.PeptideMinBackgroundRatio;
			MaxCharge = argTransformParameter.MaxCharge;
		}

		public short MaxCharge { get; set; } = 10;

		public double PeptideMinBackgroundRatio { get; set; } = 5.0;

		public int NumberOfScans
		{
			get
			{
				if (_numOfScans == 0)
				{
					_numOfScans = Raw.GetNumScans();
				}

				return _numOfScans;
			}
		}

		public int GetMsLevel(int argScan)
		{
			return Raw.GetMSLevel(argScan);
		}

		public string RawFilePath { get; }

		public HCDInfo GetHCDInfo(int argScanNo)
		{
			if (!Raw.IsHCDScan(argScanNo))
			{
				return null;
			}
			HCDInfo HCDinfo = null;
			double _hcd_score;
			GlypID.enmGlycanType _gType;
			int ParentScan = Raw.GetParentScan(argScanNo);
			// Scorers and transforms
			GlypID.HCDScoring.clsHCDScoring HCDScoring = new GlypID.HCDScoring.clsHCDScoring();
			GlypID.HornTransform.clsHornTransform Transform = new GlypID.HornTransform.clsHornTransform();

			// mzs , intensities
			float[] hcd_mzs = null;
			float[] hcd_intensities = null;
			float[] parent_mzs = null;
			float[] parent_intensities = null;

			// Peaks
			GlypID.Peaks.clsPeak[] parent_peaks;
			GlypID.Peaks.clsPeak[] hcd_peaks;

			// Peak Processors
			GlypID.Peaks.clsPeakProcessor hcdPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
			GlypID.Peaks.clsPeakProcessor parentPeakProcessor = new GlypID.Peaks.clsPeakProcessor();

			// Results
			GlypID.HCDScoring.clsHCDScoringScanResults[] hcd_scoring_results;
			GlypID.HornTransform.clsHornTransformResults[] transform_results;

			// Params
			GlypID.Scoring.clsScoringParameters scoring_parameters = new GlypID.Scoring.clsScoringParameters();
			GlypID.HornTransform.clsHornTransformParameters transform_parameters = new GlypID.HornTransform.clsHornTransformParameters();
			scoring_parameters.MinNumPeaksToConsider = 2;
			scoring_parameters.PPMTolerance = 10;
			scoring_parameters.UsePPM = true;

			// Init
			Transform.TransformParameters = transform_parameters;

			// Loading parent
			int parent_scan = Raw.GetParentScan(argScanNo);
			double parent_mz = Raw.GetParentMz(argScanNo);
			int scan_level = Raw.GetMSLevel(argScanNo);
			int parent_level = Raw.GetMSLevel(parent_scan);
			Raw.GetSpectrum(parent_scan, ref parent_mzs, ref parent_intensities);

			// Parent processing
			parent_peaks = new GlypID.Peaks.clsPeak[1];
			parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
			parentPeakProcessor.DiscoverPeaks(ref parent_mzs, ref parent_intensities, ref parent_peaks,
						Convert.ToSingle(transform_parameters.MinMZ), Convert.ToSingle(transform_parameters.MaxMZ), true);
			double bkg_intensity = parentPeakProcessor.GetBackgroundIntensity(ref parent_intensities);
			double min_peptide_intensity = bkg_intensity * transform_parameters.PeptideMinBackgroundRatio;

			transform_results = new GlypID.HornTransform.clsHornTransformResults[1];
			bool found = Transform.FindPrecursorTransform(Convert.ToSingle(bkg_intensity), Convert.ToSingle(min_peptide_intensity), ref parent_mzs, ref parent_intensities, ref parent_peaks, Convert.ToSingle(parent_mz), ref transform_results);
			if (!found && (Raw.GetMonoChargeFromHeader(ParentScan) > 0))
			{
				found = true;
				double mono_mz = Raw.GetMonoMzFromHeader(ParentScan);
				if (mono_mz == 0)
					mono_mz = parent_mz;

				short[] charges = new short[1];
				charges[0] = Raw.GetMonoChargeFromHeader(ParentScan);
				Transform.AllocateValuesToTransform(Convert.ToSingle(mono_mz), 0, ref charges, ref transform_results); // Change abundance value from 0 to parent_intensity if you wish
			}
			if (found && transform_results.Length == 1)
			{
				// Score HCD scan first
				Raw.GetSpectrum(argScanNo, ref hcd_mzs, ref hcd_intensities);
				double hcd_background_intensity = GlypID.Utils.GetAverage(ref hcd_intensities, ref hcd_mzs, Convert.ToSingle(scoring_parameters.MinHCDMz), Convert.ToSingle(scoring_parameters.MaxHCDMz));
				hcdPeakProcessor.SetPeakIntensityThreshold(hcd_background_intensity);
				hcd_peaks = new GlypID.Peaks.clsPeak[1];

				//Check Header
				string Header = Raw.GetScanDescription(argScanNo);
				hcdPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
				if (Header.Substring(Header.IndexOf("+") + 1).Trim().StartsWith("c"))
				{
					hcdPeakProcessor.ProfileType = GlypID.enmProfileType.CENTROIDED;
				}

				hcdPeakProcessor.DiscoverPeaks(ref hcd_mzs, ref hcd_intensities, ref hcd_peaks, Convert.ToSingle
					(scoring_parameters.MinHCDMz), Convert.ToSingle(scoring_parameters.MaxHCDMz), false);
				hcdPeakProcessor.InitializeUnprocessedData();

				hcd_scoring_results = new GlypID.HCDScoring.clsHCDScoringScanResults[1];

				HCDScoring.ScoringParameters = scoring_parameters;
				_hcd_score = HCDScoring.ScoreHCDSpectra(ref hcd_peaks, ref hcd_mzs, ref hcd_intensities, ref transform_results, ref hcd_scoring_results);
				_gType = (GlypID.enmGlycanType)hcd_scoring_results[0].menm_glycan_type;

				enumGlycanType GType; //Convert from GlypID.enumGlycanType to MassLib.enumGlycanType;
				if (_gType == GlypID.enmGlycanType.CA)
				{
					GType = enumGlycanType.CA;
				}
				else if (_gType == GlypID.enmGlycanType.CS)
				{
					GType = enumGlycanType.CS;
				}
				else if (_gType == GlypID.enmGlycanType.HM)
				{
					GType = enumGlycanType.HM;
				}
				else if (_gType == GlypID.enmGlycanType.HY)
				{
					GType = enumGlycanType.HY;
				}
				else
				{
					GType = enumGlycanType.NA;
				}

				HCDinfo = new HCDInfo(argScanNo, GType, _hcd_score);
			}

			return HCDinfo;
		}

		public bool IsCIDScan(int argScanNum)
		{
			return Raw.IsCIDScan(argScanNum);
		}

		public bool IsHCDScan(int argScanNum)
		{
			return Raw.IsHCDScan(argScanNum);
		}

		//string Desc = "";
		public string GetScanDescription(int argScanNum)
		{
			return Raw.GetScanDescription(argScanNum);
		}

		private MSScan GetScanFromFile(int argScanNo)
		{
			float[] _cidMzs = null;
			float[] _cidIntensities = null;
			GlypID.Peaks.clsPeak[] _cidPeaks = new GlypID.Peaks.clsPeak[1];
			GlypID.Peaks.clsPeak[] _parentPeaks = new GlypID.Peaks.clsPeak[1];

			GlypID.HornTransform.clsHornTransform mobjTransform = new GlypID.HornTransform.clsHornTransform();
			GlypID.HornTransform.clsHornTransformParameters mobjTransformParameters = new GlypID.HornTransform.clsHornTransformParameters();
			mobjTransformParameters.PeptideMinBackgroundRatio = _peptideMinBackgroundRatio;
			mobjTransformParameters.MaxCharge = MaxCharge;
			GlypID.HornTransform.clsHornTransformResults[] _transformResult;

			GlypID.Peaks.clsPeakProcessor cidPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
			GlypID.Peaks.clsPeakProcessorParameters cidPeakParameters = new GlypID.Peaks.clsPeakProcessorParameters();
			cidPeakParameters.PeakBackgroundRatio = PeptideMinBackgroundRatio;
			cidPeakParameters.SignalToNoiseThreshold = _singleToNoiseRatio;

			GlypID.Peaks.clsPeakProcessor parentPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
			GlypID.Peaks.clsPeakProcessorParameters parentPeakParameters = new GlypID.Peaks.clsPeakProcessorParameters();
			parentPeakParameters.PeakBackgroundRatio = PeptideMinBackgroundRatio;
			parentPeakParameters.SignalToNoiseThreshold = _singleToNoiseRatio;

			//Start Read Scan
			MSScan scan = new MSScan(argScanNo);

			Raw.GetSpectrum(argScanNo, ref _cidMzs, ref _cidIntensities);
			List<float> MZs = new List<float>();
			List<float> Intensites = new List<float>();
			for (int i = 0; i < _cidIntensities.Length; i++)
			{
				if (_cidIntensities[i] != 0)
				{
					MZs.Add(_cidMzs[i]);
					Intensites.Add(_cidIntensities[i]);
				}
			}

			scan.MZs = MZs.ToArray();
			scan.Intensities = Intensites.ToArray();
			scan.MsLevel = Raw.GetMSLevel(Convert.ToInt32(argScanNo));

			double min_peptide_intensity = 0;
			scan.Time = Math.Round(Raw.GetScanTime(scan.ScanNo), 5);
			scan.ScanHeader = Raw.GetScanDescription(scan.ScanNo);
			if (scan.MsLevel != 1)
			{
				float[] _parentRawMzs = null;
				float[] _parentRawIntensitys = null;

				string Header = Raw.GetScanDescription(argScanNo);
				cidPeakProcessor.ProfileType = GlypID.enmProfileType.CENTROIDED;
				if (Header.Substring(Header.IndexOf("+") + 1).Trim().StartsWith("p"))
				{
					cidPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
				}

				// cidPeakProcessor.DiscoverPeaks(ref _cidMzs, ref _cidIntensities, ref _cidPeaks,
				//         Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), false);

				for (int chNum = 0; chNum < _cidMzs.Length; chNum++)
				{
					scan.MSPeaks.Add(new MSPeak(
						Convert.ToSingle(_cidMzs[chNum]),
						Convert.ToSingle(_cidIntensities[chNum])));
				}

				//for (int chNum = 0; chNum < _cidMzs.Length; chNum++)
				//{
				//    scan.MSPeaks.Add(new MSPeak(
				//        Convert.ToSingle(_cidMzs[chNum]),
				//        Convert.ToSingle(_cidIntensities[chNum])));
				//}

				// Get parent information
				scan.ParentScanNo = Raw.GetParentScan(scan.ScanNo);

				Raw.GetSpectrum(scan.ParentScanNo, ref _parentRawMzs, ref _parentRawIntensitys);
				parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
				parentPeakProcessor.DiscoverPeaks(ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), true);
				float _parentBackgroundIntensity = (float)parentPeakProcessor.GetBackgroundIntensity(ref _parentRawIntensitys);
				_transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
				bool found = false;
				if (Raw.IsFTScan(scan.ParentScanNo))
				{
					// High resolution data
					found = mobjTransform.FindPrecursorTransform(Convert.ToSingle(_parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(scan.ParentMZ), ref _transformResult);
				}
				if (!found)//de-isotope fail
				{
					// Low resolution data or bad high res spectra
					short cs = Raw.GetMonoChargeFromHeader(scan.ScanNo);
					double monoMZ = Raw.GetMonoMzFromHeader(scan.ScanNo);
					List<float> ParentMzs = new List<float>(_parentRawMzs);
					int CloseIdx = MassUtility.GetClosestMassIdx(ParentMzs, Convert.ToSingle(monoMZ));

					if (cs > 0)
					{
						short[] charges = new short[1];
						charges[0] = cs;
						mobjTransform.AllocateValuesToTransform(Convert.ToSingle(scan.ParentMZ), Convert.ToInt32(_parentRawIntensitys[CloseIdx]), ref charges, ref _transformResult);
					}
					else
					{
						// instrument has no charge just store 2 and 3.
						short[] charges = new short[2];
						charges[0] = 2;
						charges[1] = 3;
						mobjTransform.AllocateValuesToTransform(Convert.ToSingle(scan.ParentMZ), Convert.ToInt32(_parentRawIntensitys[CloseIdx]), ref charges, ref _transformResult);
					}
				}

				if (_transformResult[0].mint_peak_index == -1) //De-isotope parent scan
				{
					//Get parent info
					MSScan _parentScan = GetScanFromFile(scan.ParentScanNo);
					float[] _MSMzs = null;
					float[] _MSIntensities = null;

					Raw.GetSpectrum(scan.ParentScanNo, ref _MSMzs, ref _MSIntensities);
					// Now find peaks
					parentPeakParameters.SignalToNoiseThreshold = 0;
					parentPeakParameters.PeakBackgroundRatio = 0.01;
					parentPeakProcessor.SetOptions(parentPeakParameters);
					parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;

					parentPeakProcessor.DiscoverPeaks(ref _MSMzs, ref _MSIntensities, ref _cidPeaks,
											Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), true);

					//Look for charge and mono.

					float[] monoandcharge = FindChargeAndMono(_cidPeaks, Convert.ToSingle(Raw.GetParentMz(scan.ScanNo)), scan.ScanNo, Raw);
					//scan.ParentMonoMW = _parentScan.MSPeaks[ClosedIdx].MonoMass;
					//scan.ParentAVGMonoMW = _parentScan.MSPeaks[ClosedIdx].;
					scan.ParentMZ = monoandcharge[0];
					if (monoandcharge[1] == 0.0f)
					{
						scan.ParentCharge = Convert.ToInt32(Raw.GetMonoChargeFromHeader(scan.ParentScanNo));
					}
					else
					{
						scan.ParentCharge = Convert.ToInt32(monoandcharge[1]);
					}

					scan.ParentMonoMW = (monoandcharge[0] - Atoms.ProtonMass) * monoandcharge[1];
				}
				else
				{
					scan.ParentMonoMW = (float)_transformResult[0].mdbl_mono_mw;
					scan.ParentAVGMonoMW = (float)_transformResult[0].mdbl_average_mw;
					scan.ParentMZ = (float)_transformResult[0].mdbl_mz;
					scan.ParentCharge = (int)_transformResult[0].mshort_cs;
				}
				scan.IsCIDScan = Raw.IsCIDScan(argScanNo);
				scan.IsFTScan = Raw.IsFTScan(argScanNo);

				//Array.Clear(_transformResult, 0, _transformResult.Length);
				//Array.Clear(_cidPeaks, 0, _cidPeaks.Length);
				//Array.Clear(_cidMzs, 0, _cidMzs.Length);
				//Array.Clear(_cidIntensities, 0, _cidIntensities.Length);
				//Array.Clear(_parentRawMzs, 0, _parentRawMzs.Length);
				//Array.Clear(_parentRawIntensitys, 0, _parentRawIntensitys.Length);
			}
			else //MS Scan
			{
				scan.ParentMZ = 0.0f;
				double mdbl_current_background_intensity = 0;

				// Now find peaks
				parentPeakParameters.SignalToNoiseThreshold = _singleToNoiseRatio;
				parentPeakParameters.PeakBackgroundRatio = PeptideMinBackgroundRatio;
				parentPeakProcessor.SetOptions(parentPeakParameters);
				parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;

				parentPeakProcessor.DiscoverPeaks(ref _cidMzs, ref _cidIntensities, ref _cidPeaks,
										Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), true);
				mdbl_current_background_intensity = parentPeakProcessor.GetBackgroundIntensity(ref _cidIntensities);

				// Settings
				min_peptide_intensity = mdbl_current_background_intensity * mobjTransformParameters.PeptideMinBackgroundRatio;
				if (mobjTransformParameters.UseAbsolutePeptideIntensity)
				{
					if (min_peptide_intensity < mobjTransformParameters.AbsolutePeptideIntensity)
						min_peptide_intensity = mobjTransformParameters.AbsolutePeptideIntensity;
				}
				mobjTransformParameters.PeptideMinBackgroundRatio = _peptideMinBackgroundRatio;
				mobjTransformParameters.MaxCharge = MaxCharge;
				mobjTransform.TransformParameters = mobjTransformParameters;

				//  Now perform deisotoping
				_transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
				mobjTransform.PerformTransform(Convert.ToSingle(mdbl_current_background_intensity), Convert.ToSingle(min_peptide_intensity), ref _cidMzs, ref _cidIntensities, ref _cidPeaks, ref _transformResult);
				// for getting results

				for (int chNum = 0; chNum < _transformResult.Length; chNum++)
				{
					double sumintensity = 0.0;
					double mostIntenseIntensity = 0.0;
					for (int i = 0; i < _transformResult[chNum].marr_isotope_peak_indices.Length; i++)
					{
						sumintensity = sumintensity + _cidPeaks[_transformResult[chNum].marr_isotope_peak_indices[i]].mdbl_intensity;
						if (Math.Abs(_transformResult[chNum].mdbl_most_intense_mw -
							(_cidPeaks[_transformResult[chNum].marr_isotope_peak_indices[i]].mdbl_mz * _transformResult[chNum].mshort_cs - Atoms.ProtonMass * _transformResult[chNum].mshort_cs))
							< 1.0 / _transformResult[chNum].mshort_cs)
						{
							mostIntenseIntensity = _cidPeaks[_transformResult[chNum].mint_peak_index].mdbl_intensity;
						}
					}
					scan.MSPeaks.Add(new MSPeak(
					Convert.ToSingle(_transformResult[chNum].mdbl_mono_mw),
					_transformResult[chNum].mint_mono_intensity,
					_transformResult[chNum].mshort_cs,
					Convert.ToSingle(_transformResult[chNum].mdbl_mz),
					Convert.ToSingle(_transformResult[chNum].mdbl_fit),
					Convert.ToSingle(_transformResult[chNum].mdbl_most_intense_mw),
					mostIntenseIntensity,
					sumintensity
					));
				}
				//Array.Clear(_transformResult, 0, _transformResult.Length);
				//Array.Clear(_cidPeaks, 0, _cidPeaks.Length);
				//Array.Clear(_cidMzs, 0, _cidMzs.Length);
				//Array.Clear(_cidIntensities, 0, _cidIntensities.Length);
			}
			_cidMzs = null;
			_cidIntensities = null;
			_cidPeaks = null;
			_parentPeaks = null;
			_transformResult = null;
			mobjTransform = null;
			mobjTransformParameters = null;
			parentPeakProcessor = null;
			parentPeakParameters = null;
			cidPeakParameters = null;
			cidPeakProcessor = null;
			MZs.Clear();
			MZs = null;
			Intensites.Clear();
			Intensites = null;
			GC.Collect();
			return scan;
		}

		public MSScan ReadScan(int argScanNo)
		{
			return GetScanFromFile(argScanNo);
		}

		public List<MSScan> ReadScans(int argStart, int argEnd)
		{
			List<MSScan> scans = new List<MSScan>();
			if (argStart <= 0)
			{
				argStart = 1;
			}
			if (argEnd > this.NumberOfScans)
			{
				argEnd = this.NumberOfScans;
			}
			for (int i = argStart; i <= argEnd; i++)
			{
				scans.Add(ReadScan(i));
			}
			return scans;
		}

		public List<MSScan> ReadAllScans()
		{
			int EndScan = this.NumberOfScans;
			return ReadScans(1, EndScan);
		}

		public List<MSScan> ReadScanWMSLevel(int argStart, int argEnd, int argMSLevel)
		{
			List<MSScan> scans = new List<MSScan>();
			if (argStart <= 0)
			{
				argStart = 1;
			}
			if (argEnd > this.NumberOfScans)
			{
				argEnd = this.NumberOfScans;
			}
			for (int i = argStart; i <= argEnd; i++)
			{
				MSScan scan = ReadScan(i);
				if (scan.MsLevel == argMSLevel)
				{
					scans.Add(scan);
				}
			}
			return scans;
		}

		private static float[] FindChargeAndMono(GlypID.Peaks.clsPeak[] argPeaks, float argTargetMZ, int argParentScanNo, GlypID.Readers.clsRawData Raw)
		{
			float[] MonoAndMz = new float[2];

			double interval = 9999.9;
			int ClosedIdx = 0;
			for (int i = 0; i < argPeaks.Length; i++)
			{
				if (Math.Abs(argPeaks[i].mdbl_mz - argTargetMZ) < interval)
				{
					interval = Math.Abs(argPeaks[i].mdbl_mz - argTargetMZ);
					ClosedIdx = i;
				}
			}

			//Charge
			float testMz = 0.0f;
			int MaxMatchedPeak = 2;

			for (int i = 1; i <= 6; i++)
			{
				double FirstMonoMz = 0.0;
				int ForwardPeak = 0;
				int BackardPeak = 0;
				//Forward Check
				testMz = argTargetMZ - 1.0f / (float)i;
				int CheckIdx = ClosedIdx - 1;
				for (int j = 1; j <= 10; j++)
				{
					if (CheckIdx < 0)
					{
						break;
					}
					if (Math.Abs(argPeaks[CheckIdx].mdbl_mz - testMz) <= 0.03)
					{
						ForwardPeak++;
						testMz = Convert.ToSingle(argPeaks[CheckIdx].mdbl_mz) - 1.0f / (float)i;
						FirstMonoMz = argPeaks[CheckIdx].mdbl_mz;
					}
					CheckIdx = CheckIdx - 1;
				}

				//Backward
				testMz = argTargetMZ + 1.0f / (float)i;
				CheckIdx = ClosedIdx + 1;

				for (int j = 1; j <= 10; j++)
				{
					if (CheckIdx >= argPeaks.Length)
					{
						break;
					}
					if (Math.Abs(argPeaks[CheckIdx].mdbl_mz - testMz) <= 0.03)
					{
						BackardPeak++;
						testMz = Convert.ToSingle(argPeaks[CheckIdx].mdbl_mz) + 1.0f / (float)i;
					}
					CheckIdx = CheckIdx + 1;
				}

				if (ForwardPeak == 0)
				{
					FirstMonoMz = argTargetMZ;
				}

				if (ForwardPeak + BackardPeak >= MaxMatchedPeak)
				{
					MaxMatchedPeak = ForwardPeak + BackardPeak;
					MonoAndMz[0] = Convert.ToSingle(FirstMonoMz);
					MonoAndMz[1] = i;
				}
			}

			if (MonoAndMz[1] == 0)
			{
				if (interval < 0.01)
				{
					MonoAndMz[0] = argTargetMZ;
				}
				MonoAndMz[1] = Raw.GetMonoChargeFromHeader(argParentScanNo);
			}

			return MonoAndMz;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Protected implementation of Dispose pattern.
		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;
			if (disposing)
			{
				// Free any other managed objects here.
				Raw = null;
			}
			// Free any unmanaged objects here.
			disposed = true;
		}
	}
}