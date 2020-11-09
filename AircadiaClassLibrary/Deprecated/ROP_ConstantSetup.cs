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
    public partial class ROP_ConstantSetup : Form
    {
        public Data dataobj;
        public OP_C_Input_set options;
        public string[] UncertaintyConstantData; //This contains:[name,IICM,IIICM,IVCM]
        public string[] UncertaintyConstantData_SA;//Xin for SA

        public ROP_ConstantSetup(Data dt, string StudyType)
        {
            dataobj = dt;
            InitializeComponent();
            label_ROP_constant_Name.Text = dt.Name;
            textBox_ROP_ConstSetup_PDFparameter1.Text = Convert.ToString(dataobj.ValueAsDouble);
            textBox_ROP_ConstSetup_PDFparameter2.Text = Convert.ToString(0.05 * dataobj.ValueAsDouble);
            if (StudyType == "RA")
            {
				Text = "Variable Setup";
                radioButton_ROP_UncertainConstant.Checked = true;
                label_ROP_constant_Name.Enabled = true;
                label_ROP_constant_Name.Location = new Point(182, 35);
                radioButton_ROP_UncertainConstant.Visible = false;
                radioButton_ROP_CertainConstant.Visible = false;
                label_ROP_constant_equal.Visible = false;
                textBox_ROP_constant_Value.Visible = false;
            }
            else
            {
                textBox_ROP_constant_Value.Text = Convert.ToString(dataobj.ValueAsDouble);
            }
        }

        private void button__ROP_constant_set_Click(object sender, EventArgs e)
        {
            if (radioButton_ROP_CertainConstant.Checked == true)
            {
                // Validity check of data:
                double MissingData = 0;
                if (textBox_ROP_constant_Value.Text == "")
                {
                    MessageBox.Show("Please enter constant value");
                    MissingData = 1;
                }

                if (MissingData == 0)
                {
                    // Storing constant information:
                    UncertaintyConstantData = new string[2];
                    UncertaintyConstantData[0] = dataobj.Name;
                    UncertaintyConstantData[1] = textBox_ROP_constant_Value.Text;
                }
                options = new OP_C_Input_set("Value", textBox_ROP_constant_Value.Text, "Data", dataobj.Name);
				Close();
            }
            else if (radioButton_ROP_UncertainConstant.Checked == true)
            {
                // Validity check of data:
                double MissingData = 0;
                if (textBox_ROP_ConstSetup_PDFparameter1.Text == "")
                {
                    MessageBox.Show("Please enter first PDF parameter");
                    MissingData = 1;
                }
                else if (textBox_ROP_ConstSetup_PDFparameter2.Text == "")
                {
                    MessageBox.Show("Please enter second PDF parameter");
                    MissingData = 1;
                }
                else if (comboBox_ROP_VarSetup_PDF.Text == "User defined" && textBox_ROP_ConstSetup_PDFparameter3.Text == "")
                {
                    MessageBox.Show("Please enter third PDF parameter");
                    MissingData = 1;
                }
                else if (comboBox_ROP_VarSetup_PDF.Text == "User defined" && textBox_ROP_ConstSetup_PDFparameter4.Text == "")
                {
                    MessageBox.Show("Please enter fourth PDF parameter");
                    MissingData = 1;
                }

                // Computing the central moments according to the chosen PDF:
                double ICM;
                double IICM;
                double IIICM;
                double IVCM;
                if (MissingData == 0)
                {
                    if (comboBox_ROP_VarSetup_PDF.Text == "Normal")
                    {
                        ICM = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter1.Text);
                        IICM = Math.Pow(Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter2.Text), 2); //This is the variance, not the standard deviation!!!!
                        IIICM = 0;
                        IVCM = 3;

                        // Storing variable information:
                        (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                        UncertaintyConstantData = new string[8];
                        UncertaintyConstantData[0] = dataobj.Name;
                        UncertaintyConstantData[1] = Convert.ToString(IICM);
                        UncertaintyConstantData[2] = Convert.ToString(IIICM);
                        UncertaintyConstantData[3] = Convert.ToString(IVCM);
                        //UncertaintyConstantData[4] = textBox_ROP_VarSetup_StartVal.Text;
                        //UncertaintyConstantData[5] = Convert.ToString(ROB_LB);
                        //UncertaintyConstantData[6] = Convert.ToString(ROB_UB);
                        //UncertaintyConstantData[7] = textBox_ROP_VarSetup_Prb.Text;

                        options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "Normal");


                        #region Store data for SA   Xin
                        UncertaintyConstantData_SA = new string[8];
                        UncertaintyConstantData_SA[0] = dataobj.Name;
                        UncertaintyConstantData_SA[1] = "Normal";
                        UncertaintyConstantData_SA[2] = textBox_ROP_ConstSetup_PDFparameter1.Text;     //MEAN
                        UncertaintyConstantData_SA[3] = textBox_ROP_ConstSetup_PDFparameter2.Text;     //std
                        UncertaintyConstantData_SA[4] = "0";    //d2
                        UncertaintyConstantData_SA[5] = "0";
						#endregion


						Close();
                    }
                    if (comboBox_ROP_VarSetup_PDF.Text == "Triangular")
                    {
                        // Getting the parameter values provided by the user:
                        double d1 = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter2.Text);
                        double d2 = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter3.Text);
                        double d3 = d1 - d2;

                        // Computing the central moments:
                        ICM = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter1.Text);
                        IICM = ((d1 + d2) * (d1 + d2) * (1 - (d1 + d3) * (d2 - d3) / ((d1 + d2) * (d1 + d2)))) / 18;//This is the variance, not the standard deviation!!!!
                        IIICM = Math.Sqrt(2) / 5 * (d2 - d1 - 2 * d3) * (-2 * d1 - d2 - d3) * (-d1 - 2 * d2 + d3) / Math.Pow(((d1 + d2) * (d1 + d2) * (1 - (d1 + d3) * (d2 - d3) / ((d1 + d2) * (d1 + d2)))), (3.0 / 2.0));
                        IVCM = 12.0 / 5;

                        // Storing variable information:
                        (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                        UncertaintyConstantData = new string[8];
                        UncertaintyConstantData[0] = dataobj.Name;
                        UncertaintyConstantData[1] = Convert.ToString(IICM);
                        UncertaintyConstantData[2] = Convert.ToString(IIICM);
                        UncertaintyConstantData[3] = Convert.ToString(IVCM);
                        //UncertaintyConstantData[4] = textBox_ROP_VarSetup_StartVal.Text;
                        //UncertaintyConstantData[5] = Convert.ToString(ROB_LB);
                        //UncertaintyConstantData[6] = Convert.ToString(ROB_UB);
                        //UncertaintyConstantData[7] = textBox_ROP_VarSetup_Prb.Text;

                        options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "Triangular", "Parameter1", d1, "Parameter2", d2);


                        #region Store data for SA   Xin
                        UncertaintyConstantData_SA = new string[8];
                        UncertaintyConstantData_SA[0] = dataobj.Name;
                        UncertaintyConstantData_SA[1] = "Triangular";
                        UncertaintyConstantData_SA[2] = textBox_ROP_ConstSetup_PDFparameter1.Text;    //MEAN
                        UncertaintyConstantData_SA[3] = textBox_ROP_ConstSetup_PDFparameter2.Text;    //d1
                        UncertaintyConstantData_SA[4] = textBox_ROP_ConstSetup_PDFparameter3.Text;    //d2
                        UncertaintyConstantData_SA[5] = "0";
						#endregion

						Close();
                    }



                    if (comboBox_ROP_VarSetup_PDF.Text == "Uniform")        //added by Xin
                    {
                        // Getting the parameter values provided by the user:
                        double lb = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter1.Text);
                        double ub = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter2.Text);


                        // Computing the central moments:
                        ICM = 0.5 * (lb + ub);
                        IICM = (ub - lb) * (ub - lb) / 12;//This is the variance, not the standard deviation!!!!
                        IIICM = 0;
                        IVCM = 1.8;

                        // Storing variable information:
                        (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                        UncertaintyConstantData = new string[8];
                        UncertaintyConstantData[0] = dataobj.Name;
                        UncertaintyConstantData[1] = Convert.ToString(IICM);
                        UncertaintyConstantData[2] = Convert.ToString(IIICM);
                        UncertaintyConstantData[3] = Convert.ToString(IVCM);
                        //UncertaintyConstantData[4] = textBox_ROP_VarSetup_StartVal.Text;
                        //UncertaintyConstantData[5] = Convert.ToString(ROB_LB);
                        //UncertaintyConstantData[6] = Convert.ToString(ROB_UB);
                        //UncertaintyConstantData[7] = textBox_ROP_VarSetup_Prb.Text;

                        options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "Uniform", "Parameter1", lb, "Parameter2", ub);


                        #region Store data for SA   Xin
                        UncertaintyConstantData_SA = new string[8];
                        UncertaintyConstantData_SA[0] = dataobj.Name;
                        UncertaintyConstantData_SA[1] = "Uniform";
                        UncertaintyConstantData_SA[2] = textBox_ROP_ConstSetup_PDFparameter1.Text;    //MEAN
                        UncertaintyConstantData_SA[3] = textBox_ROP_ConstSetup_PDFparameter2.Text;    //d1
                        UncertaintyConstantData_SA[4] = "0";    //d2
                        UncertaintyConstantData_SA[5] = "0";
						#endregion

						Close();
                    }


                    if (comboBox_ROP_VarSetup_PDF.Text == "Rayleigh")        //added by Xin
                    {
                        //you can't do propagation at this moment

                        ICM = 0;
                        IICM = 0;//This is the variance, not the standard deviation!!!!
                        IIICM = 0;
                        IVCM = 0;
                        // Storing variable information:
                        (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                        UncertaintyConstantData = new string[8];
                        UncertaintyConstantData[0] = dataobj.Name;
                        options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "Normal");



                        #region Store data for SA   Xin
                        UncertaintyConstantData_SA = new string[8];
                        UncertaintyConstantData_SA[0] = dataobj.Name;
                        UncertaintyConstantData_SA[1] = "Rayleigh";
                        UncertaintyConstantData_SA[2] = textBox_ROP_ConstSetup_PDFparameter1.Text;    //a
                        UncertaintyConstantData_SA[3] = textBox_ROP_ConstSetup_PDFparameter2.Text;    //b
                        UncertaintyConstantData_SA[4] = "0";    //d2
                        UncertaintyConstantData_SA[5] = "0";
						#endregion

						Close();
                    }

                    if (comboBox_ROP_VarSetup_PDF.Text == "Mixture Gaussian")        //added by Xin
                    {
                        //you can't do propagation at this moment


                        ICM = 0;
                        IICM = 0;//This is the variance, not the standard deviation!!!!
                        IIICM = 0;
                        IVCM = 0;
                        // Storing variable information:
                        (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                        UncertaintyConstantData = new string[8];
                        UncertaintyConstantData[0] = dataobj.Name;
                        options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "Normal");

                        #region Store data for SA   Xin
                        UncertaintyConstantData_SA = new string[8];
                        UncertaintyConstantData_SA[0] = dataobj.Name;
                        UncertaintyConstantData_SA[1] = "Mixture Gaussian";
                        UncertaintyConstantData_SA[2] = textBox_ROP_ConstSetup_PDFparameter1.Text;    //mu1
                        UncertaintyConstantData_SA[3] = textBox_ROP_ConstSetup_PDFparameter2.Text;    //mu2
                        UncertaintyConstantData_SA[4] = textBox_ROP_ConstSetup_PDFparameter3.Text;    //sigma1
                        UncertaintyConstantData_SA[5] = textBox_ROP_ConstSetup_PDFparameter4.Text;    //sigma2
                        UncertaintyConstantData_SA[6] = textBox_ROP_ConstSetup_PDFparameter5.Text;    //p1
                        UncertaintyConstantData_SA[7] = textBox_ROP_ConstSetup_PDFparameter6.Text;    //p2
						#endregion

						Close();

                    }


                    if (comboBox_ROP_VarSetup_PDF.Text == "Beta")
                    {
                        //Temporary:
                        MessageBox.Show("Work in progress...");
                        return;

                        //Beta PDF info: Mean=A/(A+B); Variance=A*B/((A+B)*(A+B)*(A+B+1)); Skewness=2*(B-A)*sqrt(A+B+1)/((A+B+2)*sqrt(A*B)); Kurtosis=6*(A*A*A-A*A*(2*B-1)+B*B*(B+1)-2*A*B*(B+2))/(A*B*(A+B+2)*(A+B+3));
                        double A = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter1.Text);
                        double B = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter2.Text);
                        // Computing the central moments according to the chosen PDF:
                        ICM = A / (A + B);
                        IICM = (A * B) / (Math.Pow(A + B, 2) * (A + B + 1));//This is the variance, not the standard deviation!!!!
                        IIICM = (2 * (B - A) * Math.Sqrt(A + B + 1)) / ((A + B + 2) * Math.Sqrt(A * B));
                        IVCM = 6 * (A * A * A - A * A * (2 * B - 1) + B * B * (B + 1) - 2 * A * B * (B + 2)) / (A * B * (A + B + 2) * (A + B + 3));

                        // Storing variable information:
                        (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                        UncertaintyConstantData = new string[8];
                        UncertaintyConstantData[0] = dataobj.Name;
                        UncertaintyConstantData[1] = Convert.ToString(IICM);
                        UncertaintyConstantData[2] = Convert.ToString(IIICM);
                        UncertaintyConstantData[3] = Convert.ToString(IVCM);
                        //UncertaintyConstantData[4] = textBox_ROP_VarSetup_StartVal.Text;
                        //UncertaintyConstantData[5] = Convert.ToString(ROB_LB);
                        //UncertaintyConstantData[6] = Convert.ToString(ROB_UB);
                        //UncertaintyConstantData[7] = textBox_ROP_VarSetup_Prb.Text;

                        options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "Beta", "Parameter1", A, "Parameter2", B);
						Close();
                    }
                    if (comboBox_ROP_VarSetup_PDF.Text == "Gamma")
                    {
                        //Temporary:
                        MessageBox.Show("Work in progress...");
                        return;

                        //Gamma PDF info: Mean=a*b; Variance=a*b*b; Skewness=2/sqrt(a); Kurtosis=6/a;
                        double a = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter1.Text);
                        double b = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter2.Text);
                        // Computing the central moments according to the chosen PDF:
                        ICM = a * b;
                        IICM = a * b * b;
                        IIICM = 2 / Math.Sqrt(a);
                        IVCM = 6 / a;

                        // Storing variable information:
                        (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                        UncertaintyConstantData = new string[8];
                        UncertaintyConstantData[0] = dataobj.Name;
                        UncertaintyConstantData[1] = Convert.ToString(IICM);
                        UncertaintyConstantData[2] = Convert.ToString(IIICM);
                        UncertaintyConstantData[3] = Convert.ToString(IVCM);
                        //UncertaintyConstantData[4] = textBox_ROP_VarSetup_StartVal.Text;
                        //UncertaintyConstantData[5] = Convert.ToString(ROB_LB);
                        //UncertaintyConstantData[6] = Convert.ToString(ROB_UB);
                        //UncertaintyConstantData[7] = textBox_ROP_VarSetup_Prb.Text;

                        options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "Gamma", "Parameter1", a, "Parameter2", b);
						Close();
                    }
                    if (comboBox_ROP_VarSetup_PDF.Text == "User defined")
                    {
                        if (Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter2.Text) < 0)
                        {
                            MessageBox.Show("The second central moment has to be positive");
                        }
                        else if (Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter4.Text) < 0)
                        {
                            MessageBox.Show("The fourth central moment has to be positive");
                        }
                        else
                        {
                            ICM = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter1.Text);
                            IICM = Math.Pow(Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter2.Text), 2);//The variance, not the standard deviation!!!!
                            IIICM = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter3.Text);
                            IVCM = Convert.ToDouble(textBox_ROP_ConstSetup_PDFparameter4.Text);

                            // Storing variable information:
                            (dataobj as Data).ValueAsString = Convert.ToString(ICM);
                            UncertaintyConstantData = new string[8];
                            UncertaintyConstantData[0] = dataobj.Name;
                            UncertaintyConstantData[1] = Convert.ToString(IICM);
                            UncertaintyConstantData[2] = Convert.ToString(IIICM);
                            UncertaintyConstantData[3] = Convert.ToString(IVCM);
                            //UncertaintyConstantData[4] = textBox_ROP_VarSetup_StartVal.Text;
                            //UncertaintyConstantData[5] = Convert.ToString(ROB_LB);
                            //UncertaintyConstantData[6] = Convert.ToString(ROB_UB);
                            //UncertaintyConstantData[7] = textBox_ROP_VarSetup_Prb.Text;

                            options = new OP_C_Input_set("Constraint", 3, "Value", ICM, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM, "Data", dataobj.Name, "Distribution", "UserDefined");

                            #region Store data for SA   Xin
                            UncertaintyConstantData_SA = new string[8];
                            UncertaintyConstantData_SA[0] = dataobj.Name;
                            UncertaintyConstantData_SA[1] = "Triangular";
                            UncertaintyConstantData_SA[2] = "0";    //MEAN
                            UncertaintyConstantData_SA[3] = "0";    //d1
                            UncertaintyConstantData_SA[4] = "0";    //d2
                            UncertaintyConstantData_SA[5] = "0";
							#endregion


							Close();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Incorrect setup of constants");
            }
        }

        private void radioButton_ROP_CertainConstant_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton_ROP_CertainConstant.Checked == true)
            {
                //Enabling the setup panel area related to constants unaffected by uncertainty:
                label_ROP_constant_Name.Enabled = true;
                label_ROP_constant_equal.Enabled = true;
                textBox_ROP_constant_Value.Enabled = true;

                //Disabling the setup panel area related to constants affected by uncertainty:
                label_ROP_VarSetup_PDF.Enabled = false;
                comboBox_ROP_VarSetup_PDF.Enabled = false;
                label_ROP_VarSetup_PDFparameter1.Enabled = false;
                textBox_ROP_ConstSetup_PDFparameter1.Enabled = false;
                label_ROP_VarSetup_PDFparameter2.Enabled = false;
                textBox_ROP_ConstSetup_PDFparameter2.Enabled = false;
                pictureBox_ROP_VarSetup_PDFimage.Enabled = false;
            }
            else if (radioButton_ROP_UncertainConstant.Checked == true)
            {
                //Disabling the setup panel area related to constants unaffected by uncertainty:
                label_ROP_constant_Name.Enabled = false;
                label_ROP_constant_equal.Enabled = false;
                textBox_ROP_constant_Value.Enabled = false;

                //Enabling the setup panel area related to constants affected by uncertainty:
                label_ROP_VarSetup_PDF.Enabled = true;
                comboBox_ROP_VarSetup_PDF.Enabled = true;
                label_ROP_VarSetup_PDFparameter1.Enabled = true;
                textBox_ROP_ConstSetup_PDFparameter1.Enabled = true;
                label_ROP_VarSetup_PDFparameter2.Enabled = true;
                textBox_ROP_ConstSetup_PDFparameter2.Enabled = true;
                pictureBox_ROP_VarSetup_PDFimage.Enabled = true;
            }
        }

		private void button_ROP_constant_cancel_Click(object sender, EventArgs e) => Close();

		private void comboBox_ROP_VarSetup_PDF_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_ROP_VarSetup_PDF.Text == "Normal")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "mu";
                label_ROP_VarSetup_PDFparameter2.Text = "sigma";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Normal;
                textBox_ROP_ConstSetup_PDFparameter1.Text = Convert.ToString(dataobj.ValueAsDouble);
                textBox_ROP_ConstSetup_PDFparameter2.Text = Convert.ToString(0.05 * dataobj.ValueAsDouble);
                textBox_ROP_ConstSetup_PDFparameter3.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter4.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter4.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter5.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter6.Visible = false;
                label_ROP_VarSetup_PDFparameter5.Visible = false;
                label_ROP_VarSetup_PDFparameter6.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "Triangular")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "mu";
                label_ROP_VarSetup_PDFparameter2.Text = "d1";
                label_ROP_VarSetup_PDFparameter3.Text = "d2";
                label_ROP_VarSetup_PDFparameter3.Visible = true;
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Triangular;
                textBox_ROP_ConstSetup_PDFparameter1.Enabled = true;
                textBox_ROP_ConstSetup_PDFparameter1.ReadOnly = false;
                textBox_ROP_ConstSetup_PDFparameter1.Text = Convert.ToString(dataobj.ValueAsDouble);
                textBox_ROP_ConstSetup_PDFparameter2.Enabled = true;
                textBox_ROP_ConstSetup_PDFparameter2.ReadOnly = false;
                textBox_ROP_ConstSetup_PDFparameter2.Text = "";
                textBox_ROP_ConstSetup_PDFparameter3.Visible = true;
                textBox_ROP_ConstSetup_PDFparameter3.Enabled = true;
                textBox_ROP_ConstSetup_PDFparameter3.ReadOnly = false;
                textBox_ROP_ConstSetup_PDFparameter3.Text = "";
                textBox_ROP_ConstSetup_PDFparameter5.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter6.Visible = false;
                label_ROP_VarSetup_PDFparameter5.Visible = false;
                label_ROP_VarSetup_PDFparameter6.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "Beta")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "alpha";
                label_ROP_VarSetup_PDFparameter2.Text = "beta";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Beta;
                textBox_ROP_ConstSetup_PDFparameter1.Text = "";
                textBox_ROP_ConstSetup_PDFparameter2.Text = "";
                textBox_ROP_ConstSetup_PDFparameter3.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter4.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter4.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "Gamma")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "kappa";
                label_ROP_VarSetup_PDFparameter2.Text = "theta";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Gamma;
                textBox_ROP_ConstSetup_PDFparameter1.Text = "";
                textBox_ROP_ConstSetup_PDFparameter2.Text = "";
                textBox_ROP_ConstSetup_PDFparameter3.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter4.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter4.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "User defined")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "mean";
                label_ROP_VarSetup_PDFparameter2.Text = "IICM";
                label_ROP_VarSetup_PDFparameter3.Text = "IIICM";
                label_ROP_VarSetup_PDFparameter4.Text = "IVCM";
                textBox_ROP_ConstSetup_PDFparameter3.Visible = true;
                textBox_ROP_ConstSetup_PDFparameter4.Visible = true;
                label_ROP_VarSetup_PDFparameter3.Visible = true;
                label_ROP_VarSetup_PDFparameter4.Visible = true;
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_UserDefined;
                textBox_ROP_ConstSetup_PDFparameter1.Text = "";
                textBox_ROP_ConstSetup_PDFparameter2.Text = "";
                textBox_ROP_ConstSetup_PDFparameter5.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter6.Visible = false;
                label_ROP_VarSetup_PDFparameter5.Visible = false;
                label_ROP_VarSetup_PDFparameter6.Visible = false;
            }

            if (comboBox_ROP_VarSetup_PDF.Text == "Uniform")      //add by Xin
            {
                label_ROP_VarSetup_PDFparameter1.Text = "Lower Bound";
                label_ROP_VarSetup_PDFparameter2.Text = "Upper Bound";
                textBox_ROP_ConstSetup_PDFparameter1.Text = Convert.ToString(0.90 * dataobj.ValueAsDouble);
                textBox_ROP_ConstSetup_PDFparameter2.Text = Convert.ToString(1.10 * dataobj.ValueAsDouble);
                textBox_ROP_ConstSetup_PDFparameter3.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter4.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter4.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter5.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter6.Visible = false;
                label_ROP_VarSetup_PDFparameter5.Visible = false;
                label_ROP_VarSetup_PDFparameter6.Visible = false;
            }

            if (comboBox_ROP_VarSetup_PDF.Text == "Rayleigh")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "a";
                label_ROP_VarSetup_PDFparameter2.Text = "b";
                textBox_ROP_ConstSetup_PDFparameter3.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter4.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter4.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter5.Visible = false;
                textBox_ROP_ConstSetup_PDFparameter6.Visible = false;
                label_ROP_VarSetup_PDFparameter5.Visible = false;
                label_ROP_VarSetup_PDFparameter6.Visible = false;
            }

            if (comboBox_ROP_VarSetup_PDF.Text == "Mixture Gaussian")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "mu1";
                label_ROP_VarSetup_PDFparameter2.Text = "mu2";
                label_ROP_VarSetup_PDFparameter3.Text = "sigma1";
                label_ROP_VarSetup_PDFparameter4.Text = "sigma2";
                textBox_ROP_ConstSetup_PDFparameter3.Visible = true;
                textBox_ROP_ConstSetup_PDFparameter4.Visible = true;
                label_ROP_VarSetup_PDFparameter3.Visible = true;
                label_ROP_VarSetup_PDFparameter4.Visible = true;
                textBox_ROP_ConstSetup_PDFparameter5.Visible = true;
                textBox_ROP_ConstSetup_PDFparameter6.Visible = true;
                label_ROP_VarSetup_PDFparameter5.Visible = true;
                label_ROP_VarSetup_PDFparameter6.Visible = true;
            }



        }


    }
}
