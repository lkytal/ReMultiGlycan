using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Collections;
namespace COL.GlycoLib
{
    public class ReadGlycanListFromFile
    {
        public static List<GlycanCompound> ReadGlycanList(string argGlycanFile, bool argIsPermethylated,bool argIsHuman, bool argReducedReducingEnd)
        {
            return ReadGlycanList(argGlycanFile, argIsPermethylated, false, true, argIsHuman, argReducedReducingEnd);
        }
        public static List<GlycanCompound> ReadGlycanList(string argGlycanFile, bool argIsPermethylated, bool argIsDeuterium, bool argIsSodium, bool argIsHuman, bool argReducedReducingEnd)
        {

            List<GlycanCompound> _GlycanList = new List<GlycanCompound>();
            StreamReader sr;
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //sr = new StreamReader(assembly.GetManifestResourceStream( "MutliNGlycanFitControls.Properties.Resources.combinations.txt"));
            int LineNumber = 0;
            sr = new StreamReader(argGlycanFile);

            string tmp; // temp line for processing
            tmp = sr.ReadLine();
            LineNumber++;
            Hashtable compindex = new Hashtable(); //Glycan Type index.



            //Read the title
            string[] splittmp = tmp.Trim().Split(',');
            try
            {
                for (int i = 0; i < splittmp.Length; i++)
                {
                    if (splittmp[i].ToLower() == "neunac" || splittmp[i].ToLower() == "neungc" || splittmp[i].ToLower() == "sialic")
                    {
                        compindex.Add("sia", i);
                        continue;
                    }
                    if (splittmp[i].ToLower() != "hexnac" && splittmp[i].ToLower() != "hex" && splittmp[i].ToLower() != "dehex" && splittmp[i].ToLower() != "sia" && splittmp[i].ToLower() != "linearregression_slope" && splittmp[i].ToLower() != "linearregression_intercept")
                    {
                        throw new Exception("Glycan list file title error. (Use:HexNAc,Hex,DeHex,Sia,NeuNAc,NeuNGc)");
                    }
                    compindex.Add(splittmp[i].ToLower(), i);
                }
            }
            catch (Exception ex)
            {
                sr.Close();
                throw ex;
            }
            int processed_count = 0;

            //Read the list    
            try
            {
                do
                {
                    tmp = sr.ReadLine();
                    LineNumber++;
                    splittmp = tmp.Trim().Split(',');
                    _GlycanList.Add(new GlycanCompound(Convert.ToInt32(splittmp[(int)compindex["hexnac"]]),
                                             Convert.ToInt32(splittmp[(int)compindex["hex"]]),
                                             Convert.ToInt32(splittmp[(int)compindex["dehex"]]),
                                             Convert.ToInt32(splittmp[(int)compindex["sia"]]),
                                             argIsPermethylated,
                                             argIsDeuterium,
                                             argReducedReducingEnd,
                                             false,
                                             true                                             
                                             ));
                    if (compindex.ContainsKey("linearregression_slope")) //has LinearRegression Data
                    {
                        float outSlope = -1;
                        if(float.TryParse(splittmp[(int)compindex["linearregression_slope"]], out outSlope))
                        {
                            _GlycanList[_GlycanList.Count - 1].LinearRegSlope = outSlope;
                        }
                        float outIntercept = -1;
                        if(float.TryParse(splittmp[(int)compindex["linearregression_intercept"]], out outIntercept))
                        {
                            _GlycanList[_GlycanList.Count - 1].LinearRegIntercept = outIntercept;
                        }
                    }
                    processed_count++;
                } while (!sr.EndOfStream);
            }
            catch (Exception ex)
            {
                throw new Exception("Glycan list file reading error on Line:" + LineNumber + ". Please check input file. (" + ex.Message + ")");
            }
            finally
            {
                sr.Close();
            }

            if (_GlycanList.Count == 0)
            {
                throw new Exception("Glycan list file reading error. Please check input file.");
            }
           _GlycanList.Sort();
           return _GlycanList;
        }

        public static List<GlycanCompound> ReadGlycanList(string argGlycanString)
        {
            List<GlycanCompound> _GlycanList = new List<GlycanCompound>();
            string[] tmpGlycanFile = argGlycanString.Split('\n');
            int LineNumber = 0;
            string tmp; // temp line for processing
            tmp = tmpGlycanFile[0].Trim();
            LineNumber++;
            Hashtable compindex = new Hashtable(); //Glycan Type index.



            //Read the title
            string[] splittmp = tmp.Trim().Split(',');
            try
            {
                for (int i = 0; i < splittmp.Length; i++)
                {
                    if (splittmp[i].ToLower() == "neunac" || splittmp[i].ToLower() == "neungc" || splittmp[i].ToLower() == "sialic")
                    {
                        compindex.Add("sia", i);
                        continue;
                    }
                    if (splittmp[i].ToLower() != "hexnac" && splittmp[i].ToLower() != "hex" && splittmp[i].ToLower() != "dehex" && splittmp[i].ToLower() != "sia")
                    {
                        throw new Exception("Glycan list file title error. (Use:HexNAc,Hex,DeHex,Sia,NeuNAc,NeuNGc)");
                    }
                    compindex.Add(splittmp[i].ToLower(), i);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            int processed_count = 0;
            for (int i = 1; i < tmpGlycanFile.Length; i++)
            {
                //Read the list    
                try
                {

                        tmp = tmpGlycanFile[i].Trim();
                        LineNumber++;
                        splittmp = tmp.Trim().Split(',');
                        _GlycanList.Add(new GlycanCompound(Convert.ToInt32(splittmp[(int)compindex["hexnac"]]),
                                                 Convert.ToInt32(splittmp[(int)compindex["hex"]]),
                                                 Convert.ToInt32(splittmp[(int)compindex["dehex"]]),
                                                 Convert.ToInt32(splittmp[(int)compindex["sia"]]),
                                                 false,
                                                 false,
                                                 false,
                                                 false,
                                                 true
                                                 ));
                        processed_count++;
                }
                catch (Exception ex)
                {
                    throw new Exception("Glycan list file reading error on Line:" + LineNumber + ". Please check input file. (" + ex.Message + ")");
                }
            }
            if (_GlycanList.Count == 0)
            {
                throw new Exception("Glycan list file reading error. Please check input file.");
            }
            _GlycanList.Sort();
            return _GlycanList;
        }
    }
}
