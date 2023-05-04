using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Text;
using COL.MassLib;
using COL.GlycoLib;
namespace COL.MultiGlycan
{
    public class MatchedGlycanPeak
    {
        private MSPeak _MSPeak;
        private double _Time;
        private int _ScanNum;
        List<MSPoint> _Points;
        private GlycanCompound _glycanComposition;
        //private double _MergedIntensity;        
        private double _CorrectedIntensity = 0;

        public MatchedGlycanPeak(int argScanNum, double argTime, MSPeak argPeak, GlycanCompound argGlycanComp)
        {
            _ScanNum = argScanNum;
            _Time = argTime;
            _MSPeak = argPeak;
            _glycanComposition = argGlycanComp;
        }
        public List<MSPoint> MSPoints
        {
            get { return _Points; }
            set { _Points = value; }
        }
        public string AdductString
        {
            get
            {
                string tmp = "";
                foreach (Tuple<string, float, int> adduct in _glycanComposition.Adducts)
                {
                    tmp = tmp + adduct.Item1 + " * " + adduct.Item3 + ";";
                }
                tmp = tmp.Substring(0, tmp.Length - 1);
                return tmp;
            }
        }

        public int Charge
        {
            get { return (int) _MSPeak.ChargeState; }
        }
        /// <summary>
        /// Sum of Intensity Value: All isotoped peaks are included.
        /// </summary>
        public double IsotopicClusterIntensity
        {
            get{ return _MSPeak.ClusterIntensity;}
        }

        public double CorrectedIntensity
        {
            get
            {
                if (_CorrectedIntensity == 0 && _Points.Count != 0) //No corrected intensity return MostIntenseIntensity
                {
                    return _MSPeak.MostIntenseIntensity;
                }
                else
                {
                    return _CorrectedIntensity;
                }
            }
            set { _CorrectedIntensity = value; }
        }

        /// <summary>
        /// Sum of Intensity Value: The most intens isotoped peak is included only.
        /// </summary>
        public double MostIntenseIntensity
        {
            get{return _MSPeak.MostIntenseIntensity;}
        }
        public double ScanTime
        {
            get { return _Time; }
        }
        public MSPeak Peak
        {
            get {return _MSPeak;}
        }
        public int ScanNum
        {
            get { return _ScanNum; }
        }
        public GlycanCompound GlycanComposition
        {
            get { return _glycanComposition; }
            set { _glycanComposition = value; }
        }  
        public string GlycanKey
        {
            get
            {
                return _glycanComposition.GlycanKey;
            }
        }
    }
}
