/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

using System.Runtime.InteropServices;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

using System.Collections.ObjectModel;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;

namespace Aircadia
{
    [Serializable()]
    public partial class ROP_VariableSetup : Form
    {
		AircadiaProject Project = AircadiaProjectE.Current;

		public ObservableCollection<Data> DataObjects;
        public ObservableCollection<Model> ModelObjects;
        public OP_Input_set options;
        public Data dataObj;
        public string[] UncertaintyVariableData; //This contains:[name,IICM,IIICM,IVCM,StartingPoint]

        public ROP_VariableSetup(Data lio, IEnumerable<Data> dataObjects, IEnumerable<Model> modelObjects)
        { 
            dataObj = lio;
            DataObjects = new ObservableCollection<Data>(dataObjects);
            ModelObjects = new ObservableCollection<Model>(modelObjects);
            InitializeComponent();
            textBox_ROP_VarSetup_MaxVal.Text = Convert.ToString(dataObj.Max);
            textBox_ROP_VarSetup_MinVal.Text = Convert.ToString(dataObj.Min);
            label_ROP_VarSetup_Name.Text = dataObj.Name;
            textBox_ROP_VarSetup_PDFparameter2.Text = Convert.ToString(0.05*dataObj.ValueAsDouble);
        }

        private void comboBox_ROP_VarSetup_PDF_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_ROP_VarSetup_PDF.Text == "Normal")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "mu";
                label_ROP_VarSetup_PDFparameter2.Text = "sigma";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Normal;
                textBox_ROP_VarSetup_PDFparameter1.Enabled = false;
                textBox_ROP_VarSetup_PDFparameter1.ReadOnly = true;
                //textBox_ROP_VarSetup_PDFparameter1.Text = textBox_ROP_VarSetup_StartVal.Text;
                textBox_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_DstrbnAssmptn.Visible = false;
                comboBox_ROP_VarSetup_DstrbnAssmptn.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "Triangular")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "d1";
                label_ROP_VarSetup_PDFparameter2.Text = "d2";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Triangular;
                textBox_ROP_VarSetup_PDFparameter1.Enabled = true;
                textBox_ROP_VarSetup_PDFparameter1.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter1.Text = "";
                textBox_ROP_VarSetup_PDFparameter2.Enabled = true;
                textBox_ROP_VarSetup_PDFparameter2.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter2.Text = "";
                textBox_ROP_VarSetup_PDFparameter3.Visible = false;
                textBox_ROP_VarSetup_PDFparameter3.Enabled = false;
                textBox_ROP_VarSetup_PDFparameter3.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter3.Text = "";
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_DstrbnAssmptn.Visible = false;
                comboBox_ROP_VarSetup_DstrbnAssmptn.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "Beta")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "alpha";
                label_ROP_VarSetup_PDFparameter2.Text = "beta";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Beta;
                textBox_ROP_VarSetup_PDFparameter1.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter1.Text = "";
                textBox_ROP_VarSetup_PDFparameter2.Text = "";
                textBox_ROP_VarSetup_PDFparameter1.Enabled = true;
                textBox_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_DstrbnAssmptn.Visible = false;
                comboBox_ROP_VarSetup_DstrbnAssmptn.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "Gamma")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "kappa";
                label_ROP_VarSetup_PDFparameter2.Text = "theta";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Gamma;
                textBox_ROP_VarSetup_PDFparameter1.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter1.Text = "";
                textBox_ROP_VarSetup_PDFparameter1.Enabled = true;
                textBox_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_PDFparameter3.Visible = false;
                label_ROP_VarSetup_DstrbnAssmptn.Visible = false;
                comboBox_ROP_VarSetup_DstrbnAssmptn.Visible = false;
            }
            if (comboBox_ROP_VarSetup_PDF.Text == "User defined")
            {
                label_ROP_VarSetup_PDFparameter1.Text = "IICM";
                label_ROP_VarSetup_PDFparameter2.Text = "IIICM";
                label_ROP_VarSetup_PDFparameter3.Text = "IVCM";
                pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_UserDefined;
                textBox_ROP_VarSetup_PDFparameter1.Enabled = true;
                textBox_ROP_VarSetup_PDFparameter1.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter2.Enabled = true;
                textBox_ROP_VarSetup_PDFparameter2.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter3.Visible = true;
                textBox_ROP_VarSetup_PDFparameter2.Text = "";
                textBox_ROP_VarSetup_PDFparameter3.Enabled = true;
                textBox_ROP_VarSetup_PDFparameter3.ReadOnly = false;
                textBox_ROP_VarSetup_PDFparameter3.Text = "";
                label_ROP_VarSetup_PDFparameter3.Visible = true;
                label_ROP_VarSetup_DstrbnAssmptn.Visible = true;
                comboBox_ROP_VarSetup_DstrbnAssmptn.Visible = true;
                
            }
        }

        public double X_k_approximation_Normal_PDF(double Prb)
        {
            double a = 8 * (Math.PI - 3) / (3 * Math.PI * (4 - Math.PI));
            double x = 2 * Prb - 1;
            double erf_inv_apprx = x / Math.Abs(x) * Math.Sqrt((Math.Sqrt(Math.Pow(2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2, 2) - Math.Log(1 - Math.Pow(x, 2)) / a)) - (2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2));
            double k_apprx = Math.Sqrt(2) * erf_inv_apprx;

            return k_apprx;
        }

        public double X_k_approximation_Triangular_PDF(double Prb)
        {
            /*double a = 8 * (Math.PI - 3) / (3 * Math.PI * (4 - Math.PI));
            double x = 2 * Prb - 1;
            double erf_inv_apprx = x / Math.Abs(x) * Math.Sqrt((Math.Sqrt(Math.Pow(2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2, 2) - Math.Log(1 - Math.Pow(x, 2)) / a)) - (2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2));
            double k_apprx = Math.Sqrt(2) * erf_inv_apprx;*/

            double k_apprx = 8;
            return k_apprx;
        }

        private void button_ROP_VarSetup_Set_Click(object sender, EventArgs e)
        {
            // Validity check of data:
            double MissingData = 0;
            if (textBox_ROP_VarSetup_Prb.Text == "")
            {
                MessageBox.Show("Please enter satisfaction probability");
                MissingData = 1;
            }
            else if (textBox_ROP_VarSetup_MinVal.Text == "")
            {
                MessageBox.Show("Please enter lower bound");
                MissingData = 1;
            }
            else if (textBox_ROP_VarSetup_MaxVal.Text == "")
            {
                MessageBox.Show("Please enter upper bound");
                MissingData = 1;
            }
            else if (textBox_ROP_VarSetup_PDFparameter1.Text == "")
            {
                if (comboBox_ROP_VarSetup_PDF.Text != "Normal")
                {
                    MessageBox.Show("Please enter first PDF parameter");
                    MissingData = 1;
                }
            }
            else if (textBox_ROP_VarSetup_PDFparameter2.Text == "")
            {
                MessageBox.Show("Please enter second PDF parameter");
                MissingData = 1;
            }
            else if (textBox_ROP_VarSetup_PDFparameter3.Text == "")
            {
                if (comboBox_ROP_VarSetup_PDF.Text == "User defined")
                {
                    MessageBox.Show("Please enter the third PDF parameter");
                    MissingData = 1;
                }
            }

            if (MissingData==0)
            {
                if (comboBox_ROP_VarSetup_PDF.Text == "Normal")
                {
                    textBox_ROP_VarSetup_PDFparameter1.Text = "";

                    // Computing the central moments according to the chosen PDF:
                    double IICM = Math.Pow(Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter2.Text),2); //The variance, not the standard deviation!!!!
                    double IIICM = 0;
                    double IVCM = 3;
                    // Computing the coefficient k:
                    double x_k = X_k_approximation_Normal_PDF(Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text));
                    // Defining the robust lower variable bound:
                    double ROB_LB = Convert.ToDouble(textBox_ROP_VarSetup_MinVal.Text) + Math.Sqrt(IICM) * x_k;
                    // Defining the robust lower variable bound:
                    double ROB_UB = Convert.ToDouble(textBox_ROP_VarSetup_MaxVal.Text) - Math.Sqrt(IICM) * x_k;
                    // Storing variable information:
                    UncertaintyVariableData = new string[8];
                    UncertaintyVariableData[0] = dataObj.Name;
                    UncertaintyVariableData[1] = Convert.ToString(IICM);
                    UncertaintyVariableData[2] = Convert.ToString(IIICM);
                    UncertaintyVariableData[3] = Convert.ToString(IVCM);
                    //UncertaintyVariableData[4] = textBox_ROP_VarSetup_StartVal.Text;
                    UncertaintyVariableData[5] = Convert.ToString(ROB_LB);
                    UncertaintyVariableData[6] = Convert.ToString(ROB_UB);
                    UncertaintyVariableData[7] = textBox_ROP_VarSetup_Prb.Text;

                    if (ROB_LB >= ROB_UB)
                    {
                        MessageBox.Show("Lower bound larger than upper bound. Please revise the provided information.");
                    }
                    else
                    {
                        var des_set = new OP_Input_set("min", ROB_LB, "max", ROB_UB, "desobj", "des", "Data", dataObj.Name, "Probability", Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text), "Distribution", comboBox_ROP_VarSetup_PDF.Text, "IICM", IICM);
                        options = des_set;
						Close();
                    }                   
                }
                if (comboBox_ROP_VarSetup_PDF.Text == "Triangular")
                {
                    // Getting the parameter values provided by the user:
                    double d1 = Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter1.Text);
                    double d2 = Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter2.Text);
                    double d3 = d1 - d2;

                    // Computing the central moments:
                    double IICM = ((d1 + d2) * (d1 + d2) * (1 - (d1 + d3) * (d2 - d3) / ((d1 + d2) * (d1 + d2)))) / 18;
                    double IIICM = Math.Sqrt(2) / 5 * (d2 - d1 - 2 * d3) * (-2 * d1 - d2 - d3) * (-d1 - 2 * d2 + d3) / Math.Pow(((d1 + d2) * (d1 + d2) * (1 - (d1 + d3) * (d2 - d3) / ((d1 + d2) * (d1 + d2)))),(3.0 / 2.0));
                    double IVCM = 12.0 / 5;
                    double LB = Convert.ToDouble(textBox_ROP_VarSetup_MinVal.Text);// Lower variable bound (deterministic)
                    double UB = Convert.ToDouble(textBox_ROP_VarSetup_MaxVal.Text);// Upper variable bound (deterministic)
                    
                    // Computing the robust variable bounds distance from the deterministic ones:
                    double Prb = Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text);
                    Cursor.Current = Cursors.WaitCursor;
                    double x_k_minus = StandardTriangularPDF_inv(1 - Prb, d1, d2);
                    double x_k_plus = StandardTriangularPDF_inv(Prb, d1, d2);
                    Cursor.Current = Cursors.Default;

                    // Defining the robust lower variable bound:
                    double ROB_LB = LB + (d1 - x_k_minus * (d1 + d2));
                    // Defining the robust upper variable bound:
                    double ROB_UB = UB - (x_k_plus * (d1 + d2) - d1);
                    // Storing variable information:
                    UncertaintyVariableData = new string[8];
                    UncertaintyVariableData[0] = dataObj.Name;
                    UncertaintyVariableData[1] = Convert.ToString(IICM);
                    UncertaintyVariableData[2] = Convert.ToString(IIICM);
                    UncertaintyVariableData[3] = Convert.ToString(IVCM);
                    //UncertaintyVariableData[4] = textBox_ROP_VarSetup_StartVal.Text;
                    UncertaintyVariableData[5] = Convert.ToString(ROB_LB);
                    UncertaintyVariableData[6] = Convert.ToString(ROB_UB);
                    UncertaintyVariableData[7] = textBox_ROP_VarSetup_Prb.Text;

                    if (ROB_LB >= ROB_UB)
                    {
                        MessageBox.Show("Lower bound larger than upper bound. Please revise the provided information.");
                    }
                    else
                    {
                        var des_set = new OP_Input_set("min", ROB_LB, "max", ROB_UB, "desobj", "des", "Data", dataObj.Name, "Probability", Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text), "Distribution", comboBox_ROP_VarSetup_PDF.Text, "Parameter1", d1, "Parameter2", d2);
                        options = des_set;
						Close();
                    }
                }
                if (comboBox_ROP_VarSetup_PDF.Text == "Beta")
                {
                    //Temporary:
                    MessageBox.Show("Work in progress...");
                    return;

                    double A = Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter1.Text);
                    double B = Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter2.Text);
                    // Computing the central moments according to the chosen PDF:
                    double IICM = (A+B)/(Math.Pow(A+B,2)*(A+B+1)); //Is it the variance or standard deviation? Check!!!
                    double IIICM = (2*(B-A)*Math.Sqrt(A+B+1))/((A+B+2)*Math.Sqrt(A*B));
                    double IVCM = 3*(((Math.Pow(A,2)*(B+2)+2*Math.Pow(B,2)+A*B*(B-2))*(A+B+1))/(A*B*(A+B+2)*(A+B+3))-1);
                    double LB = Convert.ToDouble(textBox_ROP_VarSetup_MinVal.Text);// Lower variable bound (deterministic)
                    double UB = Convert.ToDouble(textBox_ROP_VarSetup_MaxVal.Text);// Upper variable bound (deterministic)
                    
                    // Computing the robust variable bounds distance from the deterministic ones:
                    double Prb = Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text);
                    Cursor.Current = Cursors.WaitCursor;
                    double x_k_minus = BetaCDF_inv(1-Prb, A, B);
                    double x_k_plus = BetaCDF_inv(Prb, A, B);
                    Cursor.Current = Cursors.Default;
                    
                    // Defining the robust lower variable bound:
                    double ROB_LB = LB + x_k_minus * (UB - LB);
                    // Defining the robust upper variable bound:
                    double ROB_UB = LB + x_k_plus * (UB - LB);
                    // Storing variable information:
                    UncertaintyVariableData = new string[8];
                    UncertaintyVariableData[0] = dataObj.Name;
                    UncertaintyVariableData[1] = Convert.ToString(IICM);
                    UncertaintyVariableData[2] = Convert.ToString(IIICM);
                    UncertaintyVariableData[3] = Convert.ToString(IVCM);
                    //UncertaintyVariableData[4] = textBox_ROP_VarSetup_StartVal.Text;
                    UncertaintyVariableData[5] = Convert.ToString(ROB_LB);
                    UncertaintyVariableData[6] = Convert.ToString(ROB_UB);
                    UncertaintyVariableData[7] = textBox_ROP_VarSetup_Prb.Text;

                    if (ROB_LB >= ROB_UB)
                    {
                        MessageBox.Show("Lower bound larger than upper bound. Please revise the provided information.");
                    }
                    else
                    {
                        var des_set = new OP_Input_set("min", ROB_LB, "max", ROB_UB, "desobj", "des", "Data", dataObj.Name, "Probability", Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text), "Distribution", comboBox_ROP_VarSetup_PDF.Text, "Parameter1", A, "Parameter2", B);
                        options = des_set;
						Close();
                    }  
                }
                if (comboBox_ROP_VarSetup_PDF.Text == "Gamma")
                {
                    MessageBox.Show("Work in progress...");
                    return;
                }
                if (comboBox_ROP_VarSetup_PDF.Text == "User defined")
                {
                    // Storing the central moments according to the chosen PDF:
                    double IICM = Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter1.Text);//The variance, not the standard deviation!!!!
                    double IIICM = Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter2.Text);
                    double IVCM = Convert.ToDouble(textBox_ROP_VarSetup_PDFparameter3.Text);
                    
                    // Checking the second and fourth moments provided are positive:
                    if (IICM < 0)
                    {
                        MessageBox.Show("The second central moment has to be positive");
                    }
                    else if (IVCM < 0)
                    {
                        MessageBox.Show("The fourth central moment has to be positive");
                    }
                    else
                    {

                        // Computing the coefficient k:
                        double Prb = Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text);
                        string DistrAssumpt = comboBox_ROP_VarSetup_DstrbnAssmptn.Text;
                        double x_k = K_calculation(Prb, DistrAssumpt);

                        // Defining the robust lower variable bound:
                        double ROB_LB = Convert.ToDouble(textBox_ROP_VarSetup_MinVal.Text) + Math.Sqrt(IICM) * x_k;
                        // Defining the robust lower variable bound:
                        double ROB_UB = Convert.ToDouble(textBox_ROP_VarSetup_MaxVal.Text) - Math.Sqrt(IICM) * x_k;

                        // Storing variable information:
                        UncertaintyVariableData = new string[8];
                        UncertaintyVariableData[0] = dataObj.Name;
                        UncertaintyVariableData[1] = Convert.ToString(IICM);
                        UncertaintyVariableData[2] = Convert.ToString(IIICM);
                        UncertaintyVariableData[3] = Convert.ToString(IVCM);
                        //UncertaintyVariableData[4] = textBox_ROP_VarSetup_StartVal.Text;
                        UncertaintyVariableData[5] = Convert.ToString(ROB_LB);
                        UncertaintyVariableData[6] = Convert.ToString(ROB_UB);
                        UncertaintyVariableData[7] = textBox_ROP_VarSetup_Prb.Text;

                        if (ROB_LB >= ROB_UB)
                        {
                            MessageBox.Show("Lower bound larger than upper bound. Please revise the provided information.");
                        }
                        else
                        {
                            var des_set = new OP_Input_set("min", ROB_LB, "max", ROB_UB, "desobj", "des", "Data", dataObj.Name, "Probability", Convert.ToDouble(textBox_ROP_VarSetup_Prb.Text), "Distribution", comboBox_ROP_VarSetup_PDF.Text, "IICM", IICM, "IIICM", IIICM, "IVCM", IVCM);
                            options = des_set;
							Close();
                        }
                    }
                }
            }
        }

        private void button_ROP_VarSetup_CMsetup_Click(object sender, EventArgs e)
        {
            var CM_Setup_Window = new ROP_Central_Moments_Def(dataObj);
            CM_Setup_Window.ShowDialog();
        }

		private void button_ROP_VarSetup_Cancel_Click(object sender, EventArgs e) => Close();

		public double K_calculation(double Prb, string Assumption)
        {
            double k = new double();
            if (Assumption == "None")
            {
                k = Math.Sqrt(Prb / (1 - Prb));
            }
            else if (Assumption == "Normality")
            {
                k = K_approximation_Normal_PDF(Prb);
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

        public double K_approximation_Normal_PDF(double Prb)
        {
            double a = 8 * (Math.PI - 3) / (3 * Math.PI * (4 - Math.PI));
            double x = 2 * Prb - 1;
            double erf_inv_apprx = x / Math.Abs(x) * Math.Sqrt((Math.Sqrt(Math.Pow(2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2, 2) - Math.Log(1 - Math.Pow(x, 2)) / a)) - (2 / (Math.PI * a) + Math.Log(1 - Math.Pow(x, 2)) / 2));
            double k_apprx = Math.Sqrt(2) * erf_inv_apprx;

            return k_apprx;
        }

        /*
        private string mCreateModelCSDLL_ForNormalModel(string lcCode, string lcCodeIn, cWfmCollection outDataList, string DLLname)
        {
            //Creates the DLLS for model

            lcCode += "object[] outputs_all=new object[" + Convert.ToString(outDataList.Count) + "];";
            int ncount = 0;
            foreach (Data dt in outDataList)
            {
                lcCode += "outputs_all[" + Convert.ToString(ncount) + "]=" + dt.Name + ";\n";
                ncount++;
                //lcCode = lcCode +"return " + dt.name + ";";
            }
            lcCode += "object outputs_return=outputs_all;";
            lcCode = lcCode + "return outputs_return;";
            // *** Must create a fully functional assembly
            lcCode = "public object " + DLLname + @"(" + lcCodeIn + @") {
" + lcCode +
"}  ";
            return lcCode;
        }
        */
        private bool Compiler(string lcCode)
        {
            ICodeCompiler loCompiler = new CSharpCodeProvider().CreateCompiler();
            var loParameters = new CompilerParameters();
            // *** Start by adding any referenced assemblies
            loParameters.ReferencedAssemblies.Add("System.dll");
            loParameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            // *** Load the resulting assembly into memory
            loParameters.GenerateInMemory = true;
            loParameters.OutputAssembly = "BetaPDFderFunc" + ".dll";
            // *** Now compile the whole thing
            CompilerResults loCompiled = loCompiler.CompileAssemblyFromSource(loParameters, lcCode);
            if (loCompiled.Errors.HasErrors)
            {
                string lcErrorMsg = "";

                // *** Create Error String
                lcErrorMsg = loCompiled.Errors.Count.ToString() + " Errors:";
                for (int x = 0; x < loCompiled.Errors.Count; x++)
                    lcErrorMsg = lcErrorMsg + "\r\nLine: " + loCompiled.Errors[x].Line.ToString() + " - " +
                        loCompiled.Errors[x].ErrorText;

                var li = new ErrorShow(lcErrorMsg + "\r\n\r\n" + lcCode);
                li.Show();
                MessageBox.Show(lcErrorMsg + "\r\n\r\n" + lcCode, "Compiler Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return false;
            }
            else
            {
                string dllfile = Directory.GetCurrentDirectory() + "\\" + Project.ProjectName + ".dll";
                if (!File.Exists(dllfile))
                {
                    MessageBox.Show("dll not created");
                    return false;
                }
                byte[] bytes = File.ReadAllBytes(dllfile);
				Project.assemblyacd = Assembly.Load(bytes);
            }
            return true;
        }

        public double BetaPDFfunc2(double t, double alpha_PDF, double beta_PDF)
        {
            var codeProvider = CodeDomProvider.CreateProvider("CSharp"); //new CSharpCodeProvider().CreateCompiler();  
            var cp = new CompilerParameters();
            string ExecPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            //compilerParameters.ReferencedAssemblies.Add(ExecPath + "\\comp_proc_manage.exe");
            cp.ReferencedAssemblies.Add(ExecPath + "\\Aircadia.exe");
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            cp.OutputAssembly = "BetaFuncDer.dll";
            var code = new StringBuilder();
            code.Append("using System; \n");
            code.Append("namespace betaPDF { \n");
            code.Append("  public class BetaFuncDerivative { \n");
            code.Append("    public double avaliar(double t, double alpha, double beta)\n");
            code.Append("{ ");
            code.AppendFormat("      double DerivativeValue = Math.Pow(t,alpha-1)*Math.Pow(1-t,beta-1);\n");
            code.AppendFormat("      return DerivativeValue;\n");
            code.Append("}\n");
            code.Append("}\n }");
            //Console.WriteLine(code.ToString());
            CompilerResults resultado = codeProvider.CompileAssemblyFromSource(cp, code.ToString());
            System.Reflection.Assembly a = resultado.CompiledAssembly;
            object _Compiled = a.CreateInstance("betaPDF.BetaFuncDerivative");
            System.Reflection.MethodInfo mi = _Compiled.GetType().GetMethod("avaliar");
            object[] inputs_value = new object[3];
            double x_PDF = t;
            //double alpha_PDF = 2;
            //double beta_PDF = 3;
            inputs_value[0] = x_PDF;
            inputs_value[1] = alpha_PDF;
            inputs_value[2] = beta_PDF;
            object res = mi.Invoke(_Compiled, inputs_value);
            //Console.WriteLine(res);

            var numInteglist = new ArrayList();
            string[] setIntVars = new string[6];
            setIntVars[0] = "beta_der";
            setIntVars[1] = "x";
            setIntVars[2] = "0";
            setIntVars[3] = "1";
            setIntVars[4] = "0";
            setIntVars[5] = "0.001";
            numInteglist.Add(setIntVars);

            //cTreatment_InOut oOpInput = new cTreatment_InOut(numInteglist);
            //cTreatment_InOut oOpOutput = new cTreatment_InOut_OP_Output("Study not executed yet");
            //cTreatment_NInt cOPStudy = new cTreatment_NInt("Integrator", oOpInput, oOpOutput);
            //status = true;
            //status = cOPStudy.Execute(modsub);
            //cStudy stu = new cStudy(Beta_Derivative_Func_Integration, cOPStudy as cTreatment, Beta_Drvtv_Fnctn as cModelSubpro);

            //status = (stu as cMdSpSt).Execute();
            //double beta_func_value = stu.treatment.output_struct.output;*/

            //ArrayList numInteglist = input_options.setuplist;
            string output = "Xvalues Initial_F Integrated_F" + "\r\n";
            //Simple numerical integration using RungeKutta 4th order
            string inpn = ((numInteglist[0] as string[])[1] as string);
            string opnm = ((numInteglist[0] as string[])[0] as string);
            double x0 = Convert.ToDouble((numInteglist[0] as string[])[2] as string);
            double xn = Convert.ToDouble((numInteglist[0] as string[])[3] as string);
            double hh = Convert.ToDouble((numInteglist[0] as string[])[5] as string);
            double y0 = Convert.ToDouble((numInteglist[0] as string[])[4] as string);
            int nsteps = Convert.ToInt32((xn - x0) / hh);
            double k1; double k2; double k3; double k4;
            double[] ei = new double[nsteps];
            double[] xi = new double[nsteps];
            double[] yi = new double[nsteps];
            xi[0] = x0; yi[0] = y0;
            inputs_value[0] = xi[0];
            res = mi.Invoke(_Compiled, inputs_value);
            ei[0] = Convert.ToDouble(res.ToString());

            //if (File.Exists("integration_results.dat"))
            //  File.Delete("integration_results.dat");
            //FileStream filer = new FileStream("integration_results.dat", FileMode.OpenOrCreate, FileAccess.Write);
            //StreamWriter sw = new StreamWriter(filer);
            //sw.WriteLine("X values Y values");
            output = output + (Convert.ToString(xi[0]) + " " + Convert.ToString(ei[0]) + " " + Convert.ToString(yi[0])) + "\r\n";
            //sw.Write("\r\n");
            //sw.WriteLine(Convert.ToString(xi[0]) + " " + Convert.ToString(yi[0]));
            for (int ii = 1; ii < nsteps; ii++)
            {
                inputs_value[0] = xi[ii - 1];
                res = mi.Invoke(_Compiled, inputs_value);
                k1 = Convert.ToDouble(res.ToString());
                ei[ii] = k1;

                inputs_value[0] = (xi[ii - 1] + (hh / 2));
                res = mi.Invoke(_Compiled, inputs_value);
                k2 = Convert.ToDouble(res.ToString());

                inputs_value[0] = (xi[ii - 1] + (hh / 2));
                res = mi.Invoke(_Compiled, inputs_value);
                k3 = Convert.ToDouble(res.ToString());

                inputs_value[0] = (xi[ii - 1] + hh);
                res = mi.Invoke(_Compiled, inputs_value);

                k4 = Convert.ToDouble(res.ToString());

                yi[ii] = yi[ii - 1] + ((hh / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
                xi[ii] = xi[ii - 1] + hh;
                output = output + Convert.ToString(xi[ii]) + " " + Convert.ToString(ei[ii]) + " " + Convert.ToString(yi[ii]) + "\r\n";
            }

            double BetaFuncVal = yi[999];

            double betaPDFvalue = 1 / BetaFuncVal * Math.Pow(x_PDF, (alpha_PDF - 1)) * Math.Pow(1 - x_PDF, (beta_PDF - 1));

            return betaPDFvalue;
        }

        public double BetaFuncDerivative(double t, double alpha, double beta)
        {
            double DerivativeValue = Math.Pow(t, alpha - 1) * Math.Pow(1 - t, beta - 1);
            return DerivativeValue;
        }

        public double BetaPDFfunc(double t, double alpha_PDF, double beta_PDF)
        {
            //Simple numerical integration using RungeKutta 4th order
            double x0 = 0;
            double xn = 1;
            double hh = 0.001;
            double y0 = 0;
            int nsteps = Convert.ToInt32((xn - x0) / hh);
            double k1; double k2; double k3; double k4;
            double[] xi = new double[nsteps];
            double[] yi = new double[nsteps];
            xi[0] = x0; yi[0] = y0;
            double result = BetaFuncDerivative(xi[0], alpha_PDF, beta_PDF);
            for (int ii = 1; ii < nsteps; ii++)
            {
                k1 = BetaFuncDerivative(xi[ii - 1], alpha_PDF, beta_PDF);
                k2 = BetaFuncDerivative((xi[ii - 1] + (hh / 2)), alpha_PDF, beta_PDF);
                k3 = BetaFuncDerivative((xi[ii - 1] + (hh / 2)), alpha_PDF, beta_PDF);
                k4 = BetaFuncDerivative((xi[ii - 1] + hh), alpha_PDF, beta_PDF);
                
                yi[ii] = yi[ii - 1] + ((hh / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
                xi[ii] = xi[ii - 1] + hh;
            }

            double BetaFuncVal = yi[999];

            double betaPDFvalue = 1 / BetaFuncVal * Math.Pow(t, (alpha_PDF - 1)) * Math.Pow(1 - t, (beta_PDF - 1));

            return betaPDFvalue;
        }

        public double BetaCDF(double t, double alpha_PDF, double beta_PDF)
        {
            double x0 = 0;
            double xn = t;
            double hh = 0.005;
            double y0 = 0;
            int nsteps = Convert.ToInt32((xn - x0) / hh);
            if (nsteps == 0)
                return 0;
            double k1; double k2; double k3; double k4;
            double[] xi = new double[nsteps];
            double[] yi = new double[nsteps];
            xi[0] = x0; yi[0] = y0;
            double t_int = xi[0];
            for (int ii = 1; ii < nsteps; ii++)
            {
                t_int = xi[ii - 1];
                k1 = BetaPDFfunc(t_int, alpha_PDF, beta_PDF);
                
                t_int = (xi[ii - 1] + (hh / 2));
                k2 = BetaPDFfunc(t_int, alpha_PDF, beta_PDF);
                                
                t_int = (xi[ii - 1] + (hh / 2));
                k3 = BetaPDFfunc(t_int, alpha_PDF, beta_PDF);
                
                t_int = (xi[ii - 1] + hh);
                k4 = BetaPDFfunc(t_int, alpha_PDF, beta_PDF);
                
                yi[ii] = yi[ii - 1] + ((hh / 6) * (k1 + 2 * k2 + 2 * k3 + k4));
                xi[ii] = xi[ii - 1] + hh;
            }

            double BetaFuncCumulativeVal = yi[nsteps-1];

            return BetaFuncCumulativeVal;
        }

        public double BetaCDF_inv(double Prb, double alpha_PDF, double beta_PDF)
        {
            double tol = 1e-2;
            double num_iter=0;
            double k = alpha_PDF/(alpha_PDF+beta_PDF); //Initial value given by the mean
            double f = BetaPDFfunc(k, alpha_PDF, beta_PDF);
            double g = BetaCDF(k, alpha_PDF, beta_PDF) - Prb;
           
            while (Math.Abs(g)>tol)
            {
                k = k - g/f;
                if (k < 0)
                    k = 0;
                else if (k > 1)
                    k = 1;
                f = BetaPDFfunc(k, alpha_PDF, beta_PDF);
                g = BetaCDF(k, alpha_PDF, beta_PDF) - Prb;
                num_iter=num_iter+1;
            }
           
            return k;
        }

        public double StandardTriangularPDF_inv(double Prb, double d1, double d2)
        {
            double a = 0;
            double b = 1;
            double m = (2 * d1 - d2) / (d1 + d2);

            double k = 0;
            if ((Prb <= (m-a)/(b-a))  &&  (Prb >= 0))
            {
                k = a + Math.Sqrt(Prb*(m-a)*(b-a));
            }
            else if ((Prb > (m-a)/(b-a))  &&  (Prb <= 1))
            {
                k = b - Math.Sqrt((1-Prb)*(b-m)*(b-a));
            }
            return k;
        }
    }
}
