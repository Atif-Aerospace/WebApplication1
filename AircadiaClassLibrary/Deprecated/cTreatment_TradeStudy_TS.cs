/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;

namespace Aircadia
{
    [Serializable()]
    //// Subclass of the treatment class. This is the treatment class for TradeStudy
    public class Treatment_TradeStudy_TS : Treatment
    {
        public Treatment_TradeStudy_TS(string n, Treatment_InOut input_opt, Treatment_InOut outp) :
            base(n, n)
        {
            Name = n;
            input_options = input_opt;
            output_struct = outp;
        }
        /// <summary>
        /// The string returned by this function is the one which will be printed in the properties window of AirCADia for the selected data object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = "";
            foreach (TS_Input_set oDt in (input_options as Treatment_InOut_TS_Input).setuplist)
            {
                output = output + oDt.min + "->" + oDt.Data + "->" + oDt.max + "(Levels: " + oDt.Increment + ")" + "\r\n";
            }
            return output;
        }

		public override bool ApplyOn() => true;
		/// <summary>
		/// Executes the tradestudy
		/// </summary>
		/// <param name="oModSub"></param>
		/// <returns></returns>
		public override bool ApplyOn(ExecutableComponent oModSub)
        {

            double curval = 0;
            string dataval = "";
            string output = "";
            if (File.Exists("evaluatedSolutions.txt"))
                File.Delete("evaluatedSolutions.txt");
            var filer = new FileStream("evaluatedSolutions.txt", FileMode.OpenOrCreate, FileAccess.Write);
            var sw = new StreamWriter(filer);

            foreach (TS_Input_set IthInput in input_options.setuplist)
            {
                output = output + IthInput.Data + " ";
            }
            string dataNames = "";
            foreach (Data data in oModSub.ModelDataOutputs)
            {
                if (data is IntegerData || data is DoubleData) // Onlu scalar data variables can be visualised in 'results grid'
                    dataNames += (data.Name + " ");
            }
            output += (dataNames + "\r\n");
            output.Trim();
            (int sz, long tot, double[,] indices) = Permutearray();
            for (int i = 0; i < tot; i++)
            {
				for (int j = 0; j < (input_options as Treatment_InOut_TS_Input).setuplist.Count; j++)
                {
					TS_Input_set oDt = (TS_Input_set)(input_options as Treatment_InOut_TS_Input).setuplist[j];
					curval = indices[i, j];
                    oModSub.ModelDataInputs.Find(d => d.Name == oDt.Data).Value = curval;
                }
                bool status = oModSub.Execute();


                //Notifying the user that the execution of the Model/Workflow on which the DoE study is based has been unsuccessful
                if (status == false)
                {
					//if (Console.WriteLine("The execution of the Model/Workflow on which the selected DoE study is based has been unsuccessful for at least one evaluation. Would you like to continue?", "SCC solvers", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
					{
						sw.Close();
                        filer.Close();
                        return false;
                    }
                }



                //Writing DoE variables in TXT file of stored evaluations
                for (int IthVarCounter = 0; IthVarCounter < input_options.setuplist.Count; IthVarCounter++)
                {
                    for (int JthParamCounter = 0; JthParamCounter < oModSub.ModelDataInputs.Count; JthParamCounter++)
                    {
                        if ((input_options.setuplist[IthVarCounter] as TS_Input_set).Data == oModSub.ModelDataInputs[JthParamCounter].Name)
                        {
                            dataval = dataval + oModSub.ModelDataInputs[JthParamCounter].ValueAsString + " ";
                            break;
                        }
                    }
                }

                //Writing all subprocess/model outputs in TXT file of stored evaluations
                string outputValues = "";
                foreach (Data data in oModSub.ModelDataOutputs)
                {
                    if (data is IntegerData || data is DoubleData)
                        outputValues += (data.ValueAsString + " ");
                }
                dataval = dataval + outputValues;

                dataval.Trim();
                //textBox_TS_Output.AppendText(dataval);
                output = output + dataval;
                sw.Write(dataval + "\r\n");
                dataval = "";//resetting dataval for the next subprocess/model execution
            }
            sw.Close();
            filer.Close();
            output_struct = new Treatment_InOut_TS_Output(output);
            //cTreatment_InOut_TS_Output out_struct = new cTreatment_InOut_TS_Output(output);
            //output_struct = output_struct as cTreatment_InOut;
            return true;
        }
        /// <summary>
        /// This function calculates the full factorial array based on user defined
        /// choices of variables and variable ranges
        /// </summary>
        /// <returns>Array of full factorial</returns>
        public (int Ninputs, long NCombinations, double[,] indices) Permutearray()
        {
			Treatment_InOut_TS_Input inputOptions = (input_options as Treatment_InOut_TS_Input);

			int Ninputs = inputOptions.setuplist.Count;

			int[] limit = new int[Ninputs];
            double[] step = new double[Ninputs];
			long NCombinations = 1;
			for (int j = 0; j < Ninputs; j++)
            {
				var oDt = (TS_Input_set)inputOptions.setuplist[j];
				limit[j] = Convert.ToInt16(oDt.Increment);
                NCombinations = NCombinations * limit[j];
                step[j] = (oDt.max - oDt.min) / (limit[j] - 1);
            }

			double[,] indices = new double[NCombinations, Ninputs];
            int maxl = limit.Max();
            double[,] arrayeach = new double[maxl, Ninputs];
			for (int j = 0; j < Ninputs; j++)
			{
				var oDt = (TS_Input_set)inputOptions.setuplist[j];
				double cv = oDt.min;
				int k = 0;
				while (cv <= oDt.max)
				{
					arrayeach[k, j] = cv;
					cv = cv + step[j];

					//Checking if the updated value of 'cv' has exceeded the corresponding oDt.max due to truncation error on its 'step' value by considering a tolerance of 1e-12
					if ((cv - oDt.max) > 0 && (cv - oDt.max) < 1e-12)
						cv = oDt.max;

					k = k + 1;
				}
			}

			int k1;
			for (int i = 0; i < Ninputs; i++)
			{
				int tpt = 1;
				for (int j = i + 1; j < Ninputs; j++)
				{
					tpt = tpt * limit[j];
				}
				int plc = 0;
				while (plc < NCombinations)
				{
					for (int k = 0; k < limit[i]; k++)
					{
						for (k1 = plc; k1 < plc + tpt; k1++)
						{
							indices[k1, i] = arrayeach[k, i];

						}
						plc = plc + tpt;
					}
				}

			}
            return (Ninputs, NCombinations, indices);
        }
    }
}
