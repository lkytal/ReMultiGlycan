using System;
using System.Collections.Generic;
using System.Text;
using COL.MassLib;
namespace COL.GlycoLib
{
    public class YxFinder
    {
        private float _y1;
        private int _charge;
        private int _YxCount;
        private float[] _YxMz;
        private float[] _YxMzIdx;
        public YxFinder(List<MSPeak> argSpectrum, float argY1, int argCharge, float argTolerance)
        {
            _y1 = argY1;
            _YxCount = 0;
            _charge = argCharge;
            GeneratedYxPeak();
            _YxMzIdx = new float[5];
            for (int i = 0; i <= 4; i++)
            {
                _YxMzIdx[i] = -1;
            }

            for (int i = 1; i <= 4; i++)
            {
                int idx = MassUtility.GetClosestMassIdx(argSpectrum,_YxMz[i]);
                if (MassUtility.GetMassPPM(argSpectrum[idx].MonoMass, _YxMz[i]) < argTolerance)
                {
                    _YxMzIdx[i] = idx;
                    _YxCount++;
                }
            }
        }
        public int YxCount
        {
            get { return _YxCount; }
        }
        public float[] YxMZ
        {
            get { return _YxMz; }
        }        
        private void GeneratedYxPeak()
        {
            _YxMz = new float[5];
            _YxMz[1] = _y1;
            _YxMz[2] = _YxMz[1] + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.HexNAc, _charge);
            _YxMz[3] = _YxMz[2] + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.Hex, _charge);
            _YxMz[4] = _YxMz[3] + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.Hex, _charge);
        }
    }
}
