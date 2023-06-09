﻿using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MultiGlycan
{
    class CandidatePeak  : IComparable<CandidatePeak>
    {
        /// Cluster of glycan
        /// Z = 1 [M+H]     [M+NH4]
        /// Z = 2 [M+2H]   [M+NH4+H]	    [M+2NH4]
        /// Z = 3 [M+3H]	[M+NH4+2H]	[M+2NH4+H] 	[M+3NH4]
        /// Z = 4 [M+4H]	[M+NH4+3H]	[M+2NH4+2H]	[M+3NH4+H]	[M+4NH4]
        private int _charge;
        private float _adductMass;
        private int _adductNo;
        private GlycoLib.GlycanCompound _glycanComposition;
        private string _adductLabel;
        public CandidatePeak(GlycoLib.GlycanCompound argCompound, int argCharge, float argAdductMass, int argAdductNo, string argAdductLabel)
        {
            _glycanComposition = (GlycoLib.GlycanCompound)argCompound.Clone();
            _charge = argCharge;
            _adductMass = argAdductMass;
            _adductNo = argAdductNo;
            _adductLabel = argAdductLabel;
        }
        /// <summary>
        /// How many adducts in this candidate?
        /// </summary>
        public int AdductNo
        {
            get { return _adductNo; }
        }
        public string AdductLabel
        {
            get { return _adductLabel; }
        }
        public int Charge
        {
            get { return _charge; }
        }
        public GlycoLib.enumLabelingTag GlycanLabelTag
        {
            get { return _glycanComposition.LabelingTag; }
        }
        public GlycoLib.GlycanCompound GlycanComposition
        {
            get { return _glycanComposition; }
        }
        public float AdductMass
        {
            get { return _adductMass; }
        }
        public float TotalMZ
        {
            get {                
                return (Convert.ToSingle(_glycanComposition.MonoMass) + _adductMass * _adductNo + (_charge - _adductNo) * MassLib.Atoms.ProtonMass) / _charge;
                  }
        }
        public COL.GlycoLib.enumLabelingTag LabelingTag
        {
            get { return _glycanComposition.LabelingTag; }
        }
        public string GlycanKey
        {
            get
            {
                return _glycanComposition.NoOfHexNAc.ToString() + "-" +
                              _glycanComposition.NoOfHex.ToString() + "-" +
                              _glycanComposition.NoOfDeHex.ToString() + "-" +
                              _glycanComposition.NoOfSia.ToString();
            }
        }
        
        public int CompareTo(CandidatePeak other)
        {
            if (this.TotalMZ > other.TotalMZ) return 1;
            else if (this.TotalMZ < other.TotalMZ) return -1;
            else return 0;
        }
    }
}
