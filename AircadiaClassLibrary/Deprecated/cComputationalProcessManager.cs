/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Diagnostics;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Workflows;
using MathNet.Numerics.LinearAlgebra;
using Aircadia.Numerics.Solvers;

namespace Aircadia
{
	[Serializable()]
	//// This class contains all the algorithm for computational process modelling
	public class ComputationalProcessManager
	{

		public static int nvIncmsetGLOBAL = 0;
		public static int inExploreCounterGLOBAL = 0;
		public static int returnCounterGLOBAL = 0;
		public static object[] vIncmsetPublic = new object[1000];
		public static object[] vIncmsetPublic2 = new object[1000];
		public static Stopwatch explorationWatch = new Stopwatch();
		// Added by VCD 05/07/2013 ///////////////////////////////
		public static List<int[,]> incidenceMatrixTestLib = new List<int[,]>();
		public static List<int[,]> bestIncidenceMatrices = new List<int[,]>();
		public static int currentNumberOfRevModels = 0;
		//////////////////////////////////////////////////////////

		// Added 18/07/13 - vcd
		public static List<object> globalIncidenceExploreList = new List<object>();

		// Added 08/08/2013
		public static int[] previous_vIndep = new int[1];

		/// <summary>
		/// Given the Model Objects, data objects and the vector(vIndep) in which '1' represent an independent variables , this function creates 
		/// a subprocess after performing the computational proces modelling
		/// </summary>
		/// <param name="vSubName"></param>
		/// <param name="modelObjects"></param>
		/// <param name="dataObjects"></param>
		/// <param name="vIndep"></param>
		/// <returns></returns>
		public static Workflow Compute_Process_Model(string vSubName, List<WorkflowComponent> modelObjects, List<Data> dataObjects, int[] vIndep)//, int[] prev_vIndep)
		{
			explorationWatch.Restart();
			List<WorkflowComponent> vResultSub = ComputeModelsSchedule(modelObjects, dataObjects, vIndep, vSubName);

			object[] vInOuts = MatrixCalculators.GetInputOutputs(vResultSub, dataObjects);
			//return new WorkflowOld(vSubName, "", vInOuts[0] as List<Data>, vInOuts[1] as List<Data>, vResultSub);
			return new Workflow(vSubName, "", vInOuts[0] as List<Data>, vInOuts[1] as List<Data>, vResultSub, vResultSub);
		}

		public static List<WorkflowComponent> ComputeModelsSchedule(List<WorkflowComponent> modelObjects, List<Data> dataObjects, int[] vIndep, string workflowName)
		{
			// Incidence matrices for the workfow
			Matrix<double> imMatrixf = WorkflowComponent.GetIncidenceMatrix(modelObjects, dataObjects);
			Matrix<double> imMatrixi = VariableFlowModelling(imMatrixf, vIndep);
			// Model-based DSM
			Matrix<double> vDsm = ImmtoDsm(imMatrixi);
			// Locates SCCs
			object[] vClus = Decompose(vDsm);
			// SCC-based incidence matrix
			Matrix<double> imMatrixU = CreateUpMatrix(imMatrixi, vClus);
			// SCC-based DSM
			Matrix<double> vDsmU = ImmtoDsm(imMatrixU);
			// Orders the models, so there is only feed-forward
			Vector<double> vDsmArr = SequenceDsm(vDsmU);
			// Creates the workflows for the reversed models
			WorkflowComponent[] vResultMod = CreateModModels(imMatrixi, imMatrixf, modelObjects, dataObjects, vClus, workflowName);
			// Creates the workflows for the SCCs (default order)
			List<WorkflowComponent> vResultSub = CreateSCC(vResultMod, vClus, dataObjects, workflowName);
			// Applies the order (vDsmArr) to the models
			ReSchedule(ref vResultSub, vDsmArr);
			return vResultSub;
		}

		/// <summary>
		/// Reschedules the elements in vResultSub based on the sorting in vDsmArr
		/// </summary>
		/// <param name="vResultSub"></param>
		/// <param name="vDsmArr"></param>
		public static void ReSchedule(ref List<WorkflowComponent> vResultSub, Vector<double> vDsmArr)
		{
			//final scheduling
			var tvResultSub = new List<WorkflowComponent>();
			for (int ncount = vDsmArr.Count - 1; ncount >= 0; ncount--)
			{
				tvResultSub.Add(vResultSub[(int)vDsmArr[ncount]]);
			}

			vResultSub = tvResultSub;
		}
		/// <summary>
		/// This function identifies the SCC (from vClus) in the vResultMod, then groups it and therafter combines it with other models in the vResultMod and return it as a cWfmCollection
		/// </summary>
		/// <param name="vResultMod"></param>
		/// <param name="vClus"></param>
		/// <param name="dataObjects"></param>
		/// <param name="workflowName"></param>
		/// <returns></returns>
		public static List<WorkflowComponent> CreateSCC(WorkflowComponent[] vResultMod, object[] vClus, List<Data> dataObjects, string workflowName)
		{
			var vResultSub = new List<WorkflowComponent>();
			//cTreatment treat = new cTreatment();
			for (int nvClus = 0; nvClus < vClus.Length; nvClus++)
			{
				if ((vClus[nvClus] as List<int>).Count == 1)
				{
					vResultSub.Add(vResultMod[(vClus[nvClus] as List<int>)[0]]);
				}
				else
				{
					var vSCCmod = new List<WorkflowComponent>();
					foreach (int nRow in vClus[nvClus] as List<int>)
					{
						vSCCmod.Add(vResultMod[nRow]);
					}
					object[] vInOuts = MatrixCalculators.GetInputOutputs(vSCCmod, dataObjects);

					//cSubprocessSCC scc = new cSubprocessSCC("scc", (vInOuts[0] as List<Data>), (vInOuts[1] as List<Data>), vSCCmod);
					// Set inputs and outputs of this scc to 
					var solvers = new List<ISolver>() { new FixedPointSolver() };
					var scc2 = new WorkflowSCC($"SCC{nvClus}#{workflowName}", "", (vInOuts[0] as List<Data>), (vInOuts[1] as List<Data>), vSCCmod, vSCCmod, solvers);

					vResultSub.Add(scc2);

					//Project.WorkflowStore.Add(scc2);
				}
			}
			return vResultSub;
		}

		/// <summary>
		/// This function identifies the Modified models (from imMatrixi and imMatrif) in the modelObjects, then creates a 'modified model subprocess' for each modified model, 
		/// and therafter combines it with other models in the modelObjects and return it in a object array
		/// </summary>
		/// <param name="imMatrixi"></param>
		/// <param name="imMatrixf"></param>
		/// <param name="modelObjects"></param>
		/// <param name="dataObjects"></param>
		/// <param name="vClus"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static WorkflowComponent[] CreateModModels(Matrix<double> imMatrixi, Matrix<double> imMatrixf, List<WorkflowComponent> modelObjects, List<Data> dataObjects, object[] vClus, string name)
		{
			// 
			var vModelLisWMod = new WorkflowComponent[imMatrixf.RowCount];
			for (int nrow = 0; nrow < imMatrixi.RowCount; nrow++)
			{
				WorkflowComponent component = modelObjects[nrow];
				if (!MatrixCalculators.CompareVectors(imMatrixf.Row(nrow), imMatrixi.Row(nrow)))
				{
					var vInp = new List<int>();
					for (int i = 0; i < imMatrixi.Row(nrow).Count; i++)
						if (imMatrixi.Row(nrow)[i] == 2)
							vInp.Add(i);
					var vOut = new List<int>();
					for (int i = 0; i < imMatrixi.Row(nrow).Count; i++)
						if (imMatrixi.Row(nrow)[i] == 3)
							vOut.Add(i);
					var vInputs = new List<Data>();
					foreach (int index in vInp)
						vInputs.Add(dataObjects[index]);
					var vOutputs = new List<Data>();
					foreach (int index in vOut)
						vOutputs.Add(dataObjects[index]);
					var vModModel = new List<WorkflowComponent>();
					if (component is Model model) //(modelObjects[nrow] is cModel)
					{
						vModModel.Add(model);
						//cWfmCollection vModInputs= cMatrixCalculators.mGetModInput(vInputs, mod.inputs);
						//cTreatment treat = new cTreatment();
						//vModelLisWMod[nrow] = new cSubprocessMOD("Modified Model_" + model.Name, vInputs, vOutputs, vModModel);
						var opts = new NewtonOptions() { MaxIterations = 20, DerivativeStep = new double[] { 0.01 } };
						var solver = new NewtonSolver(opts);
						vModelLisWMod[nrow] = new WorkflowReversedModel($"{name}#Reversed#{model.Name}", "", vInputs, vOutputs, model, new List<ISolver>() { solver });

						//Project.WorkflowStore.Add(vModelLisWMod[nrow] as WorkflowSergioReversedModel);
					}
					else if (component is Workflow workflow)
					{
						vModModel.Add(workflow);
						//cWfmCollection vModInputs= cMatrixCalculators.mGetModInput(vInputs, mod.inputs);
						//cTreatment treat = new cTreatment();
						//vModelLisWMod[nrow] = new cSubprocessMOD("Modified Model_" + workflow.Name, vInputs, vOutputs, vModModel);
						var opts = new NewtonOptions() { MaxIterations = 20, DerivativeStep = new double[] { 0.01 } };
						var solver = new NewtonSolver(opts);
						vModelLisWMod[nrow] = new WorkflowGlobal($"{name}#GlobalWorkflow#{workflow.Name}", "", vInputs, vOutputs, workflow.Components, workflow.Components, new List<ISolver>() { solver });

						//Project.WorkflowStore.Add(vModelLisWMod[nrow] as WorkflowGlobalSimple);
					}

				}
				else
				{
					vModelLisWMod[nrow] = modelObjects[nrow];
				}
			}
			return vModelLisWMod;
		}
		/// <summary>
		/// Identifies the sequence for the Dsm for which its contents become all feed forward.
		/// The method is described in detail  in my thesis chapter 3.2.3.2.
		/// </summary>
		/// <param name="vDsm"></param>
		/// <returns></returns>
		public static Vector<double> SequenceDsm(Matrix<double> vDsm)
		{
			Vector<double> vDsmArr = Vector.Build.Dense(vDsm.ColumnCount);
			Matrix<double> vEcost = Matrix.Build.Dense(vDsm.ColumnCount, 1);
			int nLoc = 0;
			vEcost.Add(1.0);
			while (nLoc < vDsm.ColumnCount)
			{
				Matrix<double> vPeProd = vDsm * vEcost;
				Vector<double> lio = MatrixCalculators.FindLocValueInVector(vPeProd.Column(0), 1);
				for (int ncount = 0; ncount < lio.Count; ncount++)
				{
					vDsmArr[nLoc] = lio[ncount];
					nLoc++;
				}
				vEcost = Matrix.Build.DenseOfArray(vPeProd.AsArray()) as Matrix<double>;
				MatrixCalculators.ReplaceLessthanWith(vEcost, 2, 0);
				MatrixCalculators.ReplaceGreaterthanWith(vEcost, 1, 1);
			}
			return vDsmArr;
		}
		/// <summary>
		/// creates the new incidence matrix based on the SCCs. SCCs are clustered in the new DSM in a single row.
		/// </summary>
		/// <param name="imMatrixi"></param>
		/// <param name="vClus"></param>
		/// <returns></returns>
		public static Matrix<double> CreateUpMatrix(Matrix<double> imMatrixi, object[] vClus)
		{
			Matrix<double> imMatrixU = Matrix.Build.Dense(vClus.Length, imMatrixi.ColumnCount);
			for (int nvClus = 0; nvClus < vClus.Length; nvClus++)
			{
				if ((vClus[nvClus] as List<int>).Count == 1)
				{
					imMatrixU.SetRow(nvClus, imMatrixi.Row((vClus[nvClus] as List<int>)[0]));
				}
				else
				{
					Matrix<double> imMatrixT = Matrix.Build.Dense((vClus[nvClus] as List<int>).Count, imMatrixi.ColumnCount);
					int nvC = 0;
					foreach (int nRow in vClus[nvClus] as List<int>)
					{
						imMatrixT.SetRow(nvC, imMatrixi.Row(nRow));
						nvC++;
					}
					for (int ncount = 0; ncount < imMatrixi.ColumnCount; ncount++)
					{
						if ((MatrixCalculators.FindValueInVector(imMatrixT.Column(ncount), 2) > 0) & (MatrixCalculators.FindValueInVector(imMatrixT.Column(ncount), 3) > 0))
						{
							imMatrixU[nvClus, ncount] = 3;
						}
						else if (MatrixCalculators.FindValueInVector(imMatrixT.Column(ncount), 3) > 0)
						{
							imMatrixU[nvClus, ncount] = 3;
						}
						else if (MatrixCalculators.FindValueInVector(imMatrixT.Column(ncount), 2) > 0)
						{
							imMatrixU[nvClus, ncount] = 2;
						}
						else
						{
							imMatrixU[nvClus, ncount] = 0;
						}
					}

				}

			}
			return imMatrixU;
		}
		/// <summary>
		/// Performs the varaible flow modelling given the foundation IM and the vector representing the independent variable.
		/// </summary>
		/// <param name="imMatrixf"></param>
		/// <param name="vIndep"></param>
		/// <returns></returns>
		public static Matrix<double> VariableFlowModelling(Matrix<double> imMatrixf, int[] vIndep)
		{
			object[] vIncmset = ImmTop(imMatrixf, vIndep);
			//object[] vDsmset = mIncmsettoDsmset(vIncmset);
			int vLeastModified = LeastModified(vIncmset, imMatrixf);
			//return vIncmset[0] as Matrix<double>;
			return vIncmset[vLeastModified] as Matrix<double>;
			// code for genetic algorithm

		}
		/// <summary>
		/// Identifies SCC in vDSm (read chapter 3.2.2 in my thesis), this information is passed back in the object array vClus.
		/// </summary>
		/// <param name="vDsm"></param>
		/// <returns></returns>
		public static object[] Decompose(Matrix<double> vDsm)
		{
			Matrix<double> vDsmt = Matrix.Build.Dense(vDsm.ColumnCount, vDsm.ColumnCount);
			Matrix<double> vDsmMul = Matrix.Build.Dense(vDsm.ColumnCount, vDsm.ColumnCount);
			MatrixCalculators.Eye(vDsmMul);
			for (int nDsm = 0; nDsm < vDsm.ColumnCount; nDsm++)
			{
				vDsmMul = vDsm * vDsmMul;
				MatrixCalculators.ReplaceGreaterthanWith(vDsmMul, 1, 1);
				vDsmt = vDsmt + vDsmMul;
			}
			vDsmt = (vDsmt.PointwiseMultiply(vDsmt.Transpose() as Matrix) as Matrix<double>);
			MatrixCalculators.ReplaceGreaterthanWith(vDsmt, 1, 1);
			object[] vClus = ClusGather(vDsmt);
			return vClus;
		}
		/// <summary>
		/// This function reads through the dsm vDsmt identifies the identical rows (these are sccs ) and pass back this information(in an object array format) to the calling function
		/// </summary>
		/// <param name="vDsmt"></param>
		/// <returns></returns>
		public static object[] ClusGather(Matrix<double> vDsmt)
		{
			//gathers the clusters

			var vClusPres = new List<int>();
			object[] vClus = new object[vDsmt.RowCount];
			int nvClus = 0;
			for (int nDsmt = 0; nDsmt < vDsmt.ColumnCount; nDsmt++)
			{
				var vClusList = new List<int>();
				if (!vClusPres.Contains(nDsmt))
				{
					vClusPres.Add(nDsmt);
					vClusList.Add(nDsmt);
					for (int nDsmt1 = 0; nDsmt1 < vDsmt.RowCount; nDsmt1++)
					{
						if (nDsmt == nDsmt1)
							continue;
						if (vDsmt.Row(nDsmt).Equals(vDsmt.Row(nDsmt1)))
						{
							vClusList.Add(nDsmt1);
							vClusPres.Add(nDsmt1);
						}

					}
					vClus[nvClus] = vClusList;
					nvClus++;
					vClusList = null;
				}
			}
			object[] vClusR = new object[nvClus];
			for (int ncount = 0; ncount < nvClus; ncount++)
			{ vClusR[ncount] = vClus[ncount]; }

			return vClusR;
		}
		/// <summary>
		/// Given a collection of incidence matrices this function chooses the one which has least number of modified models
		/// </summary>
		/// <param name="vIncmset"></param>
		/// <param name="imMatrixf"></param>
		/// <returns></returns>
		public static int LeastModified(object[] vIncmset, Matrix<double> imMatrixf)
		{
			int vLeastModified = 0;
			int vNumModPre = MatrixCalculators.CompareMatrix((vIncmset[0] as Matrix<double>), imMatrixf); ;
			int vNumMod;
			for (int nvIncmset = 0; nvIncmset < vIncmset.Length; nvIncmset++)
			{
				if (vIncmset[nvIncmset] == null)
				{
					return vLeastModified;
				}
				vNumMod = MatrixCalculators.CompareMatrix((vIncmset[nvIncmset] as Matrix<double>), imMatrixf);
				if (vNumModPre > vNumMod)
				{
					vLeastModified = nvIncmset;
					vNumModPre = vNumMod;
				}

			}
			return vLeastModified;
		}
		/// <summary>
		/// Given a collection of incidence matrices this function convertes these to dsms and returns it in an object array
		/// </summary>
		/// <param name="vIncmset"></param>
		/// <returns></returns>
		public static object[] IncmsettoDsmset(object[] vIncmset)
		{
			// converts immset to dsmset
			object[] vDsmset = new object[vIncmset.Length];
			for (int nvIncmset = 0; nvIncmset < vIncmset.Length; nvIncmset++)
			{
				vDsmset[nvIncmset] = ImmtoDsm(vIncmset[nvIncmset] as Matrix<double>);
			}
			return vDsmset;
		}

		/// <summary>
		/// Given a single incidence matrix this function convertes it to a dsm and returns it.
		/// </summary>
		/// <param name="iMatrix"></param>
		/// <returns></returns>
		public static Matrix<double> ImmtoDsm(Matrix<double> iMatrix)
		{
			// converts imm to dsm
			Matrix<double> vDsm = Matrix.Build.Dense(iMatrix.RowCount, iMatrix.RowCount);
			MatrixCalculators.Eye(vDsm);
			double[,] vSpRowCol = MatrixCalculators.FindValue(iMatrix, 3);
			for (int nsprow = 0; nsprow < vSpRowCol.GetLength(0); nsprow++)
			{
				Vector<double> vSpInp = MatrixCalculators.FindLocValueInVector(iMatrix.Column((int)vSpRowCol[nsprow, 1]), 2);
				if (vSpInp != null)
				{
					for (int nvSpInp = 0; nvSpInp < vSpInp.Count; nvSpInp++)
					{
						vDsm[(int)vSpRowCol[nsprow, 0], (int)vSpInp[nvSpInp]] = 1;
					}
				}
			}
			return vDsm;
			//dsm=eye(size(spm,1));
			//[sprow,spcol]=find(spm==3);
			//for nsprow=1:length(sprow)
			//    spinp=find(spm(:,spcol(nsprow))==2);
			//    dsm(sprow(nsprow),spinp)=1;
			//end

		}
		/// <summary>
		/// This funnction returns all possible workflows in an incidence matrix format, given the foundation matrix 
		/// as well as the vIndep vector which represent the independent variables.
		/// This function is the top level function which performs the incidence matrix method.
		/// </summary>
		/// <param name="imMatrixf"></param>
		/// <param name="vIndep"></param>
		/// <returns></returns>
		public static object[] ImmTop(Matrix<double> imMatrixf, int[] vIndep)
		{
			Matrix<double> imMatrixi = Matrix.Build.DenseOfMatrix(imMatrixf);
			int[] vNoutput = new int[imMatrixf.RowCount];
			MatrixCalculators.ReplaceAlltoOne(imMatrixi);
			object[] vIncmset;
			for (int nincm = 0; nincm < imMatrixi.RowCount; nincm++)
			{
				Vector<double> imMatrixfRow = imMatrixf.Row(nincm);
				vNoutput[nincm] = MatrixCalculators.FindValueInVector(imMatrixfRow, 3);
			}

			double[] vNoutvalr = new double[imMatrixf.RowCount];
			double[] vNoutvalc = new double[imMatrixf.ColumnCount];
			for (int nrow = 0; nrow < imMatrixf.RowCount; nrow++)
			{
				Vector<double> imMatrixfRow = imMatrixi.Row(nrow);
				int ncol = MatrixCalculators.FindValueInVector(imMatrixfRow, 1);
				vNoutvalr[nrow] = Math.Pow(3, vNoutput[nrow]) * Math.Pow(2, (ncol - vNoutput[nrow]));
			}
			for (int ncol = 0; ncol < imMatrixf.ColumnCount; ncol++)
			{
				Vector<double> imMatrixfCol = imMatrixi.Column(ncol);
				int nrow = MatrixCalculators.FindValueInVector(imMatrixfCol, 1);
				vNoutvalc[ncol] = Math.Pow(2, (nrow - 1)) * 3;
			}
			for (int ncount = 0; ncount < imMatrixi.ColumnCount; ncount++)
			{
				if (vIndep[ncount] == 1)
					MatrixCalculators.MultiplyColumn(imMatrixi, ncount, 2);
				if (vIndep[ncount] == 2)
				{
					Vector<double> col = MatrixCalculators.FindLocValueInVector(imMatrixf.Column(ncount), 3);
					if (col != null)
						imMatrixi[(int)MatrixCalculators.FindLocValueInVector(imMatrixf.Column(ncount), 3)[0], ncount] = 3;
				}
			}
			object[] vChanvals = FindChanVal(imMatrixi, imMatrixf);
			if (vChanvals[1] == null)
			{
				vIncmset = new object[1];
				vIncmset[0] = imMatrixf;
				return vIncmset;
			}

			IncidenceMatrixFirst(imMatrixi, vNoutvalr, vNoutvalc, 1);
			FillasInput(imMatrixi, vNoutvalr, vNoutvalc);
			IncidenceMatrixFirst(imMatrixi, vNoutvalr, vNoutvalc, 1);

			////////////////////////

			//dnAnalytics.LinearAlgebra.Vector temp = vChanvals[1] as dnAnalytics.LinearAlgebra.Vector;
			double[] temp = FindModifiedModels(imMatrixi, imMatrixf);
			int modelsInSCC_counter = 0;
			List<int> modelsInScc = ModelsInSCCFromIMM(imMatrixi);
			foreach (int ithModel in modelsInScc)
			{
				// foreach model in SCC is any model...
				bool searchRes = FindValueInArray(temp, ithModel);
				if (searchRes == true) // if ith model is in SCC and reversed then break
				{
					modelsInSCC_counter++;
					break;
				}
			}

			if (modelsInSCC_counter == 0) //i.e. no models in SCC are modified/reversed
			{
				if (MatrixCalculators.FindValue(imMatrixi, 1) == null)
				{
					vIncmset = new object[1];
					vIncmset[0] = imMatrixi;
					return vIncmset;
				}
				// copy default SCC inputs and outputs to new incidence matrix, imMatrixi
				for (int ithRow = 0; ithRow < imMatrixf.RowCount; ithRow++)
				{
					if (FindIntegerValueInArray(modelsInScc.ToArray(), ithRow))
					{
						for (int ithCol = 0; ithCol < imMatrixf.ColumnCount; ithCol++)
						{
							imMatrixi[ithRow, ithCol] = imMatrixf[ithRow, ithCol];
						}
					}


				}
				vIncmset = new object[1];
				vIncmset[0] = imMatrixi;
				return vIncmset;
			}
			else
			{
				// When clicking 'edit' then get incidence before it has been modified or if creating new object, then get default incidence
				Matrix<double> vDsm_temp = ImmtoDsm(imMatrixf);
				object[] vClus_temp = Decompose(vDsm_temp);
				var modelsInScc2 = new List<int>();
				int modelsInSCC_counter2 = 0;
				double[] modified_models = temp;
				foreach (object othObj in vClus_temp)
				{
					int[] temp_store = othObj as int[];
					var temp_SCC_Store = othObj as List<int>;
					if (temp_SCC_Store.Count > 1)
					{
						modelsInScc2.AddRange(temp_SCC_Store.GetRange(0, temp_SCC_Store.Count));
					}


				}
				foreach (int ithModel in modelsInScc2)
				{
					// foreach model in SCC is any model...
					bool searchRes = FindValueInArray(modified_models, ithModel);
					if (searchRes == true) // if ith model is in SCC and reversed then break
					{
						modelsInSCC_counter2++;
						break;
					}
				}

				if (modelsInSCC_counter2 == 0) //i.e. copy existing SCC configurations
				{
					for (int ithRow = 0; ithRow < imMatrixf.RowCount; ithRow++)
					{
						if (FindIntegerValueInArray(modelsInScc2.ToArray(), ithRow))
						{
							for (int ithCol = 0; ithCol < imMatrixf.ColumnCount; ithCol++)
							{
								imMatrixi[ithRow, ithCol] = imMatrixf[ithRow, ithCol];
							}
						}


					}
				}

			}
			////////////////////

			if (MatrixCalculators.FindValue(imMatrixi, 1) != null)
			{
				globalIncidenceExploreList.Clear();
				vIncmsetPublic = new object[1000];//Reset
				vIncmsetPublic2 = new object[1000];//Reset
				IncidenceExplore(imMatrixi, imMatrixf, vNoutvalr, vNoutvalc, 0);
				vIncmset = globalIncidenceExploreList.ToArray();
				if (vIncmset.Length == 0)
				{ return vIncmset; }
				//incmset=uniqueincmset(incmset);Unque object has to be added here....
			}

			else
			{
				vIncmset = new object[1];
				vIncmset[0] = imMatrixi;
				return vIncmset;
			}
			return vIncmset;
		}

		public static List<int> ModelsInSCCFromIMM(Matrix<double> imMatrixf)
		{
			var modelsInSCC = new List<int>();
			for (int ithRow = 0; ithRow < imMatrixf.RowCount; ithRow++)
			{
				var ithModel = new List<double>();
				for (int ithCol = 0; ithCol < imMatrixf.ColumnCount; ithCol++)
				{
					ithModel.Add(imMatrixf[ithRow, ithCol]);
				}
				bool checkRes = FindValueInArray(ithModel.ToArray(), 1);
				if (checkRes == true)
				{
					modelsInSCC.Add(ithRow);
				}
			}
			return modelsInSCC;
		}

		private static bool FindValueInArray(double[] arrayToSearch, double value)
		{
			foreach (double d in arrayToSearch)
			{
				if (d == value)
				{
					return true;
				}
			}
			return false;
		}


		private static bool FindIntegerValueInArray(int[] arrayToSearch, int value)
		{
			foreach (int i in arrayToSearch)
			{
				if (i == value)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// This function solves the incidence matrix which remains unsolved after
		/// applying the first step of Imm ( mIncidenceMatrixFirst)
		/// the outputs of certain models are considered as determined for solving
		/// purpose.
		/// </summary>
		/// <param name="imMatrixi"></param>
		/// <param name="imMatrixf"></param>
		/// <param name="vNoutvalr"></param>
		/// <param name="vNoutvalc"></param>
		/// <param name="vCanstat"></param>
		/// <returns></returns>
		private static void IncidenceExplore(Matrix<double> imMatrixi, Matrix<double> imMatrixf, double[] vNoutvalr, double[] vNoutvalc, int vCanstat)
		{
			object[] vIncmset = new object[1000];
			Vector<double> vChangevalr = null;
			object[] vChanvals = FindChanVal(imMatrixi, imMatrixf);

			if (vChanvals[0] != null)
			{ vChangevalr = vChanvals[0] as Vector<double>; }
			if (vChanvals[1] != null)
			{ var vChangevalrcan = vChanvals[1] as Vector<double>; }
			if ((vChanvals[1] == null) & (vCanstat == 0))
			{ vIncmset[0] = imMatrixf; }
			else if (vChanvals[0] != null)
			{
				if (vChangevalr == null)
				{
					double[,] vChangevalrTMP = MatrixCalculators.FindValue(imMatrixi, 1);
					Matrix<double> vChangevalrTMP1 = Matrix.Build.DenseOfArray(vChangevalrTMP);
					vChangevalr = (vChangevalrTMP1.Column(0) as Vector<double>);
				}
				vChangevalr = MatrixCalculators.Unique(vChangevalr);
				for (int nvChangevalr = 0; nvChangevalr < vChangevalr.Count; nvChangevalr++)
				{
					double vCurrentNoutvalr, vCvalr, vCurrentNoutvalc, vCvalc, vRtwo, vCtwo;
					Vector<double> vChangevalc = MatrixCalculators.FindLocValueInVector(imMatrixi.Row((int)vChangevalr[nvChangevalr]), 1);
					vCurrentNoutvalr = MatrixCalculators.MultiplyNonZeroVector(imMatrixi.Row((int)vChangevalr[nvChangevalr]));
					vCvalr = vNoutvalr[(int)vChangevalr[nvChangevalr]] / vCurrentNoutvalr;
					for (int nvChangevalc = 0; nvChangevalc < vChangevalc.Count; nvChangevalc++)
					{
						vCurrentNoutvalc = MatrixCalculators.MultiplyNonZeroVector(imMatrixi.Column((int)vChangevalc[nvChangevalc]));
						vCvalc = vNoutvalc[(int)vChangevalc[nvChangevalc]] / vCurrentNoutvalc;
						vRtwo = Math.Log(vCvalr) / Math.Log(2);
						vCtwo = Math.Log(vCvalc) / Math.Log(2);
						if (Math.Abs(Math.Truncate(vCtwo) - vCtwo) > 1e-15 & Math.Abs(Math.Truncate(vRtwo) - vRtwo) > 1e-15)
						{
							Matrix<double> imMatrixiMed = Matrix.Build.DenseOfMatrix(imMatrixi);
							imMatrixiMed[(int)vChangevalr[nvChangevalr], (int)vChangevalc[nvChangevalc]] = 3;
							IncidenceMatrixFirst(imMatrixiMed, vNoutvalr, vNoutvalc, 1);
							FillasInput(imMatrixiMed, vNoutvalr, vNoutvalc);
							if (MatrixCalculators.FindValue(imMatrixiMed, 1) == null)
							{
								/*if (nvIncmset > 999)
                                { return vIncmset; }
                                else
                                {
                                    RunTestFn(ConvertIncidenceFromStringToIntArray(imMatrixi), ConvertIncidenceFromStringToIntArray(imMatrixiMed));
                                    vIncmset[nvIncmset] = imMatrixiMed;
                                    if (nvIncmsetGLOBAL > 999)
                                    { return vIncmset; }
                                    vIncmsetPublic[nvIncmsetGLOBAL] = imMatrixiMed;
                                    //vIncmsetPublic2[
                                    nvIncmset++;
                                    nvIncmsetGLOBAL++;
                                }*/

								/*
                                //========================================LIBISH's================================================================================================
                                if (nvIncmset > 999)
                                { return vIncmset; }
                                else
                                {
                                    vIncmset[nvIncmset] = imMatrixiMed;
                                    nvIncmset++;
                                }
                                //===============================================================================================================================================
                                 * */
								if (globalIncidenceExploreList.Count > 999 || explorationWatch.ElapsedMilliseconds > 60000)
								//if (globalIncidenceExploreList.Count > 999)
								{ return; }
								else
								{
									int[,] incidenceMatrix = ConvertIncidenceFromStringToIntArray(imMatrixiMed);
									int noOfRevModels = RunTestFn(ConvertIncidenceFromStringToIntArray(imMatrixi), incidenceMatrix);
									if (globalIncidenceExploreList.Count == 0)
									{
										globalIncidenceExploreList.Add(imMatrixiMed);
										currentNumberOfRevModels = noOfRevModels;
									}
									else
									{
										if (currentNumberOfRevModels > noOfRevModels)
										{
											globalIncidenceExploreList.Clear();
											globalIncidenceExploreList.Add(imMatrixiMed);
											currentNumberOfRevModels = noOfRevModels;
										}
										else
										{
											if (currentNumberOfRevModels == noOfRevModels)
											{
												globalIncidenceExploreList.Add(imMatrixiMed);
											}
											else
											{
												// do nothing as inspected incidence already has more rev models than those stored.
											}
										}
									}

								}
							}
							else
							{
								//========================================LIBISH's================================================================================================
								if (globalIncidenceExploreList.Count < 999)
									IncidenceExplore(imMatrixiMed, imMatrixf, vNoutvalr, vNoutvalc, 1);
								/*for (int nvIncmsetRecy = 0; nvIncmsetRecy < vIncmsetRecy.Length; nvIncmsetRecy++)
                                {
                                    vIncmset[nvIncmset] = vIncmsetRecy[nvIncmsetRecy];
                                    nvIncmset++;
                                }
                                if (nvIncmset > 999)
                                    return vIncmset;*/
								//===============================================================================================================================================

								/*inExploreCounterGLOBAL++;
                                returnCounterGLOBAL++;
                                object[] vIncmsetRecy = mIncidenceExplore(imMatrixiMed, imMatrixf, vNoutvalr, vNoutvalc, 1);

                                if (returnCounterGLOBAL == 0)
                                {
                                    //vIncmset.CopyTo(vIncmsetPublic2, 0);// copy original incidence explore results
                                    //vIncmsetRecy.CopyTo(vIncmsetPublic2, inExploreCounterGLOBAL); // copy new incidence explore results

                                    int globalFillCounter = 0;

                                    for(int itv1=0; itv1<vIncmset.Length; itv1++)
                                    {
                                        if(vIncmset[itv1]!=null)
                                        {
                                            vIncmsetPublic2[globalFillCounter] = vIncmset[itv1];
                                            globalFillCounter++;
                                        }
                                    }

                                    for(int itv2=0; itv2<vIncmsetRecy.Length; itv2++)
                                    {
                                        if(vIncmsetRecy[itv2]!=null)
                                        {
                                            vIncmsetPublic2[globalFillCounter] = vIncmsetRecy[itv2];
                                            globalFillCounter++;
                                        }
                                    }
                                    //return vIncmsetPublic2;//Commented by Marco 14Nov2012
                                }
                                else
                                {
                                   for (int nvIncmsetRecy = 0; nvIncmsetRecy < vIncmsetRecy.Length; nvIncmsetRecy++)
                                    {
                                        if (vIncmsetRecy[nvIncmsetRecy] != null    &&   nvIncmsetRecy < 999)
                                        {
                                            vIncmset[nvIncmset] = vIncmsetRecy[nvIncmsetRecy];
                                            nvIncmset++;
                                        }
                                    }
                                }
                                if (nvIncmset > 999)
                                {//Added by Marco 14Nov2012
                                    returnCounterGLOBAL--;
                                    return vIncmset;
                                }//Added by Marco 14Nov2012*/
							}
						}
					}

				}
			}
			return;

		}

		public static int[,] ConvertIncidenceFromStringToIntArray(Matrix<double> vImm)
		{
			int[,] IMM = new int[vImm.RowCount, vImm.ColumnCount];
			for (int RowIndex = 0; RowIndex < vImm.RowCount; RowIndex++)
			{
				for (int ColIndex = 0; ColIndex < vImm.ColumnCount; ColIndex++)
				{
					IMM[RowIndex, ColIndex] = Convert.ToInt32(vImm[RowIndex, ColIndex]);
				}
			}
			return IMM;
		}

		public static int RunTestFn(int[,] defaultInci, int[,] incidenceMatrix)
		{
			int noOfRevModels = 0;
			for (int i = 0; i < incidenceMatrix.GetLength(0); i++)
			{
				for (int j = 0; j < incidenceMatrix.GetLength(1); j++)
				{
					int defaultCellValue = defaultInci[i, j];
					int revInciValue = incidenceMatrix[i, j];
					if (revInciValue == 0)
					{
						// skip as no interaction...
					}
					else
					{
						if (defaultCellValue == revInciValue)
						{
							// do nothing as no reversal detected...
						}
						else
						{
							if (revInciValue != 1)
							{
								// therefore model i, must be reversed 
								noOfRevModels++;
								break;
							}
						}
					}
				}
			}
			return noOfRevModels;
		}

		public static double[] FindModifiedModels(Matrix<double> imMatrixi, Matrix<double> imMatrixf)
		{
			var modifiedModels = new List<int>();
			for (int nrow = 0; nrow < imMatrixi.RowCount; nrow++)
			{
				if (CompareTwoModelsIfModified(imMatrixf.Row(nrow), imMatrixi.Row(nrow)) == true)
				{
					modifiedModels.Add(nrow);
				}

			}
			double[] modModelsDOUBLE = new double[modifiedModels.Count];
			int counter = 0;
			foreach (int i1 in modifiedModels)
			{
				modModelsDOUBLE[counter] = Convert.ToDouble(i1);
				counter++;
			}
			return modModelsDOUBLE;
		}

		public static bool CompareTwoModelsIfModified(Vector<double> imMatrixf, Vector<double> imMatrixi)
		{
			for (int i = 0; i < imMatrixi.Count; i++)
			{
				if (imMatrixf[i] != imMatrixi[i])
				{
					if (imMatrixi[i] != 1)
					{
						return true;
					}
				}
			}
			return false;
		}


		/// <summary>
		/// Given a matrix incm, given an intial completely populated real matrix incmreal
		/// changeval are those rows of incm where the models associated varaible are
		/// modified
		/// </summary>
		/// <param name="imMatrixi"></param>
		/// <param name="imMatrixf"></param>
		/// <returns></returns>
		private static object[] FindChanVal(Matrix<double> imMatrixi, Matrix<double> imMatrixf)
		{

			Vector<double> vChangevalr = Vector.Build.Dense(imMatrixi.RowCount);
			Vector<double> vChangevalrcan = Vector.Build.Dense(imMatrixi.RowCount);
			object[] vChanvals = new object[2];

			int ncan = 0;
			int nr = 0;
			for (int nrow = 0; nrow < imMatrixi.RowCount; nrow++)
			{
				if (!MatrixCalculators.CompareVectors(imMatrixf.Row(nrow), imMatrixi.Row(nrow)))
				{
					vChangevalrcan[ncan] = nrow;
					ncan++;
				}
				if (MatrixCalculators.FindValueInVector(imMatrixi.Row(nrow), 1) > 0)
				{
					vChangevalr[nr] = nrow;
					nr++;
				}
				//}
			}
			if (ncan == 0)
			{
				vChanvals[1] = null;
			}
			if (nr == 0)
			{
				vChanvals[0] = null;
			}
			if (ncan != 0)
			{
				Vector<double> vRcan = vChangevalrcan.SubVector(0, ncan);
				//vRcan = cMatrixCalculators.mUnique(vRcan);
				vChanvals[1] = vRcan;
			}
			if (nr != 0)
			{
				Vector<double> vR = vChangevalr.SubVector(0, nr);
				//vR = cMatrixCalculators.mUnique(vR);
				vChanvals[0] = vR;
			}

			return vChanvals;
		}
		/// <summary>
		/// This function adds additional varaibles as indpendent varaibles. This is done when the system is underdetermined and 
		/// additional variables are required to solve it. 
		/// </summary>
		/// <param name="imMatrixi"></param>
		/// <param name="vNoutvalr"></param>
		/// <param name="vNoutvalc"></param>
		private static void FillasInput(Matrix<double> imMatrixi, double[] vNoutvalr, double[] vNoutvalc)
		{
			//% fills the 1's with 2 if 2 has to be put in row and in column
			//% this make the variable in the column to be an input
			double vCurrentNoutvalr, vCvalr, vCurrentNoutvalc, vCvalc, vRtwo, vCthree;
			double[,] vRowCol = MatrixCalculators.FindValue(imMatrixi, 1);
			if (vRowCol != null)
			{
				for (int nones = 0; nones < vRowCol.GetLength(0); nones++)
				{
					vCurrentNoutvalr = MatrixCalculators.MultiplyNonZeroVector(imMatrixi.Row((int)vRowCol[nones, 0]));
					vCvalr = vNoutvalr[(int)vRowCol[nones, 0]] / vCurrentNoutvalr;
					vCurrentNoutvalc = MatrixCalculators.MultiplyNonZeroVector(imMatrixi.Column((int)vRowCol[nones, 1]));
					vCvalc = vNoutvalc[(int)vRowCol[nones, 1]] / vCurrentNoutvalc;
					//checking for rows
					vRtwo = Math.Log(vCvalr) / Math.Log(2);
					vCthree = Math.Log(vCvalc) / Math.Log(3);
					if ((Math.Abs(Math.Truncate(vRtwo) - vRtwo) < 1e-15) & (Math.Abs(Math.Truncate(vCthree) - vCthree) < 1e-15))
						imMatrixi[(int)vRowCol[nones, 0], (int)vRowCol[nones, 1]] = 2;
				}
			}
		}
		/// <summary>
		/// This is the first step of incidence matrix method. This function performs the operation described in chapter3.2.1 in my thesis
		/// </summary>
		/// <param name="imMatrixi"></param>
		/// <param name="vNoutvalr"></param>
		/// <param name="vNoutvalc"></param>
		/// <param name="vActivat"></param>
		private static void IncidenceMatrixFirst(Matrix<double> imMatrixi, double[] vNoutvalr, double[] vNoutvalc, int vActivat)
		{
			Matrix<double> vBegIncm = Matrix.Build.Dense(imMatrixi.RowCount, imMatrixi.ColumnCount);
			vBegIncm.Add(1.0);
			Matrix<double> imMatrixis = Matrix.Build.DenseOfMatrix(imMatrixi);
			double vCurrentNoutvalr, vCvalr, vCurrentNoutvalc, vCvalc, vRtwo, vRthree, vCtwo, vCthree;
			//Main code incidence matrix method
			while (!vBegIncm.Equals(imMatrixis))
			{
				vBegIncm = Matrix.Build.DenseOfMatrix(imMatrixi);
				//vBegIncm = imMatrixi;
				double[,] vRowCol = MatrixCalculators.FindValue(imMatrixi, 1);
				if (vRowCol != null)
				{
					for (int nones = 0; nones < vRowCol.GetLength(0); nones++)
					{
						vCurrentNoutvalr = MatrixCalculators.MultiplyNonZeroVector(imMatrixi.Row((int)vRowCol[nones, 0]));
						vCvalr = vNoutvalr[(int)vRowCol[nones, 0]] / vCurrentNoutvalr;
						vCurrentNoutvalc = MatrixCalculators.MultiplyNonZeroVector(imMatrixi.Column((int)vRowCol[nones, 1]));
						vCvalc = vNoutvalc[(int)vRowCol[nones, 1]] / vCurrentNoutvalc;
						//checking for rows
						vRtwo = Math.Log(vCvalr) / Math.Log(2);
						vRthree = Math.Log(vCvalr) / Math.Log(3);
						vCtwo = Math.Log(vCvalc) / Math.Log(2);
						vCthree = Math.Log(vCvalc) / Math.Log(3);
						if (Math.Abs(Math.Truncate(vRtwo) - vRtwo) < 1e-15 & Math.Abs(Math.Truncate(vCthree) - vCthree) > 1e-15)
						{ imMatrixi[(int)vRowCol[nones, 0], (int)vRowCol[nones, 1]] = 2; }
						else if (Math.Abs(Math.Truncate(vRthree) - vRthree) < 1e-15 & Math.Abs(Math.Truncate(vCtwo) - vCtwo) > 1e-15)
						{ imMatrixi[(int)vRowCol[nones, 0], (int)vRowCol[nones, 1]] = 3; }
						else if (Math.Abs(Math.Truncate(vCtwo) - vCtwo) < 1e-15 & Math.Abs(Math.Truncate(vRthree) - vRthree) > 1e-15)
						{ imMatrixi[(int)vRowCol[nones, 0], (int)vRowCol[nones, 1]] = 2; }
						else if (Math.Abs(Math.Truncate(vCthree) - vCthree) < 1e-15 & Math.Abs(Math.Truncate(vRtwo) - vRtwo) > 1e-15)
						{
							if ((vActivat == 0) & (MatrixCalculators.FindAnyValueInVector(imMatrixi.Column((int)vRowCol[nones, 1])) > 1))
								imMatrixi[(int)vRowCol[nones, 0], (int)vRowCol[nones, 1]] = 3;
							else if (vActivat == 1)
								imMatrixi[(int)vRowCol[nones, 0], (int)vRowCol[nones, 1]] = 3;
						}
					}
				}

				imMatrixis = imMatrixi;
			}
		}
	}
}

