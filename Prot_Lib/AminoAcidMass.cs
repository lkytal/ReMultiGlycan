using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace COL.ProtLib
{
    public class AminoAcidMass
    {
        //http://www.abrf.org/index.cfm/dm.home?AvgMass=all
        //http://web.expasy.org/findmod/findmod_masses.html
        Hashtable MWMapping;
        Hashtable AVGMWMapping;
        public AminoAcidMass()
        {
            MWMapping = new Hashtable();
            MWMapping.Add("A", 71.03711);
            MWMapping.Add("R", 156.10111);
            MWMapping.Add("N", 114.04293);
            MWMapping.Add("D", 115.02694);
            MWMapping.Add("C", 103.00919);
            MWMapping.Add("E", 129.04259);
            MWMapping.Add("Q", 128.05858);
            MWMapping.Add("G", 57.02146);
            MWMapping.Add("H", 137.05891);
            MWMapping.Add("I", 113.08406);
            MWMapping.Add("L", 113.08406);
            MWMapping.Add("K", 128.09496);
            MWMapping.Add("M", 131.04049);
            MWMapping.Add("F", 147.06841);
            MWMapping.Add("P", 97.05276);
            MWMapping.Add("S", 87.03203);
            MWMapping.Add("T", 101.04768);
            MWMapping.Add("W", 186.07931);
            MWMapping.Add("Y", 163.06333);
            MWMapping.Add("V", 99.06841);
            MWMapping.Add("U", 150.95363);
            MWMapping.Add("O", 237.14772);
            MWMapping.Add("Cys_CM", 161.01466); //Carboxymethyl cystein C2H3O2
            MWMapping.Add("Cys_CAM", 160.03065); //Carbamidomethyl cysteine C2H3NO  //Carboxyamidomethylation
            MWMapping.Add("Cys_PE", 208.067039); //Pyridyl-ethyl cysteine  C7H7N    4-VINYLPYRIDINE
            MWMapping.Add("Cys_PAM", 174.04631); //Propionamide cysteine C3H7NO
            MWMapping.Add("MSO", 147.0354); //Methionine sulfoxide
            MWMapping.Add("TPO", 202.0742); //Oxydized tryptophan
            MWMapping.Add("HSL", 100.03985); //Homoserine Lactone 
            MWMapping.Add("H2O", 18.01056);
            AVGMWMapping = new Hashtable();
            AVGMWMapping.Add("A", 71.0788);
            AVGMWMapping.Add("R", 156.1875);
            AVGMWMapping.Add("N", 114.1038);
            AVGMWMapping.Add("D", 115.0886);
            AVGMWMapping.Add("C", 103.1388);
            AVGMWMapping.Add("E", 129.1155);
            AVGMWMapping.Add("Q", 128.1307);
            AVGMWMapping.Add("G", 57.0519);
            AVGMWMapping.Add("H", 137.1411);
            AVGMWMapping.Add("I", 113.1594);
            AVGMWMapping.Add("L", 113.1594);
            AVGMWMapping.Add("K", 128.1741);
            AVGMWMapping.Add("M", 131.1926);
            AVGMWMapping.Add("F", 147.1766);
            AVGMWMapping.Add("P", 97.1167);
            AVGMWMapping.Add("S", 87.0782);
            AVGMWMapping.Add("T", 101.1051);
            AVGMWMapping.Add("W", 186.2132);
            AVGMWMapping.Add("Y", 163.176);
            AVGMWMapping.Add("V", 99.1326);
            AVGMWMapping.Add("U", 150.0379);
            AVGMWMapping.Add("O", 237.3018);
            AVGMWMapping.Add("Cys_CM", 161.1755);
            AVGMWMapping.Add("Cys_CAM", 160.1908);
            AVGMWMapping.Add("Cys_PE", 208.284);
            AVGMWMapping.Add("Cys_PAM", 174.2176);
            AVGMWMapping.Add("MSO", 147.1920);
            AVGMWMapping.Add("TPO", 202.2126);
            AVGMWMapping.Add("HSL", 100.09714);
            AVGMWMapping.Add("H2O", 18.01524);
        }
        public float GetMonoMW(string argString, bool argCYS_CAM)
        {
            return this.GetMonoMW(argString, argCYS_CAM, "X", 40);
        }

        public float GetMonoMW(string argString, bool argCYS_CAM, string argDecoyChar, double argDecoyMass)
        {
            float SUM = 0.0f;
            string target = argString.ToUpper();
            MWMapping.Add(argDecoyChar,argDecoyMass);
            for (int i = 0; i < target.Length; i++)
            {
                if (argCYS_CAM)
                {
                    if (target[i] == 'C')
                    {
                        SUM = SUM + Convert.ToSingle(MWMapping["Cys_CAM"]);
                    }
                    else
                    {
                        SUM = SUM + Convert.ToSingle(MWMapping[target[i].ToString()]);
                    }
                }
                else
                {
                    SUM = SUM + Convert.ToSingle(MWMapping[target[i].ToString()]);
                }
            }
            SUM = SUM + Convert.ToSingle(MWMapping["H2O"]);
            return SUM;
        }

        public float GetAVGMonoMW(string argString, bool argCYS_CAM)
        {
            return this.GetAVGMonoMW(argString, argCYS_CAM, "X", 40);
        }

        public float GetAVGMonoMW(string argString, bool argCYS_CAM, string argDecoyChar, double argDecoyMass)
        {
            float SUM = 0.0f;
            string target = argString.ToUpper();
            AVGMWMapping.Add(argDecoyChar,argDecoyMass);
            for (int i = 0; i < target.Length; i++)
            {
                if (argCYS_CAM)
                {
                    if (target[i] == 'C')
                    {
                        SUM = SUM + Convert.ToSingle(AVGMWMapping["Cys_CAM"]);
                    }
                    else
                    {
                        SUM = SUM + Convert.ToSingle(AVGMWMapping[target[i].ToString()]);
                    }
                }
                else
                {
                    SUM = SUM + Convert.ToSingle(AVGMWMapping[target[i].ToString()]);
                }
            }
            SUM = SUM + Convert.ToSingle(AVGMWMapping["H2O"]);
            return SUM;
        }
        public float GetAminoAcidMass(string argInput, bool argIsAvg)
        {
            float mass = 0;
            if (!argIsAvg)
            {
                if (MWMapping.ContainsKey(argInput))
                {
                    mass = Convert.ToSingle(MWMapping[argInput]);
                }
                else
                {
                    throw new Exception("Amino acid mass mapping not found");
                }
            }
            else
            {
                if (AVGMWMapping.ContainsKey(argInput))
                {
                    mass = Convert.ToSingle(AVGMWMapping[argInput]);
                }
                else
                {
                    throw new Exception("Amino acid mass mapping not found");
                }
            }
            return mass;
        }
    }
}
