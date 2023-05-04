using System;

namespace COL.MassLib
{
	[Serializable]
	public class HCDInfo
	{
		private int _scanNum;

		public int ScanNum => _scanNum;

		private enumGlycanType _type;

		public enumGlycanType GlycanType => _type;

		private double _hcdscore;

		public double HCDScore => _hcdscore;

		public HCDInfo(int argScanNum, enumGlycanType argGType, double argHCDScore)
		{
			_scanNum = argScanNum;
			_type = argGType;
			_hcdscore = argHCDScore;
		}
	}
}