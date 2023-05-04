using System;

namespace COL.MassLib.MascotParser
{
	public class Peptides
	{
		private Summary _Summary;
		private int _QueryNo;
		private string _Sequence;
		private float _CalcMass;
		private float _ExptMass;
		private float _Score;
		private string _ProteinName;
		private int _ProteinStartPos;
		private int _ProteinEndPos;
		private string _ProteinTerm;
		private string _PrimaryNL;

		public Peptides(int argNum, string argValue, string argTerm, string argPrimaryNL, Summary argSummary)
		{
			string[] tmpAry = argValue.Split(',');
			_QueryNo = argNum;
			_CalcMass = Convert.ToSingle(tmpAry[1]);
			_ExptMass = _CalcMass + Convert.ToSingle(tmpAry[2]);
			_Sequence = tmpAry[4];
			_Score = Convert.ToSingle(tmpAry[7]);

			string[] tmpProtAry = tmpAry[10].Split(':');
			_ProteinName = tmpProtAry[0].Split(';')[1].Replace("\"", "");
			_ProteinStartPos = Convert.ToInt32(tmpProtAry[2]);
			_ProteinEndPos = Convert.ToInt32(tmpProtAry[3]);
			_PrimaryNL = argPrimaryNL;
			_ProteinTerm = argTerm;
			_Summary = argSummary;
		}

		public int QueryNo => _QueryNo;

		public string Sequence => _Sequence;

		public float CalculatedMass => _CalcMass;

		public float ExperimentMass => _ExptMass;

		public float Score => _Score;

		public string ProteinName => _ProteinName;

		public int ProteinStartPosition => _ProteinStartPos;

		public int ProteinEndPosition => _ProteinEndPos;

		public string PrimaryNL => _PrimaryNL;

		public string Protein_NTerm => _ProteinTerm.Split(',')[0];

		public string Protein_CTerm => _ProteinTerm.Split(',')[1];

		public Summary QuerySummary => _Summary;
	}
}