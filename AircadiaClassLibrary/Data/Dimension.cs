using Aircadia.Services.Serializers;
using System;
using System.Linq;

namespace Aircadia.ObjectModel.DataObjects
{
	/// <summary>
	/// [M L T theta I C n]
	/// </summary>
	public class Dimension
	{
		private readonly int[] _value = new int[7];

        [Serialize]
        public int M => _value[0];
        [Serialize]
        public int L => _value[1];
        [Serialize]
        public int T => _value[2];
        [Serialize]
        public int theta => _value[3];
        [Serialize]
        public int I => _value[4];
        [Serialize]
        public int C => _value[5];
        [Serialize]
        public int n => _value[6];

		public Dimension(int M = 0, int L = 0, int T = 0, int theta = 0, int I = 0, int C = 0, int n = 0)
		{
			_value[0] = M;
			_value[1] = L;
			_value[2] = T;
			_value[3] = theta;
			_value[4] = I;
			_value[5] = C;
			_value[6] = n;
		}

		public static explicit operator Dimension(int[] arr)
		{
			if (arr.Length != 7)
			{
				throw new Exception("Dimension arrays must have Lenght = 7");
			}
			return new Dimension(arr[0], arr[1], arr[2], arr[3], arr[4], arr[5], arr[6]);
		}

		public static explicit operator int[] (Dimension arr)
		{
			if (arr == null)
			{
				return new int[7];
			}

			return arr._value;
		}


		public static Dimension operator *(Dimension d1, Dimension d2)
		{
			return new Dimension(d1.M + d2.M, d1.L + d2.L, d1.T + d2.T, d1.theta + d2.theta, d1.I + d2.I, d1.C + d2.C, d1.n + d2.n);
		}

		public static Dimension operator /(Dimension d1, Dimension d2)
		{
			return new Dimension(d1.M - d2.M, d1.L - d2.L, d1.T - d2.T, d1.theta - d2.theta, d1.I - d2.I, d1.C - d2.C, d1.n - d2.n);
		}

		public override bool Equals(object obj)
		{
			if ((Dimension)obj is Dimension d)
			{
				for (int i = 0; i < _value.Length; i++)
				{
					if (_value[i] != d._value[i])
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hash = 0;
			int factor = 1; // 10^0
			for (int i = 0; i < _value.Length; i++)
			{
				hash += factor * _value[i];
				factor *= 10;
			}
			return hash;
		}

		public override string ToString() => $"[M: {M}, L: {L}, C: {C}, theta: {theta}, I: {I}, C: {C}, n: {n}]";

		// Some weel known dimension
		public static Dimension None = new Dimension();

		public static Dimension Mass = new Dimension(M: 1);
		public static Dimension Lenght = new Dimension(L: 1);
		public static Dimension Time = new Dimension(T: 1);
		public static Dimension Velocity = Lenght / Time;
		public static Dimension Acceleration = Velocity / Time;
		public static Dimension Force = Mass * Acceleration;
		public static Dimension Torque = Force * Lenght;
		public static Dimension Energy = Force * Lenght;
		public static Dimension Power = Force * Velocity;

		public static Dimension Area = Lenght * Lenght;
		public static Dimension Volume = Area * Lenght;
		public static Dimension Density = Mass / Volume;
		public static Dimension Pressure = Force / Area;

		public static Dimension Current = new Dimension(I: 1);
		public static Dimension Voltage = Power / Current;

		public static Dimension Temperature = new Dimension(T: 1);
	}
}
