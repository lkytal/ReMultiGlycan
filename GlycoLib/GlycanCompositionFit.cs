using System;
using System.Collections.Generic;
using System.Text;

namespace COL.GlycoLib
{
    public static class GlycanCompositionFit
    {
        public static string GetGlycanCompositions(float argGlycanMass, List<Glycan> argGlycans)
        {
            List<int> NoOfGlycan = new List<int>();
            List<int> MaxNoOfGlycan = new List<int>();
            List<string> Combination = new List<string>();
            foreach (Glycan g in argGlycans)
            {
                MaxNoOfGlycan.Add(Convert.ToInt32(Math.Floor(argGlycanMass / g.AVGMass)));
            }

            //Create List
            int Count = 1;
            for (int i = 0; i < MaxNoOfGlycan.Count; i++)
            {
                Count =( MaxNoOfGlycan[i] + 1) * Count;
            }
            int GlycanCount = 0;
            for (int j = 1; j <= Count; j++)
            {
                Combination.Add(GlycanCount.ToString());
                if (j%(Count / (MaxNoOfGlycan[0]+1)) == 0)
                {
                    GlycanCount++;
                }
            }
            
            //Append the rest glycan
            for (int i = 1; i < MaxNoOfGlycan.Count; i++)
            {
                Count = 1;
                for (int j = i; j < MaxNoOfGlycan.Count; j++)
                {
                    Count = (MaxNoOfGlycan[j] + 1) * Count;
                }
                GlycanCount = 0;
                for (int j = 0; j < Combination.Count; j++)
                {
                    Combination[j] = Combination[j] + "-" + GlycanCount.ToString();
                    if ((j+1) % (Count / (MaxNoOfGlycan[i] + 1)) == 0)
                    {
                        GlycanCount++;
                        if (GlycanCount == MaxNoOfGlycan[i]+1)
                        {
                            GlycanCount = 0;
                        }
                    }
                }
            }
            int SmallestIdx = -1;
            float SmallestDifferent = 100.0f;
            for (int i = 0; i < Combination.Count; i++) 
            {
                string[] GlycanCombin = Combination[i].Split('-');
                float GlycanMass = 0.0f;
                for (int j = 0; j < GlycanCombin.Length; j++)
                {
                    GlycanMass = GlycanMass + (Convert.ToInt32(GlycanCombin[j]) * argGlycans[j].AVGMass);
                }
                if (Math.Abs(GlycanMass - argGlycanMass) < SmallestDifferent)
                {
                    SmallestIdx = i;
                    SmallestDifferent = Math.Abs(GlycanMass - argGlycanMass);
                }
            }
            return Combination[SmallestIdx];
        }
    }
}
