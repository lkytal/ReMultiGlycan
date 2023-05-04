using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COL.MultiGlycan
{
    class QuantitationPeak
    {
        string _glycanKey;
        string _dataSetName;
        List<Tuple<float, float>> _protonatedPeaks = new List<Tuple<float,float>>();
        Dictionary<string, List<Tuple<float, float>>> _otherAdductsPeaks;
        int _protonatedApexIdx = -1;
        float _totalProtonatedIntensity =0;
        //float _totalOtherAdductIntensity = 0;        
        public QuantitationPeak(string argDataSetName, string argGlycanKey)
        {
            _glycanKey = argGlycanKey;
            _dataSetName = argDataSetName;
        }
        public string GlycanKey
        {
            get { return _glycanKey; }
        }
        public string DataSetName
        {
            get { return _dataSetName; }
        }
        public void AssignPeaks(Dictionary<string, List<Tuple<float, float>>> argPeaks)
        {           
            //Get only consecutive sagment Proton
            if (argPeaks.ContainsKey("H"))
            {
                List<Tuple<float, float>> tmp = argPeaks["H"].OrderBy(t => t.Item1).ToList();
                
                _protonatedPeaks = new List<Tuple<float, float>>();
                int maxIntensityIndex = GetIndexOfMaxIntensity(tmp);
                _protonatedPeaks.Add(tmp[maxIntensityIndex]);
                float currentLCTime = _protonatedPeaks[0].Item1;
                if (maxIntensityIndex > 0)
                {
                    for (int i = maxIntensityIndex - 1; i >= 0; i--)
                    {
                        if (currentLCTime - tmp[i].Item1 > 0.071f)
                        {
                            break;
                        }
                        _protonatedPeaks.Insert(0, tmp[i]);
                        currentLCTime = tmp[i].Item1;
                    }
                }
                currentLCTime = _protonatedPeaks[_protonatedPeaks.Count - 1].Item1;
                if (maxIntensityIndex < tmp.Count - 1)
                {
                    for (int i = maxIntensityIndex + 1; i < tmp.Count; i++)
                    {
                        if (tmp[i].Item1 - currentLCTime > 0.071f)
                        {
                            break;
                        }
                        _protonatedPeaks.Add(tmp[i]);
                        currentLCTime = tmp[i].Item1;
                    }
                }
            }
            _otherAdductsPeaks = new Dictionary<string, List<Tuple<float, float>>>();
            foreach (string adductKey in argPeaks.Keys)
            {
                if (adductKey != "H")
                {
                    if (!_otherAdductsPeaks.ContainsKey(adductKey))
                    {
                        _otherAdductsPeaks.Add(adductKey, new List<Tuple<float,float>>());
                    }
                        List<Tuple<float, float>> tmp = argPeaks[adductKey];               
                         _otherAdductsPeaks[adductKey].AddRange(tmp);                                                               
                }
            }
        }
        public List<Tuple<float, float>> ProtonatedPeaks
        {
            get { return _protonatedPeaks; }
            set { _protonatedPeaks = value; }
        }
        public Dictionary<string, List<Tuple<float, float>>> OtherAdductsPeaks
        {
            get { return _otherAdductsPeaks; }
        }
        private int GetIndexOfMaxIntensity(List<Tuple<float,float>> argTmp)
        {            
                    int maxIdx = -1;
                    float maxIntensity = 0;
                    for (int i = 0; i < argTmp.Count; i++)
                    {
                        if (argTmp[i].Item2 >= maxIntensity)
                        {
                            maxIdx = i;
                            maxIntensity = argTmp[i].Item2;
                        }
                    }
                    return  maxIdx;
        }
        public int ProtonatedApexIdx    
        {
            get
            {
                if (_protonatedApexIdx != -1)
                {
                    return _protonatedApexIdx;
                }
                _protonatedApexIdx = GetIndexOfMaxIntensity(_protonatedPeaks);
                
                return _protonatedApexIdx;
            }           
        }
        public float TotalProtonatedIntensity
        {
            get
            {
                if (_protonatedPeaks != null)
                {
                    _totalProtonatedIntensity = _protonatedPeaks.Sum(t => t.Item2);
                }
                return _totalProtonatedIntensity;
            }
        }
        /// <summary>
        /// Total intnsities include protonated adduct and all other adducts greater than 10% protonated adduct;
        /// </summary>
        public double TotalIntensity
        {
            get {
                double sum = 0;
                float protonatedIntensity = TotalProtonatedIntensity;
                float protonatedStart = 0;
                float protonatedEnd = 9999;
                if (_protonatedPeaks.Count != 0)
                {
                    protonatedStart = _protonatedPeaks[0].Item1;
                     protonatedEnd = _protonatedPeaks[_protonatedPeaks.Count - 1].Item1;
                }
                sum += protonatedIntensity;
                foreach (string adductKey in _otherAdductsPeaks.Keys)
                {
                    double adductSum = _otherAdductsPeaks[adductKey].Where((t) => t.Item1 >= protonatedStart && t.Item1 <= protonatedEnd).ToList().Sum(t => t.Item2);
                    if (adductSum >= protonatedIntensity * 0.1 && protonatedIntensity > 0)
                    {
                        sum += adductSum;
                    }
                    
                }
                return sum;
            }
        }
     

    }
}
