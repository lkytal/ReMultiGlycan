using System;
using System.Collections.Generic;
using System.Text;

namespace COL.ElutionViewer
{
    public class MSPointSet3D
    {
        List<float> _time;
        List<float> _mz;
        List<float> _intensity;
        int _maxMZidx=0;
        int _minMZidx = 0;
        int _maxTimeidx = 0;
        int _minTimeidx = 0;
        int _maxIntensityidx = 0;
        int _minIntensityidx = 0;
        public float MaxX
        {
            get{return _time[_maxTimeidx];}
        }
        public float MinX
        {
            get { return _time[_minTimeidx]; }
        }
        public float MaxY
        {
            get { return _mz[_maxMZidx]; }
        }
        public float MinY
        {
            get { return _mz[_minMZidx]; }
        }
        public float MaxXwWhiteBoarder
        {
            get { return _time[_maxTimeidx]+0.03f; }
        }
        public float MinXwWhiteBoarder
        {
            get { return _time[_minTimeidx]-0.03f; }
        }
        public float MaxYwWhiteBoarder
        {
            get { return _mz[_maxMZidx]+2f; }
        }
        public float MinYwWhiteBoarder
        {
            get { return _mz[_minMZidx]-2f; }
        }
        public float MaxZ
        {
            get { return _intensity[_maxIntensityidx]; }
        }
        public float MinZ
        {
            get { return _intensity[_minIntensityidx]; }
        }
        public int Count
        {
            get { return _mz.Count; }
        }
        public List<float> _x
        {
            get { return _time; }
        }
        public List<float> _y
        {
            get { return _mz; }
        }
        public List<float> _z
        {
            get { return _intensity; }
        }
        public float X(int argIdx )
        {
            return _time[argIdx]; 
        }
        public float Y(int argIdx)
        {
            return _mz[argIdx];
        }
        public float Z(int argIdx)
        {
            return _intensity[argIdx];
        }
        public void Add(float argX, float argY, float argZ)
        {
            _time.Add(argX);
            _mz.Add(argY);
            _intensity.Add(argZ);
            UpdateMaxMin();
        }
        public MSPointSet3D()
        {
            _time = new List<float>();
            _mz = new List<float>();
            _intensity = new List<float>();
        }
        public MSPointSet3D(List<float> argMZ, List<float> argIntensity, List<float> argTime)
        {            
            _time =argTime;
            _mz =argMZ;
            _intensity = argIntensity;
            UpdateMaxMin();
        }
        public void AddMSPoints(List<float> argTime, List<float> argMZ, List<float> argIntensity)
        {
            _mz.AddRange(argMZ);
            _time.AddRange(argTime);
            _intensity.AddRange(argIntensity);
            UpdateMaxMin();
        }
        private void UpdateMaxMin()
        {
            for (int i = 0; i < _mz.Count; i++)
            {
                if (_mz[i] > _mz[_maxMZidx])
                {
                    _maxMZidx = i;
                }
                if (_mz[i] < _mz[_minMZidx])
                {
                    _minMZidx = i;
                }
            }
            for (int i = 0; i < _time.Count; i++)
            {
                if (_time[i] > _time[_maxTimeidx])
                {
                    _maxTimeidx = i;
                }
                if (_time[i] < _time[_minTimeidx])
                {
                    _minTimeidx = i;
                }
            }
            for (int i = 0; i < _intensity.Count; i++)
            {
                if (_intensity[i] > _intensity[_maxIntensityidx])
                {
                    _maxIntensityidx = i;
                }
                if (_intensity[i] < _intensity[_minIntensityidx])
                {
                    _minIntensityidx = i;
                }
            }
            }
        
    }
}
