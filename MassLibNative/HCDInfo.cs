using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    [Serializable]
    public class HCDInfo
    {
        private int _scanNum;
        public int ScanNum
        {
            get {return _scanNum;}
        }
        private enumGlycanType _type;
        public  enumGlycanType GlycanType
        {
            get{ return _type;}
        }
        private double _hcdscore;
        public double HCDScore
        {
            get { return _hcdscore; }
        }
        public HCDInfo(int argScanNum, enumGlycanType argGType, double argHCDScore)
        {
            _scanNum = argScanNum;
            _type = argGType;
            _hcdscore = argHCDScore;
        }      
    }
}
