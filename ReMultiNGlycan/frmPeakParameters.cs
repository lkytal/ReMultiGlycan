using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace COL.MultiGlycan
{
    public partial class frmPeakParameters : Form
    {

        public frmPeakParameters()
        {
            InitializeComponent();

            txtSN.Text = trackSN.Value.ToString();
            txtPeakPeakBackgroundRatioRatio.Text = (trackPeakBackgroundRatio.Value / 100.0).ToString();
            txtPeptideMinRatio.Text = (trackPeptideMinRatio.Value / 100.0).ToString();
            txtMaxCharge.Text = (10 - trackMaxCharge.Value).ToString();


        }

        private void trackSN_Scroll(object sender, EventArgs e)
        {
            txtSN.Text = trackSN.Value.ToString();
        }

        private void trackPeakMinRatio_Scroll(object sender, EventArgs e)
        {
            txtPeakPeakBackgroundRatioRatio.Text = (trackPeakBackgroundRatio.Value/100.0).ToString();
        }

        private void trackPeptideMinRatio_Scroll(object sender, EventArgs e)
        {
            txtPeptideMinRatio.Text = (trackPeptideMinRatio.Value / 100.0).ToString();
        }

        private void trackMaxCharge_Scroll(object sender, EventArgs e)
        {
            txtMaxCharge.Text = (11 - trackMaxCharge.Value).ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            

            this.Close();
        }

        private void chkPeptideMinAbso_CheckedChanged(object sender, EventArgs e)
        {
            txtPeptideMinAbso.Enabled = chkPeptideMinAbso.Checked;
            trackPeptideMinRatio.Enabled = !chkPeptideMinAbso.Checked;
            txtPeptideMinRatio.Enabled = !chkPeptideMinAbso.Checked;
        }

        private void btnDefault_Click(object sender, EventArgs e)
        {
            trackSN.Value = 60;
            txtSN.Text = "60";

            trackPeakBackgroundRatio.Value = 100;
            txtPeakPeakBackgroundRatioRatio.Text = (100/100.0).ToString();

            trackPeptideMinRatio.Value = 1000;
            txtPeptideMinRatio.Text = (1000/100.0).ToString();

            chkPeptideMinAbso.Checked = false;
            txtPeptideMinAbso.Text = "";

            trackMaxCharge.Value = 5;
            txtMaxCharge.Text = "5";
        }

        private void frmPeakParameters_FormClosing(object sender, FormClosingEventArgs e)
        {   
                if (MessageBox.Show("Save parameters?", "Exit setting?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                }
                else
                {
                    e.Cancel = true;
                }
        }

        private void txtSN_TextChanged(object sender, EventArgs e)
        {
            int ParsedValue = 0;
            if (Int32.TryParse(txtSN.Text, out ParsedValue))
            {
                if (ParsedValue > trackSN.Maximum)
                {
                    trackSN.Value = trackSN.Maximum;
                    txtSN.Text = trackSN.Value.ToString();
                }
                else if (ParsedValue < trackSN.Minimum)
                {
                    trackSN.Value = trackSN.Minimum;
                    txtSN.Text = trackSN.Value.ToString();
                }
                else
                {
                    trackSN.Value = Convert.ToInt32(txtSN.Text);
                }
            }
            else
            {
                MessageBox.Show("Please enter value: " + trackSN.Minimum.ToString() +" ~ " + trackSN.Maximum.ToString());
                txtSN.Text = trackSN.Value.ToString();
            }
            
        }

        private void txtPeakPeakBackgroundRatioRatio_TextChanged(object sender, EventArgs e)
        {
            float ParsedValue = 0;
            if (float.TryParse(txtPeakPeakBackgroundRatioRatio.Text, out ParsedValue))
            {
                ParsedValue = ParsedValue * 100.0f;
                if (ParsedValue > trackPeakBackgroundRatio.Maximum)
                {
                    trackPeakBackgroundRatio.Value = (trackPeakBackgroundRatio.Maximum);
                    txtPeakPeakBackgroundRatioRatio.Text = (trackPeakBackgroundRatio.Value/100.0).ToString();
                }
                else if (ParsedValue < trackPeakBackgroundRatio.Minimum)
                {
                    trackPeakBackgroundRatio.Value = trackPeakBackgroundRatio.Minimum;
                    txtPeakPeakBackgroundRatioRatio.Text = (trackPeakBackgroundRatio.Value/100.0).ToString();
                }
                else
                {
                    trackPeakBackgroundRatio.Value = Convert.ToInt32(ParsedValue);
                }
            }
            else
            {
                MessageBox.Show("Please enter value: " + (trackPeakBackgroundRatio.Minimum/100.0).ToString() + " ~ " + (trackPeakBackgroundRatio.Maximum/100.0).ToString());
                txtPeakPeakBackgroundRatioRatio.Text = (trackPeakBackgroundRatio.Value/100.0).ToString();
            }
        }

        private void txtPeptideMinRatio_TextChanged(object sender, EventArgs e)
        {
            float ParsedValue = 0;
            if (float.TryParse(txtPeptideMinRatio.Text, out ParsedValue))
            {
                ParsedValue = ParsedValue * 100.0f;
                if (ParsedValue > trackPeptideMinRatio.Maximum)
                {
                    trackPeptideMinRatio.Value = (trackPeptideMinRatio.Maximum);
                    txtPeptideMinRatio.Text = (trackPeptideMinRatio.Value / 100.0).ToString();
                }
                else if (ParsedValue < trackPeptideMinRatio.Minimum)
                {
                    trackPeptideMinRatio.Value = trackPeptideMinRatio.Minimum;
                    txtPeptideMinRatio.Text = (trackPeptideMinRatio.Value / 100.0).ToString();
                }
                else
                {
                    trackPeptideMinRatio.Value = Convert.ToInt32(ParsedValue);
                }
            }
            else
            {
                MessageBox.Show("Please enter value: " + (trackPeptideMinRatio.Minimum / 100.0).ToString() + " ~ " + (trackPeptideMinRatio.Maximum / 100.0).ToString());
                txtPeptideMinRatio.Text = (trackPeptideMinRatio.Value / 100.0).ToString();
            }
        }

        private void txtMaxCharge_TextChanged(object sender, EventArgs e)
        {
            int ParsedValue = 0;
            if (Int32.TryParse(txtMaxCharge.Text, out ParsedValue))
            {
                if (ParsedValue > trackMaxCharge.Maximum)
                {
                    trackMaxCharge.Value = trackMaxCharge.Maximum;
                    txtMaxCharge.Text = trackMaxCharge.Value.ToString();
                }
                else if (ParsedValue < trackMaxCharge.Minimum)
                {
                    trackMaxCharge.Value = trackMaxCharge.Minimum;
                    txtMaxCharge.Text = trackMaxCharge.Value.ToString();
                }
                else
                {
                    trackMaxCharge.Value = Convert.ToInt32(ParsedValue);
                }
            }
            else
            {
                MessageBox.Show("Please enter value: " + trackMaxCharge.Minimum.ToString() + " ~ " + trackMaxCharge.Maximum.ToString());
                txtMaxCharge.Text = trackMaxCharge.Value.ToString();
            }
        }

 
    }
}
