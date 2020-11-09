/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Aircadia.ObjectModel.DataObjects;

namespace Aircadia
{
    [Serializable()]
    public partial class ROP_ConstraintSetup : Form
    {
        public Data dataobj;
        public OP_C_Input_set options;
        public string[] UncertaintyConstraintData; //This contains:[name,k_cnstr,intent,limit]

        public ROP_ConstraintSetup(Data dt)
        {
            dataobj = dt;
            InitializeComponent();
            label_ROP_constraint_Name.Text = dt.Name;
        }
        
        private void button_ROP_constraint_set_Click(object sender, EventArgs e)
        {
            // Validity check of data:
            double MissingData = 0;
            if (textBox_ROP_constraint_Prb.Text == "")
            {
                MessageBox.Show("Please enter desired satisfaction probability.");
                MissingData = 1;
            }
            else if (radioButton_ROP_FormulationOption1.Checked == true  &&  textBox_ROP_FormulationOption1_LimitValue.Text == "")
            {
                MessageBox.Show("Please enter the limit value of the constraint.");
                MissingData = 1;
            }
            else if (radioButton_ROP_FormulationOption2.Checked == true && textBox_ROP_FormulationOption2_LimitValue.Text == "")
            {
                MessageBox.Show("Please enter the limit value of the constraint.");
                MissingData = 1;
            }
            else if (radioButton_ROP_FormulationOption3.Checked == true && textBox_ROP_FormulationOption3_LimitValue.Text == "")
            {
                MessageBox.Show("Please enter the limit value of the constraint.");
                MissingData = 1;
            }

            if (MissingData == 0)
            {
                //Formulation Option 1:
                if ( radioButton_ROP_FormulationOption1.Checked == true)
                {
                    UncertaintyConstraintData = new string[8];
                    UncertaintyConstraintData[0] = dataobj.Name;
                    UncertaintyConstraintData[1] = "0";//Coefficient k
                    UncertaintyConstraintData[2] = comboBox_ROP_FormulationOption1_InequalitySign.Text;
                    UncertaintyConstraintData[3] = textBox_ROP_FormulationOption1_LimitValue.Text;
                    UncertaintyConstraintData[4] = "0";//Requested probability - NA
                    UncertaintyConstraintData[5] = "None";//Distributional assumption - NA
                    UncertaintyConstraintData[6] = "quantile";//Metric - NA

                    options = new OP_C_Input_set("Constraint", comboBox_ROP_FormulationOption1_InequalitySign.SelectedIndex, "Value", textBox_ROP_FormulationOption1_LimitValue.Text, "Data", dataobj.Name, "k_coeff", UncertaintyConstraintData[1], "LossFuncSign", UncertaintyConstraintData[7], "Probability", UncertaintyConstraintData[4], "Distribution", UncertaintyConstraintData[5], "Metric", UncertaintyConstraintData[6], "InequalitySign", UncertaintyConstraintData[2]);
					Close();
                }
                //Formulation Option 2:
                else if (radioButton_ROP_FormulationOption2.Checked == true)
                {
                    UncertaintyConstraintData = new string[8];
                    UncertaintyConstraintData[0] = dataobj.Name;
                    UncertaintyConstraintData[1] = textBox_ROP_FormulationOption2K.Text;//Coefficient k
                    UncertaintyConstraintData[2] = comboBox_ROP_FormulationOption2_InequalitySign.Text;
                    UncertaintyConstraintData[3] = textBox_ROP_FormulationOption2_LimitValue.Text;
                    UncertaintyConstraintData[4] = "0";//Requested probability - NA
                    UncertaintyConstraintData[5] = "None";//Distributional assumption - NA
                    UncertaintyConstraintData[6] = "quantile";//Metric - NA
                    UncertaintyConstraintData[7] = comboBox_ROP_FormulationOption2Sign.Text;//Sign in Loss Function

                    options = new OP_C_Input_set("Constraint", comboBox_ROP_FormulationOption2_InequalitySign.SelectedIndex, "Value", textBox_ROP_FormulationOption2_LimitValue.Text, "Data", dataobj.Name, "k_coeff", UncertaintyConstraintData[1], "LossFuncSign", UncertaintyConstraintData[7], "Probability", UncertaintyConstraintData[4], "Distribution", UncertaintyConstraintData[5], "Metric", UncertaintyConstraintData[6], "InequalitySign", UncertaintyConstraintData[2]);
					Close();
                }
                //Formulation Option 3:
                else if (radioButton_ROP_FormulationOption3.Checked == true)
                {
                    // Computing the coefficient k:
                    double coefficient_k = k_calculation(Convert.ToDouble(textBox_ROP_constraint_Prb.Text), comboBox_ROP_constraint_DstrbnAssmptn.Text);
                    // Storing variable information:
                    UncertaintyConstraintData = new string[8];
                    UncertaintyConstraintData[0] = dataobj.Name;
                    UncertaintyConstraintData[1] = Convert.ToString(coefficient_k);
                    UncertaintyConstraintData[2] = comboBox_ROP_FormulationOption3_InequalitySign.Text;
                    UncertaintyConstraintData[3] = textBox_ROP_FormulationOption3_LimitValue.Text;
                    UncertaintyConstraintData[4] = textBox_ROP_constraint_Prb.Text;
                    UncertaintyConstraintData[5] = comboBox_ROP_constraint_DstrbnAssmptn.Text;
                    UncertaintyConstraintData[6] = comboBox_ROP_constraint_Metric.Text;
                    UncertaintyConstraintData[7] = comboBox_ROP_FormulationOption3Sign.Text;//Sign in Loss Function

                    options = new OP_C_Input_set("Constraint", comboBox_ROP_FormulationOption3_InequalitySign.SelectedIndex, "Value", textBox_ROP_FormulationOption3_LimitValue.Text, "Data", dataobj.Name, "k_coeff", UncertaintyConstraintData[1], "LossFuncSign", UncertaintyConstraintData[7], "Probability", UncertaintyConstraintData[4], "Distribution", UncertaintyConstraintData[5], "Metric", UncertaintyConstraintData[6], "InequalitySign", UncertaintyConstraintData[2]);
					Close();
                }
            }
        }

        public double k_calculation(double Prb, string Assumption)
        {
            double k = new double();
            if (Assumption == "None")
            {
                k = Math.Sqrt(Prb / (1 - Prb));
            }
            else if (Assumption == "Normality")
            {
                k = k_approximation_Normal_PDF(Prb);
            }
            else if (Assumption == "Symmetry")
            {
                if (Prb >= 0.5)
                {
                    k = 1 / Math.Sqrt(2 * (1 - Prb));
                }
            }
            else if (Assumption == "Unimodality")
            {
                if (Prb >= 5 / 6)
                {
                    k = Math.Sqrt((9 * Prb - 5) / (9 * (1 - Prb)));
                }
                else if (Prb < 5 / 6)
                {
                    k = Math.Sqrt((3 * Prb) / (3 * Prb - 2));
                }
            }
            else if (Assumption == "Symmetry+Unimodality")
            {
                if (Prb >= 0.5)
                {
                    k = Math.Sqrt(2 / (9 * (1 - Prb)));
                }
            }

            return k;
        }

        public double k_approximation_Normal_PDF(double Prb)
        {
            double a = 8 * (Math.PI - 3) / (3 * Math.PI * (4 - Math.PI));
            double x = 2 * Prb - 1;
            double erf_inv_apprx = x / Math.Abs(x) * Math.Sqrt((Math.Sqrt(Math.Pow(2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2, 2) - Math.Log(1 - Math.Pow(x, 2)) / a)) - (2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2));
            double k_apprx = Math.Sqrt(2) * erf_inv_apprx;

            return k_apprx;
        }

        private void comboBox_ROP_objective_Metric_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_ROP_constraint_Metric.Text == "Quantile")
            {
                comboBox_ROP_constraint_DstrbnAssmptn.Items.Clear();
                comboBox_ROP_constraint_DstrbnAssmptn.Items.AddRange(new object[] {
                "None",
                "Symmetry",
                "Unimodality",
                "Symmetry+Unimodality"});
                comboBox_ROP_constraint_DstrbnAssmptn.Text = "None";
            }
            else if (comboBox_ROP_constraint_Metric.Text == "TCE")
            {
                comboBox_ROP_constraint_DstrbnAssmptn.Items.Clear();
                comboBox_ROP_constraint_DstrbnAssmptn.Items.AddRange(new object[] {
                "None",
                "Symmetry"});
                comboBox_ROP_constraint_DstrbnAssmptn.Text = "None";
            }
        }

        private void button_ROP_objective_cancel_Click(object sender, EventArgs e)
        {
			Close();
        }

        private void radioButton_ROP_FormulationOption1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_ROP_FormulationOption1.Checked == true)
            {
                //Option1:
                label_ROP_FormulationOption1.Enabled = true;
                comboBox_ROP_FormulationOption1.Enabled = true;
                comboBox_ROP_FormulationOption1_InequalitySign.Enabled = true;
                textBox_ROP_FormulationOption1_LimitValue.Enabled = true;
                label_ROP_FormulationOption1_constraint_Limit.Enabled = true;
                //Option2:
                label_ROP_FormulationOption2.Enabled = false;
                label_ROP_FormulationOption2Mean.Enabled = false;
                comboBox_ROP_FormulationOption2Sign.Enabled = false;
                textBox_ROP_FormulationOption2K.Enabled = false;
                label_ROP_FormulationOption2Variance.Enabled = false;
                comboBox_ROP_FormulationOption2_InequalitySign.Enabled = false;
                textBox_ROP_FormulationOption2_LimitValue.Enabled = false;
                label_ROP_FormulationOption2_constraint_Limit.Enabled = false;
                //Option3:
                label_ROP_FormulationOption3.Enabled = false;
                label_ROP_FormulationOption3Mean.Enabled = false;
                comboBox_ROP_FormulationOption3Sign.Enabled = false;
                label_ROP_FormulationOption3K.Enabled = false;
                label_ROP_FormulationOption3Variance.Enabled = false;
                comboBox_ROP_FormulationOption3_InequalitySign.Enabled = false;
                textBox_ROP_FormulationOption3_LimitValue.Enabled = false;
                label_ROP_FormulationOption3_constraint_Limit.Enabled = false;
                label_ROP_FormulationOption3LossFunc_with.Enabled = false;
                label_ROP_constraint_Prb.Enabled = false;
                label_ROP_constraint_Prb_subscript.Enabled = false;
                textBox_ROP_constraint_Prb.Enabled = false;
                label_ROP_constraint_DstrbnAssmptn.Enabled = false;
                comboBox_ROP_constraint_DstrbnAssmptn.Enabled = false;
                label_ROP_constraint_Metric.Enabled = false;
                comboBox_ROP_constraint_Metric.Enabled = false;
            }
            else if (radioButton_ROP_FormulationOption2.Checked == true)
            {
                //Option1:
                label_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_FormulationOption1_InequalitySign.Enabled = false;
                textBox_ROP_FormulationOption1_LimitValue.Enabled = false;
                label_ROP_FormulationOption1_constraint_Limit.Enabled = false;
                //Option2:
                label_ROP_FormulationOption2.Enabled = true;
                label_ROP_FormulationOption2Mean.Enabled = true;
                comboBox_ROP_FormulationOption2Sign.Enabled = true;
                textBox_ROP_FormulationOption2K.Enabled = true;
                label_ROP_FormulationOption2Variance.Enabled = true;
                comboBox_ROP_FormulationOption2_InequalitySign.Enabled = true;
                textBox_ROP_FormulationOption2_LimitValue.Enabled = true;
                label_ROP_FormulationOption2_constraint_Limit.Enabled = true;
                //Option3:
                label_ROP_FormulationOption3.Enabled = false;
                label_ROP_FormulationOption3Mean.Enabled = false;
                comboBox_ROP_FormulationOption3Sign.Enabled = false;
                label_ROP_FormulationOption3K.Enabled = false;
                label_ROP_FormulationOption3Variance.Enabled = false;
                comboBox_ROP_FormulationOption3_InequalitySign.Enabled = false;
                textBox_ROP_FormulationOption3_LimitValue.Enabled = false;
                label_ROP_FormulationOption3_constraint_Limit.Enabled = false;
                label_ROP_FormulationOption3LossFunc_with.Enabled = false;
                label_ROP_constraint_Prb.Enabled = false;
                label_ROP_constraint_Prb_subscript.Enabled = false;
                textBox_ROP_constraint_Prb.Enabled = false;
                label_ROP_constraint_DstrbnAssmptn.Enabled = false;
                comboBox_ROP_constraint_DstrbnAssmptn.Enabled = false;
                label_ROP_constraint_Metric.Enabled = false;
                comboBox_ROP_constraint_Metric.Enabled = false;
            }
            else if (radioButton_ROP_FormulationOption3.Checked == true)
            {
                //Option1:
                label_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_FormulationOption1_InequalitySign.Enabled = false;
                textBox_ROP_FormulationOption1_LimitValue.Enabled = false;
                label_ROP_FormulationOption1_constraint_Limit.Enabled = false;
                //Option2:
                label_ROP_FormulationOption2.Enabled = false;
                label_ROP_FormulationOption2Mean.Enabled = false;
                comboBox_ROP_FormulationOption2Sign.Enabled = false;
                textBox_ROP_FormulationOption2K.Enabled = false;
                label_ROP_FormulationOption2Variance.Enabled = false;
                comboBox_ROP_FormulationOption2_InequalitySign.Enabled = false;
                textBox_ROP_FormulationOption2_LimitValue.Enabled = false;
                label_ROP_FormulationOption2_constraint_Limit.Enabled = false;
                //Option3:
                label_ROP_FormulationOption3.Enabled = true;
                label_ROP_FormulationOption3Mean.Enabled = true;
                comboBox_ROP_FormulationOption3Sign.Enabled = true;
                label_ROP_FormulationOption3K.Enabled = true;
                label_ROP_FormulationOption3Variance.Enabled = true;
                comboBox_ROP_FormulationOption3_InequalitySign.Enabled = true;
                textBox_ROP_FormulationOption3_LimitValue.Enabled = true;
                label_ROP_FormulationOption3_constraint_Limit.Enabled = true;
                label_ROP_FormulationOption3LossFunc_with.Enabled = true;
                label_ROP_constraint_Prb.Enabled = true;
                label_ROP_constraint_Prb_subscript.Enabled = true;
                textBox_ROP_constraint_Prb.Enabled = true;
                label_ROP_constraint_DstrbnAssmptn.Enabled = true;
                comboBox_ROP_constraint_DstrbnAssmptn.Enabled = true;
                label_ROP_constraint_Metric.Enabled = true;
                comboBox_ROP_constraint_Metric.Enabled = true;
            }
        }

        private void radioButton_ROP_FormulationOption2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton_ROP_FormulationOption1_CheckedChanged(sender, e);
        }

        private void radioButton_ROP_FormulationOption3_CheckedChanged(object sender, EventArgs e)
        {
            radioButton_ROP_FormulationOption1_CheckedChanged(sender, e);
        }

        /*private void fConstraint_button_ok_Click(object sender, EventArgs e)
        {
            options = new cOP_C_Input_set("Constraint", comboBox_ROP_constraint_minmax.SelectedIndex, "Value", textBox_ROP_constraint_Limit.Text, "Data", dataobj.name);
            this.Close();
        }*/
    }
}
