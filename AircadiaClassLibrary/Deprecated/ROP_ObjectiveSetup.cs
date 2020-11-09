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
    public partial class ROP_ObjectiveSetup : Form
    {
        public OP_Input_set options;
        public Data dataObj;
        public string[] UncertaintyObjectiveData; //This contains:[name,k_obj,intent]

        public ROP_ObjectiveSetup(Data lio)
        {
            InitializeComponent();
            dataObj = lio;
            label_ROP_objective_Name.Text = dataObj.Name;
        }

        private void button_ROP_objective_set_Click(object sender, EventArgs e)
        {
            // Validity check of data:
            double MissingData = 0;
            if (textBox_ROP_objective_Prb.Text == "")
            {
                MessageBox.Show("Please enter desired satisfaction probability value");
                MissingData = 1;
            }

            if (MissingData==0)
            {
                //Formulation Option 1:
                if (radioButton_ROP_FormulationOption1.Checked == true)
                {
                    UncertaintyObjectiveData = new string[7];
                    UncertaintyObjectiveData[0] = dataObj.Name;
                    UncertaintyObjectiveData[1] = Convert.ToString(0);//Coefficient k
                    if (comboBox_ROP_objective_minmax_FormulationOption1.Text == "To be minimised")
                    {
                        UncertaintyObjectiveData[2] = "minimise";
                    }
                    else if (comboBox_ROP_objective_minmax_FormulationOption1.Text == "To be maximised")
                    {
                        UncertaintyObjectiveData[2] = "maximise";
                    }
                    UncertaintyObjectiveData[3] = "0";//Requested probability - NA
                    UncertaintyObjectiveData[4] = "None";//Distributional assumption - NA
                    UncertaintyObjectiveData[5] = "quantile";//Metric - NA

                    if (comboBox_ROP_objective_minmax_FormulationOption1.SelectedItem as string == "To be minimised")
                    {
                        var des_set = new OP_Input_set("min", 1.0, "max", 0.0, "desobj", "obj", "Data", dataObj.Name, "k_coeff", UncertaintyObjectiveData[1], "LossFuncSign", UncertaintyObjectiveData[6], "Probability", UncertaintyObjectiveData[3], "Distribution", UncertaintyObjectiveData[4], "Metric", UncertaintyObjectiveData[5]);
                        options = des_set;
                    }
                    else
                    {
                        var des_set = new OP_Input_set("min", 1.0, "max", 0.0, "desobj", "obj", "Data", dataObj.Name, "k_coeff", UncertaintyObjectiveData[1], "LossFuncSign", UncertaintyObjectiveData[6], "Probability", UncertaintyObjectiveData[3], "Distribution", UncertaintyObjectiveData[4], "Metric", UncertaintyObjectiveData[5]);
                        options = des_set;
                    }
                }
                //Formulation Option 2:
                else if (radioButton_ROP_FormulationOption2.Checked == true)
                {
                    UncertaintyObjectiveData = new string[7];
                    UncertaintyObjectiveData[0] = dataObj.Name;
                    UncertaintyObjectiveData[1] = textBox_ROP_FormulationOption2K.Text;//Coefficient k
                    if (comboBox_ROP_objective_minmax_FormulationOption2.Text == "To be minimised")
                    {
                        UncertaintyObjectiveData[2] = "minimise";
                    }
                    else if (comboBox_ROP_objective_minmax_FormulationOption2.Text == "To be maximised")
                    {
                        UncertaintyObjectiveData[2] = "maximise";
                    }
                    UncertaintyObjectiveData[3] = "0";//Requested probability - NA
                    UncertaintyObjectiveData[4] = "None";//Distributional assumption - NA
                    UncertaintyObjectiveData[5] = "quantile";//Metric - NA
                    UncertaintyObjectiveData[6] = comboBox_ROP_FormulationOption2Sign.Text;//Sign in Loss Function

                    if (comboBox_ROP_objective_minmax_FormulationOption2.SelectedItem as string == "To be minimised")
                    {
                        var des_set = new OP_Input_set("min", 1.0, "max", 0.0, "desobj", "obj", "Data", dataObj.Name, "k_coeff", UncertaintyObjectiveData[1], "LossFuncSign", UncertaintyObjectiveData[6], "Probability", UncertaintyObjectiveData[3], "Distribution", UncertaintyObjectiveData[4], "Metric", UncertaintyObjectiveData[5]);
                        options = des_set;
                    }
                    else
                    {
                        var des_set = new OP_Input_set("min", 0.0, "max", 1.0, "desobj", "obj", "Data", dataObj.Name, "k_coeff", UncertaintyObjectiveData[1], "LossFuncSign", UncertaintyObjectiveData[6], "Probability", UncertaintyObjectiveData[3], "Distribution", UncertaintyObjectiveData[4], "Metric", UncertaintyObjectiveData[5]);
                        options = des_set;
                    }
                }
                //Formulation Option 3:
                else if (radioButton_ROP_FormulationOption3.Checked == true)
                {
                    // Computing the coefficient k:
                    double coefficient_k = k_calculation(Convert.ToDouble(textBox_ROP_objective_Prb.Text), comboBox_ROP_objective_DstrbnAssmptn.Text);
                    // Storing variable information:
                    UncertaintyObjectiveData = new string[7];
                    UncertaintyObjectiveData[0] = dataObj.Name;
                    UncertaintyObjectiveData[1] = Convert.ToString(coefficient_k);
                    if (comboBox_ROP_objective_minmax_FormulationOption3.Text == "To be minimised")
                    {
                        UncertaintyObjectiveData[2] = "minimise";
                    }
                    else if (comboBox_ROP_objective_minmax_FormulationOption3.Text == "To be maximised")
                    {
                        UncertaintyObjectiveData[2] = "maximise";
                    }
                    UncertaintyObjectiveData[3] = textBox_ROP_objective_Prb.Text;//Requested probability
                    UncertaintyObjectiveData[4] = comboBox_ROP_objective_DstrbnAssmptn.Text;//Distributional assumption
                    UncertaintyObjectiveData[5] = comboBox_ROP_objective_Metric.Text;//Metric
                    UncertaintyObjectiveData[6] = comboBox_ROP_FormulationOption3Sign.Text;//Sign in Loss Function

                    if (comboBox_ROP_objective_minmax_FormulationOption3.SelectedItem as string == "To be minimised")
                    {
                        var des_set = new OP_Input_set("min", 1.0, "max", 0.0, "desobj", "obj", "Data", dataObj.Name, "k_coeff", UncertaintyObjectiveData[1], "LossFuncSign", UncertaintyObjectiveData[6], "Probability", UncertaintyObjectiveData[3], "Distribution", UncertaintyObjectiveData[4], "Metric", UncertaintyObjectiveData[5]);
                        options = des_set;
                    }
                    else
                    {
                        var des_set = new OP_Input_set("min", 0.0, "max", 1.0, "desobj", "obj", "Data", dataObj.Name, "k_coeff", UncertaintyObjectiveData[1], "LossFuncSign", UncertaintyObjectiveData[6], "Probability", UncertaintyObjectiveData[3], "Distribution", UncertaintyObjectiveData[4], "Metric", UncertaintyObjectiveData[5]);
                        options = des_set;
                    }
                }
            }


			Close();
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
                    k = Math.Sqrt((3 * Prb) / (4 - 3 * Prb));
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
            if (comboBox_ROP_objective_Metric.Text == "Quantile")
            {
                comboBox_ROP_objective_DstrbnAssmptn.Items.Clear();
                comboBox_ROP_objective_DstrbnAssmptn.Items.AddRange(new object[] {
                "None",
                "Symmetry",
                "Unimodality",
                "Symmetry+Unimodality"});
                comboBox_ROP_objective_DstrbnAssmptn.Text = "None";
            }
            else if (comboBox_ROP_objective_Metric.Text == "TCE")
            {
                comboBox_ROP_objective_DstrbnAssmptn.Items.Clear();
                comboBox_ROP_objective_DstrbnAssmptn.Items.AddRange(new object[] {
                "None",
                "Symmetry"});
                comboBox_ROP_objective_DstrbnAssmptn.Text = "None";
            }
        }

        private void radioButton_ROP_FormulationOption1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_ROP_FormulationOption1.Checked == true)
            {
                //Option1:
                label_ROP_FormulationOption1.Enabled = true;
                comboBox_ROP_FormulationOption1.Enabled = true;
                comboBox_ROP_objective_minmax_FormulationOption1.Enabled = true;
                //Option2:
                label_ROP_FormulationOption2.Enabled = false;
                label_ROP_FormulationOption2Mean.Enabled = false;
                comboBox_ROP_FormulationOption2Sign.Enabled = false;
                textBox_ROP_FormulationOption2K.Enabled = false;
                label_ROP_FormulationOption2Variance.Enabled = false;
                comboBox_ROP_objective_minmax_FormulationOption2.Enabled = false;
                //Option3:
                label_ROP_FormulationOption3.Enabled = false;
                label_ROP_FormulationOption3Mean.Enabled = false;
                comboBox_ROP_FormulationOption3Sign.Enabled = false;
                label_ROP_FormulationOption3K.Enabled = false;
                label_ROP_FormulationOption3Variance.Enabled = false;
                comboBox_ROP_objective_minmax_FormulationOption3.Enabled = false;
                label_ROP_FormulationOption3LossFunc_with.Enabled = false;
                textBox_ROP_objective_Prb.Enabled = false;
                label_ROP_objective_Prb.Enabled = false;
                label_ROP_objective_Prb_subscript.Enabled = false;
                label_ROP_objective_DstrbnAssmptn.Enabled = false;
                comboBox_ROP_objective_DstrbnAssmptn.Enabled = false;
                label_ROP_objective_Metric.Enabled = false;
                comboBox_ROP_objective_Metric.Enabled = false;
            }
            else if (radioButton_ROP_FormulationOption2.Checked == true)
            {
                //Option1:
                label_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_objective_minmax_FormulationOption1.Enabled = false;
                //Option2:
                label_ROP_FormulationOption2.Enabled = true;
                label_ROP_FormulationOption2Mean.Enabled = true;
                comboBox_ROP_FormulationOption2Sign.Enabled = true;
                textBox_ROP_FormulationOption2K.Enabled = true;
                label_ROP_FormulationOption2Variance.Enabled = true;
                comboBox_ROP_objective_minmax_FormulationOption2.Enabled = true;
                //Option3:
                label_ROP_FormulationOption3.Enabled = false;
                label_ROP_FormulationOption3Mean.Enabled = false;
                comboBox_ROP_FormulationOption3Sign.Enabled = false;
                label_ROP_FormulationOption3K.Enabled = false;
                label_ROP_FormulationOption3Variance.Enabled = false;
                comboBox_ROP_objective_minmax_FormulationOption3.Enabled = false;
                label_ROP_FormulationOption3LossFunc_with.Enabled = false;
                textBox_ROP_objective_Prb.Enabled = false;
                label_ROP_objective_Prb.Enabled = false;
                label_ROP_objective_Prb_subscript.Enabled = false;
                label_ROP_objective_DstrbnAssmptn.Enabled = false;
                comboBox_ROP_objective_DstrbnAssmptn.Enabled = false;
                label_ROP_objective_Metric.Enabled = false;
                comboBox_ROP_objective_Metric.Enabled = false;
            }
            else if (radioButton_ROP_FormulationOption3.Checked == true)
            {
                //Option1:
                label_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_FormulationOption1.Enabled = false;
                comboBox_ROP_objective_minmax_FormulationOption1.Enabled = false;
                //Option2:
                label_ROP_FormulationOption2.Enabled = false;
                label_ROP_FormulationOption2Mean.Enabled = false;
                comboBox_ROP_FormulationOption2Sign.Enabled = false;
                textBox_ROP_FormulationOption2K.Enabled = false;
                label_ROP_FormulationOption2Variance.Enabled = false;
                comboBox_ROP_objective_minmax_FormulationOption2.Enabled = false;
                //Option3:
                label_ROP_FormulationOption3.Enabled = true;
                label_ROP_FormulationOption3Mean.Enabled = true;
                comboBox_ROP_FormulationOption3Sign.Enabled = true;
                label_ROP_FormulationOption3K.Enabled = true;
                label_ROP_FormulationOption3Variance.Enabled = true;
                comboBox_ROP_objective_minmax_FormulationOption3.Enabled = true;
                label_ROP_FormulationOption3LossFunc_with.Enabled = true;
                textBox_ROP_objective_Prb.Enabled = true;
                label_ROP_objective_Prb.Enabled = true;
                label_ROP_objective_Prb_subscript.Enabled = true;
                label_ROP_objective_DstrbnAssmptn.Enabled = true;
                comboBox_ROP_objective_DstrbnAssmptn.Enabled = true;
                label_ROP_objective_Metric.Enabled = true;
                comboBox_ROP_objective_Metric.Enabled = true;
            }
        }

        private void radioButton_ROP_FormulationOption2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton_ROP_FormulationOption1_CheckedChanged(sender, e);
        }

        private void button_ROP_objective_cancel_Click(object sender, EventArgs e)
        {
			Close();
        }


        
        /*private void button_OP_objective_set_Click(object sender, EventArgs e)
        {
            if (comboBox_OP_objective_minmax.SelectedItem as string == "minimise")
            {
                cOP_Input_set des_set = new cOP_Input_set("min", 1.0, "max", 0.0, "desobj", "obj", "Data", dataObj.name);
                options = des_set;
            }
            else
            {
                cOP_Input_set des_set = new cOP_Input_set("min", 0.0, "max", 1.0, "desobj", "obj", "Data", dataObj.name);
                options = des_set;
            }
            this.Close();
        }*/
    }
}
