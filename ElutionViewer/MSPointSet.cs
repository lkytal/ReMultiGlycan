﻿using System.Collections.Generic;

namespace COL.ElutionViewer
{
	public class MSPointSet
	{
		private List<float> _x;
		private List<float> _y;
		private int _maxMZidx = 0;
		private int _minMZidx = 0;
		private int _maxIntensityidx = 0;
		private int _minIntensityidx = 0;

		public List<float> XLst => _x;

		public List<float> YLst => _y;

		public float X(int argIdx)
		{
			return _x[argIdx];
		}

		public float Y(int argIdx)
		{
			return _y[argIdx];
		}

		public int Count => _x.Count;

		public int MaxIntensityIdx => _maxIntensityidx;

		public int MinIntensityIdx => _minIntensityidx;

		public MSPointSet()
		{
			_x = new List<float>();
			_y = new List<float>();
		}

		public void Add(float argX, float argY)
		{
			_x.Add(argX);
			_y.Add(argY);
			UpdateMaxMin();
		}

		public void AddMSPoints(List<float> argMZ, List<float> argIntensity)
		{
			_x.AddRange(argMZ);
			_y.AddRange(argIntensity);
			UpdateMaxMin();
		}

		private void UpdateMaxMin()
		{
			for (int i = 0; i < _x.Count; i++)
			{
				if (_x[i] > _x[_maxMZidx])
				{
					_maxMZidx = i;
				}
				if (_x[i] < _x[_minMZidx])
				{
					_minMZidx = i;
				}
			}
			for (int i = 0; i < _y.Count; i++)
			{
				if (_y[i] > _y[_maxIntensityidx])
				{
					_maxIntensityidx = i;
				}
				if (_y[i] < _y[_minIntensityidx])
				{
					_minIntensityidx = i;
				}
			}
		}

		public void Sort()
		{
			List<MSPoint> msp = new List<MSPoint>();
			for (int i = 0; i < _x.Count; i++)
			{
				msp.Add(new MSPoint(_x[i], _y[i]));
			}
			msp.Sort(delegate (MSPoint mp1, MSPoint mp2) { return mp1.Mass.CompareTo(mp2.Mass); });
			_x = new List<float>();
			_y = new List<float>();
			for (int i = 0; i < msp.Count; i++)
			{
				_x.Add(msp[i].Mass);
				_y.Add(msp[i].Intensity);
			}
		}
	}
}