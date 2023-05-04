namespace COL.MassLib.MascotParser
{
	internal class Unimod
	{
		private string _ModifiedSiteAA;
		private int _ModifiedSiteIdx;
		private string _ModifiedType;

		public Unimod(string argModType, string argModAA, int argModIdx)
		{
			_ModifiedType = argModType;
			_ModifiedSiteAA = argModAA;
			_ModifiedSiteIdx = argModIdx;
		}

		public string ModifiedSiteAminiAcid => _ModifiedSiteAA;

		public string ModifiedType => _ModifiedType;

		public int ModifiedSiteIndex => _ModifiedSiteIdx;
	}
}