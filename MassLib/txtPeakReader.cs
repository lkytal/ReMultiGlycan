using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace COL.MassLib
{
    class txtPeakReader
    {
        MSScan  _Scan;
        string _filepath;

        public txtPeakReader(string argFileName)
        {
            _filepath = argFileName;
        }
        public MSScan GetAllPeaks()
        {
            _Scan = new MSScan(0);
            int LineNumber = 0;
            StreamReader sr = new StreamReader(_filepath);
            string tmp;
            double floatout = -0.0f, mz, intensity;
            try
            {
                do
                {
                    tmp = sr.ReadLine();
                    LineNumber++;
                    if (double.TryParse(tmp.Split('\t')[0], out floatout))
                    {
                        mz = Convert.ToDouble(tmp.Split('\t')[0]);
                        intensity = Convert.ToDouble(tmp.Split('\t')[1]);
                        _Scan.AddPoint(new MSPoint(mz, intensity));
                    }

                } while (!sr.EndOfStream);

                if (_Scan.MSPointList.Count == 0)
                {
                    throw new Exception("Reading Peak in txt format error on Line:"+ LineNumber+"!! Please Check input File");
                }
                _Scan.ConvertHashTableToList();
                return _Scan;
            }
            catch
            {

                throw new Exception("Reading Peak in txt format error!! Please Check input File");
            }
            finally
            {
                sr.Close();
            }
            
        }
 
    }
}
