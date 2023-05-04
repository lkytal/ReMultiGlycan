using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    public class Class1
    {
         private string _filename;
        GlypID.Readers.clsRawData Raw;
        private List<Class1> _lstClass1;
        private GlypID.Peaks.clsPeak[] _parentPeaks;
        private float[] _parentRawMzs;
        private float[] _parentRawIntensitys;
        private int _parentScanNum = 0;
        private int _parentCharge = 0;
        private float _parentMz = 0;
        private float _parentMonoMW = 0;
        private float _parentBackgroundIntensity = 0;

        private GlypID.Peaks.clsPeak[] _cidPeaks;
        private int _scanNum = 0;        
        private int _msLevel = 0; 
        private float[] _cidMzs;
        private float[] _cidIntensities;
        private int  _maxIntensityIdx;
        
        private GlypID.HornTransform.clsHornTransformResults[] _transformResult;
        public List<Class1> Class1s
        {
            get { return _lstClass1; }
        }
        public int ParentScanNumber
        {
            get { return _parentScanNum; }
        }
        public int ParentCharge
        {
            get { return _parentCharge; }
        }
        public float ParentMZ
        {
            get { return _parentMz; }
        }
        public int ScanNum
        {
            get { return _scanNum; }
        }
        public int MSLevel
        {
            get { return _msLevel; }
        }
        public float[] ScanMZs
        {
            get { return _cidMzs; }
        }
        public float[] ScanIntensities
        {
            get { return _cidIntensities; }
        }
        public float ParentMonoMW
        {
            get { return _parentMonoMW; }
        }
        public int MaxIntensityIdx
        {
            get { return _maxIntensityIdx; }
        }
        public Class1()
        {
        }
        public Class1(string argFileName,int argScanNo)
        {
            _scanNum = argScanNo;
            _filename = argFileName;
            GlypID.HornTransform.clsHornTransform Transform = new GlypID.HornTransform.clsHornTransform();
            GlypID.Peaks.clsPeakProcessor cidPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
            GlypID.Peaks.clsPeakProcessor parentPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
            GlypID.HornTransform.clsHornTransformParameters transform_parameters = new GlypID.HornTransform.clsHornTransformParameters();
            GlypID.Peaks.clsPeakProcessorParameters peak_parameters = new GlypID.Peaks.clsPeakProcessorParameters();
            Raw = new GlypID.Readers.clsRawData(_filename, GlypID.Readers.FileType.FINNIGAN);
           
            Raw.GetSpectrum(_scanNum, ref _cidMzs, ref _cidIntensities);

            Transform.TransformParameters = transform_parameters;

            _msLevel = Raw.GetMSLevel(_scanNum);

            if (_msLevel > 1)
            {
                _parentScanNum = Raw.GetParentScan(_scanNum);
                _parentMz = (float)Raw.GetParentMz(_scanNum);
            }
            else
            {
                return; //MS scan                  
            }
            Raw.GetSpectrum(_parentScanNum, ref _parentRawMzs, ref _parentRawIntensitys);

            _cidPeaks = new GlypID.Peaks.clsPeak[1];
            cidPeakProcessor.ProfileType = GlypID.enmProfileType.CENTROIDED;
            cidPeakProcessor.DiscoverPeaks(ref _cidMzs, ref _cidIntensities, ref _cidPeaks,
                        Convert.ToSingle(transform_parameters.MinMZ), Convert.ToSingle(transform_parameters.MaxMZ), false);
            _parentPeaks = new GlypID.Peaks.clsPeak[1];
            parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
            parentPeakProcessor.DiscoverPeaks(ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks,
                        Convert.ToSingle(transform_parameters.MinMZ), Convert.ToSingle(transform_parameters.MaxMZ), true);
            _parentBackgroundIntensity = (float)parentPeakProcessor.GetBackgroundIntensity(ref _parentRawIntensitys);
            double min_peptide_intensity = _parentBackgroundIntensity * transform_parameters.PeptideMinBackgroundRatio;

            if (transform_parameters.UseAbsolutePeptideIntensity)
            {
                if (min_peptide_intensity < transform_parameters.AbsolutePeptideIntensity)
                    min_peptide_intensity = transform_parameters.AbsolutePeptideIntensity;
            }
            _transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
            bool found = Transform.FindPrecursorTransform(Convert.ToSingle(_parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(_parentMz), ref _transformResult);
            if (Raw.IsFTScan(_parentScanNum))
            {
                // High resolution data
                found = Transform.FindPrecursorTransform(Convert.ToSingle(_parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(_parentMz), ref _transformResult);
            }
            if (!found)
            {
                // Low resolution data or bad high res spectra
                short cs = Raw.GetMonoChargeFromHeader(_scanNum);
                if (cs > 0)
                {
                    short[] charges = new short[1];
                    charges[0] = cs;
                    Transform.AllocateValuesToTransform(Convert.ToSingle(_parentMz),500, ref charges, ref _transformResult);

                }
                else
                {
                    // instrument has no charge just store 2 and 3.      
                    short[] charges = new short[2];
                    charges[0] = 2;
                    charges[1] = 3;
                    Transform.AllocateValuesToTransform(Convert.ToSingle(_parentMz), 500,ref charges, ref _transformResult);
                }
            }

            _parentMonoMW = (float)_transformResult[0].mdbl_mono_mw;
            _parentCharge = (int)_transformResult[0].mshort_cs;
            MaxIntensityPeak();
        
            Raw.Close();
        }
        public List<Class1> GetMultipleClass1(int argStartScan, int argEndScan)
        {
            _lstClass1 = new List<Class1>();

            GlypID.HornTransform.clsHornTransform Transform = new GlypID.HornTransform.clsHornTransform();
            GlypID.Peaks.clsPeakProcessor cidPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
            GlypID.Peaks.clsPeakProcessor parentPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
            GlypID.HornTransform.clsHornTransformParameters transform_parameters = new GlypID.HornTransform.clsHornTransformParameters();
            GlypID.Peaks.clsPeakProcessorParameters peak_parameters = new GlypID.Peaks.clsPeakProcessorParameters();
            Raw = new GlypID.Readers.clsRawData(_filename, GlypID.Readers.FileType.FINNIGAN);

            for (int i = argStartScan; i <= argEndScan; i++)
            {
                Class1 _tmpScan = new Class1();
                
                int _scanNum = i;
                _tmpScan._scanNum = _scanNum;
                Raw.GetSpectrum(_scanNum, ref _tmpScan._cidMzs, ref _tmpScan._cidIntensities);
                Transform.TransformParameters = transform_parameters;
                _tmpScan._msLevel = Raw.GetMSLevel(_scanNum);
                if (_tmpScan.MSLevel > 1)
                {
                    _tmpScan._parentScanNum = Raw.GetParentScan(_scanNum);
                    _tmpScan._parentMz = (float)Raw.GetParentMz(_scanNum);
                }
                else
                {
                  continue; //MS scan 
                }

                Raw.GetSpectrum(_tmpScan._parentScanNum, ref _tmpScan._parentRawMzs, ref _tmpScan._parentRawIntensitys);

                _tmpScan._cidPeaks = new GlypID.Peaks.clsPeak[1];
                cidPeakProcessor.ProfileType = GlypID.enmProfileType.CENTROIDED;
                cidPeakProcessor.DiscoverPeaks(ref _tmpScan._cidMzs, ref _tmpScan._cidIntensities, ref _tmpScan._cidPeaks,
                            Convert.ToSingle(transform_parameters.MinMZ), Convert.ToSingle(transform_parameters.MaxMZ), false);
                _tmpScan._parentPeaks = new GlypID.Peaks.clsPeak[1];
                parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
                parentPeakProcessor.DiscoverPeaks(ref _tmpScan._parentRawMzs, ref _tmpScan._parentRawIntensitys, ref _tmpScan._parentPeaks,
                            Convert.ToSingle(transform_parameters.MinMZ), Convert.ToSingle(transform_parameters.MaxMZ), true);
                _tmpScan._parentBackgroundIntensity = (float)parentPeakProcessor.GetBackgroundIntensity(ref _tmpScan._parentRawIntensitys);
                double min_peptide_intensity = _tmpScan._parentBackgroundIntensity * transform_parameters.PeptideMinBackgroundRatio;

                if (transform_parameters.UseAbsolutePeptideIntensity)
                {
                    if (min_peptide_intensity < transform_parameters.AbsolutePeptideIntensity)
                        min_peptide_intensity = transform_parameters.AbsolutePeptideIntensity;
                }
                _tmpScan._transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
                bool found = Transform.FindPrecursorTransform(Convert.ToSingle(_tmpScan._parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _tmpScan._parentRawMzs, ref _tmpScan._parentRawIntensitys, ref _tmpScan._parentPeaks, Convert.ToSingle(_tmpScan._parentMz), ref _tmpScan._transformResult);
                if (Raw.IsFTScan(_tmpScan._parentScanNum))
                {
                    // High resolution data
                    found = Transform.FindPrecursorTransform(Convert.ToSingle(_tmpScan._parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _tmpScan._parentRawMzs, ref _tmpScan._parentRawIntensitys, ref _tmpScan._parentPeaks, Convert.ToSingle(_tmpScan._parentMz), ref _tmpScan._transformResult);
                }
                if (!found)
                {
                    // Low resolution data or bad high res spectra
                    short cs = Raw.GetMonoChargeFromHeader(_scanNum);
                    if (cs > 0)
                    {
                        short[] charges = new short[1];
                        charges[0] = cs;
                        Transform.AllocateValuesToTransform(Convert.ToSingle(_tmpScan._parentMz), ref charges, ref _tmpScan._transformResult);

                    }
                    else
                    {
                        // instrument has no charge just store 2 and 3.      
                        short[] charges = new short[2];
                        charges[0] = 2;
                        charges[1] = 3;
                        Transform.AllocateValuesToTransform(Convert.ToSingle(_tmpScan._parentMz), ref charges, ref _tmpScan._transformResult);
                    }
                }
                _tmpScan._parentMonoMW = (float)_tmpScan._transformResult[0].mdbl_mono_mw;
                _tmpScan._parentCharge = (int)_tmpScan._transformResult[0].mshort_cs;
                _tmpScan.MaxIntensityPeak();
                _lstClass1.Add(_tmpScan);
            }
            Raw.Close();
            return _lstClass1;
        }
        public Class1(string argFileName)
        {
            _filename = argFileName;
        }
        private void MaxIntensityPeak()
        {
            float maxValue = 0.0f;
            int maxIdx = 0;
            for (int i = 0; i < _cidMzs.Length; i++)
            {
                if (_cidIntensities[i] > maxValue)
                {
                    maxIdx = i;
                    maxValue = _cidIntensities[i];
                }
            }
            _maxIntensityIdx = maxIdx;
        }
        public int FindClosedPeakIdx(float argMz)
        {
            int min = 0;
            int max = _cidMzs.Length - 1;
            int mid = -1;

            if (_cidMzs[max] < argMz)
            {
                return max;
            }
            if (_cidMzs[min] > argMz)
            {
                return min;
            }

            do
            {
                mid = min + (max - min) / 2;
                if (argMz > _cidMzs[mid])
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
            } while (min < max);
            float MinInterval = 1000.0f;
            int MinIntervalIdx = 0;
            for (int i = mid - 5; i < mid + 5; i++)
            {
                if (i < 0)
                {
                    i = 0;
                }
                if (i >= _cidMzs.Length)
                {
                    break;
                }
                if (Math.Abs(argMz - _cidMzs[i]) < MinInterval)
                {
                    MinInterval = Math.Abs(argMz - _cidMzs[i]) ;
                    MinIntervalIdx = i;
                }
            }
            return MinIntervalIdx;

        }
    }
}
