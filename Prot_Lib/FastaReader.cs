using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace COL.ProtLib
{
    public class FastaReader
    {
        /// <summary>
        /// Read Fasta file into List<ProteinInfo>
        /// </summary>
        /// <param name="argFullPath">Fasta file full path</param>
        /// <returns></returns>
        public static List<ProteinInfo> ReadFasta(string argFullPath)
        {
            StreamReader SR = new StreamReader(argFullPath);
            List<ProteinInfo> ProteinInfo = new List<ProteinInfo>();
            string title = "";
            string sequence = "";
            do
            {
                string tmp = SR.ReadLine().Trim();
                if (tmp.StartsWith(">"))
                {
                    if (title != "")
                    {
                        ProteinInfo.Add(new ProteinInfo(title, sequence));
                    }
                    title = tmp.Substring(1); 
                    sequence = "";
                }
                else
                {
                    sequence = sequence + tmp;
                }
            } while (!SR.EndOfStream);
            ProteinInfo.Add(new ProteinInfo(title, sequence));
            SR.Close();
            return ProteinInfo;
        }
    }
}
