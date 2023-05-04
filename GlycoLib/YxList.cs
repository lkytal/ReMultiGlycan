using System;
using System.Collections.Generic;
using System.Text;
using COL.MassLib;
using COL.GlycoLib;
using COL.ProtLib;
namespace COL.GlycoLib
{
    public class YxList
    {
        MSScan _scan;
        List<ProteinInfo> _pInfo;
        float _torelance;
        public YxList(MSScan argScan, List<ProteinInfo> argProtInfo, float argTorelance)
        {
            _scan = argScan;
            _pInfo = argProtInfo;
            _torelance = argTorelance;
        }
        /// <summary>
        /// Scan No,Charge,Peptide Seq, MaxMz,Y1Mz,Y1Intensity,Y2Mz,Y2Intensity,Y3Mz,Y3Intensity,Y4Mz,Y4Intensity
        /// </summary>
        /// <returns></returns>
        public List<string> Process(int argMissCleavage)
        {
            List<string> result = new List<string>();
            AminoAcidMass AAMW = new AminoAcidMass();
            foreach (ProteinInfo p in _pInfo)
            {
                p.CreateCleavage(argMissCleavage);
                List<string> Glycopeptide = p.Glycopeptide(0);
                foreach (string Pep in Glycopeptide)
                {
                    for (int i = 0; i <= 2; i++)
                    {
                        float PeptideMass = AAMW.GetMonoMW(Pep, true);
                        List<float> Peakmz = GetPeakCluster(PeptideMass, _scan.ParentCharge - i);
                        List<int> ClosePeakIdx = new List<int>();
                        int foundpeak = 0;
                        double MaxIntensity = 0.0;
                        double MaxMz = 0.0;
                        foreach (float peak in Peakmz)
                        {
                            int closepeakidx = MassUtility.GetClosestMassIdx(_scan.MSPeaks,peak);
                            ClosePeakIdx.Add(closepeakidx);
                            if (MassUtility.GetMassPPM(peak, _scan.MSPeaks[closepeakidx].MonoMass) < _torelance)
                            {
                                if (_scan.MSPeaks[closepeakidx].MonoIntensity > MaxIntensity)
                                {
                                    MaxIntensity = _scan.MSPeaks[closepeakidx].MonoIntensity;
                                    MaxMz = _scan.MSPeaks[closepeakidx].MonoMass;
                                }
                                foundpeak++;
                            }
                        }
                        if (foundpeak >= 3)
                        {
                            string tmp = _scan.ScanNo + "," + Convert.ToString(_scan.ParentCharge - i) + "," + Pep + "," + MaxMz.ToString() + ",";
                            for (int j = 0; j < 4; j++)
                            {
                                if (MassUtility.GetMassPPM(_scan.MSPeaks[ClosePeakIdx[j]].MonoMass, Peakmz[j]) < _torelance)
                                {
                                    tmp = tmp + _scan.MSPeaks[ClosePeakIdx[j]].MonoMass + "," + (_scan.MSPeaks[ClosePeakIdx[j]].MonoIntensity / MaxIntensity) + ",";
                                }
                                else
                                {
                                    tmp = tmp + ",,";
                                }
                            }
                            result.Add(tmp);
                        }
                    }

                }
            }
            return result;
        }
        private List<float> GetPeakCluster(float argPeptideMono, int argCharge)
        {
            List<float> peakmz = new List<float>();
            //Y1            
            peakmz.Add((argPeptideMono + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.HexNAc, 1) + 1.0078f * argCharge) / argCharge);
            //Y2
            peakmz.Add((argPeptideMono + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.HexNAc, 1) * 2 + 1.0078f * argCharge) / argCharge);
            //Y3
            peakmz.Add((argPeptideMono + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.HexNAc, 1) * 2 + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.Hex, 1) + 1.0078f * argCharge) / argCharge);
            //Y4
            peakmz.Add((argPeptideMono + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.HexNAc, 1) * 2 + GlycanMass.GetGlycanMasswithCharge(Glycan.Type.Hex, 1) * 2 + 1.0078f * argCharge) / argCharge);

            return peakmz;
        }
    }
}
