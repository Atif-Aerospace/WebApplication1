using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;



namespace Aircadia
{
	class RandomVariable
    {
        public string DisType;
        public double FirstPara;
        public double SecondPara;
        public double ThirdPara;
        public double ForthPara;
        public double FifthPara;
        public double SixthPara;
        public string SamMethod;
		public double[] CustomSamples;


        /*public cRandomVariable(string newDisType, string newFirstPara, string newSecondPara, string newThirdPara, string newForthPara, string newSamMethod)
        {
            DisType = newDisType;
            FirstPara = Convert.ToDouble(newFirstPara);
            SecondPara = Convert.ToDouble(newSecondPara);
            ThirdPara = Convert.ToDouble(newThirdPara);
            ForthPara = Convert.ToDouble(newForthPara);
            SamMethod = newSamMethod;
        }*/

        public RandomVariable(string newDisType, string newFirstPara, string newSecondPara, string newThirdPara, string newForthPara, string newFifthPara, string newSixthPara, string newSamMethod, double[] samples = null)
        {
            DisType = newDisType;
            FirstPara = Convert.ToDouble(newFirstPara);
            SecondPara = Convert.ToDouble(newSecondPara);
            ThirdPara = Convert.ToDouble(newThirdPara);
            ForthPara = Convert.ToDouble(newForthPara);
            if (newFifthPara != null)
            {
                string s = newFifthPara.Substring(0, 17);
                int l = newFifthPara.Length;
                double d2 = Convert.ToDouble(s);
                double d = Double.Parse(s.TrimEnd('\n'));
                FifthPara = Convert.ToDouble(s.TrimEnd('\n'));
            }
            FifthPara = Convert.ToDouble(newFifthPara);
            SixthPara = Convert.ToDouble(newSixthPara);
            SamMethod = newSamMethod;
			CustomSamples = samples;
		}

        public double[] GenSamples_FAST(int Number_of_Sim, int Omiga_i, List<double> s_k)
        {
            double[] Samples = new double[Number_of_Sim];
            for (int i = 0; i < Number_of_Sim; i++)
            {

                Samples[i] = 0.5 + 1 / Math.PI * Math.Asin(Math.Sin(Omiga_i * s_k[i]));                         //generate ditermistic sampling points from [0,1]

            }
			#region store the raw data to .csv for debugging
			//should be a uniform distribution (not strictly because FAST sampling strategy)
			AircadiaProject Project = AircadiaProject.Instance;
			string file_path = Project.ProjectPath + "\\temp_FAST_raw.csv";
            var FileStream = new FileStream(file_path, FileMode.Append);
            var sw = new StreamWriter(FileStream, System.Text.Encoding.UTF8);
            for (int j = 0; j < Number_of_Sim; j++)
            {
                sw.WriteLine(Samples[j].ToString());
            }
            sw.Flush();
            sw.Close();
            FileStream.Close();
            #endregion


            switch (DisType)
            {
                case "Uniform":
                    //in uniform distribution: lower boundary = FirstPara
                    //                         upper boundary = SecondPara
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        Samples[i] = (SecondPara - FirstPara) * Samples[i] + FirstPara;
                    }
                    break;

                case "Triangular":
                    //in triangular distribution: mean        = FirstPara   
                    double a = FirstPara - SecondPara;              //                 left point  = mean - SecondPara
                    double b = FirstPara + ThirdPara;               //                 right point = mean + ThirdPara
                    double c = FirstPara + SecondPara - ThirdPara;  //                 top point   = mean + SecondPara - ThirdPara

                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        if (Samples[i] <= (c - a) / (b - a))
                        {
                            Samples[i] = a + Math.Sqrt(Samples[i] * (c - a) * (b - a));
                        }
                        else
                        {
                            Samples[i] = b - Math.Sqrt((1 - Samples[i]) * (b - a) * (b - c));
                        }
                    }
                    break;

                case "Normal":
                    /*
                    Approximation from
                    author = {Dongbin Xiu and George E M Karniadakis},
                    title = {The Wiener-Askey Polynomial Chaos for Stochastic Differential Equations},
                    journal = {SIAM J. SCI. COMPUT},
                    year = {2002},
                    pages = {619--644}
                    */
                    double c0 = 2.515517;
                    double c1 = 0.802853;
                    double c2 = 0.010328;
                    double d1 = 1.432788;
                    double d2 = 0.189269;
                    double d3 = 0.001308;

                    double t;  //temp
                    double X;  //temp

                    double mu = FirstPara; //mean
                    double sigma = SecondPara;//std

                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        t = Math.Sqrt(-Math.Log(Math.Pow(Math.Min(Samples[i], 1 - Samples[i]), 2)));
                        X = Math.Sign(Samples[i] - 0.5) * (t - (c0 + c1 * t + c2 * Math.Pow(t, 2)) / (1 + d1 * t + d2 * Math.Pow(t, 2) + d3 * Math.Pow(t, 3)));
                        Samples[i] = mu + X * sigma;
                    }
                    break;

                case "Rayleigh":
                    //Equation from lecture notes 3F1 Random Processes Course
                    //Nick Kingsbury, 2012.
                    //Also in Book by Peyton Z. Peebles, JR.
                    //Title Probability Random Variables and Random Signal Principles
                    a = FirstPara;
                    b = SecondPara;
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        Samples[i] = Math.Sqrt(-b * Math.Log(1.0 - Samples[i])) + a;
                    }
                    break;

                case "Mixture Gaussian":
                    c0 = 2.515517;
                    c1 = 0.802853;
                    c2 = 0.010328;
                    d1 = 1.432788;
                    d2 = 0.189269;
                    d3 = 0.001308;

                    double mu1 = FirstPara;
                    double mu2 = SecondPara;
                    double sigma1 = ThirdPara;
                    double sigma2 = ForthPara;
                    double p1 = FifthPara;
                    double p2 = SixthPara;

                    double u;
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        if (Samples[i] <= p1)
                        {
                            u = Samples[i] / p1;
                            t = Math.Sqrt(-Math.Log(Math.Pow(Math.Min(u, 1.0 - u), 2)));
                            X = Math.Sign(u - 0.5) * (t - (c0 + c1 * t + c2 * Math.Pow(t, 2)) / (1 + d1 * t + d2 * Math.Pow(t, 2) + d3 * Math.Pow(t, 3)));
                            Samples[i] = mu1 + X * sigma1;
                        }
                        else
                        {
                            u = (Samples[i] - (p1 + 1e-8)) / p2;
                            t = Math.Sqrt(-Math.Log(Math.Pow(Math.Min(u, 1.0 - u), 2)));
                            X = Math.Sign(u - 0.5) * (t - (c0 + c1 * t + c2 * Math.Pow(t, 2)) / (1 + d1 * t + d2 * Math.Pow(t, 2) + d3 * Math.Pow(t, 3)));
                            Samples[i] = mu2 + X * sigma2;
                        }
                    }
                    break;
            }
            return Samples;
        }

        public double[] GenSamples_PureRandom(int Number_of_Sim, bool Random_Switch, Random MyRandom)
        {
            double[] Samples = new double[Number_of_Sim];

            for (int i = 0; i < Number_of_Sim; i++)
            {
                Samples[i] = MyRandom.NextDouble();                          //generate random points from [0,1]
            }


            switch (DisType)
            {
                case "Uniform":

                    //in uniform distribution: lower boundary = FirstPara
                    //                         upper boundary = SecondPara
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        Samples[i] = (SecondPara - FirstPara) * Samples[i] + FirstPara;
                    }
                    break;

                case "Triangular":

                    //in triangular distribution: mean        = FirstPara   
                    double a = FirstPara - SecondPara;              //                 left point  = mean - SecondPara
                    double b = FirstPara + ThirdPara;               //                 right point = mean + ThirdPara
                    double c = FirstPara + SecondPara - ThirdPara;  //                 top point   = mean + SecondPara - ThirdPara

                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        if (Samples[i] <= (c - a) / (b - a))
                        {
                            Samples[i] = a + Math.Sqrt(Samples[i] * (c - a) * (b - a));
                        }
                        else
                        {
                            Samples[i] = b - Math.Sqrt((1 - Samples[i]) * (b - a) * (b - c));
                        }
                    }
                    break;

                case "Normal":
                    //using boxer-muller method

                    //a new set of random is needed
                    double Samples2;
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        Samples2 = MyRandom.NextDouble();

                        //in normal distribution:           mean   = FirstPara 
                        //                                      standard deviation = SecondPara
                        Samples[i] = SecondPara * Math.Sqrt(-2 * Math.Log(Samples[i])) * Math.Cos(2 * Math.PI * Samples2) + FirstPara;   //log based on e, cos based on radian
                    }
                    break;


                case "Rayleigh":
                    //Equation from lecture notes 3F1 Random Processes Course
                    //Nick Kingsbury, 2012.
                    //Also in Book by Peyton Z. Peebles, JR.
                    //Title Probability Random Variables and Random Signal Principles
                    a = FirstPara;
                    b = SecondPara;
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        Samples[i] = Math.Sqrt(-b * Math.Log(1 - Samples[i])) + a;
                    }
                    break;

                case "Mixture Gaussian":
                    double c0 = 2.515517;
                    double c1 = 0.802853;
                    double c2 = 0.010328;
                    double d1 = 1.432788;
                    double d2 = 0.189269;
                    double d3 = 0.001308;

                    double mu1 = FirstPara;
                    double mu2 = SecondPara;
                    double sigma1 = ThirdPara;
                    double sigma2 = ForthPara;
                    double p1 = FifthPara;
                    double p2 = SixthPara;

                    double u;
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        if (Samples[i] <= p1)
                        {
                            u = Samples[i] / p1;
                            double t = Math.Sqrt(-Math.Log(Math.Pow(Math.Min(u, 1 - u), 2)));
                            double X = Math.Sign(u - 0.5) * (t - (c0 + c1 * t + c2 * Math.Pow(t, 2)) / (1 + d1 * t + d2 * Math.Pow(t, 2) + d3 * Math.Pow(t, 3)));
                            Samples[i] = mu1 + X * sigma1;
                        }
                        else
                        {
                            u = (Samples[i] - (p1 + 1e-8)) / p2;
                            double t = Math.Sqrt(-Math.Log(Math.Pow(Math.Min(u, 1 - u), 2)));
                            double X = Math.Sign(u - 0.5) * (t - (c0 + c1 * t + c2 * Math.Pow(t, 2)) / (1 + d1 * t + d2 * Math.Pow(t, 2) + d3 * Math.Pow(t, 3)));
                            Samples[i] = mu2 + X * sigma2;
                        }
                    }
                    break;


                case "Lognormal_e":
                    //lognormal based on e

                    double Samples3;
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        Samples3 = MyRandom.NextDouble();
                        Samples[i] = 1 * Math.Sqrt(-2 * Math.Log(Samples[i])) * Math.Cos(2 * Math.PI * Samples3) + 0;   //fisrt generate standard normal  N(0,1)

                        Samples[i] = Math.Exp(FirstPara + SecondPara * Samples[i]);             //transfer normal to lognormal  
                    }
                    break;


                case "Lognormal_10":
                    //lognormal based on 10

                    double Samples4;
                    for (int i = 0; i < Number_of_Sim; i++)
                    {
                        Samples4 = MyRandom.NextDouble();
                        Samples[i] = 1 * Math.Sqrt(-2 * Math.Log(Samples[i])) * Math.Cos(2 * Math.PI * Samples4) + 0;   //fisrt generate standard normal  N(0,1)

                        Samples[i] = Math.Pow(10, (FirstPara + SecondPara * Samples[i]));             //transfer normal to lognormal_10  
                    }
                    break;

                case "File":
					//var openFileDialog_wmd = new OpenFileDialog
					//{
					//	InitialDirectory = Directory.GetCurrentDirectory(),
					//	Title = "Select a File",
					//	FileName = ""
					//};

					//openFileDialog_wmd.ShowDialog();


					//string[] lines = System.IO.File.ReadAllLines(openFileDialog_wmd.FileName);
					//for (int i = 0; i < Number_of_Sim; i++)
					//{
					//	Samples[i] = Convert.ToDouble(lines[i]);
					//}

					Samples = CustomSamples.ToArray();

					break;
            }

            return Samples;
        }

    }
}
