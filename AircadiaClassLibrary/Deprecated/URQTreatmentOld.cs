﻿using System;
using System.Collections.Generic;
using System.Linq;





using System.Collections;
using System.IO;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;


using System.Data;
using System.Data.SqlServerCe;


using Aircadia.ObjectModel.Studies;



using System.Xml.Linq;
using Aircadia.ObjectModel.Workflows;

namespace Aircadia.ObjectModel.Treatments
{
	[Serializable()]
    public class URQTreatmentOld : Treatment
    {
        public WorkflowComponent workflow;

        List<string> names;
        List<List<decimal>> meanValues;


        public double[] Outvar_mean;
        public double[] Outvar_var;

        public string[,] RobustData;



        public RobOptTemplate robOptTemplate;

        public int Counter = 1;


        public List<Data> uncertainParameters;
        public List<Data> factors; // List of factors
        public List<int> noOfLevels; // List of corresponding number of levels for factors
        public List<decimal> startingValues;
        public List<decimal> stepSizes;
        public List<Data> responses; // List of responses

        public URQTreatmentOld(string n, Treatment_InOut input_opt, Treatment_InOut outp, List<string> names, List<List<decimal>> meanValues, List<Data> factors, List<Data> uncertainParameters, List<decimal> startingValues, List<decimal> stepSizes, List<int> noOfLevels, List<Data> responses, WorkflowComponent workflow) :
             base(n, n)
        {
            Name = n;
            input_options = input_opt;
            output_struct = outp;


            this.names = names;
            this.meanValues = meanValues;

            this.uncertainParameters = uncertainParameters;

            this.factors = factors;
            this.startingValues = startingValues;
            this.stepSizes = stepSizes;
            this.noOfLevels = noOfLevels;
            this.responses = responses;




            var DesignVariables = new List<RobOptDesignVariable>();
            for (int i = 0; i < ((string[,])(input_options.setuplist[0])).GetLength(0); i++)
            {
                DesignVariables.Add(new RobOptDesignVariable() { Name = ((string[,])(input_options.setuplist[0]))[i, 0] });
            }
            var Objectives = new List<RobOptObjective>();
            for (int i = 0; i < ((string[,])(input_options.setuplist[1])).GetLength(0); i++)
            {
                Objectives.Add(new RobOptObjective() { Name = ((string[,])(input_options.setuplist[1]))[i, 0] });
            }
            var Constraints = new List<RobOptConstraint>();
            for (int i = 0; i < ((string[,])(input_options.setuplist[2])).GetLength(0); i++)
            {
                Constraints.Add(new RobOptConstraint() { Name = ((string[,])(input_options.setuplist[2]))[i, 0] });
            }
            robOptTemplate = new RobOptTemplate() { DesignVariables = DesignVariables, Objectives = Objectives, Constraints = Constraints };




            this.workflow = workflow;
            
        }

		public override bool ApplyOn() => true;
		public override bool ApplyOn(ExecutableComponent ec)
        {
            if (ec is WorkflowComponent)
            {
				workflow = (Workflow)ec;
            }
            Counter = 1;

            for (int atif = 0; atif < meanValues.Count; atif++)
            {

                #region Assign Mean Values
                for (int cnt = 0; cnt < names.Count; cnt++)
                {
                    foreach (Data dt in ec.ModelDataInputs)
                    {
                        if (dt.Name == names[cnt])
                        {
                            if (dt is DoubleData)
                                dt.ValueAsDouble = (double)meanValues[atif][cnt];
                            break;
                        }
                    }
                }
                #endregion Assign Mean Values



                string[,] ROB_vars = (input_options as Treatment_InOut_OP_Input).setuplist[0] as string[,];//This contains:[name,IICM,IIICM,IVCM,StartingPoint,LB,UB,Prb]
                string[,] ROB_objs = (input_options as Treatment_InOut_OP_Input).setuplist[1] as string[,];//This contains:[name,k_obj,intent,Prb,Assumtpion,Metric,Sign in Loss Function]
                string[,] ROB_cnstrs = (input_options as Treatment_InOut_OP_Input).setuplist[2] as string[,];//This contains:[name,k_cnstr,intent,limit]
                string[,] ROB_consts = (input_options as Treatment_InOut_OP_Input).setuplist[3] as string[,];//This contains:[name,value]

                // Creating manageable arrays where to store the variables data required for the uncertainty propagation:
                int xDim = ROB_vars.GetLength(0); //Number of variables
                int constDim = ROB_consts.GetLength(0); //Number of constants
                // Identifying how many constants are affected by uncertainty (here they will be treated as variables):
                for (int i = 0; i < constDim; i++)
                {
                    if (ROB_consts[i, 2] != null)
                    {
                        xDim = xDim + 1;
                    }
                }
                string[] VarsNames = new string[xDim]; // Variables name
                double[] Sigma = new double[xDim]; // Variables mean
                double[] Skewn = new double[xDim]; // Variables skewness
                double[] Kurt = new double[xDim]; // Variables kurtosis
                double[] unk = new double[xDim]; // Variables value
                int xDim_real = ROB_vars.GetLength(0); //Number of real design variables (without considering the incorporation of uncertain constants)
                for (int i = 0; i < xDim_real; i++)
                {
                    VarsNames[i] = ROB_vars[i, 0];
                    //Sigma[i] = Convert.ToDouble(ROB_vars[i, 1]);
                    Sigma[i] = Math.Sqrt(Convert.ToDouble(ROB_vars[i, 1]));
                    Skewn[i] = Convert.ToDouble(ROB_vars[i, 2]);
                    Kurt[i] = Convert.ToDouble(ROB_vars[i, 3]);
                    unk[i] = Convert.ToDouble(ROB_vars[i, 4]);
                }
                // Correcting the initialisation value of the design parameters formulated as constants:
                string[] NewConstNames = new string[constDim];
                double[] NewConstValues = new double[constDim];
                int VarsIndex = ROB_vars.GetLength(0) - 1; //Identifier of the row of the "variables" array to be filled in with the uncertain constant at hand
                for (int i = 0; i < constDim; i++)
                {
                    if (ROB_consts[i, 2] == null)
                    {
                        NewConstNames[i] = ROB_consts[i, 0];
                        NewConstValues[i] = Convert.ToDouble(ROB_consts[i, 1]);
                    }
                    else
                    {
                        VarsIndex = VarsIndex + 1;
                        VarsNames[VarsIndex] = ROB_consts[i, 0];
                        //Sigma[VarsIndex] = Convert.ToDouble(ROB_consts[i, 2]);
                        Sigma[VarsIndex] = Math.Sqrt(Convert.ToDouble(ROB_consts[i, 2]));
                        Skewn[VarsIndex] = Convert.ToDouble(ROB_consts[i, 3]);
                        Kurt[VarsIndex] = Convert.ToDouble(ROB_consts[i, 4]);
                        unk[VarsIndex] = Convert.ToDouble(ROB_consts[i, 1]);
                    }
                }

                // Defining the "deltas" for the computation of the propagation stencils:
                double[] h_plus = new double[xDim];
                double[] h_minus = new double[xDim];
                for (int j = 0; j < xDim; j++)
                    h_plus[j] = Skewn[j] / 2 + Math.Sqrt(Kurt[j] - (3.0 / 4) * Math.Pow(Skewn[j], 2));
                for (int j = 0; j < xDim; j++)
                    h_minus[j] = Skewn[j] / 2 - Math.Sqrt(Kurt[j] - (3.0 / 4) * Math.Pow(Skewn[j], 2));

                // Setup of the URQ weights:
                double[] W_nSP = new double[2 * xDim + 1];
                double[] Wp_pl = new double[xDim];
                double[] Wp_mi = new double[xDim];
                double[] Wp_pm = new double[xDim];
                for (int i = 0; i < xDim; i++)
                {
                    int[] indexW_nSP = new int[2];
                    indexW_nSP[0] = 1 + i;
                    indexW_nSP[1] = 1 + i + xDim;
                    W_nSP[indexW_nSP[0]] = 1.0 / (h_plus[i] - h_minus[i]);
                    W_nSP[indexW_nSP[1]] = 1.0 / (h_plus[i] - h_minus[i]);
                    Wp_pl[i] = (Math.Pow(h_plus[i], 2) - h_plus[i] * h_minus[i] - 1) / (Math.Pow(h_plus[i] - h_minus[i], 2));
                    Wp_mi[i] = (Math.Pow(h_minus[i],
                        2) - h_plus[i] * h_minus[i] - 1) / (Math.Pow(h_plus[i] - h_minus[i], 2));
                    Wp_pm[i] = 2 / (Math.Pow(h_plus[i] - h_minus[i], 2));
                }
                double[] SPrdct = new double[xDim];
                double Sum = 0;
                for (int i = 0; i < xDim; i++)
                {
                    SPrdct[i] = h_plus[i] * h_minus[i];
                    Sum = Sum + 1.0 / (SPrdct[i]);
                }
                W_nSP[0] = 1 + Sum;

                // Setting the input values of the model from the selected constants:
                var ModelInputsNames = new ArrayList();
                foreach (Data data in ec.ModelDataInputs)
                    ModelInputsNames.Add(data.Name);
                var ModelOutputsNames = new ArrayList();
                foreach (Data data in ec.ModelDataOutputs)
                    ModelOutputsNames.Add(data.Name);

                double[] ModelInputsValues = new double[ec.ModelDataInputs.Count];
                var IntegerInputsIndex = new List<int>();
                var IntegerInputsContent = new List<int>();
                var StringInputsIndex = new List<int>();
                var StringInputsContent = new List<string>();
                var DoubleArrayInputsIndex = new List<int>();
                var DoubleArrayInputsContent = new List<double[]>();
                var IntegerArrayInputsIndex = new List<int>();
                var IntegerArrayInputsContent = new List<int[]>();
                int ncount1 = 0;
                foreach (Data dt in ec.ModelDataInputs)
                {
                    if (dt is DoubleData)
                        ModelInputsValues[ncount1] = dt.ValueAsDouble;
                    else
                    {
                        //ModelInputsValues[ncount1] = 1.0;//Replaced by Marco with the cases breakdown below
                        if (dt is IntegerData)
                        {
                            IntegerInputsIndex.Add(ncount1);
							int DataValueInString = Convert.ToInt32(dt.Value);
                            IntegerInputsContent.Add(DataValueInString);
                        }
                        if (dt is StringData)
                        {
                            StringInputsIndex.Add(ncount1);
                            StringInputsContent.Add(dt.Value as string);
                        }
                        else if (dt is DoubleVectorData)
                        {
                            DoubleArrayInputsIndex.Add(ncount1);
                            DoubleArrayInputsContent.Add(dt.Value as double[]);
                        }
                        else if (dt is IntegerVectorData)
                        {
                            IntegerArrayInputsIndex.Add(ncount1);
                            IntegerArrayInputsContent.Add(dt.Value as int[]);
                        }
                        else
                        {
                            if (!(dt is IntegerData))//Opening a MessageBox in case of unrecognised data type - except for "int" type, which is anyway handled by the class
                            {
                                Console.WriteLine("Data of type \"Integer\" not yet handled in Robust Studies.");
                                return false;
                            }
                        }
                    }
                    //Strings , arrays and other varaibles are supressed as double(1.0) they are not used inside MOGA
                    ncount1++;
                }

                int outDim = ModelOutputsNames.Count; // Number of the outputs provided by the model
                double[] ModInVals = new double[ModelInputsValues.Length];
                for (int j = 0; j < ModelInputsValues.Length; j++)
                    ModInVals[j] = ModelInputsValues[j];
                for (int i = 0; i < constDim; i++)
                {
                    for (int j = 0; j < ModelInputsNames.Count; j++)
                    {
                        if ((string)ModelInputsNames[j] == NewConstNames[i])
                        {
                            ModInVals[j] = NewConstValues[i];
                        }
                    }
                }

                // Setting the input values of the model from the selected variables:
                /*for (int i = 0; i < xDim; i++)
                {
                    for (int j = 0; j < ModelInputsNames.Count; j++)
                    {
                        if (ModelInputsNames[j] == VarsNames[i])
                        {
                            ModInVals[j] = unk[i];
                        }
                    }
                }*/
                //**********
                //Added by Libish
                //***********
                for (int i = 0; i < xDim_real; i++) //Only for the real variables (without considering the incorporation of uncertain constants)!!!
                {
                    for (int j = 0; j < ModelInputsNames.Count; j++)
                    {
                        if ((string)ModelInputsNames[j] == VarsNames[i])
                        {
                            unk[i] = ModInVals[j];
                        }
                    }
                }
                double[] initalinputs = (ModInVals.Clone() as double[]);
                // Executing the model according to the desired problem formulation and storing the output values:
                Data.SetValuesDouble((ec as WorkflowComponent).ModelDataInputs, ModInVals);

                //Restoring the original content of (potential) Integer inputs:
                for (int IthIntegerInput = 0; IthIntegerInput < IntegerInputsIndex.Count; IthIntegerInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[IntegerInputsIndex[IthIntegerInput]] as Data).Value = IntegerInputsContent[IthIntegerInput];
                }
                //Restoring the original content of (potential) string inputs:
                for (int IthStringInput = 0; IthStringInput < StringInputsIndex.Count; IthStringInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[StringInputsIndex[IthStringInput]] as Data).Value = StringInputsContent[IthStringInput];
                }
                //Restoring the original content of (potential) Double_Array inputs:
                for (int IthDoubleArrayInput = 0; IthDoubleArrayInput < DoubleArrayInputsIndex.Count; IthDoubleArrayInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[DoubleArrayInputsIndex[IthDoubleArrayInput]] as Data).Value = DoubleArrayInputsContent[IthDoubleArrayInput];
                }
                //Restoring the original content of (potential) Int_Array inputs:
                for (int IthIntegerArrayInput = 0; IthIntegerArrayInput < IntegerArrayInputsIndex.Count; IthIntegerArrayInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[IntegerArrayInputsIndex[IthIntegerArrayInput]] as Data).Value = IntegerArrayInputsContent[IthIntegerArrayInput];
                }

                bool SubProcessExecutionStatus = (ec as ExecutableComponent).Execute();
                if (SubProcessExecutionStatus == false)
                {
                    #region Execution Failed
                    RobustData = new string[1, xDim_real];
                    for (int IthVar = 0; IthVar < xDim_real; IthVar++)
                    {
                        RobustData[0, IthVar] = Convert.ToString(unk[IthVar]);
                    }
                    var filer = new FileStream(Project.ProjectPath + "\\ROP_log.txt", FileMode.Append, FileAccess.Write);
                    var sw = new StreamWriter(filer);
                    for (int j = 0; j < xDim_real; j++)
                    {
                        sw.Write(RobustData[0, j]);
                        sw.Write(@"  ");
                    }
                    sw.Write(@"" + "\r\n");
                    sw.Close();
                    filer.Close();

                        Console.WriteLine("The evaluation of (at least) one point in the uncertainty propagation stencil was unsuccessful. This could potentially impact on the accuracy of the results. The values of the design point(s) that resulted in an unsuccessful execution of the considered model/subprocess have been stored in the ROP_log.txt file located in the AirCADia installation folder.");
                    return false;
                    #endregion Execution Failed
                }

                double[] outvar_0 = new double[(ec as WorkflowComponent).ModelDataOutputs.Count];
                ncount1 = 0;
                foreach (Data dt in (ec as WorkflowComponent).ModelDataOutputs)
                {
                    if (dt is DoubleData)
                        outvar_0[ncount1] = dt.ValueAsDouble;
                    else
                        outvar_0[ncount1] = 1.0;
                    //Strings , arrays and other varaibles are supressed as double(1.0) they are not used inside MOGA
                    ncount1++;
                }

                double[] outvar_mean = Const_vect_mltpl(W_nSP[0], outvar_0);
                double[] outvar_var = new double[outDim];

                // Defining the stencil points to compute and the vectors and matrices where to store the respective evaluations:
                double[,] unk_plus_matrix = new double[xDim, xDim];
                double[] unk_plus_vect = new double[xDim];
                double[,] unk_minus_matrix = new double[xDim, xDim];
                double[] unk_minus_vect = new double[xDim];
                double[,] outvar_plus_matrix = new double[xDim, outDim];
                double[,] outvar_minus_matrix = new double[xDim, outDim];
                double[,] output_mean_der_appr = new double[xDim, outDim];
                double[,] output_var_der_appr = new double[xDim, outDim];

                // Stencil evaluation:
                double[] unk_0 = unk;
                for (int i = 0; i < xDim; i++)
                {
                    double[] e_vect = new double[xDim];
                    e_vect[i] = 1;

                    for (int j = 0; j < xDim; j++) unk_plus_matrix[i, j] = unk_0[j];
                    for (int j = 0; j < xDim; j++) unk_plus_vect[j] = unk_0[j];
                    for (int j = 0; j < xDim; j++) unk_minus_matrix[i, j] = unk_0[j];
                    for (int j = 0; j < xDim; j++) unk_minus_vect[j] = unk_0[j];

                    // Definition of the arrays representative point of the stencil:
                    unk_plus_matrix[i, i] = unk_0[i] + h_plus[i] * e_vect[i] * Sigma[i];
                    unk_plus_vect[i] = unk_0[i] + h_plus[i] * e_vect[i] * Sigma[i];
                    unk_minus_matrix[i, i] = unk_0[i] + h_minus[i] * e_vect[i] * Sigma[i];
                    unk_minus_vect[i] = unk_0[i] + h_minus[i] * e_vect[i] * Sigma[i];

                    // Execution and storage of the model evaluation for the forward stencil point:
                    double[] ModInVals_plus = ModInVals;
                    for (int k = 0; k < xDim; k++)
                    {
                        for (int j = 0; j < ModelInputsNames.Count; j++)
                        {
                            if ((string)ModelInputsNames[j] == VarsNames[k])
                            {
                                ModInVals_plus[j] = unk_plus_vect[k];
                            }
                        }
                    }
                    Data.SetValuesDouble((ec as WorkflowComponent).ModelDataInputs, ModInVals_plus);

                    //Restoring the original content of (potential) Integer inputs:
                    for (int IthIntegerInput = 0; IthIntegerInput < IntegerInputsIndex.Count; IthIntegerInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[IntegerInputsIndex[IthIntegerInput]] as Data).Value = IntegerInputsContent[IthIntegerInput];
                    }
                    //Restoring the original content of (potential) string inputs:
                    for (int IthStringInput = 0; IthStringInput < StringInputsIndex.Count; IthStringInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[StringInputsIndex[IthStringInput]] as Data).Value = StringInputsContent[IthStringInput];
                    }
                    //Restoring the original content of (potential) Double_Array inputs:
                    for (int IthDoubleArrayInput = 0; IthDoubleArrayInput < DoubleArrayInputsIndex.Count; IthDoubleArrayInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[DoubleArrayInputsIndex[IthDoubleArrayInput]] as Data).Value = DoubleArrayInputsContent[IthDoubleArrayInput];
                    }
                    //Restoring the original content of (potential) Int_Array inputs:
                    for (int IthIntegerArrayInput = 0; IthIntegerArrayInput < IntegerArrayInputsIndex.Count; IthIntegerArrayInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[IntegerArrayInputsIndex[IthIntegerArrayInput]] as Data).Value = IntegerArrayInputsContent[IthIntegerArrayInput];
                    }

                    SubProcessExecutionStatus = (ec as ExecutableComponent).Execute();
                    if (SubProcessExecutionStatus == false)
                    {
                        #region Execution Failed
                        RobustData = new string[1, xDim_real];
                        for (int IthVar = 0; IthVar < xDim_real; IthVar++)
                        {
                            RobustData[0, IthVar] = Convert.ToString(unk[IthVar]);
                        }
                        var filer = new FileStream(Project.ProjectPath + "\\ROP_log.txt", FileMode.Append, FileAccess.Write);
                        var sw = new StreamWriter(filer);
                        for (int j = 0; j < xDim_real; j++)
                        {
                            sw.Write(RobustData[0, j]);
                            sw.Write(@"  ");
                        }
                        sw.Write(@"" + "\r\n");
                        sw.Close();
                        filer.Close();


                            Console.WriteLine("The evaluation of (at least) one point in the uncertainty propagation stencil was unsuccessful. This could potentially impact on the accuracy of the results. The values of the design point(s) that resulted in an unsuccessful execution of the considered model/subprocess have been stored in the ROP_log.txt file located in the AirCADia installation folder.");

                        return false;
                        #endregion Execution Failed
                    }

                    double[] outvar_plus = new double[(ec as WorkflowComponent).ModelDataOutputs.Count];
                    ncount1 = 0;
                    foreach (Data dt in (ec as WorkflowComponent).ModelDataOutputs)
                    {
                        if (dt is DoubleData)
                            outvar_plus[ncount1] = dt.ValueAsDouble;
                        else
                            outvar_plus[ncount1] = 1.0;
                        //Strings , arrays and other varaibles are supressed as double(1.0) they are not used inside MOGA
                        ncount1++;
                    }

                    for (int j = 0; j < outDim; j++)
                        outvar_plus_matrix[i, j] = outvar_plus[j];

                    // Execution and storage of the model evaluation for the backward stencil point:
                    double[] ModInVals_minus = ModInVals;
                    for (int k = 0; k < xDim; k++)
                    {
                        for (int j = 0; j < ModelInputsNames.Count; j++)
                        {
                            if ((string)ModelInputsNames[j] == VarsNames[k])
                            {
                                ModInVals_minus[j] = unk_minus_vect[k];
                            }
                        }
                    }
                    Data.SetValuesDouble((ec as WorkflowComponent).ModelDataInputs, ModInVals_minus);

                    //Restoring the original content of (potential) Integer inputs:
                    for (int IthIntegerInput = 0; IthIntegerInput < IntegerInputsIndex.Count; IthIntegerInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[IntegerInputsIndex[IthIntegerInput]] as Data).Value = IntegerInputsContent[IthIntegerInput];
                    }
                    //Restoring the original content of (potential) string inputs:
                    for (int IthStringInput = 0; IthStringInput < StringInputsIndex.Count; IthStringInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[StringInputsIndex[IthStringInput]] as Data).Value = StringInputsContent[IthStringInput];
                    }
                    //Restoring the original content of (potential) Double_Array inputs:
                    for (int IthDoubleArrayInput = 0; IthDoubleArrayInput < DoubleArrayInputsIndex.Count; IthDoubleArrayInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[DoubleArrayInputsIndex[IthDoubleArrayInput]] as Data).Value = DoubleArrayInputsContent[IthDoubleArrayInput];
                    }
                    //Restoring the original content of (potential) Int_Array inputs:
                    for (int IthIntegerArrayInput = 0; IthIntegerArrayInput < IntegerArrayInputsIndex.Count; IthIntegerArrayInput++)
                    {
                        ((ec as WorkflowComponent).ModelDataInputs[IntegerArrayInputsIndex[IthIntegerArrayInput]] as Data).Value = IntegerArrayInputsContent[IthIntegerArrayInput];
                    }

                    SubProcessExecutionStatus = (ec as ExecutableComponent).Execute();
                    if (SubProcessExecutionStatus == false)
                    {
                        #region Execution Failed
                        RobustData = new string[1, xDim_real];
                        for (int IthVar = 0; IthVar < xDim_real; IthVar++)
                        {
                            RobustData[0, IthVar] = Convert.ToString(unk[IthVar]);
                        }
                        var filer = new FileStream(Project.ProjectPath + "\\ROP_log.txt", FileMode.Append, FileAccess.Write);
                        var sw = new StreamWriter(filer);
                        for (int j = 0; j < xDim_real; j++)
                        {
                            sw.Write(RobustData[0, j]);
                            sw.Write(@"  ");
                        }
                        sw.Write(@"" + "\r\n");
                        sw.Close();
                        filer.Close();

                            Console.WriteLine("The evaluation of (at least) one point in the uncertainty propagation stencil was unsuccessful. This could potentially impact on the accuracy of the results. The values of the design point(s) that resulted in an unsuccessful execution of the considered model/subprocess have been stored in the ROP_log.txt file located in the AirCADia installation folder.");
                        return false;
                        #endregion Execution Failed
                    }

                    double[] outvar_minus = new double[(ec as WorkflowComponent).ModelDataOutputs.Count];
                    ncount1 = 0;
                    foreach (Data dt in (ec as WorkflowComponent).ModelDataOutputs)
                    {
                        if (dt is DoubleData)
                            outvar_minus[ncount1] = dt.ValueAsDouble;
                        else
                            outvar_minus[ncount1] = 1.0;
                        //Strings , arrays and other varaibles are supressed as double(1.0) they are not used inside MOGA
                        ncount1++;
                    }

                    for (int j = 0; j < outDim; j++)
                        outvar_minus_matrix[i, j] = outvar_minus[j];

                    // Estimation of the mean and variance for all the model outputs:
                    for (int j = 0; j < outDim; j++)
                        outvar_mean[j] = outvar_mean[j] + W_nSP[i + 1] * (outvar_plus_matrix[i, j] / h_plus[i] - outvar_minus_matrix[i, j] / h_minus[i]);
                    for (int j = 0; j < outDim; j++)
                        outvar_var[j] = outvar_var[j] + Wp_pl[i] * (Math.Pow((outvar_plus_matrix[i, j] - outvar_0[j]) / (h_plus[i]), 2)) + Wp_mi[i] * (Math.Pow((outvar_minus_matrix[i, j] - outvar_0[j]) / (h_minus[i]), 2)) + Wp_pm[i] * ((outvar_plus_matrix[i, j] - outvar_0[j]) * (outvar_minus_matrix[i, j] - outvar_0[j])) / (h_plus[i] * h_minus[i]);
                }

                // Storage of the robust solution for the selected m objective(s) and n constraint(s). It is done via the RObOutput Matrix: the first column contains the names of the user-desired outputs, the second their corresponding value. The first m rows correspond to the m selected objectives, whereas the remaining n rows are related to the n selected constraints.
                string[,] RobOutput = new string[ROB_objs.GetLength(0) + ROB_cnstrs.GetLength(0), 2];
                string[,] RobEvaluation = new string[ROB_objs.GetLength(0) + ROB_cnstrs.GetLength(0), 2];
                for (int j = 0; j < ROB_objs.GetLength(0); j++) RobOutput[j, 0] = ROB_objs[j, 0];
                for (int j = ROB_objs.GetLength(0); j < ROB_objs.GetLength(0) + ROB_cnstrs.GetLength(0); j++) RobOutput[j, 0] = ROB_cnstrs[j - ROB_objs.GetLength(0), 0];
                double Obj_k_sign = new double();
                double Cnstr_k_sign = new double();
                for (int k = 0; k < RobOutput.GetLength(0); k++)
                {
                    for (int j = 0; j < ModelOutputsNames.Count; j++)
                    {
                        if (RobOutput[k, 0] == (string)ModelOutputsNames[j])
                        {
                            if (k < ROB_objs.GetLength(0))
                            {
                                if (ROB_objs[k, 6] == "+")
                                {
                                    Obj_k_sign = 1;
                                }
                                else if (ROB_objs[k, 6] == "-")
                                {
                                    Obj_k_sign = -1;
                                }
                                else if (ROB_objs[k, 6] != null)
                                {
                                    Console.WriteLine("A problem has arisen in the computation of the (objectives) Loss Function for Uncertainty Propagation with URQ.");
                                }
                                RobOutput[k, 1] = Convert.ToString(outvar_mean[j] + Obj_k_sign * Convert.ToDouble(ROB_objs[k, 1]) * Math.Sqrt(outvar_var[j]));
                                double Kfactor_check = Convert.ToDouble(ROB_objs[k, 1]);
                                RobEvaluation[k, 0] = Convert.ToString(outvar_mean[j]);
                                RobEvaluation[k, 1] = Convert.ToString(outvar_var[j]);
                            }
                            else
                            {
                                if (ROB_cnstrs[k - ROB_objs.GetLength(0), 2] == "<=")
                                {
                                    Cnstr_k_sign = 1;
                                }
                                else if (ROB_cnstrs[k - ROB_objs.GetLength(0), 2] == ">=")
                                {
                                    Cnstr_k_sign = -1;
                                }
                                else
                                {
                                    Console.WriteLine("A problem has arisen in the computation of the (constraints) Loss Function for Uncertainty Propagation with URQ.");
                                }
                                RobOutput[k, 1] = Convert.ToString(outvar_mean[j] + Cnstr_k_sign * Convert.ToDouble(ROB_cnstrs[k - ROB_objs.GetLength(0), 1]) * Math.Sqrt(outvar_var[j]));
                                double Kfactor_check = Convert.ToDouble(ROB_cnstrs[k - ROB_objs.GetLength(0), 1]);
                                RobEvaluation[k, 0] = Convert.ToString(outvar_mean[j]);
                                RobEvaluation[k, 1] = Convert.ToString(outvar_var[j]);
                            }
                        }
                    }
                }

                for (int nout = 0; nout < (ec as ExecutableComponent).ModelDataOutputs.Count; nout++)
                {
                    for (int nrout = 0; nrout < RobOutput.GetLength(0); nrout++)
                    {
                        if (RobOutput[nrout, 0] == ((ec as ExecutableComponent).ModelDataOutputs[nout] as Data).Name)
                            ((ec as ExecutableComponent).ModelDataOutputs[nout] as Data).Value = Convert.ToDouble(RobOutput[nrout, 1]);
                    }
                }
                //Restoring the original inputs:
                Data.SetValuesDouble((ec as ExecutableComponent).ModelDataInputs, initalinputs);
                //Restoring the original content of (potential) Integer inputs:
                for (int IthIntegerInput = 0; IthIntegerInput < IntegerInputsIndex.Count; IthIntegerInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[IntegerInputsIndex[IthIntegerInput]] as Data).Value = IntegerInputsContent[IthIntegerInput];
                }
                //Restoring the original content of (potential) string inputs:
                for (int IthStringInput = 0; IthStringInput < StringInputsIndex.Count; IthStringInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[StringInputsIndex[IthStringInput]] as Data).Value = StringInputsContent[IthStringInput];
                }
                //Restoring the original content of (potential) Double_Array inputs:
                for (int IthDoubleArrayInput = 0; IthDoubleArrayInput < DoubleArrayInputsIndex.Count; IthDoubleArrayInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[DoubleArrayInputsIndex[IthDoubleArrayInput]] as Data).Value = DoubleArrayInputsContent[IthDoubleArrayInput];
                }
                //Restoring the original content of (potential) Int_Array inputs:
                for (int IthIntegerArrayInput = 0; IthIntegerArrayInput < IntegerArrayInputsIndex.Count; IthIntegerArrayInput++)
                {
                    ((ec as WorkflowComponent).ModelDataInputs[IntegerArrayInputsIndex[IthIntegerArrayInput]] as Data).Value = IntegerArrayInputsContent[IthIntegerArrayInput];
                }

                int NoRows = xDim_real + 3 * (ROB_objs.GetLength(0) + ROB_cnstrs.GetLength(0));
                RobustData = new string[1, NoRows];
                for (int i = 0; i < xDim_real; i++)
                {
                    RobustData[0, i] = Convert.ToString(unk[i]);
                }
                for (int i = 0; i < ROB_objs.GetLength(0); i++)
                {
                    RobustData[0, xDim_real + 3 * i + 0] = Convert.ToString(RobOutput[i, 1]);
                    RobustData[0, xDim_real + 3 * i + 1] = Convert.ToString(RobEvaluation[i, 0]);
                    RobustData[0, xDim_real + 3 * i + 2] = Convert.ToString(RobEvaluation[i, 1]);
                }
                for (int i = 0; i < ROB_cnstrs.GetLength(0); i++)
                {
                    RobustData[0, xDim_real + 3 * ROB_objs.GetLength(0) + 3 * i + 0] = Convert.ToString(RobOutput[ROB_objs.GetLength(0) + i, 1]);
                    RobustData[0, xDim_real + 3 * ROB_objs.GetLength(0) + 3 * i + 1] = Convert.ToString(RobEvaluation[ROB_objs.GetLength(0) + i, 0]);
                    RobustData[0, xDim_real + 3 * ROB_objs.GetLength(0) + 3 * i + 2] = Convert.ToString(RobEvaluation[ROB_objs.GetLength(0) + i, 1]);
                }

                var filerFinal = new FileStream(Project.ProjectPath + "\\FuncEvalsRob.txt", FileMode.Append, FileAccess.Write);
                var swFinal = new StreamWriter(filerFinal);






				#region Atif

				Outvar_mean = outvar_mean;
				Outvar_var = outvar_var;

                string databaseFileName = Path.Combine(Project.ProjectPath, this.databaseFileName); // Microsoft SQL server compact edition file

                // Create database for optimisation results
                string connectionString;

                connectionString = String.Format("Data Source = " + databaseFileName + ";Persist Security Info=False");
                var engine = new SqlCeEngine(connectionString);



                var connection = new SqlCeConnection(connectionString);
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }



                string sql1 = "insert into " + studyName + " (ID, ";
                string valuesString1 = "values (@ID, ";
                string valuesString = "";


                for (int i = 0; i < names.Count; i++)
                {
                    sql1 += names[i] + ", ";
                    valuesString += "@" + names[i] + ", ";
                }


                for (int i = 0; i < robOptTemplate.DesignVariables.Count; i++)
                {
                    if (!names.Contains(robOptTemplate.DesignVariables[i].Name))
                    {
                        sql1 += robOptTemplate.DesignVariables[i].Name + ", ";
                        valuesString += "@" + robOptTemplate.DesignVariables[i].Name + ", ";
                    }
                }
                for (int i = 0; i < robOptTemplate.Objectives.Count; i++)
                {
                    // Loss Function
                    sql1 += robOptTemplate.Objectives[i].Name + "LF" + ", ";
                    valuesString += "@" + robOptTemplate.Objectives[i].Name + "LF" + ", ";
                    // Mean
                    sql1 += robOptTemplate.Objectives[i].Name + "mean" + ", ";
                    valuesString += "@" + robOptTemplate.Objectives[i].Name + "mean" + ", ";
                    // Variance
                    sql1 += robOptTemplate.Objectives[i].Name + "var" + ", ";
                    valuesString += "@" + robOptTemplate.Objectives[i].Name + "var" + ", ";
                }
                for (int i = 0; i < robOptTemplate.Constraints.Count; i++)
                {
                    // Loss Function
                    sql1 += robOptTemplate.Constraints[i].Name + "LF" + ", ";
                    valuesString += "@" + robOptTemplate.Constraints[i].Name + "LF" + ", ";

                    // Mean
                    sql1 += robOptTemplate.Constraints[i].Name + "mean" + ", ";
                    valuesString += "@" + robOptTemplate.Constraints[i].Name + "mean" + ", ";

                    // Variance
                    sql1 += robOptTemplate.Constraints[i].Name + "var" + ", ";
                    valuesString += "@" + robOptTemplate.Constraints[i].Name + "var" + ", ";
                }
                if (robOptTemplate.DesignVariables.Count() + robOptTemplate.Objectives.Count() + robOptTemplate.Constraints.Count() > 0)
                {
                    sql1 = sql1.Remove(sql1.Length - 2);
                    valuesString = valuesString.Remove(valuesString.Length - 2);
                }
                sql1 += ")";
                valuesString += ")";
                valuesString1 += valuesString;
                sql1 += (" " + valuesString1);



                SqlCeCommand cmd1 = null;

                try
                {
                    cmd1 = new SqlCeCommand(sql1, connection);
                }
                catch (SqlCeException sqlexception)
                {
                    Console.WriteLine(sqlexception.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                #endregion Atif
                for (int j = 0; j < NoRows; j++)
                {
                    swFinal.Write(RobustData[0, j]);
                    swFinal.Write(@"  ");






                }

                swFinal.Write(@"" + "\r\n");
                swFinal.Close();
                filerFinal.Close();
                #region Atif



                #region Atif
                cmd1.Parameters.AddWithValue("@ID", Counter++);
                int columnCounter = 0;


                #region Assign Mean Values
                for (int cnt = 0; cnt < names.Count; cnt++)
                {
                    foreach (Data dt in ec.ModelDataInputs)
                    {
                        if (dt.Name == names[cnt])
                        {
                            cmd1.Parameters.AddWithValue("@" + dt.Name, (double)meanValues[atif][cnt]);
                            
                            break;
                        }
                    }
                }
                #endregion Assign Mean Values



                foreach (RobOptDesignVariable dv in robOptTemplate.DesignVariables)
                {
                    if (!names.Contains(dv.Name))
                    {
                        cmd1.Parameters.AddWithValue("@" + dv.Name, RobustData[0, columnCounter]);
                    }
                    columnCounter++;
                }
                foreach (RobOptObjective obj in robOptTemplate.Objectives)
                {
                    cmd1.Parameters.AddWithValue("@" + obj.Name + "LF", RobustData[0, columnCounter]);
                    columnCounter++;
                    cmd1.Parameters.AddWithValue("@" + obj.Name + "mean", RobustData[0, columnCounter]);
                    columnCounter++;
                    cmd1.Parameters.AddWithValue("@" + obj.Name + "var", RobustData[0, columnCounter]);
                    columnCounter++;
                }
                foreach (RobOptConstraint constr in robOptTemplate.Constraints)
                {
                    cmd1.Parameters.AddWithValue("@" + constr.Name + "LF", RobustData[0, columnCounter]);
                    columnCounter++;
                    cmd1.Parameters.AddWithValue("@" + constr.Name + "mean", RobustData[0, columnCounter]);
                    columnCounter++;
                    cmd1.Parameters.AddWithValue("@" + constr.Name + "var", RobustData[0, columnCounter]);
                    columnCounter++;
                }

                #endregion Atif
                try
                {
                    cmd1.ExecuteNonQuery();
                }
                catch (SqlCeException sqlexception)
                {
                    Console.WriteLine(sqlexception.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                #endregion Atif

                ////vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
                ////The following has been implemented specifically for CRESCENDO
                //if (fCwmd.RequestedStudy != null)
                //{
                //    var Mean_filer = new FileStream(fCwmd.Mean_FileName, FileMode.Append, FileAccess.Write);
                //    var Mean_sw = new StreamWriter(Mean_filer);
                //    for (int j = 0; j < outDim; j++)
                //    {
                //        Mean_sw.Write(outvar_mean[j]);
                //        Mean_sw.Write(@"  ");
                //    }
                //    Mean_sw.Write(@"" + "\r\n");
                //    Mean_sw.Close();
                //    Mean_filer.Close();

                //    var Std_filer = new FileStream(fCwmd.Std_FileName, FileMode.Append, FileAccess.Write);
                //    var Std_sw = new StreamWriter(Std_filer);
                //    for (int j = 0; j < outDim; j++)
                //    {
                //        Std_sw.Write(outvar_var[j]);
                //        Std_sw.Write(@"  ");
                //    }
                //    Std_sw.Write(@"" + "\r\n");
                //    Std_sw.Close();
                //    Std_filer.Close();
                //}
                ////^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            }
            return true;
        }



        public double[] Const_vect_mltpl(double Constant, double[] Vector)
        {
            double[] Product = new double[Vector.Length];
            for (int i = 0; i < Vector.Length; i++)
            {
                Product[i] = Constant * Vector[i];
            }
            return Product;
        }



        public override void CreateFolder()
        {
            // Create a folder for the study
            string studyDirectory = System.IO.Path.Combine(Project.ProjectPath, "Studies", studyName);
            Directory.CreateDirectory(studyDirectory);



            var metadata = new XDocument();

            var resultElement = new XElement("Result");
            resultElement.Add(new XAttribute("Type", "DesignsStudy"));
            resultElement.Add(new XAttribute("WorkflowName", workflow.Name));



            var tagsElement = new XElement("Tags");
            resultElement.Add(tagsElement);

            var parametersElement = new XElement("Parameters");


            var idParameterElement = new XElement("Parameter");
            idParameterElement.Add(new XAttribute("Name", "ID"));
            idParameterElement.Add(new XAttribute("Type", "Integer"));
            idParameterElement.Add(new XAttribute("Unit", ""));
            parametersElement.Add(idParameterElement);

            for (int i = 0; i < factors.Count; i++)
            {
                var parameterElement = new XElement("Parameter");
                string columnHeader = factors[i].Name;
                parameterElement.Add(new XAttribute("Name", columnHeader));
                if (factors[i] is IntegerData)
                {
                    parameterElement.Add(new XAttribute("Type", "Integer"));
                    parameterElement.Add(new XAttribute("Unit", ((IntegerData)factors[i]).Unit));
                }
                else if (factors[i] is DoubleData)
                {
                    parameterElement.Add(new XAttribute("Type", "Double"));
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)factors[i]).DecimalPlaces));
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)factors[i]).Unit));
                }
                var fullFactorialFactorElement = new XElement("FullFactorialFactor");
                fullFactorialFactorElement.Add(new XAttribute("StartingValue", startingValues[i]));
                fullFactorialFactorElement.Add(new XAttribute("StepSize", stepSizes[i]));
                fullFactorialFactorElement.Add(new XAttribute("NoOfLevels", noOfLevels[i]));
                parameterElement.Add(fullFactorialFactorElement);
                parametersElement.Add(parameterElement);
            }

            for (int i = 0; i < uncertainParameters.Count; i++)
            {
                var parameterElement = new XElement("Parameter");
                string columnHeader = uncertainParameters[i].Name;
                parameterElement.Add(new XAttribute("Name", columnHeader));
                Data data = uncertainParameters[i];
                if (data is IntegerData)
                {
                    parameterElement.Add(new XAttribute("Type", "Integer"));
                    parameterElement.Add(new XAttribute("Unit", ((IntegerData)data).Unit));
                }
                else if (data is DoubleData)
                {
                    parameterElement.Add(new XAttribute("Type", "Double"));
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)data).DecimalPlaces));
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)data).Unit));
                }
                parametersElement.Add(parameterElement);
            }

            for (int i = 0; i < robOptTemplate.Objectives.Count; i++)
            {
                // Loss Function
                var parameterElement = new XElement("Parameter");
                string columnHeader = robOptTemplate.Objectives[i].Name + "LF";
                parameterElement.Add(new XAttribute("Name", columnHeader));
                if (responses[i] is IntegerData)
                {
                    parameterElement.Add(new XAttribute("Type", "Integer"));
                    parameterElement.Add(new XAttribute("Unit", ((IntegerData)responses[i]).Unit));
                }
                else if (responses[i] is DoubleData)
                {
                    parameterElement.Add(new XAttribute("Type", "Double"));
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)responses[i]).DecimalPlaces));
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)responses[i]).Unit));
                }
                var fullFactorialFactorElement = new XElement("FullFactorialResponse");
                parameterElement.Add(fullFactorialFactorElement);
                parametersElement.Add(parameterElement);

                // Mean
                var parameterElementMean = new XElement("Parameter");
                string columnHeaderMean = robOptTemplate.Objectives[i].Name + "mean";
                parameterElementMean.Add(new XAttribute("Name", columnHeaderMean));
                if (responses[i] is IntegerData)
                {
                    parameterElementMean.Add(new XAttribute("Type", "Integer"));
                    parameterElementMean.Add(new XAttribute("Unit", ((IntegerData)responses[i]).Unit));
                }
                else if (responses[i] is DoubleData)
                {
                    parameterElementMean.Add(new XAttribute("Type", "Double"));
                    parameterElementMean.Add(new XAttribute("DecimalPlaces", ((DoubleData)responses[i]).DecimalPlaces));
                    parameterElementMean.Add(new XAttribute("Unit", ((DoubleData)responses[i]).Unit));
                }
                var fullFactorialFactorElementMean = new XElement("FullFactorialResponse");
                parameterElementMean.Add(fullFactorialFactorElementMean);
                parametersElement.Add(parameterElementMean);


                // Variance
                var parameterElementVar = new XElement("Parameter");
                string columnHeaderVar = robOptTemplate.Objectives[i].Name + "var";
                parameterElementVar.Add(new XAttribute("Name", columnHeaderVar));
                if (responses[i] is IntegerData)
                {
                    parameterElementVar.Add(new XAttribute("Type", "Integer"));
                    parameterElementVar.Add(new XAttribute("Unit", ((IntegerData)responses[i]).Unit));
                }
                else if (responses[i] is DoubleData)
                {
                    parameterElementVar.Add(new XAttribute("Type", "Double"));
                    parameterElementVar.Add(new XAttribute("DecimalPlaces", ((DoubleData)responses[i]).DecimalPlaces));
                    parameterElementVar.Add(new XAttribute("Unit", ((DoubleData)responses[i]).Unit));
                }
                var fullFactorialFactorElementVar = new XElement("FullFactorialResponse");
                parameterElementVar.Add(fullFactorialFactorElementVar);
                parametersElement.Add(parameterElementVar);
            }

            resultElement.Add(parametersElement);

            metadata.Add(resultElement);

            string resFilePath = System.IO.Path.Combine(Project.ProjectPath, "Studies", studyName, studyName + ".xml");
            metadata.Save(resFilePath);
        }
    }
}
