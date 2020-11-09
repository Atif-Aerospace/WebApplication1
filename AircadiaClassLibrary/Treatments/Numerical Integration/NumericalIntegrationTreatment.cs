/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using System.Xml.Linq;
using MathNet.Numerics;
using Aircadia.Services.Serializers;

namespace Aircadia
{
	[Serializable()]
	public class NumericalIntegrationTreatment : Treatment
	{
		[Serialize]
		public double X0 { get; }
		[Serialize]
		public double Y0 { get; }
		[Serialize]
		public double Xn { get; }
		[Serialize]
		public double H { get; }
		[Serialize(Type = SerializationType.Reference)]
		public Data Input { get; }
		[Serialize(Type = SerializationType.Reference)]
		public Data Output { get; }
		[Serialize(Type = SerializationType.Reference)]
		public ExecutableComponent Component { get; private set; }

		[DeserializeConstructor]
		public NumericalIntegrationTreatment(string name, ExecutableComponent component, Data input, double x0, double xn, Data output, double y0, double h) : base(name, "")
		{
			X0 = x0;
			Y0 = y0;
			Xn = xn;
			H = h;
			Input = input;
			Output = output;
			Component = component;
		}

		public override string ToString()
		{
			string output = "";
			output = output + "Integrating:  " + Output.Name + "\r\n";
			output = output + "With respect to:  " + Input.Name + "\r\n";
			output = output + "Integration limits: " + X0 + " to " + Xn + "\r\n";
			output = output + "Initial value of  " + Input.Name + "is  :  " + Y0 + "\r\n";
			return output;
		}

		public override bool ApplyOn(ExecutableComponent component)
		{
			Component = component;
			return ApplyOn();
		}

		public override bool ApplyOn()
		{
			using (var csvFile = new CSVFiler(CsvPath))
			{
				//Simple numerical integration using RungeKutta 4th order
				int nsteps = Convert.ToInt32(Math.Ceiling((Xn - X0) / H));

				double[] xi = Generate.LinearSpaced(nsteps, X0, Xn);
				double[] dx = new double[nsteps]; dx[0] = ExecuteComponent(xi[0]);
				double[] yi = new double[nsteps]; yi[0] = Y0;


				//WriteLinetoCSV(csvFile, 0, xi[0], dx[0], yi[0]);

				for (int i = 1; i < nsteps; i++)
				{
					double k1 = ExecuteComponent(xi[i - 1]);
					double k2 = ExecuteComponent(xi[i - 1] + (H / 2));
					double k3 = ExecuteComponent(xi[i - 1] + (H / 2));
					double k4 = ExecuteComponent(xi[i - 1] + H);

					dx[i] = k1;
					yi[i] = yi[i - 1] + (H / 6) * (k1 + 2 * k2 + 2 * k3 + k4);

					//WriteLinetoCSV(csvFile, i, xi[i], dx[i], yi[i]);
				}

				WriteLinetoCSVArray(csvFile, xi, dx, yi);
			}
			

			return true;
		}

		

		private double ExecuteComponent(double x)
		{
			Input.Value = x;
			Component.Execute();
			return Output.GetValueAsDouble();
		}

		private void WriteLinetoCSV(CSVFiler csvFile, int i, double x, double e, double y)
		{
			csvFile.NewRow();

			csvFile.AddToRow(i);
			csvFile.AddToRow(x, 4);
			csvFile.AddToRow(e, 4);
			csvFile.AddToRow(y, 4);

			csvFile.WriteRow();
		}

		private void WriteLinetoCSVArray(CSVFiler csvFile, double[] x, double[] e, double[] y)
		{
			csvFile.NewRow();

			csvFile.AddToRow(0/*IntegerVectorData.ValueToString(i)*/);
			csvFile.AddToRow(DoubleVectorData.ValueToString(x));
			csvFile.AddToRow(DoubleVectorData.ValueToString(e));
			csvFile.AddToRow(DoubleVectorData.ValueToString(y));

			csvFile.WriteRow();
		}

		public override void CreateFolder()
		{
			base.CreateFolder();

			// Add custom metadata for Latin Hypercube
			metadata.AddAttribute("Type", "DesignsStudy");
			//metadata.AddAttribute("WorkflowName", component.Name);

			metadata.AddTag("Name", "Latin Hypercube Sampling");

			metadata.AddParameter(new IntegerData("ID"));

			var dummy = new DoubleVectorData(Input.Name, "", null, (Input as DoubleData).DecimalPlaces, Input.Unit);
			metadata.AddParameter(dummy, new XElement("IntegralFactor",
					new XAttribute("InitialValue", X0),
					new XAttribute("FinalValue", Xn),
					new XAttribute("Step", H)));

			dummy = new DoubleVectorData($"d{Output.Name}/d{Input.Name}", "", null, (Output as DoubleData).DecimalPlaces, Output.Unit);
			metadata.AddParameter(dummy, new XElement("IntegralResponse"));

			dummy = new DoubleVectorData(Output.Name, "", null, (Output as DoubleData).DecimalPlaces, Output.Unit);
			metadata.AddParameter(dummy, new XElement("IntegralResponse", new XAttribute("InitialValue", X0)));

			metadata.Save();
		}
	}

}