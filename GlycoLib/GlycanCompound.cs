using COL.MassLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace COL.GlycoLib
{
	[Serializable]
	public class GlycanCompound : IComparable
	{
		//private static double Na = 22.9899;

		private int _LCorder;
		private bool _Human = true;
		private bool _isDeuterium = false;
		private double _MonoMass;
		private double _AVGMass;
		private enumGlycanLabelingMethod _LabelingMethood;
		private enumLabelingTag _LabelingTag;
		private float _LinearRegSlope;
		private float _LinearRegIntercept;
		private bool _HasLinearRegParemeters = false;
		private bool _PositiveCharge = true;

		public GlycanCompound(int argHexNac, int argHex, int argDeHex, int argSialic)
		: this(argHexNac, argHex, argDeHex, argSialic, false, false, false, false, false) { }

		public GlycanCompound(int argHexNac, int argHex, int argDeHex, int argSialic, bool argIsPermethylated, bool argIsDeuterium, bool argReducedReducingEnd, bool argIsSodium, bool argIsHuman)
			: this(argHexNac, argHex, argDeHex, argSialic, argIsPermethylated, argIsDeuterium, argReducedReducingEnd, argIsSodium, argIsHuman, null, enumLabelingTag.None) { }

		public GlycanCompound(int argHexNac, int argHex, int argDeHex, int argSialic, bool argIsPermethylated, bool argIsDeuterium, bool argReducedReducingEnd, bool argIsSodium, bool argIsHuman, List<Tuple<string, float, int>> argAdducts, enumLabelingTag argLabelingTag)
		{
			NoOfHexNAc = argHexNac;
			NoOfHex = argHex;
			NoOfDeHex = argDeHex;
			NoOfSia = argSialic;
			_Human = argIsHuman;
			isPermethylated = argIsPermethylated;

			isSodium = argIsSodium;
			isReducedReducingEnd = argReducedReducingEnd;
			_isDeuterium = argIsDeuterium;
			_LabelingTag = argLabelingTag;
			Adducts = argAdducts;
			CalcAtom();
			CalcMass();
			CalcAVGMass();
		}

		public List<Tuple<string, float, int>> Adducts { get; }

		public bool PositiveCharge
		{
			get => _PositiveCharge;
			set => _PositiveCharge = value;
		}

		private float AdductMass
		{
			get
			{
				float totalMass = 0;
				foreach (var adduct in Adducts)
				{
					totalMass += adduct.Item2 * adduct.Item3;
				}
				return totalMass;
			}
		}

		public double MZ
		{
			get
			{
				CalcAtom();
				CalcMass();
                if (Charge == 0) return _MonoMass;

                if (_PositiveCharge)
                {
                    return (_MonoMass + AdductMass) / (double)Charge;
                }

                var proton = Adducts.Where(x => x.Item1 == "H").ToArray()[0];

                return (_MonoMass - proton.Item2 * proton.Item3) / (double)proton.Item3;
            }
		}

		public int Charge
		{
			get
			{
				var _charge = 0;
				foreach (var adduct in Adducts)
				{
					_charge += adduct.Item3;
				}
				return _charge;
			}
		}

		public enumGlycanLabelingMethod LabelingMethod
		{
			get => _LabelingMethood;
			set => _LabelingMethood = value;
		}

		public enumLabelingTag LabelingTag
		{
			get => _LabelingTag;
			set
			{
				_LabelingTag = value;
				if (_LabelingTag.ToString().Contains("DRAG_"))
				{
					_LabelingMethood = enumGlycanLabelingMethod.DRAG;
				}
				else if (_LabelingTag.ToString().Contains("MP_"))
				{
					_LabelingMethood = enumGlycanLabelingMethod.MultiplexPermethylated;
				}

				CalcAtom();
				CalcMass();
				CalcAVGMass();
			}
		}

		public bool isHuman
		{
			get => _Human;
			set => _Human = value;
		}

		public bool HasLinearRegressionParameters
		{
			get => _HasLinearRegParemeters;
			set => _HasLinearRegParemeters = value;
		}

		public float LinearRegSlope
		{
			get => _LinearRegSlope;
			set
			{
				_HasLinearRegParemeters = true;
				_LinearRegSlope = value;
			}
		}

		public float LinearRegIntercept
		{
			get => _LinearRegIntercept;
			set
			{
				_HasLinearRegParemeters = true;
				_LinearRegIntercept = value;
			}
		}

		public int NumOfPermethlationSites
		{
			get
            {
                if (!isHuman) //NeuGc has 6 permeth sites
				{
					return NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6;
				}

                return NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5;
            }
		}

		public double MonoMass
		{
			get
			{
				if (_MonoMass == 0.0)
				{
					CalcMass();
				}
				return _MonoMass;
			}
		}

		public double AVGMass
		{
			get
			{
				if (_AVGMass == 0.0)
				{
					CalcAVGMass();
				}
				return _AVGMass;
			}
		}

		private void CalcMass()
		{
			_MonoMass = Carbon * Atoms.CarbonMass +
									Carbon13 * Atoms.Carbon13Mass +
									Hydrogen * Atoms.HydrogenMass +
									Nitrogen * Atoms.NitrogenMass +
									Oxygen * Atoms.OxygenMass +
									Sodium * Atoms.SodiumMass +
									Deuterium * Atoms.DeuteriumMass;
		}

		private void CalcAVGMass()
		{
			_AVGMass = Carbon * Atoms.CarbonAVGMass +
								   Carbon13 * Atoms.Carbon13AVGMass +
								   Hydrogen * Atoms.HydrogenAVGMass +
								   Nitrogen * Atoms.NitrogenAVGMass +
								   Oxygen * Atoms.OxygenAVGMass +
								   Sodium * Atoms.SodiumMass +
								   Deuterium * Atoms.DeuteriumMass;
		}

		public double MassWithoutWater => _MonoMass - Atoms.HydrogenMass * 2 - Atoms.OxygenMass;

		public bool isPermethylated { get; } = false;

		public bool isReducedReducingEnd { get; } = false;

		public bool isSodium { get; } = false;

		public int NoOfHexNAc { get; }

		public int NoOfHex { get; }

		public int NoOfSia { get; }

		public int NoOfDeHex { get; }

		public int Sodium { get; private set; }

		public int Nitrogen { get; private set; }

		public int Carbon { get; private set; }

		public int Carbon13 { get; private set; }

		public int Hydrogen { get; private set; }

		public int Oxygen { get; private set; }

		public int Deuterium { get; private set; }

		public int GlycanLCorder
		{
			get => _LCorder;
			set => _LCorder = value;
		}

		public string GlycanKey
		{
			get
            {
                if (_Human)
				{
					return NoOfHexNAc + "-" +
						NoOfHex + "-" +
						NoOfDeHex + "-" +
						NoOfSia + "-0";
				}

                return NoOfHexNAc + "-" +
                       NoOfHex + "-" +
                       NoOfDeHex + "-0-" +
                       NoOfSia;
            }
		}

		public bool IsGlycanWithInLinearRegLCTime(float argTotalLCTime, float argTolrenaceTime, float argIdentifiedTime)
		{
			//if (_LinearRegIntercept == null || LinearRegSlope == null)
			//{
			//    return false;
			//}

			var expectedTime = _LinearRegSlope * argTotalLCTime + _LinearRegIntercept;
			if (Math.Abs(expectedTime - argIdentifiedTime) / argTotalLCTime <= argTolrenaceTime)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		///http://www.expasy.ch/tools/glycomod/glycomod_masses.html
		///                                                 Native                  Permethylated
		///Hexose (Hex) Mannose                    Man      C6H10O5  	162.0528    C9H16O5     204.0998    - 3 sites can be Permethylated
		///Hexose (Hex) Galactose                  Gal      C6H10O5  	162.0528    C9H16O5     204.0998
		///HexNAc       N-Acetylglucosamine 	   GlcNAC   C8H13NO5    203.0794    C11H19NO5   245.1263   - 3 sites can be Permethylated
		///Deoxyhexose  Fucose                     Fuc      C6H10O4     146.0579    C8H14O4     174.0892   - 2 sites can be premethylated
		///NeuAc        N-Acetylneuraminic acid    NeuNAc   C11H17NO8   291.0954    C16H27NO8   361.1737  - 5 sites can be Permethylated
		///NeuGc        N-glycolylneuraminic acid  NeuNGc   C11H17NO9   307.0903    C17H29NO9   391.1842  - 6 sites can be Permethylated
		///Permethylated -H ->CH3;
		/// </summary>
		private void CalcAtom()
		{
			if (isPermethylated)
			{
				Carbon = 9 * NoOfHex + 11 * NoOfHexNAc + 8 * NoOfDeHex;
				Hydrogen = 16 * NoOfHex + 19 * NoOfHexNAc + 14 * NoOfDeHex;
				Oxygen = 5 * NoOfHex + 5 * NoOfHexNAc + 4 * NoOfDeHex;
				Nitrogen = 1 * NoOfHexNAc + 1 * NoOfSia;

				if (_isDeuterium) //Replace permethlation hydrogen to Deutrtium
				{
					Hydrogen = 10 * NoOfHex + 13 * NoOfHexNAc + 10 * NoOfDeHex;
					Deuterium = 6 * NoOfHex + 6 * NoOfHexNAc + 4 * NoOfDeHex;
				}

				if (_Human)
				{
					Carbon += 16 * NoOfSia;
					Hydrogen += 27 * NoOfSia;
					Oxygen += 8 * NoOfSia;
				}
				else
				{
					Carbon += 17 * NoOfSia;
					Hydrogen += 29 * NoOfSia;
					Oxygen += 9 * NoOfSia;
				}

                //Nonreducing end  -CH3
				Carbon += 1;
				Hydrogen += 3;
				if (isReducedReducingEnd) //C2OH7
				{
					Carbon += 2;
					Oxygen += 1;
					Hydrogen += 7;
				}
				else //COH3
				{
					Carbon += 1;
					Oxygen += 1;
					Hydrogen += 3;
				}

				//Labeling
				switch (_LabelingTag)
				{
					case enumLabelingTag.MP_CH2D:
						if (_Human)
						{
							Deuterium = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3; //3: 1 NonReducing End  + 2 ReducedReducing End
							Hydrogen -= Deuterium;
						}
						else
						{
							Deuterium = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3;
							Hydrogen -= Deuterium;
						}
						break;

					case enumLabelingTag.MP_CHD2:
						if (_Human)
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3) * 2;
							Hydrogen -= Deuterium;
						}
						else
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3) * 2;
							Hydrogen -= Deuterium;
						}
						break;

					case enumLabelingTag.MP_CD3:
						if (_Human)
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3) * 3;
							Hydrogen -= Deuterium;
						}
						else
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3) * 3;
							Hydrogen -= Deuterium;
						}
						break;

					case enumLabelingTag.MP_13CH3:
						if (_Human)
						{
							Deuterium = 0;
							Carbon13 = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3;
							Carbon -= Carbon13;
						}
						else
						{
							Deuterium = 0;
							Carbon13 = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3;
							Carbon -= Carbon13;
						}
						break;

					case enumLabelingTag.MP_13CHD2:
						if (_Human)
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3) * 2;
							Hydrogen -= Deuterium;
							Carbon13 = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3;
							Carbon -= Carbon13;
						}
						else
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3) * 2;
							Hydrogen -= Deuterium;
							Carbon13 = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3;
							Carbon -= Carbon13;
						}
						break;

					case enumLabelingTag.MP_13CD3:
						if (_Human)
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3) * 3;
							Hydrogen -= Deuterium;
							Carbon13 = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 5 + 3;
							Carbon -= Carbon13;
						}
						else
						{
							Deuterium = (NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3) * 3;
							Hydrogen -= Deuterium;
							Carbon13 = NoOfHex * 3 + NoOfHexNAc * 3 + NoOfDeHex * 2 + NoOfSia * 6 + 3;
							Carbon -= Carbon13;
						}
						break;
				}
			}
			else
			{
				Carbon = 6 * NoOfHex + 8 * NoOfHexNAc + 6 * NoOfDeHex;
				Hydrogen = 10 * NoOfHex + 13 * NoOfHexNAc + 10 * NoOfDeHex;
				Oxygen = 5 * NoOfHex + 5 * NoOfHexNAc + 4 * NoOfDeHex;
				Nitrogen = 1 * NoOfHexNAc + 1 * NoOfSia;

				if (_Human)
				{
					Carbon += 11 * NoOfSia;
					Hydrogen += 17 * NoOfSia;
					Oxygen += 8 * NoOfSia;
				}
				else
				{
					Carbon += 11 * NoOfSia;
					Hydrogen += 17 * NoOfSia;
					Oxygen += 9 * NoOfSia;
				}
				//Nonreducing end -H
				Hydrogen += 1;
				if (isReducedReducingEnd) //OH3
				{
					Oxygen += 1;
					Hydrogen += 3;
				}
				else //OH
				{
					Oxygen += 1;
					Hydrogen += 1;
				}
				switch (_LabelingTag)
				{
					/*DRAG
                     *   Light TAG: C8H10N2
                     *   Heavy TAG: 13C6  C2H10N2
                     *   NeuAC: +CH3N1   -O
                     */
					case enumLabelingTag.DRAG_Heavy:
						Carbon = Carbon + 2 + (NoOfSia * 1);
						Hydrogen = Hydrogen + 10 + (NoOfSia * 3);
						Nitrogen = Nitrogen + 2 + (NoOfSia * 1);
						Oxygen -= (NoOfSia * 1);
						Carbon13 = 6;
						break;

					case enumLabelingTag.DRAG_Light:
						Carbon = Carbon + 8 + (NoOfSia * 1);
						Hydrogen = Hydrogen + 10 + (NoOfSia * 3);
						Nitrogen = Nitrogen + 2 + (NoOfSia * 1);
						Oxygen -= (NoOfSia * 1);
						break;
					/*HDEAT
                     * Light C11H23N7
                     * Heavy C11H3D20N7
                     *
                     */
					case enumLabelingTag.HDEAT_Light:
						Carbon += 11;
						Hydrogen += 21;
						Nitrogen += 7;
						Oxygen -= 1;
						break;

					case enumLabelingTag.HDEAT_Heavy:
						Carbon += 11;
						Hydrogen += 3;
						Nitrogen += 7;
						Deuterium = 20;
						Oxygen -= 1;
						break;
				}
			}
			if (isSodium)
			{
				Sodium = 1;
			}
		}

		public int CompareTo(object obj)
        {
            if (obj is GlycanCompound)
			{
				var p2 = (GlycanCompound)obj;
				return _MonoMass.CompareTo(p2.MonoMass);
			}

            throw new ArgumentException("Object is not a Compound.");
        }

		public object Clone()
		{
			var ms = new MemoryStream();
			var bf = new BinaryFormatter();
			bf.Serialize(ms, this);
			ms.Position = 0;
			var obj = bf.Deserialize(ms);
			ms.Close();
			return obj;
		}
	}
}