using System.Collections.Generic;

namespace COL.MassLib
{
	public interface IRawFileReader
	{
		MSScan ReadScan(int argScan);

		List<MSScan> ReadScans(int argStart, int argEnd);

		List<MSScan> ReadAllScans();

		List<MSScan> ReadScanWMSLevel(int argStart, int argEnd, int argMSLevel);

		int NumberOfScans { get; }

		int GetMsLevel(int argScan);
	}
}