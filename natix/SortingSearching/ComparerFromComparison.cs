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
//   Original filename: natix/SortingSearching/ComparerFromComparison.cs
// 
using System;
using System.Collections;
using System.Collections.Generic;

namespace natix.SortingSearching
{
	
	/// <summary>
	/// Wraps a comparison function with a comparer interface
	/// </summary>
	public class ComparerFromComparison<TKey> : IComparer<TKey>
	{
		Comparison<TKey> cmp;
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="cmp">
		/// Comparison function
		/// </param>
		public ComparerFromComparison (Comparison<TKey> cmp)
		{
			this.cmp = cmp;
		}
		
		/// <summary>
		/// performs a comparison
		/// </summary>
		/// <param name="x">
		/// </param>
		/// <param name="y">
		/// </param>
		/// <returns>
		/// A <see cref="System.Int32"/>
		/// </returns>
		public int Compare (TKey x, TKey y)
		{
			return this.cmp (x, y);
		}
	}
}
