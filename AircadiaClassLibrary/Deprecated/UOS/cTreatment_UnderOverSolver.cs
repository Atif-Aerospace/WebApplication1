/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using MathNet.Numerics.LinearAlgebra;

namespace Aircadia
{
    /// <summary>
    /// This treatment is used the dynamic solving(tick i/p and o/p) implemented just in fExecuter.
    /// </summary>
    public class cTreatment_UnderOverSolver : Treatment
    {
        #region Constructor
        private ExecutableComponent modsub;
        private List<int> outList;
        private List<int> inList;
        private double[] out_val;
        /// <summary>
        /// Constructer
        /// </summary>
        /// <param name="n"></param>
        /// <param name="input_opt"></param>
        /// <param name="outp"></param>
        public cTreatment_UnderOverSolver(string n, Treatment_InOut input_opt, Treatment_InOut outp) :
            base (n, n)
        {
            Name = n;
            input_options = input_opt;
            //output_struct = outp;
        }
		#endregion
		#region View
		/// <summary>
		/// The string returned by this function is the one which will be printed in the properties window of AirCADia for the selected cTreatment_UnderOverSolver object.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => "cTreatment_UnderOverSolver";
		#endregion
		#region Execute

		public override bool ApplyOn() => true;

		/// <summary>
		/// Execute this treatment
		/// </summary>
		/// <param name="oModSub"></param>
		/// <returns></returns>
		public override bool ApplyOn(ExecutableComponent oModSub)
        {
            modsub = oModSub;
            double[] invalues = Data.GetValuesDouble(modsub.ModelDataInputs);
            double[] outvalues = Data.GetValuesDouble(modsub.ModelDataOutputs);
            outList = input_options.setuplist[0] as List<int>;
            inList = input_options.setuplist[1] as List<int>;
            bool status = ExecuteGaussNewton(mGetStartingPoint());
            if (status == false)
            {
                Data.SetValuesDouble(modsub.ModelDataInputs, invalues);
                Data.SetValuesDouble(modsub.ModelDataOutputs, outvalues);
                Console.WriteLine("Values of data reset to initial in Subprocess " + Name + "\r\n");
            }
            return status;
        }
        /// <summary>
        /// Get the starting point for the Unknown inputs of the subprocess. Unknown inputs are the default inputs of the subprocess which are unticked in fExecuter.
        /// </summary>
        /// <returns></returns>
        private double[] mGetStartingPoint()
        {
            out_val = new double[outList.Count];
            for (int i = 0; i < outList.Count; i++)
            {
                out_val[i] = (modsub.ModelDataOutputs[outList[i]] as Data).ValueAsDouble;
            }
            double[] inpf = new double[inList.Count];
            for (int i = 0; i < inList.Count; i++)
            {
                inpf[i] = (modsub.ModelDataInputs[inList[i]] as Data).ValueAsDouble;
            }
            return inpf;
        }
        /// <summary>
        /// This function acts as the function on which Gauss Newton Method is applied inorder to solve this case.
        /// </summary>
        /// <param name="invar"></param>
        /// <returns></returns>
        private double[] testfunc(double[] invar)
        {
            double[] outp = new double[outList.Count];
            for (int i = 0; i < invar.Length; i++)
            {
                (modsub.ModelDataInputs[inList[i]] as Data).ValueAsDouble = invar[i];
            }

            

            modsub.Execute();



            for (int i = 0; i < outList.Count; i++)
            {
                outp[i] = (modsub.ModelDataOutputs[outList[i]] as Data).ValueAsDouble - out_val[i];
            }
            return outp;
        }
        /// <summary>
        /// Executes the Gauss Newton Method
        /// </summary>
        /// <param name="inpf"></param>
        /// <returns></returns>
        private bool ExecuteGaussNewton(double[] inpf)
        {
            bool status = true;
            double[] outp = testfunc(inpf);
            int ncount = 0;

            //Select_Steps_and_Tolerance sst = new Select_Steps_and_Tolerance();
            //sst.ShowDialog();
            //double diffperc = sst.Diff;    //xin 1e-8 before
            //double tolerance = sst.Toler;    //xin 1e-8 before
            double diffperc = 1e-3;
            //double tolerance = 1e-8;



            //bool status = true;
            //double[] outp = testfunc(inpf);
            //int ncount = 0;
            //double diffperc = 5e-3;                  //CHANGED
            int ncount_outp = 1;
            double oldresidual=FindSumSquared(outp);
            double[] diff = diffcalculator(diffperc, inpf);
            try
            {
                while ((oldresidual > 2e-2) & (ncount < 5000))      //CHANGED
                {
                    /*Matrix_Csml Jacob = FindJacob(inpf, diff);
                    Matrix_Csml Jacob_T = Jacob.Transpose();
                    Matrix_Csml Jacob_Mult = Jacob_T * Jacob;
                    Matrix_Csml Jacob_Inv = Jacob_Mult.Inverse();
                    Matrix_Csml Jacob_Fin = Jacob_Inv * Jacob_T;
                    Matrix_Csml vecto = new Matrix_Csml(outp);
                    Matrix_Csml inpnext = Jacob_Fin * vecto;*/
                    Matrix<double> Jacob = FindJacob(inpf, diff);
                    if (Jacob == null)
                        return false;
					Matrix<double> Jacob_T = Jacob.Transpose();
					Matrix<double> Jacob_Mult = Jacob_T * Jacob;
					Matrix<double> Jacob_Inv = Jacob_Mult.Inverse();
					Matrix<double> Jacob_Fin = Jacob_Inv * Jacob_T;
					Vector<double> vecto = Vector<double>.Build.DenseOfArray(outp);
					Vector<double> inpnext = Jacob_Fin * vecto;
                    for (int ninp = 0; ninp < inpf.Length; ninp++)
                    {
                        inpf[ninp] = inpf[ninp] - inpnext[ninp];
                    }
                    outp = testfunc(inpf);
                    if (outp == null)
                        return false;
                    ncount++;
                    ncount_outp++;
                    if (ncount == 5000)
                    {
                        Console.WriteLine("Reached 4999 iterations in Guass Newton Method");
                        status = false;
                    }
                    else if (oldresidual==FindSumSquared(outp))
                    {
                        Console.WriteLine("Residual stuck at a non-minimum");
                        status = false;
                        return status;
                    }
                    Console.WriteLine("Number of iterations (GaussNewton) in " + Name + "=" + ncount + ", Residual=" + FindSumSquared(outp));
                    oldresidual = FindSumSquared(outp);
                }

                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        /// <summary>
        /// Calculates the perturbations to be introduced inorder to calculate the finite differences
        /// </summary>
        /// <param name="diffperc"></param>
        /// <param name="inp"></param>
        /// <returns></returns>
        private double[] diffcalculator(double diffperc, double[] inp)
        {
            //Calculates the diff for the inp
            double[] diff = new double[inp.Length];
            for (int ninp = 0; ninp < inp.Length; ninp++)
            {
                diff[ninp] = inp[ninp] * diffperc;   //if inp==0 gradiant is NAN!!!!!!!!!!!
            }
            return diff;
        }
        /// <summary>
        /// um of the squares of the output array produced by the testfunc.
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        private double FindSumSquared(double[] inp)
        {
            double outp = 0;
            for (int ncount = 0; ncount < inp.Length; ncount++)
            {
                outp += Math.Pow(inp[ncount], 2);
            }
            return outp;
        }
		/// <summary>
		/// Calculates jacobian for 'testfunc'
		/// </summary>
		/// <param name="inp"></param>
		/// <param name="diff"></param>
		/// <returns></returns>
		private Matrix<double> FindJacob(double[] inp, double[] diff)
		{
			int ndes = inp.Length;
			Matrix<double> Jacob = Matrix<double>.Build.Dense(inp.Length, inp.Length);
			double? tmpJacob;
			for (int nrow = 0; nrow < inp.Length; nrow++)
			{
				for (int ncol = 0; ncol < inp.Length; ncol++)
				{
					tmpJacob = FindGradient(inp, nrow, ncol, diff[ncol]);
					if (!tmpJacob.HasValue)
						return null;
					else
						Jacob[nrow, ncol] = tmpJacob.Value;
				}
			}
			return Jacob;
		}
		/// <summary>
		/// Calculates Gradient for the 'testfunc'
		/// </summary>
		/// <param name="inp"></param>
		/// <param name="nrow"></param>
		/// <param name="ncol"></param>
		/// <param name="diff"></param>
		/// <returns></returns>
		private double? FindGradient(double[] inp, int nrow, int ncol, double diff)
        {
            double grad;
            double[] outp;
            double tmpin = inp[ncol];
            inp[ncol] = tmpin + diff;
            outp = testfunc(inp);
            if (outp == null)
                return null;
            grad = outp[nrow];
            inp[ncol] = tmpin - diff;
            outp = testfunc(inp);
            grad = grad - outp[nrow];
            grad = grad / (diff + diff);
            inp[ncol] = tmpin;
            return grad;
        }
        #endregion
    }
}
