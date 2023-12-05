using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
namespace COL.MassLib
{
    public class GetHCDInfo
    {
        private AutoResetEvent autoResetEvt;
        private byte[] RecivedScan;
        private string _FullRawPath;
        private string _RawType;
        private int _ScanNum;
        private int _ExitCode;
        //Thread PipeServerThread;
        public GetHCDInfo(string argRawFilePath, string argRawFileType, int argScanNum)
        {
            _FullRawPath = argRawFilePath;
            _RawType = argRawFileType;
            _ScanNum = argScanNum;

            //PipeServerThread = new Thread(StartNamePipeServer);
            //PipeServerThread.Start();
            //PipeServerThread.Join();
        }

        public HCDInfo HCDInfo()
        {
            string HCDString ="";
            Process GlypIDReader = new Process();
            GlypIDReader.StartInfo.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\GlypIDWrapper.exe";
            GlypIDReader.StartInfo.Arguments = "\"" + _FullRawPath + "\" " + _RawType + " H " + _ScanNum.ToString();
            GlypIDReader.StartInfo.RedirectStandardOutput = true;
            GlypIDReader.StartInfo.RedirectStandardError = true;
            GlypIDReader.StartInfo.UseShellExecute = false;
            GlypIDReader.StartInfo.CreateNoWindow = true;
            GlypIDReader.OutputDataReceived += new DataReceivedEventHandler(
                (s, e) =>
                {
                    HCDString += e.Data+"\n";
                }
            );
            GlypIDReader.Start();
            GlypIDReader.BeginOutputReadLine();
            GlypIDReader.WaitForExit();

            _ExitCode = GlypIDReader.ExitCode;
            if (_ExitCode == -1)
            {
                return null;
            }

            HCDString = HCDString.Split('\n')[1];

            int ScanNum = Convert.ToInt32(HCDString.Split(';')[0]);
            string StrType = HCDString.Split(';')[1];
            double Score = Convert.ToDouble(HCDString.Split(';')[2]);
            enumGlycanType GType = enumGlycanType.NA;
            //     CA = 1, CS, HM, HY, NA 
            if (StrType == "CA")
            {
                GType = enumGlycanType.CA;
            }
            else if (StrType == "CS")
            {
                GType = enumGlycanType.CS;
            }
            else if (StrType == "HM")
            {
                GType = enumGlycanType.HM;
            }
            else if (StrType == "HY")
            {
                GType = enumGlycanType.HY;
            }



            return new HCDInfo(ScanNum,GType,Score);
            
            //autoResetEvt = new AutoResetEvent(false);

            //Thread WrapperThread = new Thread(StartWrapper);

            //WrapperThread.Start();
            //WrapperThread.Join();
            //autoResetEvt.WaitOne();
            
            //HCDInfo hcdinfo = null;

            //if (_ExitCode==1)
            //{
            //    System.Runtime.Serialization.IFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            //    System.IO.MemoryStream memStream = new System.IO.MemoryStream(RecivedScan);
            //    hcdinfo = (HCDInfo)f.Deserialize(memStream);
            //}
            
            //return hcdinfo;

        }
        private void StartWrapper()
        {
            Process GlypIDReader = new Process();
            GlypIDReader.StartInfo.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\GlypIDWrapper.exe";
            GlypIDReader.StartInfo.Arguments = "\"" + _FullRawPath + "\" " + _RawType + " H " + _ScanNum.ToString();
            GlypIDReader.StartInfo.RedirectStandardOutput = true;
            GlypIDReader.StartInfo.UseShellExecute = false;
            GlypIDReader.StartInfo.CreateNoWindow = true;
            GlypIDReader.Start();
            GlypIDReader.WaitForExit();
            _ExitCode = GlypIDReader.ExitCode;
        }
        //PipeServer pipeSrv;
        //private void StartNamePipeServer()
        //{
        //    PipeServer pipeSrv = new PipeServer();
        //    pipeSrv.MessageReceived += new PipeServer.MessageReceivedHandler(MsgRevied);
        //    pipeSrv.Start("\\\\.\\pipe\\GlypIDPipeHCD");
        //}
        //private void MsgRevied(byte[] message)
        //{
        //    autoResetEvt.Set();
        //    RecivedScan = message;
        //    pipeSrv = null;
        //}
    }
}
