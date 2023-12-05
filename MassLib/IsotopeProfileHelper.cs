using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace COL.MassLib
{     
    public class IsotopeProfileHelper
    {
        private TheoreticalDistributionDLL.clsTheoreticalDistribution mobj_theoretical_dist;
        public IsotopeProfileHelper()
        {
            mobj_theoretical_dist = new TheoreticalDistributionDLL.clsTheoreticalDistribution();
            mobj_theoretical_dist.ChargeState = 1;
            mobj_theoretical_dist.N15 = false;
            mobj_theoretical_dist.Resolution = 100000;
            mobj_theoretical_dist.Threshold = 0;
            mobj_theoretical_dist.OutputFileName = "tmp.csv";
        }
       
        public bool isN15
        {
            set
            {
                mobj_theoretical_dist.N15 = value;
            }
        }
        public double MonoMass
        {
            set
            {
                mobj_theoretical_dist.Mass = value;
            }
        }
        /// <summary>
        /// Find the Isotope Profile
        /// </summary>
        /// <returns></returns>
        public List<MSPoint> GetIsotopeProfileUsingMass(double mass)
        {
            mobj_theoretical_dist.Mass = mass;
            List<MSPoint> isotopeprofiler = new List<MSPoint>();
            mobj_theoretical_dist.CreateTheoreticalDistributionUsingMass();
            StreamReader sr = new StreamReader("tmp.csv");
            float tmpmz=0.0f,tmpintensity ,currentmz=0.0f,currentintensity;

            //Find max intensity in each peak
            string tmp = sr.ReadLine();
            currentmz = Convert.ToSingle(tmp.Split(',')[0]);
            currentintensity = Convert.ToSingle(tmp.Split(',')[1]);
            do
            {
                tmp = sr.ReadLine();
                tmpmz = Convert.ToSingle(tmp.Split(',')[0]);
                tmpintensity = Convert.ToSingle(tmp.Split(',')[1]);

                if (Math.Abs(tmpmz - currentmz) < 0.9)
                {
                    if (tmpintensity >= currentintensity)
                    {
                        currentintensity = tmpintensity;
                        currentmz = tmpmz;
                    }
                }
                else
                {
                    isotopeprofiler.Add(new MSPoint(currentmz, currentintensity));
                    currentmz = tmpmz;
                    currentintensity = tmpintensity;

                }

            } while (!sr.EndOfStream);
            isotopeprofiler.Add(new MSPoint(currentmz, currentintensity));
            sr.Close();
            return isotopeprofiler;
        }
        public List<MSPoint> GetIsotopeProfileUsingChemicalCompound(int argCarbon, int argHydrogen, int argDeuterium, int argNitrogen, int argOxygen, bool isSodium)
        {
            if (argCarbon != 0)
            {
                mobj_theoretical_dist.numC = argCarbon;
            }
            if (argHydrogen != 0)
            {
                mobj_theoretical_dist.numH = argHydrogen;
                
            }
            if (argDeuterium != 0)
            {
                mobj_theoretical_dist.numH = mobj_theoretical_dist.numH + (argDeuterium * 2);
            }
            if (argNitrogen != 0)
            {
                mobj_theoretical_dist.numN = argNitrogen;                
                mobj_theoretical_dist.N15 = true;
            }
            if (argOxygen != 0)
            {
                mobj_theoretical_dist.numO = argOxygen;
            }

            try
            {
                mobj_theoretical_dist.CreateTheoreticalDistributionUsingFormula();
            }
            catch
            {
                throw new Exception("External error: Create Isotope using Formula failed. (Outer library error)");
            }

            List<MSPoint> isotopeprofiler = new List<MSPoint>();


            StreamReader sr = new StreamReader("tmp.csv");
            float tmpmz = 0.0f, tmpintensity, currentmz = 0.0f, currentintensity;

            //Find max intensity in each peak
            string tmp = sr.ReadLine();
            currentmz = Convert.ToSingle(tmp.Split(',')[0]);
            currentintensity = Convert.ToSingle(tmp.Split(',')[1]);
            do
            {
                tmp = sr.ReadLine();
                tmpmz = Convert.ToSingle(tmp.Split(',')[0]);
                tmpintensity = Convert.ToSingle(tmp.Split(',')[1]);

                if (Math.Abs(tmpmz - currentmz) < 0.9)
                {
                    if (tmpintensity >= currentintensity)
                    {
                        currentintensity = tmpintensity;
                        currentmz = tmpmz;
                    }
                }
                else
                {
                    if (isSodium)
                    {
                        isotopeprofiler.Add(new MSPoint(currentmz + 22.98976928f, currentintensity));
                    }
                    else
                    {
                        isotopeprofiler.Add(new MSPoint(currentmz, currentintensity));
                    }
                    currentmz = tmpmz;
                    currentintensity = tmpintensity;

                }

            } while (!sr.EndOfStream);
            if (isSodium)
            {
                isotopeprofiler.Add(new MSPoint(currentmz + Atoms.SodiumMass, currentintensity));
            }
            else
            {
                isotopeprofiler.Add(new MSPoint(currentmz, currentintensity));
            }
            sr.Close();
            return isotopeprofiler;
        }
    }
}
