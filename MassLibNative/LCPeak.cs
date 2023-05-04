using System;
using System.Collections.Generic;
using System.Text;

namespace COL.MassLib
{
    public class LCPeak
    {
        private float _startLCTime = 0.0f;
        private float _endLCTime=0.0f;
        private float _mz = 0.0f;
        private List<MSPoint> _lstMSPoints = new List<MSPoint>();
        private MSPoint _apex;
        private double _sumofIntensity = 0;
        public LCPeak(float argStartTime, float argEndTime, List<MSPoint> argMSPs)
        {
            _startLCTime = argStartTime;
            _endLCTime = argEndTime;
            _lstMSPoints = argMSPs;

            //Find Apex in raw data
            if (_lstMSPoints.Count > 0)
            {
                _apex = new MSPoint(0.0f,0.0f);
                for (int i = 0; i < _lstMSPoints.Count; i++)
                {
                    _sumofIntensity = _sumofIntensity + _lstMSPoints[i].Intensity;
                    if (_lstMSPoints[i].Intensity >= _apex.Intensity)
                    {
                        _apex = _lstMSPoints[i];
                    }
                }
            }
        }
        public float StartTime
        {
            get {
                
                if(_startLCTime !=_lstMSPoints[0].Mass)
                {
                    _lstMSPoints.Sort(delegate(MSPoint p1, MSPoint p2) { return p1.Mass.CompareTo(p2.Mass); });
                    _startLCTime = _lstMSPoints[0].Mass;
                }
                return _startLCTime;
            }
        }
        public float EndTime
        {
            get {
                if (_endLCTime != _lstMSPoints[_lstMSPoints.Count-1].Mass)
                {
                    _lstMSPoints.Sort(delegate(MSPoint p1, MSPoint p2) { return p1.Mass.CompareTo(p2.Mass); });
                    _endLCTime = _lstMSPoints[_lstMSPoints.Count - 1].Mass;
                }
                
                
                return _endLCTime; }
        }
        public List<MSPoint> RawPoint
        {
            get { return _lstMSPoints; }
        }
        public double PeakArea
        {
            get {
                _lstMSPoints.Sort(delegate(MSPoint p1, MSPoint p2) { return p1.Mass.CompareTo(p2.Mass); });
                return IntegralPeakArea.IntegralArea(this); }
        }
        public MSPoint Apex
        {
            get {
                if (_lstMSPoints.Count > 0)
                {
                    _apex = new MSPoint(0.0f, 0.0f);
                    for (int i = 0; i < _lstMSPoints.Count; i++)
                    {
                        _sumofIntensity = _sumofIntensity + _lstMSPoints[i].Intensity;
                        if (_lstMSPoints[i].Intensity >= _apex.Intensity)
                        {
                            _apex = _lstMSPoints[i];
                        }
                    }
                }
                return _apex; }
        }
        public double SumOfIntensity
        {
            get {
                _sumofIntensity = 0;
                if (_lstMSPoints.Count > 0)
                {
                    _apex = new MSPoint(0.0f, 0.0f);
                    for (int i = 0; i < _lstMSPoints.Count; i++)
                    {
                        _sumofIntensity = _sumofIntensity + _lstMSPoints[i].Intensity;
                        if (_lstMSPoints[i].Intensity >= _apex.Intensity)
                        {
                            _apex = _lstMSPoints[i];
                        }
                    }
                }
                return _sumofIntensity; }
        }
        public float MZ
        {
            set { _mz = value; }
            get { return _mz; }
        }
    }
    
}
