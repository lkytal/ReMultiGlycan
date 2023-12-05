using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib.MascotParser
{
    public class Peptides
    {
          Summary _Summary;
        int _QueryNo;
        string _Sequence;
        float _CalcMass;
        float _ExptMass;
        float _Score;
        string _ProteinName;
        int _ProteinStartPos;
        int _ProteinEndPos;
        string _ProteinTerm;
        string _PrimaryNL;
        public Peptides(int argNum, string argValue, string argTerm,string argPrimaryNL, Summary argSummary)
        {
            string[] tmpAry = argValue.Split(',');
            _QueryNo =  argNum;
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

        public int QueryNo
        {
            get { return _QueryNo; }
        }
        public string Sequence
        {
            get { return _Sequence; }
        }
        public float CalculatedMass
        {
            get { return _CalcMass; }
        }
        public float ExperimentMass
        {
            get { return _ExptMass; }
        }
        public float Score
        {
            get { return _Score; }
        }
        public string ProteinName
        {
            get { return _ProteinName; }
        }
        public int ProteinStartPosition
        {
            get { return _ProteinStartPos; }
        }
        public int ProteinEndPosition
        {
            get { return _ProteinEndPos; }
        }
        public string PrimaryNL
        {
            get { return _PrimaryNL; }
        }
        public string Protein_NTerm
        {
            get { return _ProteinTerm.Split(',')[0]; }
        }
        public string Protein_CTerm
        {
            get { return _ProteinTerm.Split(',')[1]; }
        }
        public Summary QuerySummary
        {
            get { return _Summary; }
        }
    }
}
