using MSFileReaderLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace COL.MassLib
{
	public class ThermoRawReader : IRawFileReader, IDisposable
	{
		private bool disposed = false;
		private string _fullFilePath;
		private string _fileType;
		private double _peptideMinBackgroundRatio = 5.0;
		private double _peakBackgroundRatio = 5.0;
		private double _singleToNoiseRatio = 3.0;
		private short _maxCharge = 10;

		//private int _scanNo = 1;
		private int _numOfScans = 0;

		private IXRawfile5 _rawConnection;

		/// <summary>
		///
		/// </summary>
		/// <param name="argFullPath"></param>
		/// <param name="argFileType">ThermoRawFile</param>
		public ThermoRawReader(string argFullPath)
		{
			_fullFilePath = argFullPath;
			_rawConnection = (IXRawfile5)new MSFileReader_XRawfile();
			_rawConnection.Open(argFullPath);
			_rawConnection.SetCurrentController(0, 1); // first 0 is for mass spectrometer
		}

		public int NumberOfScans
		{
			get
			{
				if (_numOfScans == 0)
				{
					_rawConnection.GetLastSpectrumNumber(ref _numOfScans);
				}
				return _numOfScans;
			}
		}

		public int GetMsLevel(int argScan)
		{
			int msnOrder = 0;
			_rawConnection.GetMSOrderForScanNum(argScan, ref msnOrder);
			return msnOrder;
		}

		public string RawFilePath => _fullFilePath;

		public bool IsCIDScan(int argScanNum)
		{
			string filter = null;
			_rawConnection.GetFilterForScanNum(argScanNum, ref filter);
			if (filter.ToLower().Contains("cid"))
			{
				return true;
			}
			return false;
		}

		public bool IsHCDScan(int argScanNum)
		{
			string filter = null;
			_rawConnection.GetFilterForScanNum(argScanNum, ref filter);
			if (filter.ToLower().Contains("hcd"))
			{
				return true;
			}
			return false;
		}

		public bool IsFTScan(int argScanNum)
		{
			string filter = null;
			_rawConnection.GetFilterForScanNum(argScanNum, ref filter);
			if (filter.ToLower().Contains("ft"))
			{
				return true;
			}
			return false;
		}

		//string Desc = "";
		public string GetScanDescription(int argScanNum)
		{
			string filter = null;
			_rawConnection.GetFilterForScanNum(argScanNum, ref filter);
			return filter;
		}

		private float[] ConvertDoubleArrayToFloatArray(double[] argDouble)
		{
			float[] TargetArray = new float[argDouble.GetLength(0)];
			for (int i = 0; i < argDouble.GetLength(0); i++)
			{
				TargetArray[i] = Convert.ToSingle(argDouble[i]);
			}
			return TargetArray;
		}

		private MSScan GetScanFromFile(int argScanNo)
		{
			return GetScanFromFile(argScanNo, 2.0f);
		}

		private MSScan GetScanFromFile(int argScanNo, float argMinSN = 2)
		{
			int isProfile = 0;
			_rawConnection.IsProfileScanForScanNum(argScanNo, ref isProfile);

			object labels = null;
			object flags = null;
			_rawConnection.GetLabelData(ref labels, ref flags, ref argScanNo);
			double[,] LabeledPeaks = (double[,])labels;

			//Start Read Scan
			MSScan scan = new MSScan(argScanNo);
			List<ThermoLabeledPeak> FullLabeledPeak = new List<ThermoLabeledPeak>();
			float[] mz = new float[LabeledPeaks.GetLength(1)];
			float[] intensity = new float[LabeledPeaks.GetLength(1)];
			int j = 0;
			for (int i = 0; i < LabeledPeaks.GetLength(1); i++)
			{
				double sn = LabeledPeaks[1, i] / LabeledPeaks[4, i];
				if (sn >= argMinSN)
				{
					mz[j] = Convert.ToSingle(LabeledPeaks[0, i]);
					intensity[j] = Convert.ToSingle(LabeledPeaks[1, i]);
					j++;
					FullLabeledPeak.Add(new ThermoLabeledPeak(
					Convert.ToSingle(LabeledPeaks[0, i]),
					Convert.ToSingle(LabeledPeaks[1, i]),
					Convert.ToInt32(LabeledPeaks[5, i]),
					Convert.ToSingle(LabeledPeaks[4, i])));
				}
			}
			Array.Resize(ref mz, j);
			Array.Resize(ref intensity, j);

			//scan.MZs = ConvertDoubleArrayToFloatArray(CSMSLScan.MassSpectrum.GetMasses());
			scan.MZs = mz;
			//scan.Intensities = ConvertDoubleArrayToFloatArray(CSMSLScan.MassSpectrum.GetIntensities());
			scan.Intensities = intensity;
			scan.MsLevel = GetMsLevel(argScanNo);

			double retentionTime = 0;
			_rawConnection.RTFromScanNum(argScanNo, ref retentionTime);
			scan.Time = retentionTime;
			scan.ScanHeader = GetScanDescription(argScanNo);

			if (scan.MsLevel != 1)
			{
				for (int chNum = 0; chNum < scan.MZs.Length; chNum++)
				{
					scan.MSPeaks.Add(new MSPeak(
						Convert.ToSingle(scan.MZs[chNum]),
						Convert.ToSingle(scan.Intensities[chNum])));
				}

				// Get parent information
				object value = null;
				_rawConnection.GetTrailerExtraValueForScanNum(argScanNo, "Master Scan Number:", ref value);
				scan.ParentScanNo = Convert.ToInt32(value);
				string ParentDesc = GetScanDescription(scan.ParentScanNo);

				//_rawConnection.GetPrecursorInfoFromScanNum(argScanNo, ref PrecursorInfo, ref PrecursorInfoSize);

				//scan.ParentMonoMW = Convert.ToSingle(PrecurorArray.);
				//scan.ParentMZ = Convert.ToSingle(PrecurorArray[1,0]);
				//scan.ParentCharge = Convert.ToInt32(PrecurorArray.nChargeState);
				scan.IsCIDScan = IsCIDScan(argScanNo);
				scan.IsFTScan = IsFTScan(argScanNo);

				//Array.Clear(_transformResult, 0, _transformResult.Length);
				//Array.Clear(_cidPeaks, 0, _cidPeaks.Length);
				//Array.Clear(_cidMzs, 0, _cidMzs.Length);
				//Array.Clear(_cidIntensities, 0, _cidIntensities.Length);
				//Array.Clear(_parentRawMzs, 0, _parentRawMzs.Length);
				//Array.Clear(_parentRawIntensitys, 0, _parentRawIntensitys.Length);
			}
			else //MS Scan
			{
				do
				{
					ThermoLabeledPeak BasePeak = FullLabeledPeak[0];
					List<ThermoLabeledPeak> clusterPeaks = new List<ThermoLabeledPeak>();
					List<int> RemoveIdx = new List<int>();
					RemoveIdx.Add(0);
					clusterPeaks.Add(BasePeak);
					double Interval = 1 / (double)BasePeak.Charge;
					double FirstMZ = BasePeak.MZ;
					for (int i = 1; i < FullLabeledPeak.Count; i++)
					{
						if (FullLabeledPeak[i].MZ - FirstMZ > Interval * 10)
						{
							break;
						}
						if ((FullLabeledPeak[i].MZ - (BasePeak.MZ + Interval)) < 0.1 && (FullLabeledPeak[i].MZ - (BasePeak.MZ + Interval)) >= 0 && clusterPeaks[0].Charge == FullLabeledPeak[i].Charge)
						{
							BasePeak = FullLabeledPeak[i];
							clusterPeaks.Add(FullLabeledPeak[i]);
							RemoveIdx.Add(i);
						}
					}
					if (clusterPeaks.Count < 3)
					{
						FullLabeledPeak.RemoveAt(RemoveIdx[0]);
					}
					else
					{
						float MostIntenseMZ = 0.0f;
						double MostIntenseIntneisty = 0;
						double ClusterIntensity = 0;
						RemoveIdx.Reverse();
						for (int i = 0; i < RemoveIdx.Count; i++)
						{
							if (FullLabeledPeak[RemoveIdx[i]].Intensity > MostIntenseIntneisty)
							{
								MostIntenseIntneisty = FullLabeledPeak[RemoveIdx[i]].Intensity;
								MostIntenseMZ = FullLabeledPeak[RemoveIdx[i]].MZ;
							}
							ClusterIntensity = ClusterIntensity + FullLabeledPeak[RemoveIdx[i]].Intensity;
							FullLabeledPeak.RemoveAt(RemoveIdx[i]);
						}
						scan.MSPeaks.Add(new MSPeak(clusterPeaks[0].Mass, clusterPeaks[0].Intensity, clusterPeaks[0].Charge, clusterPeaks[0].MZ, clusterPeaks[0].SN, MostIntenseMZ, MostIntenseIntneisty, ClusterIntensity));
					}
				} while (FullLabeledPeak.Count != 0);

				//Array.Clear(_transformResult, 0, _transformResult.Length);
				//Array.Clear(_cidPeaks, 0, _cidPeaks.Length);
				//Array.Clear(_cidMzs, 0, _cidMzs.Length);
				//Array.Clear(_cidIntensities, 0, _cidIntensities.Length);
			}
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
				_rawConnection.Close();
			}
			// Free any unmanaged objects here.
			disposed = true;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PrecursorInfo
	{
		public double dIsolationMass;
		public double dMonoIsoMass;
		public int nChargeState;
		public int nScanNumber;
	}

	public struct ThermoLabeledPeak
	{
		private int _Charge;
		private float _Noise;
		private float _MZ;
		private float _Intensity;
		public float MZ => _MZ;

		public float Mass => (_MZ - Atoms.ProtonMass) * _Charge;

		public float Intensity => _Intensity;

		public int Charge => _Charge;

		public float Noise => _Noise;

		public float SN
		{
			get
			{
				if (_Noise.Equals(0)) return float.NaN;
				return _Intensity / _Noise;
			}
		}

		public ThermoLabeledPeak(float argMZ, float argIntensity, int argCharge, float argNoise)
		{
			_MZ = argMZ;
			_Intensity = argIntensity;
			_Charge = argCharge;
			_Noise = argNoise;
		}

		public double GetSignalToNoise()
		{
			return SN;
		}
	}
}