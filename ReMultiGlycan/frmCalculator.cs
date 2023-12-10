using System;
using System.Windows.Forms;

namespace COL.ReMultiGlycan
{
	public partial class frmCalculator : Form
	{
		public frmCalculator()
		{
			InitializeComponent();

			this.Font = new System.Drawing.Font("Arial", 9);
			this.StartPosition = FormStartPosition.CenterScreen;
		}

		private void btnCalc_Click(object sender, EventArgs e)
		{
			try
			{
				int Hex = Convert.ToInt32(txtHex.Text);
				int HexNac = Convert.ToInt32(txtHexNAc.Text);
				int deHex = Convert.ToInt32(txtdeHex.Text);
				int Sia = Convert.ToInt32(txtSia.Text);

				if (txtAdduct.Text == "")
				{
					txtAdduct.Text = "0.0";
				}
				float Adduct = Convert.ToSingle(txtAdduct.Text);

				GlycoLib.GlycanCompound Compound = new GlycoLib.GlycanCompound(HexNac, Hex, deHex, Sia, chkPermethylated.Checked, false, chkReducedReducingEnd.Checked, false, true);

				//for (int i = 1; i <= _MaxCharge; i++)
				//{
				//    foreach (float adductMass in _adductMass)
				//    {
				//        for (int j = 0; j <= _MaxCharge; j++)
				//        {
				//            _lstCandidatePeak.Add(new CandidatePeak(comp, i, adductMass, j));
				//            _candidateMzList.Add(_lstCandidatePeak[_lstCandidatePeak.Count - 1].TotalMZ);
				//        }
				//    }
				//}
				//Convert.ToSingle(_glycanComposition.MonoMass) + _adductMass * _adductNo + (_charge - _adductNo) * MassLib.Atoms.ProtonMass) / _charge
				float ProtonMass = MassLib.Atoms.ProtonMass;
				txtResult.Text = "";
				for (int i = 1; i <= 5; i++)
				{
					txtResult.Text = txtResult.Text + "Z=" + i.ToString() + "\t";

					for (int j = 0; j <= i; j++)
					{
						txtResult.Text = txtResult.Text + ((Compound.MonoMass + ProtonMass * (i - j) + Adduct * j) / i).ToString("0.0000") + "\t";
					}
					txtResult.Text = txtResult.Text + Environment.NewLine; ;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Only numbers are allowed");
				return;
			}
		}
	}
}