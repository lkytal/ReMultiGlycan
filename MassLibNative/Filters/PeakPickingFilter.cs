namespace COL.MassLib.Filters
{
	public class PeakPickingFilter : IFilter
	{
		private MSScan _msScan;

		public PeakPickingFilter(MSScan argScan)
		{
			_msScan = argScan;
		}

		public void ApplyFilter()
		{
		}
	}
}