﻿using COL.ProtLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace COL.GlycoLib
{
	[Serializable]
	public class TargetPeptide : ICloneable, IEquatable<TargetPeptide>
	{
		private string _PeptideSeq;
		private float _StartTime = -1; //in Mins
		private float _EndTime = 9999; //in Mins
		private string _ProteinName;
		private float _PeptideMass;
		private string _AminoAcidAfter;
		private string _AminoAcidBefore;
		private bool _IdentifiedPeptide = false;
		private Dictionary<string, int> _Modifications = new Dictionary<string, int>();

		public string PeptideSequence
		{
			get => _PeptideSeq;
			set => _PeptideSeq = value;
		}

		public bool IdentifiedPeptide
		{
			get => _IdentifiedPeptide;
			set => _IdentifiedPeptide = value;
		}

		public string AminoAcidAfter
		{
			get => _AminoAcidAfter;
			set => _AminoAcidAfter = value;
		}

		public string AminoAcidBefore
		{
			get => _AminoAcidBefore;
			set => _AminoAcidBefore = value;
		}

		public float StartTime
		{
			get => _StartTime;
			set => _StartTime = value;
		}

		public Dictionary<string, int> Modifications
		{
			get => _Modifications;
			set => _Modifications = value;
		}

		public string ModificationString
		{
			get
			{
				string mod = "";
				foreach (string key in _Modifications.Keys)
				{
					mod += key + ":" + _Modifications[key].ToString() + ";";
				}
				return mod;
			}
		}

		public float EndTime
		{
			get => _EndTime;
			set => _EndTime = value;
		}

		public string ProteinName
		{
			get => _ProteinName;
			set => _ProteinName = value;
		}

		public float PeptideMass
		{
			get
			{
				if (_PeptideMass == 0)
				{
					AminoAcidMass AAMass = new AminoAcidMass();
					_PeptideMass = AAMass.GetMonoMW(_PeptideSeq, true);
				}
				return _PeptideMass;
			}
			set => _PeptideMass = value;
		}

		public TargetPeptide(string argPeptide) : this(argPeptide, "No_data", 0, -1, 9999)
		{
		}

		public TargetPeptide(string argPeptide, string argProteinName, float argStartTime, float argEndTime) : this(argPeptide, argProteinName, 0, -1, 9999)
		{
		}

		public TargetPeptide(string argPeptide, string argProteinName, float argPeptideMass, float argStartTime, float argEndTime)
		{
			_PeptideSeq = argPeptide;
			_ProteinName = argProteinName;
			if (argPeptideMass == 0)
			{
				AminoAcidMass AAMS = new AminoAcidMass();
				_PeptideMass = AAMS.GetMonoMW(_PeptideSeq, true);
			}
			else
			{
				_PeptideMass = argPeptideMass;
			}
			_StartTime = argStartTime;
			_EndTime = argEndTime;
		}

		public object Clone()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				if (this.GetType().IsSerializable)
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, this);
					stream.Position = 0;
					return formatter.Deserialize(stream);
				}
				return null;
			}
		}

		public bool Equals(TargetPeptide obj)
		{
			if (!PeptideMass.Equals(obj.PeptideMass) || !PeptideSequence.Equals(obj.PeptideSequence) || !Modifications.Count.Equals(obj.Modifications.Count) || !IdentifiedPeptide.Equals(obj.IdentifiedPeptide))
			{
				return false;
			}

			foreach (string key in obj.Modifications.Keys)
			{
				if (obj.Modifications[key] != Modifications[key])
				{
					return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			int hashcode = PeptideMass.GetHashCode() * PeptideSequence.GetHashCode();
			return hashcode;
		}
	}
}