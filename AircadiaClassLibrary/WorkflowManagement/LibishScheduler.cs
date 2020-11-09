using System;
using System.Collections.Generic;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;
using System.Diagnostics;
using Aircadia.ObjectModel.Workflows;
using Aircadia.Numerics.Solvers;
using static System.Math;
using static Aircadia.WorkflowManagement.LibishScheduler;

namespace Aircadia.WorkflowManagement
{
	using Cluster = List<int>;
	using IncidenceMatrix = Matrix<double>;
	using DesignStructureMatrix = Matrix<double>;

	[Serializable()]
	////This class defines a collections of static methods (mainly mathematical and matrix operations) which can be called from other files.
	public class LibishScheduler
	{
		// References
		// [1]	L. K. Balachandran and M. D. Guenov, “Computational Workflow Management for Conceptual Design of Complex Systems,” 
		//		J. Aircr., vol. 47, no. 2, pp. 699–703, Mar. 2010, doi:10.2514/1.43473.
		// [2]	L.K.Balachandran, “Computational Workflow Management for Conceptual Design of Complex Systems: An Air-vehicle Design Perspective,” 2007.

		public const int NOTHING = 0;
		public const int ANY = 1;
		public const int IN = 2;
		public const int OUT = 3;

		public const int MARKED_IN = 1;
		public const int MARKED_OUT = 2;

		// Options
		private const int MaxWorkflowAlternatives = 1000;
		private const int MaxEllapsedTime = 60 * 1000;

		private const string NoIMsFoundErrorMessage = "The selected combination of inputs was not able to produce any valid incidence matrix (variable-model mathing)";
		private static Stopwatch explorationWatch = new Stopwatch();

		/// <summary>
		/// Given the Model Objects, data objects and the vector(vIndep) in which '1' represent an independent variables , this function creates 
		/// a subprocess after performing the computational proces modelling
		/// </summary>
		/// <param name="name"></param>
		/// <param name="components"></param>
		/// <param name="dataObjects"></param>
		/// <param name="independent"></param>
		/// <returns></returns>
		public static Workflow ScheduleWorkflow(string name, string description, List<Data> inputs, List<Data> outputs, List<WorkflowComponent> components)
		{
			explorationWatch.Restart();
			List<Data> data = components.GetAllData(inputs, out int[] independent);

			// Incidence matrices for the workfow
			IncidenceMatrix IMf = LibishMatrixCalculators.GetIncidenceMatrix(components, data);
			IncidenceMatrix IMi = VariableFlowModelling(IMf, independent);
			// Model-based DSM
			DesignStructureMatrix DSM = IMMtoDSM(IMi);
			// Locates SCCs
			List<Cluster> clusters = Decompose(DSM);
			// SCC-based incidence matrix
			IncidenceMatrix IMU = CreateUpMatrix(IMi, clusters);
			// SCC-based DSM
			DesignStructureMatrix DSMU = IMMtoDSM(IMU);
			// Orders the models, so there is only feed-forward
			List<int> order = SequenceDsm(DSMU);
			// Creates the workflows for the reversed models
			WorkflowComponent[] processedComponents = CreateReversedModels(IMi, IMf, components, data, clusters, name);
			// Creates the workflows for the SCCs (default order)
			List<WorkflowComponent> clusteredComponents = CreateSCCs(processedComponents, clusters, data, name);
			// Applies the order (vDsmArr) to the models
			List<WorkflowComponent> scheduledComponents = Schedule(clusteredComponents, order);
			// Enables dependency analysis
			var dependencyAnalysis = new MatrixBasedDependencyAnalysis(processedComponents);

			return new Workflow(name, description, inputs, outputs, components, scheduledComponents) { DependencyAnalysis = dependencyAnalysis };
		}

		/// <summary>
		/// Reschedules the elements in vResultSub based on the sorting in vDsmArr
		/// </summary>
		/// <param name="components"></param>
		/// <param name="secuence"></param>
		private static List<WorkflowComponent> Schedule(List<WorkflowComponent> components, List<int> secuence)
		{
			secuence.Reverse();
			return secuence.Select(i => components[i]).ToList();
		}

		/// <summary>
		/// This function identifies the SCC (from vClus) in the vResultMod, then groups it and therafter combines it with other models in the vResultMod and return it as a cWfmCollection
		/// </summary>
		/// <param name="components"></param>
		/// <param name="clusters"></param>
		/// <param name="data"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private static List<WorkflowComponent> CreateSCCs(WorkflowComponent[] components, List<Cluster> clusters, List<Data> data, string name)
		{
			var vResultSub = new List<WorkflowComponent>();
			for (int c = 0; c < clusters.Count; c++)
			{
				Cluster cluster = clusters[c];
				if (cluster.Count == 1)
				{
					vResultSub.Add(components[cluster[0]]);
				}
				else
				{
					var sccComponents = new List<WorkflowComponent>();
					foreach (int r in cluster)
					{
						sccComponents.Add(components[r]);
					}
					(List<Data> inputs, List<Data> outputs) = LibishMatrixCalculators.GetInputOutputs(sccComponents, data);

					// Set inputs and outputs of this scc to 
					var solvers = new List<ISolver>() { new FixedPointSolver() };
					string sccName = Workflow.GetSCCWorkflowName(name, components, sccComponents);
					var scc2 = new WorkflowSCC(sccName, "", inputs, outputs, sccComponents, sccComponents, solvers);

					vResultSub.Add(scc2);
				}
			}
			return vResultSub;
		}
		/// <summary>
		/// This function identifies the Modified models (from imMatrixi and imMatrif) in the modelObjects, then creates a 'modified model subprocess' for each modified model, 
		/// and therafter combines it with other models in the modelObjects and return it in a object array
		/// </summary>
		/// <param name="IMi"></param>
		/// <param name="IMf"></param>
		/// <param name="components"></param>
		/// <param name="data"></param>
		/// <param name="clusters"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		private static WorkflowComponent[] CreateReversedModels(IncidenceMatrix IMi, IncidenceMatrix IMf, List<WorkflowComponent> components, List<Data> data, List<Cluster> clusters, string name)
		{
			var processedComponents = new WorkflowComponent[IMf.RowCount];
			for (int r = 0; r < IMi.RowCount; r++)
			{
				WorkflowComponent component = components[r];
				Vector<double> row = IMi.Row(r);
				if (!IMf.Row(r).CompareAssigned(row))
				{
					List<int> inputIndices = row.FindLocations(IN);
					List<int> outputIndices = row.FindLocations(OUT);
					var inputs = inputIndices.Select(i => data[i]).ToList();
					var outputs = outputIndices.Select(i => data[i]).ToList();
					
					if (component is Model model)
					{
						processedComponents[r] = model.Reverse(inputs, outputs);
					}
					else if (component is Workflow workflow)
					{
						var opts = new NewtonOptions() { MaxIterations = 20, DerivativeStep = new double[] { 0.01 } };
						var solver = new NewtonSolver(opts);
						processedComponents[r] = new WorkflowGlobal($"{name}#GlobalWorkflow#{workflow.Name}", "", inputs, outputs, workflow.Components, workflow.Components, new List<ISolver>() { solver });
					}
				}
				else
				{
					processedComponents[r] = components[r];
				}
			}
			return processedComponents;
		}
		

		/// <summary>
		/// Identifies the sequence for the Dsm for which its contents become all feed forward.
		/// The method is described in detail  in reference [2] chapter 3.2.3.2.
		/// </summary>
		/// <param name="DSM"></param>
		/// <returns></returns>
		private static List<int> SequenceDsm(DesignStructureMatrix DSM)
		{
			int[] sequencedDSM = new int[DSM.ColumnCount];
			DesignStructureMatrix E0 = DesignStructureMatrix.Build.Dense(DSM.ColumnCount, 1, 1);
			int nLoc = 0;
			while (nLoc < DSM.ColumnCount)
			{
				DesignStructureMatrix PE0 = DSM * E0;
				List<int> locations = PE0.Column(0).FindLocations(1);
				for (int l = 0; l < locations.Count; l++)
				{
					sequencedDSM[nLoc] = locations[l];
					nLoc++;
				}
				E0 = PE0;
				E0.ReplaceLessthanWith(2, 0);
				E0.ReplaceGreaterthanWith(1, 1);
			}
			return sequencedDSM.ToList();
		}
		/// <summary>
		/// creates the new incidence matrix based on the SCCs. SCCs are clustered in the new DSM in a single row.
		/// </summary>
		/// <param name="IM"></param>
		/// <param name="clusters"></param>
		/// <returns></returns>
		private static IncidenceMatrix CreateUpMatrix(IncidenceMatrix IM, List<Cluster> clusters)
		{
			IncidenceMatrix imMatrixU = IncidenceMatrix.Build.Dense(clusters.Count, IM.ColumnCount);
			for (int r = 0; r < clusters.Count; r++)
			{
				Cluster cluster = clusters[r];
				if (cluster.Count == 1)
				{
					imMatrixU.SetRow(r, IM.Row(cluster[0]));
				}
				else
				{
					IncidenceMatrix imMatrixT = IncidenceMatrix.Build.Dense(cluster.Count, IM.ColumnCount);
					int nvC = 0;
					foreach (int nRow in cluster)
					{
						imMatrixT.SetRow(nvC, IM.Row(nRow));
						nvC++;
					}
					for (int c = 0; c < IM.ColumnCount; c++)
					{
						Vector<double> column = imMatrixT.Column(c);
						int Nin = column.FindValue(IN);
						int Nout = column.FindValue(OUT);
						if (Nin > 0 && Nout > 0)
						{
							imMatrixU[r, c] = OUT;
						}
						else if (Nout > 0)
						{
							imMatrixU[r, c] = OUT;
						}
						else if (Nin > 0)
						{
							imMatrixU[r, c] = IN;
						}
						else
						{
							imMatrixU[r, c] = NOTHING;
						}
					}

				}

			}
			return imMatrixU;
		}
		/// <summary>
		/// Performs the varaible flow modelling given the foundation IM and the vector representing the independent variable.
		/// </summary>
		/// <param name="IMf"></param>
		/// <param name="independent"></param>
		/// <returns></returns>
		private static IncidenceMatrix VariableFlowModelling(IncidenceMatrix IMf, int[] independent)
		{
			IncidenceMatrix[] IMSet = AllWorkflows(IMf, independent);
			int LeastModified = LibishScheduler.LeastModified(IMSet, IMf);
			return IMSet[LeastModified];
			// code for genetic algorithm

		}
		/// <summary>
		/// Identifies SCC in DSM (read chapter 3.2.2 in reference [2]), this information is passed back in the object array vClus.
		/// </summary>
		/// <param name="DSM"></param>
		/// <returns></returns>
		private static List<Cluster> Decompose(DesignStructureMatrix DSM)
		{
			DesignStructureMatrix DSMt = DesignStructureMatrix.Build.Dense(DSM.ColumnCount, DSM.ColumnCount);
			DesignStructureMatrix DSMmul = DesignStructureMatrix.Build.DenseIdentity(DSM.ColumnCount, DSM.ColumnCount);
			for (int c = 0; c < DSM.ColumnCount; c++)
			{
				DSMmul = DSM * DSMmul;
				DSMmul.ReplaceGreaterthanWith(1, 1);
				DSMt = DSMt + DSMmul;
			}
			DSMt = DSMt.PointwiseMultiply(DSMt.Transpose());
			DSMt.ReplaceGreaterthanWith(1, 1);
			return GatherClusters(DSMt);
		}
		/// <summary>
		/// This function reads through the dsm vDsmt identifies the identical rows (these are sccs ) and pass back this information(in an object array format) to the calling function
		/// </summary>
		/// <param name="DSM"></param>
		/// <returns></returns>
		private static List<Cluster> GatherClusters(DesignStructureMatrix DSM)
		{
			//gathers the clusters
			var clusterPresent = new List<int>();
			var clusters = new List<Cluster>(DSM.RowCount);
			for (int c = 0; c < DSM.ColumnCount; c++)
			{
				var clusterlist = new Cluster();
				if (!clusterPresent.Contains(c))
				{
					clusterPresent.Add(c);
					clusterlist.Add(c);
					for (int r = 0; r < DSM.RowCount; r++)
					{
						if (c == r)
						{
							continue;
						}

						if (DSM.Row(c).Equals(DSM.Row(r)))
						{
							clusterlist.Add(r);
							clusterPresent.Add(r);
						}

					}
					clusters.Add(clusterlist);
					clusterlist = null;
				}
			}
			return clusters;
		}
		/// <summary>
		/// Given a collection of incidence matrices this function chooses the one which has least number of modified models
		/// </summary>
		/// <param name="IMSet"></param>
		/// <param name="IMf"></param>
		/// <returns></returns>
		private static int LeastModified(IncidenceMatrix[] IMSet, IncidenceMatrix IMf)
		{
			var IndexAndReversals = IMSet.Select((m, i) => new { Index = i, NReversed = IMf.Compare(m) });
			return IndexAndReversals.OrderBy(m => m.NReversed).First().Index;
		}
		/// <summary>
		/// Given a single incidence matrix this function convertes it to a dsm and returns it.
		/// </summary>
		/// <param name="IMM"></param>
		/// <returns></returns>
		private static DesignStructureMatrix IMMtoDSM(IncidenceMatrix IMM) => LibishMatrixCalculators.IncidenceMatrixToDesignStructureMatrix(IMM);
		/// <summary>
		/// This funnction returns all possible workflows in an incidence matrix format, given the foundation matrix 
		/// as well as the vIndep vector which represent the independent variables.
		/// This function is the top level function which performs the incidence matrix method.
		/// </summary>
		/// <param name="IMf"></param>
		/// <param name="independent"></param>
		/// <returns></returns>
		private static IncidenceMatrix[] AllWorkflows(IncidenceMatrix IMf, int[] independent)
		{
			IncidenceMatrix IMi = IncidenceMatrix.Build.DenseOfMatrix(IMf);
			// Number of outputs per row (affect model r)
			IMi.ReplaceAlltoOne();

			// valrf & valcf in [1]
			double[] Noutvalr = new double[IMf.RowCount];
			for (int model = 0; model < IMf.RowCount; model++)
			{
				int NOutputs = IMf.NOutputs(model);
				int NVariables = IMi.NVariables(model); // Number of variables affecting model r
				int NInputs = (NVariables - NOutputs);
				Noutvalr[model] = ValRow(NOutputs, NInputs);
			}
			double[] Noutvalc = new double[IMf.ColumnCount];
			for (int c = 0; c < IMf.ColumnCount; c++)
			{
				int NModels = IMi.Column(c).FindValue(ANY); // Number of models related to variable c 
				int NInputs = (NModels - 1);
				Noutvalc[c] = ValColumn(NInputs);
			}

			// Assign user defined inputs/outputs
			for (int variable = 0; variable < IMi.ColumnCount; variable++)
			{
				if (independent[variable] == MARKED_IN)
				{
					IMi.SetInput(variable); // Asign variable as input in IMi
				}
				else if (independent[variable] == MARKED_OUT)
				{
					IMi.SetOutput(variable, IMf);
				}
			}

			List<int> notAssigned = FindNotAssignedModels(IMi, IMf);
			if (notAssigned.Count == 0)
			{
				return new IncidenceMatrix[1] { IMf };
			}

			IncidenceMatrixFirstStep(IMi, Noutvalr, Noutvalc, true);
			FillAsInput(IMi, Noutvalr, Noutvalc);
			IncidenceMatrixFirstStep(IMi, Noutvalr, Noutvalc, true);

			////////////////////////

			List<int> reversed = FindReversedModels(IMi, IMf);
			bool reversedModelsInSCC = false;
			List<int> modelsInScc = ModelsInSCCFromIMM(IMi);
			foreach (int model in modelsInScc)
			{
				if (FindValueInArray(reversed, model)) // if ith model is in SCC and reversed then break
				{
					reversedModelsInSCC = true;
					break;
				}
			}

			if (!reversedModelsInSCC) //i.e. no models in SCC are modified/reversed
			{
				if (IMi.FindLocations(ANY).Count != 0)
				{
					// copy default SCC inputs and outputs to new incidence matrix, imMatrixi
					IMi.CopyDefaultInputs(IMf, modelsInScc);
				}
				return new IncidenceMatrix[1] { IMi };
			}
			else
			{
				// When clicking 'edit' then get incidence before it has been modified or if creating new object, then get default incidence
				DesignStructureMatrix DSM = IMMtoDSM(IMf);
				modelsInScc.Clear();
				List<Cluster> clusters = Decompose(DSM);
				foreach (Cluster cluster in clusters)
				{
					if (cluster.Count > 1)
					{
						modelsInScc.AddRange(cluster);
					}
				}
				reversedModelsInSCC = false;
				foreach (int model in modelsInScc)
				{
					// foreach model in SCC is any model...
					if (FindValueInArray(reversed, model)) // if ith model is in SCC and reversed then break
					{
						reversedModelsInSCC = true;
						break;
					}
				}

				if (!reversedModelsInSCC) //i.e. copy existing SCC configurations
				{
					IMi.CopyDefaultInputs(IMf, modelsInScc);
				}

			}

			////////////////////

			if (IMi.FindLocations(ANY).Count > 0)
			{
				return IncidenceExplore(IMi, IMf, Noutvalr, Noutvalc);	
				//incmset=uniqueincmset(incmset);Unque object has to be added here....
			}
			else
			{
				return new IncidenceMatrix[1] { IMi };
			}

			throw new ArgumentException(NoIMsFoundErrorMessage);
		}

		private static double ValColumn(int NInputs) => Pow(IN, NInputs) * OUT;
		private static double ValRow(int NOutputs, int NInputs) => Pow(OUT, NOutputs) * Pow(IN, NInputs);

		private static List<int> ModelsInSCCFromIMM(IncidenceMatrix IM)
		{
			var modelsInSCC = new List<int>();
			for (int r = 0; r < IM.RowCount; r++)
			{
				if (FindValueInArray(IM.Row(r).ToArray(), ANY))
				{
					modelsInSCC.Add(r);
				}
			}
			return modelsInSCC;
		}

		private static bool FindValueInArray(double[] arrayToSearch, double value) => arrayToSearch.Where(d => d == value).Count() > 0;

		private static bool FindValueInArray(IEnumerable<int> arrayToSearch, int value) => arrayToSearch.Where(d => d == value).Count() > 0;

		/// <summary>
		/// This function solves the incidence matrix which remains unsolved after
		/// applying the first step of Imm ( mIncidenceMatrixFirst)
		/// the outputs of certain models are considered as determined for solving
		/// purpose.
		/// </summary>
		/// <param name="IMi"></param>
		/// <param name="IMf"></param>
		/// <param name="Noutvalr"></param>
		/// <param name="Noutvalc"></param>
		/// 
		/// <returns></returns>
		private static IncidenceMatrix[] IncidenceExplore(IncidenceMatrix IMi, IncidenceMatrix IMf, double[] Noutvalr, double[] Noutvalc)
		{
			return IncidenceExploreRecurse(IMi, IMf, Noutvalr, Noutvalc, new List<IncidenceMatrix>(1000), Int32.MaxValue).ToArray();
		}

		private static List<IncidenceMatrix> IncidenceExploreRecurse(IncidenceMatrix IMi, IncidenceMatrix IMf, double[] Noutvalr, double[] Noutvalc, List<IncidenceMatrix> IMlist, double minNReversed)
		{
			var IMset = new List<IncidenceMatrix>(1000);

			List<int> notAssigned = FindNotAssignedModels(IMi, IMf);
			List<int> reversed = FindReversedModels(IMi, IMf);
			if (reversed.Count == 0)
			{
				reversed = new HashSet<int>(IMi.FindLocations(ANY).Select(l => l.i)).ToList();
			}

			for (int rev = 0; rev < reversed.Count; rev++)
			{
				int r = reversed[rev];
				Vector<double> row = IMi.Row(r);
				List<int> locations = row.FindLocations(ANY);
				double currentValr = row.MultiplyNonZeroVector();
				double Cvalr = Noutvalr[r] / currentValr;
				for (int l = 0; l < locations.Count; l++)
				{
					int c = locations[l];
					double currentValc = IMi.Column(c).MultiplyNonZeroVector();
					double Cvalc = Noutvalc[c] / currentValc;

					double R2 = Log(Cvalr) / Log(IN); 
					double C2 = Log(Cvalc) / Log(IN); 
					if (!IsInteger(C2) && !IsInteger(R2))
					{
						IncidenceMatrix IMg = IncidenceMatrix.Build.DenseOfMatrix(IMi);
						IMg[r, c] = OUT;
						IncidenceMatrixFirstStep(IMg, Noutvalr, Noutvalc, true);
						FillAsInput(IMg, Noutvalr, Noutvalc);

						if (IMg.FindLocations(ANY).Count == 0)
						{
							int NRevModels = FindNReversed(IMi, IMg);
									
							if (NRevModels < minNReversed)
							{
								IMlist.Clear();
								IMlist.Add(IMg);
								minNReversed = NRevModels;
							}
							else if (NRevModels == minNReversed)
							{
								IMlist.Add(IMg);
							}
							else
							{
								// do nothing as inspected incidence already has more rev models than those stored.
							}

							if (IMlist.Count >= MaxWorkflowAlternatives || explorationWatch.ElapsedMilliseconds > MaxEllapsedTime)
							{
								return IMlist;
							}
						}
						else
						{ 
							IncidenceExplore(IMg, IMf, Noutvalr, Noutvalc);
						}
					}
				}
			}

			if (IMlist.Count == 0)
			{
				throw new ArgumentException(NoIMsFoundErrorMessage);
			}

			return IMlist;
		}

		private static int FindNReversed(IncidenceMatrix IMdef, IncidenceMatrix IM)
		{
			int noOfRevModels = 0;
			for (int i = 0; i < IM.RowCount; i++)
			{
				for (int j = 0; j < IM.ColumnCount; j++)
				{
					int defaultCellValue = Convert.ToInt32(IMdef[i, j]);
					int revInciValue = Convert.ToInt32(IM[i, j]);
					if (revInciValue != NOTHING && defaultCellValue != revInciValue && revInciValue != ANY)
					{
						// therefore model i, must be reversed 
						noOfRevModels++;
						break;
					}
				}
			}
			return noOfRevModels;
		}

		private static bool CompareTwoModelsIfModified(Vector<double> rowf, Vector<double> rowi) => !LibishMatrixCalculators.CompareAssigned(rowf, rowi);


		/// <summary>
		/// Given a matrix incm, given an intial completely populated real matrix incmreal
		/// changeval are those rows of incm where the models associated varaible are
		/// modified
		/// </summary>
		/// <param name="IMi"></param>
		/// <param name="IMf"></param>
		/// <returns></returns>
		private static List<int> FindReversedModels(IncidenceMatrix IMi, IncidenceMatrix IMf)
		{
			var reversedRows = new List<int>();

			for (int model = 0; model < IMi.RowCount; model++)
			{
				if (IMi.IsModelReversed(model, IMf))
				{
					reversedRows.Add(model);
				}
			}
			return reversedRows;
		}
		/// <summary>
		/// Given a matrix incm, given an intial completely populated real matrix incmreal
		/// changeval are those rows of incm where the models associated varaible are
		/// modified
		/// </summary>
		/// <param name="IMi"></param>
		/// <param name="IMf"></param>
		/// <returns></returns>
		private static List<int> FindNotAssignedModels(IncidenceMatrix IMi, IncidenceMatrix IMf)
		{
			var notAssignedRows = new List<int>();

			for (int model = 0; model < IMi.RowCount; model++)
			{
				if (IMi.IsModelUnassigned(model))
				{
					notAssignedRows.Add(model);
				}
			}
			return notAssignedRows;
		}
		/// <summary>
		/// This function adds additional varaibles as indpendent varaibles. This is done when the system is underdetermined and 
		/// additional variables are required to solve it. 
		/// </summary>
		/// <param name="IM"></param>
		/// <param name="Noutvalr"></param>
		/// <param name="Noutvalc"></param>
		private static void FillAsInput(IncidenceMatrix IM, double[] Noutvalr, double[] Noutvalc)
		{
			List<(int i, int j)> locations = IM.FindLocations(ANY);
			foreach ((int row, int col) in locations)
			{
				//% fills the ANYs with IN if IN has to be put in row and in column
				//% this make the variable in the column to be an input
				double currentValr = IM.Row(row).MultiplyNonZeroVector();
				double currentValc = IM.Column(col).MultiplyNonZeroVector();
				// Coefficients in equations (1) to (4) in reference [1] 
				double Cvalr = Noutvalr[row] / currentValr;
				double Cvalc = Noutvalc[col] / currentValc;
				//checking for rows
				double R2 = Log(Cvalr) / Log(IN);
				double C3 = Log(Cvalc) / Log(OUT);

				if (IsInteger(R2) && IsInteger(C3)) // D1 in Fig. 1
				{
					IM[row, col] = IN;
				}
			}
		}
		/// <summary>
		/// This is the first step of incidence matrix method. This function performs the operation described in chapter3.2.1 in [1]
		/// </summary>
		/// <param name="IM"></param>
		/// <param name="Noutvalr"></param>
		/// <param name="Noutvalc"></param>
		/// <param name="Activat"></param>
		private static void IncidenceMatrixFirstStep(IncidenceMatrix IM, double[] Noutvalr, double[] Noutvalc, bool Activat)
		{
			IncidenceMatrix IMstart;
			do
			{
				IMstart = IncidenceMatrix.Build.DenseOfMatrix(IM);
				var locations = IM.FindLocations(ANY);
				foreach ((int row, int col) in locations)
				{
					double currentValr = IM.Row(row).MultiplyNonZeroVector();
					double currentValc = IM.Column(col).MultiplyNonZeroVector();
					// Coefficients in equations (1) to (4) in reference [1] 
					double Cvalr = Noutvalr[row] / currentValr;
					double Cvalc = Noutvalc[col] / currentValc;
					//checking for rows
					double R2 = Log(Cvalr) / Log(IN);
					double R3 = Log(Cvalr) / Log(OUT);
					double C2 = Log(Cvalc) / Log(IN);
					double C3 = Log(Cvalc) / Log(OUT);

					if (IsInteger(R2) && !IsInteger(C3)) // D1 in Fig. 1
					{
						IM[row, col] = IN;
					}
					else if (IsInteger(R3) && !IsInteger(C2)) // D2 in Fig. 1
					{
						IM[row, col] = OUT;
					}
					else if (IsInteger(C2) && !IsInteger(R3)) // D3 in Fig. 1
					{
						IM[row, col] = IN;
					}
					else if (IsInteger(C3) && !IsInteger(R2))
					{
						if (!Activat && (IM.Column(col).FindAnyValue() > 1))
							IM[row, col] = OUT;
						else if (Activat)
							IM[row, col] = OUT;
					}
				}
			} while (!IMstart.Equals(IM));
		}

		private static bool IsInteger(double d) => Abs(Truncate(d) - d) < 1e-15;
	}

	internal static class IncidenceMatrixExtensions
	{
		public static int NVariables(this IncidenceMatrix IM, int model) => IM.Row(model).FindValue(ANY);
		public static int NInputs(this IncidenceMatrix IM, int model) => IM.Row(model).FindValue(IN);
		public static int NOutputs(this IncidenceMatrix IM, int model) => IM.Row(model).FindValue(OUT);
		public static void SetInput(this IncidenceMatrix IM, int variable) => IM.MultiplyColumn(variable, IN);
		public static void SetOutput(this IncidenceMatrix IM, int variable, IncidenceMatrix IMf)
		{
			Cluster locs = IMf.Column(variable).FindLocations(OUT);
			if (locs.Count > 0)
			{
				int r = locs.First();
				IM[r, variable] = OUT; // Asign variable as output of the firs model that has it as a default output
			}
			IM.MultiplyColumn(variable, IN);
		}
		public static bool IsModelReversed(this IncidenceMatrix IM, int model, IncidenceMatrix IMf) => !LibishMatrixCalculators.CompareAssigned(IMf, IM, model);
		public static bool IsModelUnassigned(this IncidenceMatrix IM, int model) => IM.Row(model).FindValue(ANY) > 0;
		public static void CopyDefaultInputs(this DesignStructureMatrix IM, DesignStructureMatrix IMf, Cluster models)
		{
			for (int r = 0; r < IMf.RowCount; r++)
			{
				if (models.Contains(r))
				{
					IM.SetRow(r, IMf.Row(r));
				}
			}
		}
	}

	public static class LibishMatrixCalculators
	{
		public static IncidenceMatrix GetIncidenceMatrix(Workflow workflow) => GetIncidenceMatrix(workflow.GetAllComponents(), workflow.GetAllData());
		/// <summary>
		/// 
		/// </summary>
		/// <param name="models"></param>
		/// <param name="dataObjects"></param>
		/// <returns></returns>
		public static IncidenceMatrix GetIncidenceMatrix(List<WorkflowComponent> models, List<Data> dataObjects)
		{
			if (models.Count > 0 && dataObjects.Count > 0)
			{
				IncidenceMatrix IM = IncidenceMatrix.Build.Dense(models.Count, dataObjects.Count);

				// Initialiase dictionary set with variable names and indices
				Dictionary<string, int> dataIndices = GetDataIndices(dataObjects);

				for (int m = 0; m < models.Count; m++)
				{
					IEnumerable<string> inputNames = models[m].ModelDataInputs.Select(d => d.Id);
					foreach (string name in inputNames)
						IM[m, dataIndices[name]] = LibishScheduler.IN;

					IEnumerable<string> outputNames = models[m].ModelDataOutputs.Select(d => d.Id);
					foreach (string name in outputNames)
						IM[m, dataIndices[name]] = LibishScheduler.OUT;
				}

				return IM;
			}
			else
			{
				return null;
			}
		}

		public static Dictionary<string, int> GetDataIndices(Workflow workflow) => GetDataIndices(workflow.GetAllData());

		public static Dictionary<string, int> GetDataIndices(List<Data> dataObjects)
		{
			var dataIndices = new Dictionary<string, int>();
			for (int i = 0; i < dataObjects.Count; i++)
				dataIndices.Add(dataObjects[i].Id, i);
			return dataIndices;
		}

		/// <summary>
		/// Converts an incidence matrix to design structure matrix.
		/// </summary>
		/// <param name="IM"></param>
		/// <returns></returns>
		public static DesignStructureMatrix IncidenceMatrixToDesignStructureMatrix(IncidenceMatrix IM)
		{
			DesignStructureMatrix DSM = IncidenceMatrix.Build.DenseIdentity(IM.RowCount, IM.RowCount);
			List<(int i, int j)> locRowCol = IM.FindLocations(LibishScheduler.OUT);
			foreach ((int row, int col) in locRowCol)
			{
				List<int> inputLocaltions = IM.Column(col).FindLocations(LibishScheduler.IN);
				if (inputLocaltions != null)
				{
					foreach (int i in inputLocaltions)
						DSM[row, i] = LibishScheduler.ANY;
				}
			}
			return DSM;
		}
		/// <summary>
		/// retruns the inputs and output data objects for the given modelobjects collection
		/// </summary>
		/// <param name="components"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static (List<Data> inputs, List<Data> outputs) GetInputOutputs(List<WorkflowComponent> components, List<Data> data)
		{
			var (inputs, outputs, _) = components.GetInputsOutputsStatus(data);
			return (inputs, outputs);
		}
		/// <summary>
		/// replaces values in vMatrix whic are > vWhat with vWith
		/// </summary>
		/// <param name="M"></param>
		/// <param name="limit"></param>
		/// <param name="value"></param>
		public static void ReplaceGreaterthanWith(this IncidenceMatrix M, int limit, int value) => M.MapInplace(d => (d > limit) ? value : d);
		/// <summary>
		/// replaces values in vMatrix whic are %lt; vWhat with vWith
		/// </summary>
		/// <param name="vMatrix"></param>
		/// <param name="vWhat"></param>
		/// <param name="vWith"></param>
		public static void ReplaceLessthanWith(this IncidenceMatrix M, int limit, int value) => M.MapInplace(d => (d < limit) ? value : d);
		/// <summary>
		/// Returns the unique vector
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static int[] Unique(this int[] v)
		{
			var unique = new HashSet<int>(v);
			return unique.ToArray();
		}
		/// <summary>
		/// Multiplies  non zero values of a vector(vVector) and returns it as double
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static double MultiplyNonZeroVector(this Vector<double> v)
		{
			double product = 1;
			for (int i = 0; i < v.Count; i++)
			{
				if (!v[i].Equals(0))
					product *= v[i];
			}

			return product;
		}
		/// <summary>
		/// Multiplies a value(vValue) to a column(vColNum) of the Matrix(vMatrix)
		/// </summary>
		/// <param name="M"></param>
		/// <param name="column"></param>
		/// <param name="value"></param>
		public static void MultiplyColumn(this Matrix<double> M, int column, int value) => M.SetColumn(column, M.Column(column) * value);
		/// <summary>
		/// Compares two Matrices for modified models.
		/// Returns the number of modified models
		/// </summary>
		/// <param name="M1"></param>
		/// <param name="M2"></param>
		/// <returns></returns>
		public static int Compare(this Matrix<double> M1, Matrix<double> M2)
		{
			int modelsReversed = 0;
			for (int i = 0; i < M1.RowCount; i++)
				if (!CompareAssigned(M1.Row(i), M2.Row(i)))
					modelsReversed++;
			return modelsReversed;
		}
		/// <summary>
		/// Compares two vectors only for the values greater than one.
		/// Returns true or false
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public static bool CompareAssigned(this Vector<double> v1, Vector<double> v2)
		{
			for (int i = 0; i < v1.Count; i++)
			{
				if (v1[i] != v2[i] && v2[i] != ANY)
					return false;
			}

			return true;
		}
		public static bool CompareAssigned(this IncidenceMatrix M1, IncidenceMatrix M2, int row)
		{
			for (int i = 0; i < M1.ColumnCount; i++)
			{
				if (M1[row, i] != M2[row, i] && M2[row, i] != ANY)
					return false;
			}

			return true;
		}
		/// <summary>
		/// finds the 'value' in 'vMatrix'
		/// </summary>
		/// <param name="v"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int FindValue(this Vector<double> v, int value) => v.Where(d => d == value).Count();
		/// <summary>
		/// finds the 'value' in 'vMatrix' and returns the location
		/// </summary>
		/// <param name="v"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static List<int> FindLocations(this Vector<double> v, int value) => v.MapIndexed((i, d) => (d == value) ? i : -1.0).Where(d => d != -1.0).Select(d => Convert.ToInt32(d)).ToList();
		//int nvFoundValues = 0;
		//for (int i = 0; i < v.Count; i++)
		//{
		//	if (v[i] == value)
		//	{
		//		foundIndices[nvFoundValues] = i;
		//		nvFoundValues++;
		//	}
		//}
		/// <summary>
		/// finds the 'value' in 'vMatrix' and returns the total number
		/// </summary>
		/// <param name="v"></param>
		/// <returns></returns>
		public static int FindAnyValue(this Vector<double> v) => v.Where(d => d > 0).Count();
		/// <summary>
		/// finds the 'value' in 'vMatrix' and return it in an double Array
		/// </summary>
		/// <param name="M"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static List<(int i, int j)> FindLocations(this Matrix<double> M, int value)
		{
			Matrix<double> viFoundValues = Matrix<double>.Build.Dense(M.ColumnCount * M.RowCount, 2);
			var foundIndices = new List<(int i, int j)>();
			for (int i = 0; i < M.RowCount; i++)
				for (int j = 0; j < M.ColumnCount; j++)
					if (M[i, j] == value)
						foundIndices.Add((i, j));

			return foundIndices;
		}
		/// <summary>
		/// replaces non-zero values in the vMatrix with one.
		/// </summary>
		/// <param name="M"></param>
		public static void ReplaceAlltoOne(this Matrix<double> M) => M.MapInplace(d => (d != 0) ? 1 : 0);

		public static Vector<double> Or(this Vector<double> v1, Vector<double> v2)
		{
			return v1.Map2((d1, d2) => (d1 == 0 && d2 == 0) ? 0 : 1, v2);
		}

		/// <summary>
		/// This method gives next E-vector
		/// </summary>
		/// <param name="v"></param>
		public static void NextLevelVector(Vector<double> v) //YHB
		{
			for (int i = 0; i < v.Count; i++)
			{
				if (v[i] == 0 || v[i] == 1)
				{
					v[i] = 0;
				}
				else
				{
					v[i] = 1;
				}
			}

		}
	}

}
