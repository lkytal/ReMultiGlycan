using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
namespace COL.MassLib.MascotParser
{    
    public class MascotDAT
    {
        private string _datfile;
        private string _boundary;
        Dictionary<int, Summary> _SummaryDict; //Key: Query Num;
        Dictionary<int, string> _ModificationsDict ; //Key: Modification Index;
        Dictionary<int, List<Peptides>> _PeptideDict; //Key:Query Num;
        public MascotDAT(string argDATfile)
        {
            _datfile = argDATfile;
           
            Parse();
        }
        private void Parse()
        {
            StreamReader sr = new StreamReader(_datfile);
            string tmpStr;
            do
            {
                tmpStr=sr.ReadLine();
                if (tmpStr == "")
                {
                    continue;
                }
                if (_boundary == null && tmpStr.Contains("boundary="))
                {
                    _boundary = tmpStr.Substring(tmpStr.LastIndexOf('=') + 1);
                    continue;
                }
                if (_boundary!=null && tmpStr.Contains(_boundary))
                {
                    tmpStr = sr.ReadLine();
                    if (tmpStr == null)
                    {
                        break;
                    }
                    //Get Content-Type
                    string ContentType = tmpStr.Substring(tmpStr.LastIndexOf("=") + 1).Replace("\"","");
                    if(ContentType.Contains("query"))
                    {
                        ContentType = "query";
                    }
                    List<string> lstBlockData = new List<string>();
                    do
                    {
                        tmpStr = sr.ReadLine(); 
                        if (tmpStr != "")
                        {
                            lstBlockData.Add(tmpStr);
                        }
                    } while (sr.Peek() !=45 );
                    switch (ContentType)
                    {
                        case  "parameters":
                            ParseParametersBlock(lstBlockData);
                            break;
                        case "masses":
                            ParseMassesBlock(lstBlockData);
                            break;
                        case "quantitation":
                            ParseQuantitationBlock(lstBlockData);
                            break;
                        case "unimod":
                            ParseUnimodBlock(lstBlockData);
                            break;
                        case "header":
                            ParseHeaderBlock(lstBlockData);
                            break;
                        case "summary":
                            ParseSummaryBlock(lstBlockData);
                            break;
                        case "peptides":
                            ParsePeptidesBlock(lstBlockData);
                            break;
                        case "proteins":
                            ParseProteinsBlock(lstBlockData);
                            break;
                        case "query":
                            ParseQueryBlock(lstBlockData);
                            break;
                        case "index":
                            ParseIndexBlock(lstBlockData);
                            break;
                        default:
                            Console.WriteLine(ContentType);
                            break;
                    }
                }

            } while (!sr.EndOfStream);
        }
        private void ParseParametersBlock(List<string> argData)
        {

        }
        private void ParseMassesBlock(List<string> argData)
        {

        }
        private void ParseQuantitationBlock(List<string> argData)
        {

        }
        private void ParseUnimodBlock(List<string> argData)
        {
            _ModificationsDict = new Dictionary<int, string>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(string.Join("\n", argData.ToArray()));
            XmlNamespaceManager xNSMng = new XmlNamespaceManager(xDoc.NameTable);
            xNSMng.AddNamespace("umod", "http://www.unimod.org/xmlns/schema/unimod_2");
            XmlNodeList Nodes = xDoc.SelectNodes("/umod:unimod/umod:modifications/umod:mod", xNSMng);
            
            int ModIdx = 0;
            foreach (XmlNode node in Nodes)
            {
                _ModificationsDict.Add(ModIdx, node.Attributes["title"].Value);
                ModIdx++;
            }
        }
        private void ParseHeaderBlock(List<string> argData)
        {

        }
        private void ParseSummaryBlock(List<string> argData)
        {
             _SummaryDict = new Dictionary<int, Summary>();
             int PreviousNum = 1;
             List<string> tmpLines = new List<string>();
             foreach (string line in argData)
             {
                 if (line.Contains( "num_hits"))
                 {
                     continue;
                 }
                 System.Text.RegularExpressions.Match QueryNumMatch = System.Text.RegularExpressions.Regex.Match( line.Split('=')[0],"\\d+");
                 int QueryNo = Convert.ToInt32(QueryNumMatch.Value);
                 if (PreviousNum != QueryNo)
                 {
                     //Store
                     float PlugHoles=0;
                     float Mass=0;
                     float MZ =0;
                     int Charge =0;
                     int MatchNum=0 ;                 
                     foreach (string tmpLine in tmpLines)   
                     {
                         System.Text.RegularExpressions.Match QueryNameMatch = System.Text.RegularExpressions.Regex.Match(tmpLine.Split('=')[0], "\\D+");
                         if (QueryNameMatch.Value == "qplughole")
                         {
                             PlugHoles = Convert.ToSingle(tmpLine.Split('=')[1]);
                         }
                         else if (QueryNameMatch.Value == "qmass")
                         {
                             Mass = Convert.ToSingle(tmpLine.Split('=')[1]);
                         }
                         else if (QueryNameMatch.Value == "qexp")
                         {
                             MZ = Convert.ToSingle(tmpLine.Split('=')[1].Split(',')[0]);
                             Charge = Convert.ToInt32(tmpLine.Split('=')[1].Split(',')[1].Replace("+",""));
                         }
                         else if (QueryNameMatch.Value == "qmatch")
                         {
                             MatchNum = Convert.ToInt32(tmpLine.Split('=')[1]);
                         }
                     }
                     _SummaryDict.Add(PreviousNum, new Summary(PreviousNum, Mass, MZ, Charge, MatchNum));

                     tmpLines.Clear();
                     PreviousNum = QueryNo;
                 }
                 tmpLines.Add(line);        
             }               
        }

        private void ParsePeptidesBlock(List<string> argData)
        {
            _PeptideDict = new Dictionary<int, List<Peptides>>();
            int PreviousQueryNum = 1;
            List<string> tmpLines = new List<string>();
            foreach (string line in argData)
            {
                System.Text.RegularExpressions.Match QueryNumMatch = System.Text.RegularExpressions.Regex.Match(line.Split('=')[0].Split('_')[0], "\\d+");
                int QueryNum = Convert.ToInt32(QueryNumMatch.Value);
                if (PreviousQueryNum != QueryNum)
                {
                    int PreviousPeptideNumber = 1;
                    List<string> PeptideTmp = new List<string>();
                    //Parse in the same query
                    foreach (string tmpLine in tmpLines)
                    {
                        //Parse different peptide
                        int PeptideNum = Convert.ToInt32(tmpLine.Split('=')[0].Split('_')[1].Substring(1));
                        if (PreviousPeptideNumber != PeptideNum)
                        {
                            string PeptideTerm="";
                            string Primary_NL="";
                            Peptides NewQ = null;
                            foreach (string PeptideLine in PeptideTmp)
                            {
                                if(PeptideLine.Contains("terms"))
                                {
                                    PeptideTerm = PeptideLine.Split('=')[1];
                                }
                                else if(PeptideLine.Contains("primary_nl"))
                                {
                                    Primary_NL  = PeptideLine.Split('=')[1];
                                }
                                else
                                {
                                    NewQ = new Peptides(QueryNum,PeptideLine,PeptideTerm,Primary_NL, _SummaryDict[QueryNum]);
                                }
                            }
                            if (!_PeptideDict.ContainsKey(QueryNum))
                            {
                                _PeptideDict.Add(QueryNum, new List<Peptides>());
                            }
                            _PeptideDict[QueryNum].Add(NewQ);
                            PeptideTmp.Clear();
                            PreviousPeptideNumber = PeptideNum;
                        }
                        PeptideTmp.Add(tmpLine); // add line to store the same peptide
                    }
                    tmpLines.Clear();
                    PreviousQueryNum = QueryNum;
                }
                tmpLines.Add(line);     //Add line to store the same query number   
            }
        }
        private void ParseProteinsBlock(List<string> argData)
        {

        }
        private void ParseQueryBlock(List<string> argData)
        {

        }
        private void ParseIndexBlock(List<string> argData)
        {

        }
    }
}
