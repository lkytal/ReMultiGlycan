using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
namespace COL.MassLib
{
    /// <summary>
    /// Need to rewrite
    /// </summary>
    public class mzXMLReader : IRawFileReader
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
            _peakBackgroundRatio = argTransformParameter.PeptideMinBackgroundRatio;
            _maxCharge = argTransformParameter.MaxCharge;
        }
        public int GetMsLevel(int argScan)
        {
            return Raw.GetMSLevel(argScan);
        }

        public mzXMLReader(string argFullFilePath)
        {
            _fullFilePath = argFullFilePath;
            Raw = new GlypID.Readers.clsRawData(_fullFilePath, GlypID.Readers.FileType.MZXMLRAWDATA);

            mobjTransform = new GlypID.HornTransform.clsHornTransform(); //12KB
            mobjTransformParameters = new GlypID.HornTransform.clsHornTransformParameters(); //4KB
            _transformResult = new GlypID.HornTransform.clsHornTransformResults[1];
            cidPeakProcessor = new GlypID.Peaks.clsPeakProcessor(); //68KB    
            cidPeakParameters = new GlypID.Peaks.clsPeakProcessorParameters();
            parentPeakProcessor = new GlypID.Peaks.clsPeakProcessor();
            parentPeakParameters = new GlypID.Peaks.clsPeakProcessorParameters();

        }

        private MSScan GetMSScan(int argScanNo)
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
            peak_parameters.SignalToNoiseThreshold = 3.0;
            peak_parameters.PeakBackgroundRatio = 5.0;

            peakProcessor.SetOptions(peak_parameters);
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
        }
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

                scan.ParentMZ = (float)Raw.GetParentMz(argScanNo);

                cidPeakProcessor.ProfileType = GlypID.enmProfileType.CENTROIDED;
                cidPeakProcessor.DiscoverPeaks(ref _cidMzs, ref _cidIntensities, ref _cidPeaks,
                        Convert.ToSingle(mobjTransformParameters.MinMZ), Convert.ToSingle(mobjTransformParameters.MaxMZ), false);

                for (int chNum = 0; chNum < _cidPeaks.Length; chNum++)
                {
                    scan.MSPeaks.Add(new MSPeak(
                        Convert.ToSingle(_cidPeaks[chNum].mdbl_mz),
                        Convert.ToSingle(_cidPeaks[chNum].mdbl_intensity)));
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

                bool found = mobjTransform.FindPrecursorTransform(Convert.ToSingle(_parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(scan.ParentMZ), ref _transformResult);
                if (Raw.IsFTScan(scan.ParentScanNo))
                {
                    // High resolution data
                    found = mobjTransform.FindPrecursorTransform(Convert.ToSingle(_parentBackgroundIntensity), Convert.ToSingle(min_peptide_intensity), ref _parentRawMzs, ref _parentRawIntensitys, ref _parentPeaks, Convert.ToSingle(scan.ParentMZ), ref _transformResult);
                }
                //if (!found) //de-isotope fail
                //{
                //    // Low resolution data or bad high res spectra
                //    short cs = Raw.GetMonoChargeFromHeader(scan.ScanNo);
                //    if (cs > 0)
                //    {
                //        short[] charges = new short[1];
                //        charges[0] = cs;
                //        mobjTransform.AllocateValuesToTransform(Convert.ToSingle(scan.ParentMZ), ref charges, ref _transformResult);
                //    }
                //    else
                //    {
                //        // instrument has no charge just store 2 and 3.      
                //        short[] charges = new short[2];
                //        charges[0] = 2;
                //        charges[1] = 3;
                //        mobjTransform.AllocateValuesToTransform(Convert.ToSingle(scan.ParentMZ), ref charges, ref _transformResult);
                //    }
                //}
                scan.ParentMonoMW = (float)_transformResult[0].mdbl_mono_mw;
                scan.ParentAVGMonoMW = (float)_transformResult[0].mdbl_average_mw;
                scan.ParentCharge = (int)_transformResult[0].mshort_cs;

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
                            (_cidPeaks[_transformResult[chNum].marr_isotope_peak_indices[i]].mdbl_mz * _transformResult[chNum].mshort_cs - Atoms.ProtonMass * _transformResult[chNum].mshort_cs)
                            ) < 1.0 / _transformResult[chNum].mshort_cs
                            )
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
                    sumintensity));
                }
                Array.Clear(_transformResult, 0, _transformResult.Length);
                Array.Clear(_cidPeaks, 0, _cidPeaks.Length);
                Array.Clear(_cidMzs, 0, _cidMzs.Length);
                Array.Clear(_cidIntensities, 0, _cidIntensities.Length);
            }
            return scan;
        }
        //string _filepath;
        //MSScan _scan;
        //public mzXMLReader(string filepath)
        //{
        //    _filepath = filepath;
        //}
        ///// <summary>
        ///// Merge All Peaks to Scan 0
        ///// </summary>
        ///// <returns></returns>
        //public MSScan GetAllPeaks()
        //{

        //    XmlTextReader xmlReader = new XmlTextReader(_filepath);
        //    _scan = new MSScan(0);
        //    //List<MSPoint> _msp = new List<MSPoint>();

        //    xmlReader.XmlResolver = null;
        //    try
        //    {
        //        while (xmlReader.Read())
        //        {

        //            switch (xmlReader.NodeType)
        //            {
        //                case XmlNodeType.Element: // The node is an element.
        //                    if (xmlReader.Name == "scan")
        //                    {

        //                        if (xmlReader.GetAttribute("msLevel") == "1")
        //                        {
        //                            int PeakCount = Convert.ToInt32(xmlReader.GetAttribute("peaksCount"));
        //                            int ScanNum = Convert.ToInt32(xmlReader.GetAttribute("num"));
        //                            xmlReader.Read();
        //                            xmlReader.Read();
        //                            if (xmlReader.Name == "peaks")
        //                            {
        //                                int Precision = Convert.ToInt32(xmlReader.GetAttribute("precision"));
        //                                xmlReader.Read();
        //                                string peakString = xmlReader.Value;
        //                                List<MSPoint> tmp = ParsePeakNode(peakString, PeakCount);
        //                                foreach (MSPoint p in tmp)
        //                                {
        //                                  //  _scan.AddPoint(p);
        //                                }

        //                            }
        //                        }
        //                    }
        //                    break;
        //            }
        //        }
        //    //    _scan.ConvertHashTableToList();
        //        return _scan;
        //    }
        //    catch
        //    {
        //        throw new Exception("Reading Peak in mzXML format error!! Please Check input File");
        //    }
        //}
        //protected List<MSPoint> ParsePeakNode(string peakString, int peaksCount)
        //{
        //    int offset;
        //    try
        //    {
        //        byte[] decoded = System.Convert.FromBase64String(peakString);

        //        List<MSPoint> _points = new List<MSPoint>();
        //        //float mz = 0.0f, intensity = 0.0f;
        //        for (int i = 0; i < peaksCount; i++)
        //        {

        //            //Array.Reverse(decoded, i * 8, 4);
        //            //Array.Reverse(decoded, i * 8 + 4, 4);
        //            XYPair val;
        //            val.x = 0;
        //            val.y = 0;
        //            offset = i * 8;
        //            val.b0 = decoded[offset + 7];
        //            val.b1 = decoded[offset + 6];
        //            val.b2 = decoded[offset + 5];
        //            val.b3 = decoded[offset + 4];
        //            val.b4 = decoded[offset + 3];
        //            val.b5 = decoded[offset + 2];
        //            val.b6 = decoded[offset + 1];
        //            val.b7 = decoded[offset];
        //            _points.Add(new MSPoint(val.x, val.y));
        //        }

        //        return _points;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception(e.Message);
        //    }
        //}


        //[System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
        //private struct XYPair
        //{
        //    [FieldOffset(0)]
        //    public byte b0;
        //    [FieldOffset(1)]
        //    public byte b1;
        //    [FieldOffset(2)]
        //    public byte b2;
        //    [FieldOffset(3)]
        //    public byte b3;
        //    [FieldOffset(4)]
        //    public byte b4;
        //    [FieldOffset(5)]
        //    public byte b5;
        //    [FieldOffset(6)]
        //    public byte b6;
        //    [FieldOffset(7)]
        //    public byte b7;

        //    [FieldOffset(4)]
        //    public float x;
        //    [FieldOffset(0)]
        //    public float y;
        //}
    }
}
