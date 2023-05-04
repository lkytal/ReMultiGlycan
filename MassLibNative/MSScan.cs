using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COL.MassLib
{
    [Serializable]
    public class MSScan : ICloneable , IDisposable
    {
        bool disposed = false;
        private List<MSPeak> _lstMsPeak;
        private float[] _rawMz;
        private float[] _rawIntensities;
        private float[] _mz;
        private float[] _intensities;
        private int _scanNo;
        private int _parentScanNo;
        private int _parentCharge = 0;
        private float _parentMz = 0;
        private float _parentMonoMz = 0;
        private float _parentMonoMW = 0;
        private float _parentAVGMonoMW = 0;
        private float _parentIntensity = 0;
        private float _parentBasePeak = 0;
        private int _msLevel;
        private float _minIntensity;
        private float _maxIntensity;
        private double _time;
        private float _minMZ;
        private float _maxMZ;
        private string _scanHeader;
        bool _isCIDScan;
        bool _isHCDScan;
        bool _isFTScan;
        public MSScan()
        {
            _lstMsPeak = new List<MSPeak>();
            _minIntensity = 10000000000.0f;
            _minMZ = 10000000000.0f;
            _maxIntensity = -10000000000.0f;
            _maxMZ = -10000000000.0f;
        }

        public MSScan(int argScanNo)
        {
            _lstMsPeak = new List<MSPeak>();
            _scanNo = argScanNo;

            _minIntensity = 10000000000.0f;
            _minMZ = 10000000000.0f;
            _maxIntensity = -10000000000.0f;
            _maxMZ = -10000000000.0f;
        }
        public MSScan(float[] argMz, float[] argIntensity, float argParentMZ, float argParentMonoMW, float argParentAVGMonoMW, int argParentCharge)
        {
            _lstMsPeak = new List<MSPeak>();
            for (int i = 0; i < argMz.Length; i++)
            {
                AddPeak(new MSPeak(argMz[i], argIntensity[i]));
            }
            _parentMonoMW = argParentMonoMW;
            _parentAVGMonoMW = argParentAVGMonoMW;
            _minIntensity = 10000000000.0f;
            _minMZ = 10000000000.0f;
            _maxIntensity = -10000000000.0f;
            _maxMZ = -10000000000.0f;
            _parentCharge = argParentCharge;
            _parentMz = argParentMZ;

        }
        public List<MSPeak> MSPeaks
        {
            get
            {
                if (_lstMsPeak.Count == 0 && _mz.Length != 0)
                {
                    for (int i = 0; i < _mz.Length; i++)
                    {
                        _lstMsPeak.Add(new MSPeak(_mz[i],_intensities[i]));
                    }
                }
                return _lstMsPeak;
            }
            set
            {
                _lstMsPeak = value;
                _lstMsPeak.Sort();
                foreach (MSPeak argPeak in _lstMsPeak)
                {
                    if (argPeak.MonoisotopicMZ > _maxMZ)
                    {
                        _maxMZ = argPeak.MonoisotopicMZ;
                    }
                    if (argPeak.MonoisotopicMZ <= _minMZ)
                    {
                        _minMZ = argPeak.MonoisotopicMZ;
                    }
                    if (argPeak.MonoIntensity > _maxIntensity)
                    {
                        _maxIntensity = argPeak.MonoIntensity;
                    }
                    if (argPeak.MonoIntensity <= _minIntensity)
                    {
                        _minIntensity = argPeak.MonoIntensity;
                    }
                }
            }
        }

        public float ParentMonoMz
        {
            get { return _parentMonoMz; }
            set { _parentMonoMz = value; }
        }
        public List<float> MZList
        {
            get
            {
                if (_lstMsPeak.Count != 0)
                {
                    List<float> mzs = new List<float>();
                    foreach (MSPeak p in _lstMsPeak)
                    {
                        mzs.Add(p.MonoisotopicMZ);
                    }
                    mzs.Sort();
                    return mzs;
                }
                return null;
            }
        }
        public float[] MZs
        {
            get { return _mz; }
            set { _mz = value; }
        }
        public float[] Intensities
        {
            get { return _intensities; }
            set { _intensities = value; }
        }
        public float[] RawMZs
        {
            get { return _rawMz; }
            set { _rawMz = value; }
        }
        public float[] RawIntensities
        {
            get { return _rawIntensities; }
            set { _rawIntensities = value; }
        }
        public bool IsCIDScan
        {
            get { return _isCIDScan; }
            set { _isCIDScan = value; }
        }
        public bool IsHCDScan
        {
            get { return _isHCDScan; }
            set { _isHCDScan = value; }
        }
        public bool IsFTScan
        {
            get { return _isFTScan; }
            set { _isFTScan = value; }
        }
        public string ScanHeader
        {
            get { return _scanHeader; }
            set { _scanHeader = value; }
        }
        public int ParentCharge
        {
            get { return _parentCharge; }
            set { _parentCharge = value; }
        }
        public float ParentMZ
        {
            get { return _parentMz; }
            set { _parentMz = value; }
        }

        public float ParentIntensity
        {
            get { return _parentIntensity; }
            set { _parentIntensity = value; }
        }

        public float ParentBasePeak
        {
            get { return _parentBasePeak; }
            set { _parentBasePeak = value; }
        }
        public double Time
        {
            get {
                _time = Math.Round(_time, 5); 
                return _time;
            }
            set { _time = value; }
        }
        public int ParentScanNo
        {
            get { return _parentScanNo; }
            set { _parentScanNo = value; }
        }
        public float ParentMonoMW
        {
            get { return (_parentMonoMz - Atoms.ProtonMass)*_parentCharge; }
            set { _parentMonoMW = value; }
        }
        public float ParentAVGMonoMW
        {
            get { return _parentAVGMonoMW; }
            set { _parentAVGMonoMW = value; }
        }
        public int MsLevel
        {
            get { return _msLevel; }
            set { _msLevel = value; }
        }
        public float MaxIntensity
        {
            get
            {
                if ( _intensities.Length != 0)
                {
                    _maxIntensity = _intensities.Max();
                }
                return _maxIntensity;
            }
            set { _maxIntensity = value; }
        }
        public float MinIntensity
        {
            get
            {
                if (_intensities.Length != 0)
                {
                    _minIntensity = _intensities.Min();
                }
                return _minIntensity;
            }
            set { _minIntensity = value; }
        }
        public float MaxMZ
        {
            get
            {
                if (_mz.Length != 0)
                {
                    _maxMZ = _mz.Max();
                }
                return _maxMZ;
            }
            set { _maxMZ = value; }
        }
        public float MinMZ
        {
            get
            {
                if (_mz.Length != 0)
                {
                    _minMZ = _mz.Min();
                }
                return _minMZ;
            }
            set { _minMZ = value; }
        }
        public int ScanNo
        {
            get { return _scanNo; }
            set { _scanNo = value; }
        }
        public int Count
        {
            get { return _lstMsPeak.Count; }
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void Clear()
        {
            _lstMsPeak.Clear();
        }
        public void AddPeak(MSPeak argPeak)
        {

            if (argPeak.MonoisotopicMZ > _maxMZ)
            {
                _maxMZ = argPeak.MonoisotopicMZ;
            }
            if (argPeak.MonoisotopicMZ <= _minMZ)
            {
                _minMZ = argPeak.MonoisotopicMZ;
            }
            if (argPeak.MonoIntensity > _maxIntensity)
            {
                _maxIntensity = argPeak.MonoIntensity;
            }
            if (argPeak.MonoIntensity <= _minIntensity)
            {
                _minIntensity = argPeak.MonoIntensity;
            }
            _lstMsPeak.Add(argPeak);
            _lstMsPeak.Sort();
        }
        public void Dispose()
        { 
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return; 
            if (disposing)
            {
                // Free any other managed objects here. 
                _lstMsPeak.Clear();
                _lstMsPeak = null;
            }
            // Free any unmanaged objects here. 
            disposed = true;
        }
    }


}
