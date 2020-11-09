/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Models;

namespace Aircadia
{
	[Serializable()]
	////Subclass of the treatment class. This is the treatment class for MOGA.The method 'Execute' calls the C++ MOGA.
	public class MOGAOptimizer : Treatment
    {
		public WorkflowComponent Component { get; private set; }

		private readonly List<BoundedDesignVariableNoInital> designVariables;
		//private readonly List<Parameter> parameters;
		private readonly List<Objective> objectives;
		private readonly List<Constraint> constraints;

		private readonly int Population;
		private readonly int Generations;
		private readonly double CrossoverProbability;
		private readonly double MutationProbability;

		private readonly bool continueFromPreviousResults;

		[DllImport("Moga_final.dll")]
        private static extern int main_enter([MarshalAs(UnmanagedType.LPStr)]string afileloc, [MarshalAs(UnmanagedType.LPStr)]string aName, int aoutCount, int ainCount, double[] inValues, int[] ndesloc, int[] mobjloc, int[] mconsloc, int[] mconsact, double[] mconsval);
        /// <summary>
        /// Constructer
        /// </summary>
        /// <param name="n"></param>
        /// <param name="input_opt"></param>
        /// <param name="outp"></param>
        public MOGAOptimizer(string n) :
            base(n, n)
        {
        }
        /// <summary>
        /// The string returned by this function is the one which will be printed in the properties window of AirCADia for the selected treatment object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = "";

			//foreach (Parameter parameter in parameters)
			//	output += $"{parameter.Name} = {parameter.Value}\r\n";

			foreach (BoundedDesignVariableNoInital variable in designVariables)
				output += $"{variable.LowerBound} <= {variable.Name} <= {variable.UpperBound}\r\n";

			foreach (Objective objective in objectives)
				output += $"{objective.Type}: {objective.Name}\r\n";

			foreach (Constraint constraint in constraints)
			{
				string oper = (constraint.Type == ConstraintType.GreatorThanOrEqual) ? ">="
					: (constraint.Type == ConstraintType.LessThanOrEqual) ? "<="
						: "=";

				output += $"{constraint.Name} {oper} {constraint.Value}\r\n";
			}

			//foreach (InputSet opt_setup in (input_options as Treatment_InOut_OP_Input).setuplist)
   //         {
   //             if (opt_setup is OP_Input_set inputSet)
   //             {
			//		if (inputSet.desobj == "des")
   //                 {
   //                     output = output + Convert.ToString(inputSet.min) + "<=" + inputSet.Data + "<=" + Convert.ToString(inputSet.max) + "\r\n";
   //                 }
   //                 else if (inputSet.desobj == "obj")
   //                 {
   //                     if (inputSet.max == 1)
   //                         output = output + "Maximise:" + inputSet.Data + "\r\n";
   //                     else
   //                         output = output + "Minimise:" + inputSet.Data + "\r\n";
   //                 }
   //             }
   //             else if (opt_setup is OP_C_Input_set cInputSet)
   //             {
			//		if (cInputSet.Constraint == 0)
   //                     output = output + cInputSet.Data + "<=" + cInputSet.value + "\r\n";
   //                 else if (cInputSet.Constraint == 1)
   //                     output = output + cInputSet.Data + ">=" + cInputSet.value + "\r\n";
   //                 else if (cInputSet.Constraint == 2)
   //                     output = output + cInputSet.Data + "=" + cInputSet.value + "\r\n";
   //             }
   //         }

            return output;
        }

		/// <summary>
		/// Executes the treatment
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		public override bool ApplyOn(ExecutableComponent component)
		{
			Component = Component;

			return ApplyOn();
		}

		public override bool ApplyOn()
		{
			int Nvariavles = designVariables.Count;
            int Nobjective = objectives.Count;
			//int Nparameters = parameters.Count;
			int Nequality = constraints.Where(c => c.Type == ConstraintType.Equal).Count();

			// Set equality Constraints
			foreach (Constraint equalityConstraint in constraints.Where(c => c.Type == ConstraintType.Equal))
				equalityConstraint.Data.Value = equalityConstraint.Value;

			//foreach (Parameter parameter in parameters)
			//	parameter.Data.Value = parameter.Value;


			if (!WriteMOGAInput(Nvariavles, Nobjective, Nequality))
                return false;


			// Input Values
            double[] inValues = new double[Component.ModelDataInputs.Count];
            int ncount1 = 0;
            foreach (Data dt in Component.ModelDataInputs)
            {
                if (dt is DoubleData)
                    inValues[ncount1] = dt.GetValueAsDouble();
                else
                    inValues[ncount1] = 1.0;
                //Strings , arrays and other varaibles are supressed as double(1.0) they are not used inside MOGA
                ncount1++;
            }

			// Location of Design Variables
			int[] ndesloc = designVariables.Select(dv => Component.ModelDataInputs.IndexOf(dv.Data)).ToArray();

			// Location of Objectives
			int[] mobjloc = objectives.Select(o => Component.ModelDataOutputs.IndexOf(o.Data)).ToArray();

			// Location of Constraints
			int[] mconsloc = constraints.Select(c => Component.ModelDataOutputs.IndexOf(c.Data)).ToArray();

			// Type of Constraints
			int[] mconsact = constraints.Select(c => c.Type == ConstraintType.LessThanOrEqual ? 0 : 1).ToArray();

			// Value of Constraints
			double[] mconsval = constraints.Select(c => c.Value).ToArray();


			// Call to MOGA in dll
			main_enter("lo.txt", Component.Name, Component.ModelDataOutputs.Count, Component.ModelDataInputs.Count, inValues, ndesloc, mobjloc, mconsloc, mconsact, mconsval);


			// Handle Outputs
			string designvar = designVariables.Aggregate(String.Empty, (t, l) => $"{l.Name}(Var)\t");
			string objvar = objectives.Aggregate(String.Empty, (t, l) => $"{l.Name}(Obj)\t");
			string consstrvar = constraints.Where(c => c.Type != ConstraintType.Equal).Aggregate(String.Empty, (t, l) => $"{l.Name}(Cnstr_violation)\t");

			string output = File.ReadAllText("evaluatedSolutions.txt");
            //////
            string tempfil = Path.GetTempFileName();
            var outfile = new StreamWriter(tempfil);
            outfile.Write(output);
            outfile.Close();

			// Added by Marco: =================================================================================================
			string execpath = Directory.GetCurrentDirectory();

			if (File.Exists(execpath + "\\evaluatedSolutions.txt"))
                File.Delete(execpath + "\\evaluatedSolutions.txt");

            File.Copy("evaluatedSolutions.txt", execpath + "\\evaluatedSolutions.txt");
            // =================================================================================================================

            string readerout;
            output = null;
            using (var sr = new StreamReader(tempfil))
            {
                string reader = sr.ReadLine();
                readerout = designvar+objvar+consstrvar;
                if (reader.Contains("penalty"))
                    readerout = readerout+"penalty"+"\r\n";
                else
                    readerout = readerout +"\r\n";
                while (reader != null)
                {
                    reader = sr.ReadLine();
                    readerout = readerout+reader+"\r\n";
                }
            }
            readerout=readerout.Substring(0, readerout.Length - 2);
            //////
            return true;
        }
		/// <summary>
		/// Writes the input for MOGA into a file. This will be read by the moga_final.dll.
		/// </summary>
		/// <param name="Nvariables"></param>
		/// <param name="desvarnames"></param>
		/// <param name="desvarbounds"></param>
		/// <param name="Nobjectives"></param>
		/// <param name="objectivestring"></param>
		/// <param name="Nparameters"></param>
		bool WriteMOGAInput(int Nvariables, int Nobjectives, int Nparameters)
        {
			var desvarnames = designVariables.Select(dv => dv.Name).ToList();
			string desvarbounds = String.Empty;
			foreach (BoundedDesignVariableNoInital variable in designVariables)
				desvarbounds += $"double {variable.LowerBound} {variable.UpperBound}\r\n";

			string objectiveString = "";
			foreach (Objective objective in objectives)
				objectiveString += (objective.Type == ObjectiveType.Maximise) ? "Max\r\n" : "Min\r\n";

			if (File.Exists("lo.txt"))
				File.Delete("lo.txt");
			var filer = new FileStream("lo.txt", FileMode.OpenOrCreate, FileAccess.Write);
			var sw = new StreamWriter(filer);

			sw.Write(@"# Input file for the GA Toolbox
			//# Author: Kumara Sastry
			//# Date: April, 2006
			//#

			//#
			//# GA type: SGA or NSGA
			//#
			//");
			if (Nobjectives == 1)
				sw.Write("SGA" + "\r\n");
			else
				sw.Write("NSGA" + "\r\n");

			sw.Write(@"#
			# Number of decision variables
			#" + "\r\n");
			sw.Write(Convert.ToString(Nvariables) + "\r\n");
			sw.Write(@"
			#
			# For each decision variable, enter: 
			# 	decision variable type, Lower bound, Upper bound	 
			# Decision variable type can be double or int
			#" + "\r\n");
			sw.Write(desvarbounds);
			sw.Write(@"
			//#
			//# Objectives: 
			//#	Number of objectives
			//#	For each objective enter the optimization type: Max or Min
			//#" + "\r\n");
			sw.Write(Convert.ToString(Nobjectives) + "\r\n");
			sw.Write(objectiveString);
			sw.Write(@"
			#
			# Constraints:
			#	Number of constraints
			#	For each constraint enter a penalty weight 
			#" + "\r\n");
			sw.Write(Convert.ToString(Nparameters) + "\r\n");
			for (int ncount = 0; ncount < Nparameters; ncount++)
				sw.Write("1.0" + "\r\n");
			sw.Write(@"
			#
			# General parameters: If these parameters are not entered default
			#                     values will be chosen. However you must enter 
			#                     \""default\"" in the place of the parameter.
			#	[population size]
			#	[maximum generations]
			#	[replace proportion]
			#
			" + "\r\n");
			sw.Write(Convert.ToString(Population) + "\r\n");  //100
			sw.Write(Convert.ToString(Generations) + "\r\n");  //50
			sw.Write(@"0.9

			//#
			//# Niching (for maintaining multiple solutions)
			//# To use default setting type \""default\""
			//#  Usage: Niching type, [parameter(s)...]
			//#  Valid Niching types and optional parameters are:
			//#	NoNiching
			//#	Sharing [niching radius] [scaling factor]
			//#	RTS [Window size]
			//#	DeterministicCrowding
			//#
			//#  When using NSGA, it must be NoNiching (OFF).
			//#
			//NoNiching

			//#
			//# Selection
			//#  Usage: Selection type, [parameter(s)...]
			//#  To use the default setting type \""default\""
			//#
			//#  Valid selection types and optional parameters are:
			//#	RouletteWheel
			//#	SUS
			//#	TournamentWOR [tournament size]
			//#	TournamentWR [tournament size]
			//#	Truncation [# copies]
			//#
			//#  When using NSGA, it can be neither SUS nor RouletteWheel.
			//#
			//TournamentWOR 2

			//#
			//# Crossover
			//#  Crossover probability
			//#  To use the default setting type \""default\""
			//#
			//#  Usage: Crossover type, [parameter(s)...]
			//#  To use the default crossover method type \""default\""
			//#  Valid crossover types and optional parameters are
			//#	OnePoint
			//#	TwoPoint
			//#	Uniform [genewise swap probability]
			//#	SBX [genewise swap probability][order of the polynomial]
			//#
			//" + "\r\n");
			sw.Write(Convert.ToString(CrossoverProbability) + "\r\n");  //100
			sw.Write(@"SBX 0.5 10

			#
			# Mutation
			#  Mutation probability
			#  To use the default setting type \""default\""
			#
			#  Usage: Mutation type, [parameter(s)...]
			#  Valid mutation types and the optional parameters are:
			#	Selective
			#	Polynomial [order of the polynomial]
			#	Genewise [sigma for gene #1][sigma for gene #2]...[sigma for gene #ell]
			#
			" + "\r\n");
			sw.Write(Convert.ToString(MutationProbability) + "\r\n");  //100
			sw.Write(@"Polynomial 20

			#
			# Scaling method
			#  To use the default setting type \""default\""
			#
			#  Usage: Scaling method, [parameter(s)...]
			#  Valid scaling methods and optional parameters are:
			#	NoScaling
			#	Ranking
			#	SigmaScaling [scaling parameter]
			#
			NoScaling

			#
			# Constraint-handling method
			# To use the default setting type \""default\""
			#
			# Usage: Constraint handling method, [parameters(s)...]
			# Valid constraint handling methods and optional parameters are
			#	NoConstraints
			#	Tournament
			#	Penalty [Linear|Quadratic]
			#
			");
			if (Nparameters == 0)
				sw.Write("NoConstraints" + "\r\n");
			else
				sw.Write("Tournament" + "\r\n");
			sw.Write(@"
			#
			# Local search method
			# To use the default setting type \""default\""
			#
			# Usage: localSearchMethod, [maxLocalTolerance], [maxLocalEvaluations], 
			#		[initialLocalPenaltyParameter], [localUpdateParameter], 
			#		[lamarckianProbability], [localSearchProbability]
			#
			# Valid local search methods are: NoLocalSearch and SimplexSearch
			#
			# For example, SimplexSearch 0.001000 20 0.500000 2.000000 0.000000 0.000000
			NoLocalSearch

			#
			# Stopping criteria
			# To use the default setting type \""default\""
			#
			# Number of stopping criterias
			#
			# If the number is greater than zero
			#    Number of generation window
			#    Stopping criterion, Criterion parameter
			#
			# Valid stopping criterias and the associated parameters are
			#	NoOfEvaluations, Maximum number of function evaluations
			#	FitnessVariance, Minimum fitness variance
			#	AverageFitness, Maximum value
			#	AverageObjective, Max/Min value
			#	ChangeInBestFitness, Minimum change
			#	ChangeInAvgFitness, Minimum change
			#	ChangeInFitnessVar, Minimum change
			#	ChangeInBestObjective, Minimum change
			#	ChangeInAvgObjective, Minimum change
			#	NoOfFronts (NSGA only), Minimum number
			#	NoOfGuysInFirstFront (NSGA only), Minimum number
			#	ChangeInNoOfFronts (NSGA only), Minimum change
			#	BestFitness (SGA with NoNiching only), Maximum value
			#
			0

			#
			# Load the initial population from a file or not
			# To use the default setting type \""default\""
			#
			# Usage: Load population (0|1)
			#
			# For example, if you want random initialization type 0
			# On the other and if you want to load the initial population from a
			# file, type
			# 	1 <population file name> [0|1]
			#
			# Valid options for \""Load population\"" are 0/1
			# If you type \""1\"" you must specify the name of the file to load the
			# population from. The second optional parameter which indicates 
			# whether to evaluate the individuals of the loaded population or not.
			0

			# Save the evaluated individuals to a file 
			#
			# To use default setting type \""default\"".
			#
			# Here by default all evaluated individuals are stored and you will be
			# asked for a file name later when you run the executable.
			# 
			# Usage: Save population (0|1)
			# For example, if you don't want to save the evaluated solutions type 0
			# On the other and if you want to save the evaluated solutions
			# 	1 <save file name>
			#
			# Note that the evaluated solutions will be appended to the file.
			#
			# Valid options for \""Save population\"" are 0/1
			# If you type \""1\"" you must specify the name of the file to save the
			# population to.
			1 evaluatedSolutions.txt


			#END");
			sw.Close();
			filer.Close();

			return true;
		}
    }
}
