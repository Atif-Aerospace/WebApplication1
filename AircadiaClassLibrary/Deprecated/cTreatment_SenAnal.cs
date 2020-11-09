/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;

using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using MathNet.Numerics;

namespace Aircadia
{
	[Serializable()]
	public class Treatment_SenAnal : Treatment
	{
		private readonly int count;
		private readonly Data[] input;
		private readonly Data[] output;
		private readonly double[] xi;
		private readonly double[] xn;
		private readonly string[] option;
		private readonly double[] stepSize;
		#region Constructor
		public Treatment_SenAnal(string n, Treatment_InOut input_opt, Treatment_InOut outp) :
			base(n, n)
		{
			Name = n;
			input_options = input_opt;
			output_struct = outp;

			count = input_options.setuplist.Count;
			input = new Data[count];
			output = new Data[count];
			xi = new double[count];
			xn = new double[count];
			option = new string[count];
			stepSize = new double[count];
			for (int i = 0; i < count; i++)
			{
				string[] list = input_options.setuplist[i] as string[];
				input[i] = Project.GetData(list[0]);
				output[i] = Project.GetData(list[1]);
				xi[i] = Convert.ToDouble(list[2]);
				xn[i] = Convert.ToDouble(list[3]);
				option[i] = list[4];
				stepSize[i] = Convert.ToDouble(list[5]);
			}
		}
		#endregion
		#region View
		public override string ToString()
		{
			ArrayList t1 = input_options.setuplist;
			string output = "";
			foreach (string[] intg in t1)
			{
				output = output + "Finding sensitivity of:  " + intg[1] + "\r\n";
				output = output + "With respect to:  " + intg[0] + "\r\n";
				output = output + "In the range: " + intg[2] + " to " + intg[3] + "\r\n";
				output = output + "Type:  " + intg[4] + "\r\n";
			}
			return output;
		}
		#endregion
		#region Execute
		public override bool ApplyOn() => true;
		public override bool ApplyOn(ExecutableComponent oModSub)
		{

			//string outputString = "";
			string[] output1 = new string[count];
			using (var csvFile = new CSVFiler(CsvPath))
			{
				for (int i = 0; i < count; i++)
				{
					double xi = this.xi[i];
					double xn = this.xn[i];
					double stepSize = this.stepSize[i];
					Data input = this.input[i];
					Data output = this.output[i];
					string option = this.option[i];

					//Selecting the initial step size as 1/1000th of interval
					double h1 = (xn - xi) / 1000;
					bool zeroSensitivity = false;
					while (true)
					{
						double f1 = EvalModel(oModSub, input, output, xi);
						double f2 = EvalModel(oModSub, input, output, xi + h1);

						// If the output appears to be insensitive to the input
						if ((Math.Abs(f1 - f2)) < 1e-16)
						{
							// Duplicate the step
							h1 *= 2;
							// If the step is greater than the studied interval, 
							// declare than the output is insensitive
							if (h1 > (xn - xi))
							{
								zeroSensitivity = true;
								break;
							}
						}
						else
						{
							zeroSensitivity = false;
							break;
						}
					}

					double h2 = h1 / 10;
					double h = 0;
					double termc = 0.01;
					if (zeroSensitivity == true)
					{
						Console.WriteLine("The output has zero sensitivity with respect  to selected input");
					}
					else
					{
						double x;
						double f1;
						double f2;
						while (true)
						{
							x = xi;
							f1 = EvalModel(oModSub, input, output, x);
							x = xi + 2 * h1;
							f2 = EvalModel(oModSub, input, output, x);
							double s1 = (f2 - f1) / (2 * h1);

							x = xi;
							f1 = EvalModel(oModSub, input, output, x);
							x = xi + 2 * h2;
							f2 = EvalModel(oModSub, input, output, x);
							double s2 = (f2 - f1) / (2 * h2);

							double diff = (s1 - s2) / s1;

							// Deciding the step size for partial derivative
							// If the sensitivites are similar keep the step
							if (diff <= termc)
								break;

							// Other wise reduce the step by 10
							h1 = h2;
							h2 /= 10;
						}

						h = h1;

						double[] xinp = Generate.LinearRange(xi, stepSize, xn);
						int nSteps = xinp.Length;
						double[] sens = new double[nSteps];
						double[] f0s = new double[nSteps];



						xinp[0] = xi;
						f0s[0] = EvalModel(oModSub, input, output, xi);

						csvFile.NewRow();
						output1[i] = output1[i] + input.Name + " Sen(abs)" + "\r\n";
						for (int j = 0; j < nSteps; j++)
						{
							x = xinp[j] - h;
							f1 = EvalModel(oModSub, input, output, x);
							x = xinp[j] + h;
							f2 = EvalModel(oModSub, input, output, x);

							if (option == "Absolute Sensitivity")
							{
								sens[j] = EvalSenAb(f1, f2, h);
							}
							else
							{
								sens[j] = EvalSenRel(f1, f2, f0s[j], xinp[j], h);
								f0s[j + 1] = EvalModel(oModSub, input, output, xinp[j + 1]);
							}

							csvFile.AddToRow(xinp[j]);
							csvFile.AddToRow(sens[j]);
							csvFile.WriteRow();
						}
					}
				} 
			}
			//outputString = output1[0];
			//output_struct = new Treatment_InOut(outputString);
			return true;
		}

		private double EvalSenAb(double fi, double fn, double inter) => (fn - fi) / (2 * inter);

		private double EvalSenRel(double fi, double fn, double fo, double xo, double inter) => EvalSenAb(fi, fn, inter) * xo / fo;

		private double EvalModel(ExecutableComponent component, Data input, Data output, double xo)
		{
			input.ValueAsDouble = xo;
			component.Execute();
			return output.ValueAsDouble;
		}



		#endregion
	}
}
