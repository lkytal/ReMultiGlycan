using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Runtime.InteropServices;
namespace COL.MassLib
{
    public class mzXMLRawReader : IRawFileReader, IDisposable
    {
        bool disposed = false;
        private string _filepath;
        private bool _hasIndex = false;
      
       
        private Dictionary<int, UInt32> _scanOffset; 
        public mzXMLRawReader(string argFilepath)
        {
            _filepath = argFilepath;
            int IdxOffset = FindIndexOffset();
            if (IdxOffset != -1)
            {
                ReadIndexOffset(IdxOffset);
            }
            using (XmlReader reader = XmlReader.Create(_filepath))
            {
                reader.ReadToFollowing("indexOffset");

            }
        }

        ~mzXMLRawReader()
        {
            GC.Collect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>-1 : no index</returns>
        private int FindIndexOffset()
        {
            int StartIndexOffSet = 0;
            using (var reader = new StreamReader(_filepath))
            {
                if (reader.BaseStream.Length > 1024)
                {
                    reader.BaseStream.Seek(-1024, SeekOrigin.End);
                }
                string line =reader.ReadToEnd();
                int idxOffsetStartByte = line.IndexOf("<indexOffset>");
                if (idxOffsetStartByte == -1)
                {
                    return -1;
                }
                idxOffsetStartByte += 13;
                int idxOffsetEndByte = idxOffsetStartByte+1;
                do
                {
                    if (line[idxOffsetEndByte] == '<')
                    {
                        break;
                    }
                    else
                    {
                        idxOffsetEndByte++;
                    }
                } while (true);
                StartIndexOffSet = Convert.ToInt32(line.Substring(idxOffsetStartByte, idxOffsetEndByte - idxOffsetStartByte));
            }
            return StartIndexOffSet;
        }

        private void ReadIndexOffset(int argOffset)
        {
            _scanOffset = new Dictionary<int, uint>();
            
            using (StreamReader sr = new StreamReader(_filepath))
            {
                sr.BaseStream.Seek(argOffset, SeekOrigin.Begin);
                sr.ReadLine(); //title
                string line = sr.ReadLine();
                do
                {
                    int ScanNum = Convert.ToInt32(line.Substring(line.IndexOf("\"") + 1, line.LastIndexOf("\"") - line.IndexOf("\"") - 1));
        
                    UInt32 Offset =Convert.ToUInt32( line.Substring(line.IndexOf(">") + 1, line.LastIndexOf('<') - line.IndexOf(">")-1));

                    _scanOffset.Add(ScanNum, Offset);
                    line = sr.ReadLine();
                } while (!line.Contains("</index>"));
            }
        }
        public int GetMsLevel(int argScan)
        {
            return 0;
        }


        public int NumberOfScans
        {
            get
            {
                if (_scanOffset != null)
                {
                    return _scanOffset.Count - 1;
                }
                return 0; //_xmlDoc.SelectNodes("//*[name()='scan']").Count;
            }
        }

        public MSScan ReadScan(int argScanNum)
        {
            string ScanXML = "";
            if (_scanOffset != null && _scanOffset.ContainsKey(argScanNum)) //has index offset
            {
                using (StreamReader sr = new StreamReader(_filepath))
                {
                    sr.BaseStream.Seek(_scanOffset[argScanNum], SeekOrigin.Begin);
                    string line = sr.ReadLine();
                    do
                    {
                        ScanXML = ScanXML + line+"\n";
                        line = sr.ReadLine();
                    } while (!line.Contains("</scan>"));
                    ScanXML = ScanXML + line + "\n";
                }
            }
            else //no indexOffset
            {
                using (XmlReader reader = XmlReader.Create(_filepath))
                {
                    while (reader.ReadToFollowing("scan"))
                    {
                        if (reader.GetAttribute("num") == argScanNum.ToString())
                        {
                            ScanXML = reader.ReadInnerXml();
                            break;
                        }
                    }
                }
            }
            MSScan _scan =new MSScan(argScanNum);
            XmlDocument doc = new XmlDocument();
            
            doc.LoadXml(ScanXML);
            XmlNode XmlScan = doc.FirstChild;
            int PeakCount = 0;
            foreach (XmlAttribute attribute in XmlScan.Attributes)
            {
                switch (attribute.Name)
                {
                    case "msLevel":
                        _scan.MsLevel = Convert.ToInt32(attribute.Value);
                        break;
                    case"peaksCount":
                        PeakCount = Convert.ToInt32(attribute.Value);
                        break;
                    case "retentionTime":
                        _scan.Time = Convert.ToDouble(attribute.Value.Substring(2, attribute.Value.Length - 3)) / 60.0;
                        break;
                }
            }
            //Peaks
            XmlNode Peaks = XmlScan.FirstChild;
            while (Peaks.Name != "peaks")
            {
                Peaks =Peaks.NextSibling;
            }
            
            int Precision = Convert.ToInt32(Peaks.Attributes["precision"].Value);
            List<MSPoint> tmp = ParsePeakNode(Peaks.InnerText, PeakCount, Precision);
            float[] mz = new float[PeakCount];
            float[] intensity = new float[PeakCount];

            for (int i = 0; i < PeakCount; i++)
            {
                mz[i] = Convert.ToSingle(tmp[i].Mass);
                intensity[i] = Convert.ToSingle(tmp[i].Intensity);
            }
            _scan.RawMZs = mz;
            _scan.RawIntensities = intensity;
            _scan.MZs = mz;
            _scan.Intensities = intensity;
            return _scan;
        }

        public List<MSScan> ReadScans(int argStart, int argEnd)
        {
            List<MSScan> lstScans = new List<MSScan>();
            for (int i = argStart; i <= argEnd; i++)
            {
                lstScans.Add(ReadScan(i));
            }
            return lstScans;
        }

        public List<MSScan> ReadAllScans()
        {
            return ReadScans(1, NumberOfScans);
        }

        public List<MSScan> ReadScanWMSLevel(int argStart, int argEnd, int argMSLevel)
        {
            List<MSScan> lstScans = new List<MSScan>();
            for (int i = argStart; i <= argEnd; i++)
            {
                if (GetMsLevel(i) == argMSLevel)
                {
                    lstScans.Add(ReadScan(i));
                }
            }
            return lstScans;
        }
        /// <summary>
        /// Merge All Peaks to Scan 0
        /// </summary>
        /// <returns></returns>
        //public MSScan GetAllPeaks()
        //{

        //  xmlReader = new XmlTextReader(_filepath);
        //  MSScan _scan = new MSScan(0);
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
        //                                    _scan.AddPoint(p);
        //                                }

        //                            }
        //                        }
        //                    }
        //                    break;
        //            }
        //        }
        //       // _scan.ConvertHashTableToList();
        //        return _scan;
        //    }
        //    catch
        //    {
        //        throw new Exception("Reading Peak in mzXML format error!! Please Check input File");
        //    }
        //}

        private MSScan GetScanFromFile(int argScanNo)
        {
            MSScan _scan = new MSScan(argScanNo);


            return _scan;
        }

        protected List<MSPoint> ParsePeakNode(string peakString, int peaksCount, int argPrecision)
        {
            int offset;
            try
            {
                byte[] decoded = System.Convert.FromBase64String(peakString);

                List<MSPoint> _points = new List<MSPoint>();
                //float mz = 0.0f, intensity = 0.0f;
                if (argPrecision == 32)
                {
                    for (int i = 0; i < peaksCount; i++)
                    {
                        XYPair val;
                        val.x = 0;
                        val.y = 0;
                        offset = i*8;
                        val.b0 = decoded[offset + 7];
                        val.b1 = decoded[offset + 6];
                        val.b2 = decoded[offset + 5];
                        val.b3 = decoded[offset + 4];
                        val.b4 = decoded[offset + 3];
                        val.b5 = decoded[offset + 2];
                        val.b6 = decoded[offset + 1];
                        val.b7 = decoded[offset];
                        _points.Add(new MSPoint(val.x, val.y));
                    }
                }
                else if (argPrecision == 64)
                {
                    for (int i = 0; i < peaksCount; i++)
                    {
                        XYPair_double val;
                        val.x = 0;
                        val.y = 0;
                        offset = i*16;
                        val.b0 = decoded[offset + 15];
                        val.b1 = decoded[offset + 14];
                        val.b2 = decoded[offset + 13];
                        val.b3 = decoded[offset + 12];
                        val.b4 = decoded[offset + 11];
                        val.b5 = decoded[offset + 10];
                        val.b6 = decoded[offset + 9];
                        val.b7 = decoded[offset + 8];
                        val.b8 = decoded[offset + 7];
                        val.b9 = decoded[offset + 6];
                        val.b10 = decoded[offset + 5];
                        val.b11 = decoded[offset + 4];
                        val.b12 = decoded[offset + 3];
                        val.b13 = decoded[offset + 2];
                        val.b14 = decoded[offset + 1];
                        val.b15 = decoded[offset];
                        _points.Add(new MSPoint(
                            Convert.ToSingle(val.x), 
                            Convert.ToSingle(val.y)));
                    }
                }
                else
                {
                    throw new Exception("Precision not support");
                }
                return _points;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
        private struct XYPair
        {
            [FieldOffset(0)]
            public byte b0;
            [FieldOffset(1)]
            public byte b1;
            [FieldOffset(2)]
            public byte b2;
            [FieldOffset(3)]
            public byte b3;
            [FieldOffset(4)]
            public byte b4;
            [FieldOffset(5)]
            public byte b5;
            [FieldOffset(6)]
            public byte b6;
            [FieldOffset(7)]
            public byte b7;

            [FieldOffset(4)]
            public float x;
            [FieldOffset(0)]
            public float y;
        }
        [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
        private struct XYPair_double
        {
            [FieldOffset(0)]
            public byte b0;
            [FieldOffset(1)]
            public byte b1;
            [FieldOffset(2)]
            public byte b2;
            [FieldOffset(3)]
            public byte b3;
            [FieldOffset(4)]
            public byte b4;
            [FieldOffset(5)]
            public byte b5;
            [FieldOffset(6)]
            public byte b6;
            [FieldOffset(7)]
            public byte b7;
            [FieldOffset(8)]
            public byte b8;
            [FieldOffset(9)]
            public byte b9;
            [FieldOffset(10)]
            public byte b10;
            [FieldOffset(11)]
            public byte b11;
            [FieldOffset(12)]
            public byte b12;
            [FieldOffset(13)]
            public byte b13;
            [FieldOffset(14)]
            public byte b14;
            [FieldOffset(15)]
            public byte b15;

            [FieldOffset(8)]
            public double x;
            [FieldOffset(0)]
            public double y;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                // Free any other managed objects here. 
            }
            // Free any unmanaged objects here. 
            disposed = true;
        }
    }
}
