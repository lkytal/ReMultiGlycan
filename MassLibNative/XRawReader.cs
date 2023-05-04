using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    /// <summary>
    /// Use GlypIDEngine dll to read Xcalibur RAW (.raw)
    /// </summary>
    public class XRawReader : IRawFileReader
    {
        private string _fullFilePath;
        private List<MSScan> _scans;
        GlypID.Readers.clsRawData Raw;

        private double _peptideMinBackgroundRatio = 5.0;
        private double _peakBackgroundRatio = 5.0;
        private double _singleToNoiseRatio = 3.0;
        private short _maxCharge = 10;


        GlypID.Peaks.clsPeak[] _cidPeaks = new GlypID.Peaks.clsPeak[1];
        GlypID.Peaks.clsPeak[] _parentPeaks = new GlypID.Peaks.clsPeak[1];

        private static GlypID.HornTransform.clsHornTransform mobjTransform;
        private static GlypID.HornTransform.clsHornTransformParameters mobjTransformParameters;
        private static GlypID.HornTransform.clsHornTransformResults[] _transformResult;


        private static GlypID.Peaks.clsPeakProcessor cidPeakProcessor;
        private static GlypID.Peaks.clsPeakProcessorParameters cidPeakParameters;

        private static GlypID.Peaks.clsPeakProcessor parentPeakProcessor;
        private static GlypID.Peaks.clsPeakProcessorParameters parentPeakParameters;
        
        public GlypID.Readers.clsRawData GlypIDReader
        {
            get { return Raw; }
        }
        public int NumberOfScans
        {
            get { return Raw.GetNumScans(); }
        }
        public string RawFilePath
        {
            get { return _fullFilePath; }
        }
        //public GlypID.Peaks.clsPeakProcessorParameters PeakProcessorParameter
        //{
        //    set
        //    {
        //        _singleToNoiseRatio = value.SignalToNoiseThreshold;
        //        _peakBackgroundRatio = value.PeakBackgroundRatio;
        //    }
        //}
        public void SetPeakProcessorParameter(GlypID.Peaks.clsPeakProcessorParameters argPeakProcessorParameter)
        {
            _singleToNoiseRatio = argPeakProcessorParameter.SignalToNoiseThreshold;
            _peakBackgroundRatio = argPeakProcessorParameter.PeakBackgroundRatio;
        }
        //public GlypID.HornTransform.clsHornTransformParameters TransformParameter
        //{
        //    set
        //    {
        //        _peptideMinBackgroundRatio = value.PeptideMinBackgroundRatio;
        //        _maxCharge = value.MaxCharge;
        //    }
        //}
        public void SetTransformParameter(GlypID.HornTransform.clsHornTransformParameters argTransformParameter)
        {
            _peptideMinBackgroundRatio = argTransformParameter.PeptideMinBackgroundRatio;
            _maxCharge = argTransformParameter.MaxCharge;
        }
        public int GetMsLevel(int argScan)
        {
            return Raw.GetMSLevel(argScan);
        }
  
        public XRawReader(string argFullFilePath)
        {
            _fullFilePath = argFullFilePath;
            Raw = new GlypID.Readers.clsRawData(_fullFilePath, GlypID.Readers.FileType.FINNIGAN);

           mobjTransform = new GlypID.HornTransform.clsHornTransform(); //12KB
            mobjTransformParameters = new GlypID.HornTransform.clsHornTransformParameters(); //4KB
            _transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
            cidPeakProcessor = new GlypID.Peaks.clsPeakProcessor(); //68KB    
            cidPeakParameters = new GlypID.Peaks.clsPeakProcessorParameters();
            parentPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
            parentPeakParameters = new GlypID.Peaks.clsPeakProcessorParameters();

        }

       /*private MSScan GetMSScan(int argScanNo)
        {
            MSScan msScan = new MSScan(argScanNo);         
           // GlypID.Readers.clsRawData rawData = new GlypID.Readers.clsRawData() ; 
            GlypID.HornTransform.clsHornTransform massTransform = new GlypID.HornTransform.clsHornTransform();
            GlypID.Peaks.clsPeakProcessor peakProcessor = new GlypID.Peaks.clsPeakProcessor();

            GlypID.HornTransform.clsHornTransformParameters transform_parameters = new GlypID.HornTransform.clsHornTransformParameters();
            GlypID.Peaks.clsPeakProcessorParameters peak_parameters = new GlypID.Peaks.clsPeakProcessorParameters();           


            // Declarations
            float[] _mzs = null;
            float[] _intensities = null;
            GlypID.Peaks.clsPeak[] peaks; 
            GlypID.HornTransform.clsHornTransformResults[] _transformResults; 

            // Settings
            peak_parameters.SignalToNoiseThreshold = 3.0 ;
            peak_parameters.PeakBackgroundRatio = 5.0 ;

            peakProcessor.SetOptions(peak_parameters) ; 
            peakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
            transform_parameters.PeptideMinBackgroundRatio = 5.0;
            transform_parameters.MaxCharge = 10;
            massTransform.TransformParameters = transform_parameters; 



            // Load
            Raw = new GlypID.Readers.clsRawData(_fullFilePath, GlypID.Readers.FileType.FINNIGAN);


            if (Raw.GetMSLevel(argScanNo) == 1)
                {
                    // Get spectra
                    Raw.GetSpectrum(argScanNo, ref _mzs, ref _intensities);

                    // Get peaks
                    peaks = new GlypID.Peaks.clsPeak[1];
                    peakProcessor.DiscoverPeaks(ref _mzs, ref _intensities, ref peaks, Convert.ToSingle(transform_parameters.MinMZ), Convert.ToSingle(transform_parameters.MaxMZ), true);

                    // Deisotope
                    double min_background_intensity = peakProcessor.GetBackgroundIntensity(ref _intensities);
                    double min_peptide_intensity = min_background_intensity * transform_parameters.PeptideMinBackgroundRatio;

                    _transformResults = new GlypID.HornTransform.clsHornTransformResults[1];
                    massTransform.PerformTransform(Convert.ToSingle(min_background_intensity), Convert.ToSingle(min_peptide_intensity), ref _mzs, ref _intensities, ref peaks, ref _transformResults);

                    Array.Clear(_transformResults, 0, _transformResults.Length);
                    Array.Clear(peaks, 0, peaks.Length);
                }            

            return msScan;
        }*/
        public List<MSScan> ReadScans(int argStart, int argEnd)
        {
            _scans = new List<MSScan>();
            for (int i = argStart; i <= argEnd; i++)
            {
                _scans.Add(GetScanFromFile(i));
            }
            return _scans;
        }
        public MSScan ReadScan(int argScan)
        {
            return GetScanFromFile(argScan);
        }
        public List<MSScan> ReadAllScans()
        {
            int argStart = 1;
            int argEnd = Raw.GetNumScans();
            return ReadScans(argStart, argEnd);
        }
        public void Close()
        {
            Raw.Close();
        }
        public List<MSScan> ReadScanWMSLevel(int argStart, int argEnd, int argMSLevel)
        {
            _scans = new List<MSScan>();
            List<MSScan> tmpScans = ReadScans(argStart, argEnd);
            foreach (MSScan scan in tmpScans)
            {
                if (scan.MsLevel == argMSLevel)
                {
                    _scans.Add(scan);
                }
            }
            return _scans;
        }       

        private MSScan GetScanFromFile(int argScanNo)
        {
            MSScan scan = new MSScan(argScanNo);
            float[] _cidMzs = null;
            float[] _cidIntensities = null;

            Raw.GetSpectrum(argScanNo, ref _cidMzs, ref  _cidIntensities);
            scan.MsLevel = Raw.GetMSLevel(argScanNo);

            double min_peptide_intensity = 0;
            scan.Time = Raw.GetScanTime(scan.ScanNo);
            scan.ScanHeader = Raw.GetScanDescription(scan.ScanNo);
            if (scan.MsLevel != 1)
            {
                float[] _parentRawMzs = null;
                float[] _parentRawIntensitys = null;            

                string Header = Raw.GetScanDescription(argScanNo);
                cidPeakProcessor.ProfileType = GlypID.enmProfileType.CENTROIDED; 
                if (Header.Substring(Header.IndexOf("+") + 1).Trim().StartsWith("p"))
                {
                    cidPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
                }

               // cidPeakProcessor.DiscoverPeaks(ref _cidMzs, ref _cidIntensities, ref _cidPeaks,
               //         Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), false);

                for (int chNum = 0; chNum < _cidMzs.Length; chNum++)
                {
                    scan.MSPeaks.Add(new MSPeak(
                        Convert.ToSingle(_cidMzs[chNum]),
                        Convert.ToSingle(_cidIntensities[chNum])));
                }

                //for (int chNum = 0; chNum < _cidMzs.Length; chNum++)
                //{
                //    scan.MSPeaks.Add(new MSPeak(
                //        Convert.ToSingle(_cidMzs[chNum]),
                //        Convert.ToSingle(_cidIntensities[chNum])));
                //}
              
                // Get parent information
                scan.ParentScanNo = Raw.GetParentScan(scan.ScanNo);

                Raw.GetSpectrum(scan.ParentScanNo, ref _parentRawMzs, ref _parentRawIntensitys);
                parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;
                parentPeakProcessor.DiscoverPeaks(ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), true);
                float _parentBackgroundIntensity = (float)parentPeakProcessor.GetBackgroundIntensity(ref _parentRawIntensitys);
                _transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
                bool found =false;
                if (Raw.IsFTScan(scan.ParentScanNo))
                {
                    // High resolution data
                    found = mobjTransform.FindPrecursorTransform(Convert.ToSingle(_parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(scan.ParentMZ), ref _transformResult);              
                }
                if (!found)//de-isotope fail
                {                    
                    // Low resolution data or bad high res spectra
                    short cs = Raw.GetMonoChargeFromHeader(scan.ScanNo);
                    double monoMZ = Raw.GetMonoMzFromHeader(scan.ScanNo);
                    List<float> ParentMzs = new List<float>(_parentRawMzs);
                    int CloseIdx = MassUtility.GetClosestMassIdx(ParentMzs, Convert.ToSingle(monoMZ));

                    if (cs > 0)
                    {
                        short[] charges = new short[1];
                        charges[0] = cs;
                        mobjTransform.AllocateValuesToTransform(Convert.ToSingle(scan.ParentMZ), Convert.ToInt32(_parentRawIntensitys[CloseIdx]), ref charges, ref _transformResult);
                    }
                    else
                    {
                        // instrument has no charge just store 2 and 3.      
                        short[] charges = new short[2];
                        charges[0] = 2;
                        charges[1] = 3;
                        mobjTransform.AllocateValuesToTransform(Convert.ToSingle(scan.ParentMZ), Convert.ToInt32(_parentRawIntensitys[CloseIdx]), ref charges, ref _transformResult);
                    }
                }

                if (_transformResult[0].mint_peak_index == -1) //De-isotope parent scan
                {
                    //Get parent info
                    MSScan _parentScan = GetScanFromFile(scan.ParentScanNo);
                    float[] _MSMzs = null;
                    float[] _MSIntensities = null;
                
                    Raw.GetSpectrum(scan.ParentScanNo, ref _MSMzs, ref  _MSIntensities);
                    // Now find peaks
                    parentPeakParameters.SignalToNoiseThreshold = 0;
                    parentPeakParameters.PeakBackgroundRatio = 0.01;
                    parentPeakProcessor.SetOptions(parentPeakParameters);
                    parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;

                    parentPeakProcessor.DiscoverPeaks(ref _MSMzs, ref _MSIntensities, ref _cidPeaks,
                                            Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), true);



                    //Look for charge and mono.


                    float[] monoandcharge = FindChargeAndMono(_cidPeaks, Convert.ToSingle(Raw.GetParentMz(scan.ScanNo)), scan.ScanNo);
                    //scan.ParentMonoMW = _parentScan.MSPeaks[ClosedIdx].MonoMass;
                    //scan.ParentAVGMonoMW = _parentScan.MSPeaks[ClosedIdx].;
                    scan.ParentMZ = monoandcharge[0];
                    if (monoandcharge[1] == 0.0f)
                    {
                        scan.ParentCharge = Convert.ToInt32(Raw.GetMonoChargeFromHeader(scan.ParentScanNo));
                    }
                    else
                    {
                        scan.ParentCharge = Convert.ToInt32(monoandcharge[1]);
                    }
                    
                    scan.ParentMonoMW = ( monoandcharge[0] - Atoms.ProtonMass) * monoandcharge[1];

                }
                else
                {
                    scan.ParentMonoMW = (float)_transformResult[0].mdbl_mono_mw;
                    scan.ParentAVGMonoMW = (float)_transformResult[0].mdbl_average_mw;
                    scan.ParentMZ = (float)_transformResult[0].mdbl_mz;
                    scan.ParentCharge = (int)_transformResult[0].mshort_cs;
                }                
                scan.IsCIDScan = Raw.IsCIDScan(argScanNo);
                scan.IsFTScan = Raw.IsFTScan(argScanNo);

                Array.Clear(_transformResult, 0, _transformResult.Length);
                Array.Clear(_cidPeaks, 0, _cidPeaks.Length);
                Array.Clear(_cidMzs, 0, _cidMzs.Length);
                Array.Clear(_cidIntensities, 0, _cidIntensities.Length);
                Array.Clear(_parentRawMzs, 0, _parentRawMzs.Length);
                Array.Clear(_parentRawIntensitys, 0, _parentRawIntensitys.Length);
            }
            else //MS Scan
            {
                scan.ParentMZ = 0.0f;
                double mdbl_current_background_intensity = 0;

                // Now find peaks
                parentPeakParameters.SignalToNoiseThreshold = _singleToNoiseRatio;
                parentPeakParameters.PeakBackgroundRatio = _peakBackgroundRatio;
                parentPeakProcessor.SetOptions(parentPeakParameters);
                parentPeakProcessor.ProfileType = GlypID.enmProfileType.PROFILE;

                parentPeakProcessor.DiscoverPeaks(ref _cidMzs, ref _cidIntensities, ref _cidPeaks,
                                        Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), true);
                mdbl_current_background_intensity = parentPeakProcessor.GetBackgroundIntensity(ref _cidIntensities);

                // Settings
                min_peptide_intensity = mdbl_current_background_intensity * mobjTransformParameters.PeptideMinBackgroundRatio;
                if (mobjTransformParameters.UseAbsolutePeptideIntensity)
                {
                    if (min_peptide_intensity < mobjTransformParameters.AbsolutePeptideIntensity)
                        min_peptide_intensity = mobjTransformParameters.AbsolutePeptideIntensity;
                }
                mobjTransformParameters.PeptideMinBackgroundRatio = _peptideMinBackgroundRatio;
                mobjTransformParameters.MaxCharge = _maxCharge;
                mobjTransform.TransformParameters = mobjTransformParameters;


                //  Now perform deisotoping
                _transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
                mobjTransform.PerformTransform(Convert.ToSingle(mdbl_current_background_intensity), Convert.ToSingle(min_peptide_intensity), ref _cidMzs, ref _cidIntensities, ref _cidPeaks, ref _transformResult);
                // for getting results

                for (int chNum = 0; chNum < _transformResult.Length; chNum++)
                {
                    double sumintensity = 0.0;
                    double mostIntenseIntensity = 0.0;
                    for (int i = 0; i < _transformResult[chNum].marr_isotope_peak_indices.Length; i++)
                    {
                        sumintensity = sumintensity + _cidPeaks[_transformResult[chNum].marr_isotope_peak_indices[i]].mdbl_intensity;
                        if (Math.Abs(_transformResult[chNum].mdbl_most_intense_mw -
                            (_cidPeaks[_transformResult[chNum].marr_isotope_peak_indices[i]].mdbl_mz * _transformResult[chNum].mshort_cs - Atoms.ProtonMass * _transformResult[chNum].mshort_cs))
                            < 1.0 / _transformResult[chNum].mshort_cs)
                        {
                            mostIntenseIntensity = _cidPeaks[_transformResult[chNum].mint_peak_index].mdbl_intensity;
                        }
                    }
                    scan.MSPeaks.Add(new MSPeak(
                    Convert.ToSingle(_transformResult[chNum].mdbl_mono_mw),
                    _transformResult[chNum].mint_mono_intensity,
                    _transformResult[chNum].mshort_cs,
                    Convert.ToSingle(_transformResult[chNum].mdbl_mz),
                    Convert.ToSingle(_transformResult[chNum].mdbl_fit),
                    Convert.ToSingle(_transformResult[chNum].mdbl_most_intense_mw),
                    mostIntenseIntensity,
                    sumintensity
                    ));
                }
                Array.Clear(_transformResult, 0, _transformResult.Length);
                Array.Clear(_cidPeaks, 0, _cidPeaks.Length);
                Array.Clear(_cidMzs, 0, _cidMzs.Length);
                Array.Clear(_cidIntensities, 0, _cidIntensities.Length);
            }
            return scan;
        }
        private float[] FindChargeAndMono(GlypID.Peaks.clsPeak[] argPeaks, float argTargetMZ, int argParentScanNo)
        {
            float[] MonoAndMz = new float[2];

            double interval = 9999.9;
            int ClosedIdx = 0;
            for (int i = 0; i < _cidPeaks.Length; i++)
            {
                if (Math.Abs(_cidPeaks[i].mdbl_mz - argTargetMZ) < interval)
                {
                    interval = Math.Abs(_cidPeaks[i].mdbl_mz - argTargetMZ);
                    ClosedIdx = i;
                }
            }

    
            //Charge
            float testMz=0.0f;
            int MaxMatchedPeak = 2;
            
            for (int i = 1; i <= 6; i++)
            {
                double FirstMonoMz = 0.0;
                int ForwardPeak = 0;
                int BackardPeak = 0;
                //Forward Check
                testMz =  argTargetMZ- 1.0f / (float)i;
                int CheckIdx = ClosedIdx - 1;
                for (int j = 1; j <= 10; j++)
                {
                    if (CheckIdx < 0)
                    {
                        break;
                    }
                    if (Math.Abs(argPeaks[CheckIdx].mdbl_mz - testMz) <= 0.03)
                    {
                        ForwardPeak++;
                        testMz = Convert.ToSingle(argPeaks[CheckIdx].mdbl_mz) - 1.0f / (float)i;
                        FirstMonoMz = argPeaks[CheckIdx].mdbl_mz;
                    }
                    CheckIdx = CheckIdx - 1;
                }

          

                //Backward
                testMz = argTargetMZ + 1.0f / (float)i;
                CheckIdx = ClosedIdx + 1;

                for (int j = 1; j <= 10; j++)
                {
                    if (CheckIdx >= argPeaks.Length)
                    {
                        break;
                    }
                    if (Math.Abs(argPeaks[CheckIdx].mdbl_mz - testMz) <= 0.03)
                    {
                        BackardPeak++;
                        testMz = Convert.ToSingle(argPeaks[CheckIdx].mdbl_mz) + 1.0f / (float)i;                       
                    }
                    CheckIdx = CheckIdx + 1;  
                }

                if (ForwardPeak == 0)
                {
                    FirstMonoMz = argTargetMZ;
                }

                if (ForwardPeak + BackardPeak >= MaxMatchedPeak)
                {
                    MaxMatchedPeak = ForwardPeak + BackardPeak;                    
                    MonoAndMz[0] = Convert.ToSingle( FirstMonoMz);
                    MonoAndMz[1] = i;                                       
                }
            }

            if (MonoAndMz[1] == 0)
            {
                if (interval < 0.01)
                {
                    MonoAndMz[0] = argTargetMZ;
                }
                MonoAndMz[1] = Raw.GetMonoChargeFromHeader(argParentScanNo);
            }

            
            return MonoAndMz;
        }
    }
}
