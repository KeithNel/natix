//
//   Copyright 2012 Eric Sadit Tellez <sadit@dep.fie.umich.mx>
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Original filename: natix/natix/Numeric/Numeric.cs.template
//
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace natix
{
	/// <summary>
	/// The Numeric manager for UInt64
	/// </summary>
	public class NumericUInt64 : INumeric<UInt64>
	{
		/// <summary>
		/// Constructor
		/// </summary>

		public NumericUInt64 ()
		{
		}
		/// <summary>
		/// From double to UInt64
		/// </summary>
		
		public UInt64 FromDouble (double d)
		{
			return (UInt64)d;
		}
		/// <summary>
		/// From integer
		/// </summary>
		
		public UInt64 FromInt(int d)
		{
			return (UInt64)d;
		}

		/// <summary>
		/// From UInt64 to double
		/// </summary>
		
		public double ToDouble (UInt64 d)
		{
			return d;
		}
		/// <summary>
		/// To integer
		/// </summary>

		public int ToInt(UInt64 d)
		{
			return (int)d;
		}
		/// <summary>
		/// Write a numeric object to a binary stream
		/// </summary>

		public void WriteBinary (BinaryWriter bw, UInt64 d)
		{
			bw.Write (d);
		}
		/// <summary>
		/// Reads a numeric object from a binary stream
		/// </summary>
		
		public UInt64 ReadBinary (BinaryReader br)
		{
			return br.ReadUInt64 ();
		}

		/// <summary>
		/// Substraction
		/// </summary>

		public double Sub (UInt64 a, UInt64 b)
		{
			return a - b;
		}
		/// <summary>
		/// Sum
		/// </summary>

		public double Sum (UInt64 a, UInt64 b)
		{
			return a + b;
		}
		/// <summary>
		/// Product
		/// </summary>

		public double Prod (UInt64 a, UInt64 b)
		{
			return a * b;
		}
		/// <summary>
		///The size of an object in memory or disk
		/// </summary>

		public int SizeOf()
		{
			return sizeof(UInt64);
		}
		/// <summary>
		/// Read a binary vector of UInt64
		/// </summary>

		public void ReadBinaryVector (IList<UInt64> vec, Stream r, int dim)
		{
			int len = sizeof(UInt64) * dim;
			byte[] buff = new byte[len];
			r.Read (buff, 0, len);
			//MemoryStream m = new MemoryStream (buff);
			BinaryReader m = new BinaryReader (new MemoryStream(buff));
			for (int i = 0; i < dim; i++) {
				vec[i] = m.ReadUInt64();
			}
		}
		// *****************
		// vector distances
		// *****************
		public double DistLP(IList<UInt64> a, IList<UInt64> b, float p, bool do_sqrt) {
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i] - b[i];
				d += Math.Pow(Math.Abs(m), p);
			}
			if (do_sqrt) {
				return Math.Pow(d, 1.0/p);
			} else {
				return d;
			}
		}
		
		/// <summary>
		/// Specialization for L2
		/// </summary>
		public double DistL2 (IList<UInt64> a, IList<UInt64> b)
		{
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i] - b[i];
				d += m * m;
			}
			return Math.Sqrt(d);
		}
		
		/// <summary>
		/// Specialization for L1
		/// </summary>
		public double DistL1(IList<UInt64> a, IList<UInt64> b) {
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i] - b[i];
				if (m < 0) {
					d -= m;
				} else {
					d += m;
				}
			}
			return d;
		}
		
		/// <summary>
		/// Specialization for L-Infinity
		/// </summary>
		public double DistLInf(IList<UInt64> a, IList<UInt64> b) {
			double d = 0;
			for (int i = 0; i < a.Count; i++) {
				double m = a[i]-b[i];
				if (m < 0) m *= -1;
				if (m > d) d = m;
			}
			return d;
		}

		/// <summary>
		/// Angle between two vectors (computing the cosine between them)
		/// </summary>

		public double DistCos(IList<UInt64> a, IList<UInt64> b) {
			var M = SimCos(a, b);
			return Math.Acos(M);
		}

		public double SimCos(IList<UInt64> a, IList<UInt64> b) {
			double sum,norm1,norm2;
			norm1=norm2=sum=0.0f;
			for(int i=0; i<a.Count; i++) {
		    	norm1+=(a[i] * a[i]);
	 	   		norm2+=(b[i] * b[i]);
	    		sum+= (a[i] * b[i]);
			}
			double M = sum/(Math.Sqrt(norm1)*Math.Sqrt(norm2));
			if (M > 1.0) {
				return 1.0;
			}
			if (M < -1.0) {
				return -1.0;
			}
			return M;
		}
	   /// <summary>
	   /// c[i] = a[i] + b[i]
	   /// </summary>
	   public void Sum(IList<UInt64> a, IList<UInt64> b, IList<UInt64> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt64)(a[i] + b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] - b[i]
	   /// </summary>
	   public void Sub(IList<UInt64> a, IList<UInt64> b, IList<UInt64> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt64)(a[i] - b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] * b[i]
	   /// </summary>
	   public void Prod(IList<UInt64> a, IList<UInt64> b, IList<UInt64> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt64)(a[i] * b[i]);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] / b[i]
	   /// </summary>
	   public void Div(IList<UInt64> a, IList<UInt64> b, IList<UInt64> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt64)(a[i] / b[i]);
	    	}
	   }

	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public void Sum(IList<UInt64> a, float b, IList<UInt64> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt64)(a[i] + b);
	    	}
	   }

	   public double Sum(IList<UInt64> a)
	   {
	   		var len = a.Count;
	   		double s = 0;
	    	for (int i = 0; i < len; ++i) {
	    		s += a[i];
	    	}
	    	return s;
	   }
	   
	   public double Mean(IList<UInt64> a)
	   {
	   		return this.Sum(a) / a.Count;
	   }
	   
	   public double Max(IList<UInt64> a, out int pos)
	   {
	   		var len = a.Count;
	   		double s = a[0];
	   		pos = 0;
	    	for (int i = 1; i < len; ++i) {
	    		var x = a[i];
	    		if (s < x) {
	    			s = x;
	    			pos = i;
	    		}
	    	}
	    	return s;
	   }
	   
	   public double Min(IList<UInt64> a, out int pos)
	   {
	   		var len = a.Count;
	   		double s = a[0];
	   		pos = 0;
	    	for (int i = 1; i < len; ++i) {
	    		var x = a[i];
	    		if (s > x) {
	    			s = x;
	    			pos = i;
	    		}
	    	}
	    	return s;
	   }

	   public double Var(IList<UInt64> a, double mean)
	   {
	   		var len = a.Count;
	   		double s = 0;
	    	for (int i = 0; i < len; ++i) {
	    		var x = a[i] - mean;
	    		s += x*x;
	    	}
	    	return s / len;
	   }

	   public double StdDev(IList<UInt64> a, double mean)
	   {
	   		return Math.Sqrt(Var(a, mean));
	   }
	   
   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public void Prod (IList<UInt64> a, float b, IList<UInt64> c)
	   {
	   		var len = a.Count;
	    	for (int i = 0; i < len; ++i) {
	    		c[i] = (UInt64)(a[i] * b);
	    	}
	   }
	   
	   /// <summary>
	   /// c[i] = a[i] + b
	   /// </summary>
	   public UInt64 SumSingle(UInt64 a, float b)
	   {
			return (UInt64) (a + b);
	   }

   	   /// <summary>
	   /// c[i] = a[i] * b
	   /// </summary>
	   public UInt64 ProdSingle (UInt64 a, float b)
	   {
   			return (UInt64) (a * b);
	   }
	 	   	   
    }
}