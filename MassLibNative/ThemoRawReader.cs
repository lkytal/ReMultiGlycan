using System;
using System.Collections.Generic;
using System.Linq;

using ThermoRawFileReader;

namespace COL.MassLib
{
	public class ThermoRawReader : IRawFileReader, IDisposable
	{
		private bool disposed = false;
		private float tolMinPeakCount;

		private XRawFileIO reader;

		public ThermoRawReader(string argFullPath)
		{
			reader = new XRawFileIO(argFullPath, true);
			RawFilePath = argFullPath;
		}

		public int NumberOfScans => reader.GetNumScans();

		public int GetMsLevel(int argScanNum)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			return scanInfo.MSLevel;
		}

		public double GetRetentionTime(int argScanNum)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			return scanInfo.RetentionTime;
		}

		public string RawFilePath { get; }

		public bool IsCIDScan(int argScanNum)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			return scanInfo.ActivationType == ActivationTypeConstants.CID ? true : false;
		}

		public bool IsHCDScan(int argScanNum)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			return scanInfo.ActivationType == ActivationTypeConstants.HCD ? true : false;
		}

		public bool IsFTScan(int argScanNum)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			return scanInfo.IsFTMS;
		}

		public float PPM { get; set; } = 30;

		public float SN { get; set; } = 2;

		public string GetScanDescription(int argScanNum)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			return scanInfo.ToString();
		}

		public int GetLastScanNum()
		{
			return reader.ScanEnd;
		}

		private MSScan GetScanFromFile(int argScanNum, float argMinSN = 2)//, float argPPM = 6, int argMinPeakCount=3)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);

			int mslevel = GetMsLevel(argScanNum);
			SN = argMinSN;
			MSScan scan = new MSScan(argScanNum);
			scan.MsLevel = GetMsLevel(argScanNum);
			scan.Time = GetRetentionTime(argScanNum);
			scan.ScanHeader = GetScanDescription(argScanNum);

			double[] mzs;
			double[] intensities;
			reader.GetScanData(argScanNum, out mzs, out intensities);
			scan.RawMZs = mzs.Select(x => (float)x).ToArray();
			scan.RawIntensities = intensities.Select(x => (float)x).ToArray();

			if (mslevel == 1)
			{
				scan.MZs = scan.RawMZs;
				scan.Intensities = scan.RawIntensities;
			}
			else // MS/MS
			{
				var parentScanInfo = new clsScanInfo(GetParentScanNumber(argScanNum));
				reader.GetScanInfo(GetParentScanNumber(argScanNum), out parentScanInfo);

				scan.MZs = scan.RawMZs;
				scan.Intensities = scan.RawIntensities;

				scan.ParentScanNo = parentScanInfo.ScanNumber;
				scan.ParentMZ = (float)scanInfo.ParentIonMonoisotopicMZ;
				scan.ParentCharge = scanInfo.ChargeState;

				scan.ParentMonoMz = scan.ParentMZ;
				scan.ParentBasePeak = (float)parentScanInfo.BasePeakIntensity;
				scan.ParentIntensity = (float)scanInfo.TotalIonCurrent;
			}

			scan.IsCIDScan = IsCIDScan(argScanNum);
			scan.IsHCDScan = IsHCDScan(argScanNum);
			scan.IsFTScan = IsFTScan(argScanNum);
			return scan;
		}

		public MSScan ReadScan(int argScanNo)
		{
			return GetScanFromFile(argScanNo, 2);
		}

		public MSScan ReadScan(int argScanNo, float argSN)
		{
			return GetScanFromFile(argScanNo, argSN);
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

		public int GetParentScanNumber(int argScanNum)
		{
			if (GetMsLevel(argScanNum) == 1)
				return 0;
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			reader.GetScanInfo(scanInfo.ParentScan, out scanInfo);
			return scanInfo.ScanNumber;
		}

		public float GetPrecusorMz(int argScanNum, int argMsLevel = 2)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(argScanNum, out scanInfo);
			return Convert.ToSingle(scanInfo.ParentIonMZ);
		}

		public int GetPrecusorCharge(int argScanNum, int argMSLevel = 2)
		{
			var scanInfo = new clsScanInfo(argScanNum);
			reader.GetScanInfo(GetParentScanNumber(argScanNum), out scanInfo);
			return scanInfo.ChargeState;
		}

		public HCDInfo GetHCDInfo(int argScanNum)
		{
			if (argScanNum > GetLastScanNum() || !IsHCDScan(argScanNum))
			{
				return null;
			}
			HCDScoring Scoring = new HCDScoring();
			MSScan scan = ReadScan(argScanNum);

			double HCDScore = Scoring.CalculateHCDScore(scan.MSPeaks);
			enumGlycanType GType = Scoring.DetermineGlycanType(scan.MSPeaks);
			HCDInfo HInfo = new HCDInfo(argScanNum, GType, HCDScore);
			return HInfo;
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
				reader.CloseRawFile();
			}
			// Free any unmanaged objects here.
			disposed = true;
		}

		private List<int> FindPeakIdx(float[] argMZAry, int argTargetIdx, int argCharge, float argPPM)
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
				List<int> ClosedPeaks = MassUtility.GetClosestMassIdxsWithinPPM(argMZAry, argMZAry[CurrentIdx] - Interval, argPPM);
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
						if (MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] - Interval) < minPPM)
						{
							minPPMIdx = ClosedPeaks[j];
							minPPM = MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval);
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
				List<int> ClosedPeaks = MassUtility.GetClosestMassIdxsWithinPPM(argMZAry, argMZAry[CurrentIdx] + Interval, argPPM);
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
						if (MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval) < minPPM)
						{
							minPPMIdx = ClosedPeaks[j];
							minPPM = MassUtility.GetMassPPM(argMZAry[ClosedPeaks[j]], argMZAry[CurrentIdx] + Interval);
						}
					}
					CurrentIdx = minPPMIdx;
					Peak.Add(CurrentIdx);
				}
			}

			return Peak;
		}
	}
}