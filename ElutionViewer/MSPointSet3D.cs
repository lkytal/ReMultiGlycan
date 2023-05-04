using System.Collections.Generic;

namespace COL.ElutionViewer
{
	public class MSPointSet3D
	{
		private List<float> _time;
		private List<float> _mz;
		private List<float> _intensity;
		private int _maxMZidx = 0;
		private int _minMZidx = 0;
		private int _maxTimeidx = 0;
		private int _minTimeidx = 0;
		private int _maxIntensityidx = 0;
		private int _minIntensityidx = 0;

		public float MaxX => _time[_maxTimeidx];

		public float MinX => _time[_minTimeidx];

		public float MaxY => _mz[_maxMZidx];

		public float MinY => _mz[_minMZidx];

		public float MaxXwWhiteBoarder => _time[_maxTimeidx] + 0.03f;

		public float MinXwWhiteBoarder => _time[_minTimeidx] - 0.03f;

		public float MaxYwWhiteBoarder => _mz[_maxMZidx] + 2f;

		public float MinYwWhiteBoarder => _mz[_minMZidx] - 2f;

		public float MaxZ => _intensity[_maxIntensityidx];

		public float MinZ => _intensity[_minIntensityidx];

		public int Count => _mz.Count;

		public List<float> _x => _time;

		public List<float> _y => _mz;

		public List<float> _z => _intensity;

		public float X(int argIdx)
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
			_time = argTime;
			_mz = argMZ;
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