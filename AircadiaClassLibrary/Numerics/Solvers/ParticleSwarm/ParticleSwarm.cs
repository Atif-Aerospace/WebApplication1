using Aircadia.Services.Serializers;
using System;
using System.IO;
using System.Linq;

namespace Aircadia.Numerics.Solvers
{
	[Serializable()]
	public class ParticleSwarm : Solver, IGlobalSolver
	{
		private const int defaultMaxEvals = 1000;
		private const int defaultIndividuals = 100;
		private const int defaultGenerations = 30;
		private const double defaultTol = 1e-4;

		//Debugging switch:
		private readonly bool Debugging = true;

		private int max_FPI_evals = defaultMaxEvals;
		private int pSO_Individuals = defaultIndividuals;
		private int pSO_Generations = defaultGenerations;
		double fTol = defaultTol;

		Random random = new Random();

		[Serialize]
		[Option("MaxIterations")]
		public int Max_FPI_evals { get => max_FPI_evals; set => max_FPI_evals = (value > 0) ? value : defaultMaxEvals; }
		[Serialize]
		[Option("Individuals")]
		public int PSO_Individuals { get => pSO_Individuals; set => pSO_Individuals = (value > 0) ? value : defaultIndividuals; }
		[Serialize]
		[Option("Generations")]
		public int PSO_Generations { get => pSO_Generations; set => pSO_Generations = (value > 0) ? value : defaultGenerations; }
		[Serialize]
		[Option("YTolerance")]
		public double FTol { get => fTol; set => fTol = (value > Double.Epsilon) ? value : defaultTol; }

		public int Nvariables { get; set; }
		public double[] LowerBounds { get; set; }
		public double[] UpperBounds { get; set; }


		public double[] Solve(Func<double[], double[]> f)
		{
			IsSolved = false;

			//Retrieving and setting the number of generations and individuals:
			int size = PSO_Individuals;
			int itermax = PSO_Generations;
			//int size = 50; int itermax = 5000;

			int iter = 0;
			//double[] fdb = new double[Nvariables];
			double[] lb = LowerBounds;
			double[] ub = UpperBounds;

			double[,] pop = InitializeSwarm(size, lb, ub);
			double[,] popp = new double[size, Nvariables];
			popp = Array2Copy(popp, pop);
			int flag = 1;
			double[] inval = new double[Nvariables];
			double[] finval = new double[Nvariables];
			double gb = 100;
			double[] vglob = new double[Nvariables];
			double[,] vcog = new double[size, Nvariables];
			double[] diff = new double[Nvariables];
			double[] s = new double[size];
			double[] pb = new double[size];

			if (Debugging == true)
			{
				//Setting the file where the PSO evaluations are stored for debugging and trouble-shooting:
				string execpath =Directory.GetCurrentDirectory();
				if (File.Exists(execpath + "\\PSO_Evals.txt"))
					File.Delete(execpath + "\\PSO_Evals.txt");
				var filer_Evals = new FileStream(execpath + "\\PSO_Evals.txt", FileMode.Append, FileAccess.Write);
				var sw_Evals = new StreamWriter(filer_Evals);
				for (int j = 0; j < Nvariables; j++)
				{
					//sw_Evals.Write((this.fdbvariables[j] as Data).Name + "  ");
					sw_Evals.Write(j + "  ");
				}
				sw_Evals.Write(@"" + "\r\n");
				sw_Evals.Close();
				filer_Evals.Close();

				//Setting the file where the difference between the guessed and the effective value of the Feedback Variables is stored for debugging and trouble-shooting:
				if (File.Exists(execpath + "\\PSO_Diff.txt"))
					File.Delete(execpath + "\\PSO_Diff.txt");
				var filer_Diff = new FileStream(execpath + "\\PSO_Diff.txt", FileMode.Append, FileAccess.Write);
				var sw_Diff = new StreamWriter(filer_Diff);
				for (int j = 0; j < Nvariables; j++)
				{
					//sw_Diff.Write((this.fdbvariables[j] as Data).Name + "  ");
					sw_Diff.Write(j + "  ");
				}
				sw_Diff.Write(@"" + "\r\n");
				sw_Diff.Close();
				filer_Diff.Close();
			}

			while (flag == 1)
			{
				if (Debugging == true)
				{
					//Specifing the generation number in the file storing the Feedback Variables values:
					string execpath = Directory.GetCurrentDirectory();
					var filer_ev = new FileStream(execpath + "\\PSO_Evals.txt", FileMode.Append, FileAccess.Write);
					var sw_ev = new StreamWriter(filer_ev);
					sw_ev.Write(@"" + "\r\n");
					sw_ev.Write("Generation No. " + Convert.ToString(iter) + ":" + "\r\n");
					sw_ev.Close();
					filer_ev.Close();

					////Specifing the generation number in the file storing the Feedback Variables differences:
					var filer_dif = new FileStream(execpath + "\\PSO_Diff.txt", FileMode.Append, FileAccess.Write);
					var sw_dif = new StreamWriter(filer_dif);
					sw_dif.Write(@"" + "\r\n");
					sw_dif.Write("Generation No. " + Convert.ToString(iter) + ":" + "\r\n");
					sw_dif.Close();
					filer_dif.Close();
				}

				for (int ii = 0; ii < size; ii++)
				{
					//Defining the values of the Feedback Variables:
					for (int jj = 0; jj < Nvariables; jj++)
					{
						inval[jj] = pop[ii, jj];
					}

					diff = f(inval);

					//Computing the square root of the sum of the squared Feedback Variables values (Euclidean Distance):
					double SQR_sum = 0;
					for (int Ith_fbv = 0; Ith_fbv < Nvariables; Ith_fbv++)
					{
						double Ith_fbvValue = diff[Ith_fbv];
						SQR_sum = SQR_sum + (Ith_fbvValue * Ith_fbvValue);
					}
					SQR_sum = Math.Pow(SQR_sum, 0.5);
					/*//Computing the square root of the sum of the squared Feedback Variables values after normalising them by considering the MIN and MAX attributes of the corresponding data:
                    double SQR_sum = 0;
                    for (int Ith_fbv = 0; Ith_fbv < numfbv; Ith_fbv++)
                    {
                        double Ith_fbvValue = diff[Ith_fbv];
                        if (Ith_fbvValue < (this.fdbvariables[Ith_fbv] as Data).Min)
                        {
                            Ith_fbvValue = double.NegativeInfinity; //TEMPORARY SOLUTION - TO BE UPDATED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                            //fMultiscopeForm MinAttributeUpdate = new fMultiscopeForm("DataMinimumUpdate", 
                            //if (Console.WriteLine("The value <" + Convert.ToString(Ith_fbvValue) + "> has been computed for the feedback variable '" + (this.fdbvariables[Ith_fbv] as Data).name + "'. The Minimum Value associated to such data is currently <" + Convert.ToString((this.fdbvariables[Ith_fbv] as Data).Min) + ">, do you want to update it?", "Data Property - Minimum", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            //{
                                //Data reldt = dataObjects.mGetElementFromName((this.fdbvariables[Ith_fbv] as Data).name) as Data;
                            //}
                        }
                        if (Ith_fbvValue > (this.fdbvariables[Ith_fbv] as Data).max)
                        {
                            Ith_fbvValue = double.PositiveInfinity; //TEMPORARY SOLUTION - TO BE UPDATED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                            //if (Console.WriteLine("The value <" + Convert.ToString(Ith_fbvValue) + "> has been computed for the feedback variable '" + (this.fdbvariables[Ith_fbv] as Data).name + ". Do you want to update the Maximum Value associated to such data?", "Data Property - Maximum", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            //{
                                //Data reldt = dataObjects.mGetElementFromName((this.fdbvariables[Ith_fbv] as Data).name) as Data;
                            //}
                        }
                        double Ith_fbvValueNormalised = (Ith_fbvValue - (this.fdbvariables[Ith_fbv] as Data).Min) / ((this.fdbvariables[Ith_fbv] as Data).max - (this.fdbvariables[Ith_fbv] as Data).Min);//Normalising the value of the feedback variable
                        SQR_sum = SQR_sum + (Ith_fbvValueNormalised * Ith_fbvValueNormalised);
                        //SQR_sum = SQR_sum + (Ith_fbvValue * Ith_fbvValue);
                    }
                    SQR_sum = Math.Pow(SQR_sum, 0.5);*/
					s[ii] = SQR_sum;

					if (Debugging == true)
					{
						//Storing the final value of the Feedback Variables:
						string execpath = Directory.GetCurrentDirectory();
						var filer_evals = new FileStream(execpath + "\\PSO_Evals.txt", FileMode.Append, FileAccess.Write);
						var sw_evals = new StreamWriter(filer_evals);
						for (int Ith_fdVar = 0; Ith_fdVar < Nvariables; Ith_fdVar++)
						{
							//sw_evals.Write(Convert.ToString((this.fdbvariables[Ith_fdVar] as Data).Value) + "  ");
							sw_evals.Write(Ith_fdVar + "  ");
						}
						sw_evals.Write(@"" + "\r\n");
						sw_evals.Close();
						filer_evals.Close();

						//Storing the final value of the Feedback Variables:
						var filer_diffs = new FileStream(execpath + "\\PSO_Diff.txt", FileMode.Append, FileAccess.Write);
						var sw_diffs = new StreamWriter(filer_diffs);
						for (int Ith_fdVar = 0; Ith_fdVar < Nvariables; Ith_fdVar++)
						{
							string Val = Convert.ToString(diff[Ith_fdVar]);
							sw_diffs.Write(Val + "  ");
						}
						sw_diffs.Write(@"" + "\r\n");
						sw_diffs.Close();
						filer_diffs.Close();
					}
				}
				if (iter == 0)
				{
					vcog = Array2Copy(vcog, pop);
					pb = Array1Copy(pb, s);
					gb = pb.Min();
					int minl = Minloc(pb);
					for (int i1 = 0; i1 < Nvariables; i1++)
					{
						vglob[i1] = vcog[minl, i1];
					}
				}
				else
				{
					for (int i = 0; i < size; i++)
					{
						if (s[i] < pb[i])
						{
							for (int ii = 0; ii < Nvariables; ii++)
							{
								vcog[i, ii] = pop[i, ii];
							}
							pb[i] = s[i];
						}
					}
					if (pb.Min() < gb)
					{
						gb = pb.Min();
						int minl = Minloc(pb);
						for (int ii = 0; ii < Nvariables; ii++)
						{
							vglob[ii] = vcog[minl, ii];
						}
					}
				}

				pop = UpdatePop(iter, popp, pop, size, vglob, vcog, lb, ub);
				popp = Array2Copy(popp, pop);
				iter = iter + 1;
				if (iter > itermax)
				{
					flag = 0;
				}
				if (gb < FTol)
				{
					flag = 0;
					IsSolved = true;
				}
			}

			diff = f(vglob);
			return vglob;
		}

		private double[,] InitializeSwarm(int size, double[] lowerb, double[] upperb)
		{
			int nv = lowerb.Length;
			double[,] popi = new double[size, nv];
			for (int ii = 0; ii < size; ii++)
			{
				for (int jj = 0; jj < nv; jj++)
				{
					popi[ii, jj] = Rndgen(lowerb[jj], upperb[jj]);
				}
			}
			return popi;
		}
		//Random number generator
		private double Rndgen(double min, double max)
		{
			double r1 = random.NextDouble();
			double r2 = min + r1 * (max - min);
			return r2;
		}

		private double[,] Array2Copy(double[,] a, double[,] b)
		{
			int m = a.GetLength(0);
			int n = a.GetLength(1);
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
					a[i, j] = b[i, j];
				}
			}
			return a;
		}

		private double[] Array1Copy(double[] a, double[] b)
		{
			int m = a.GetLength(0);
			for (int i = 0; i < m; i++)
			{
				a[i] = b[i];
			}
			return a;
		}

		private int Minloc(double[] p)
		{
			int loc = 0;
			for (int ii = 1; ii < p.Length; ii++)
			{
				if (p[ii] < p[loc])
					loc = ii;
			}
			return loc;
		}

		private double[,] UpdatePop(int itr, double[,] pp, double[,] p, int sz1, double[] vg, double[,] vc, double[] lb, double[] ub)
		{

			double phip = 2;//Personal best weight
			double phig = 2;//Global best weight
			double phi = 0;
			phi = phip + phig;
			double Chi = 2 / (phi - 2 + Math.Sqrt(phi * phi - 4 * phi));
			phip = phip * Chi;
			phig = phig * Chi;
			double omega = 0;
			if (itr == 0)
			{ omega = 0; }
			else if (itr < 5 & itr > 0)//Intertial weight reduction
			{ omega = Chi; }
			else
			{ omega = (Chi * itr) / (itr + 5); }


			for (int i = 0; i < sz1; i++)
			{
				for (int j = 0; j < vg.Length; j++)
				{
					p[i, j] = (p[i, j] * omega) + (random.NextDouble() * phip * (vc[i, j] - p[i, j])) + (random.NextDouble() * phig * (vg[j] - p[i, j]));
					p[i, j] = pp[i, j] + p[i, j];

					//Checking whether the [i,j] value is beyond the permissible range of values vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
					double value = p[i, j];
					if (p[i, j] < lb[j])
						p[i, j] = lb[j];
					else if (p[i, j] > ub[j])
						p[i, j] = ub[j];
					value = p[i, j];
					//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
				}
			}
			return p;
		}
	}
}
