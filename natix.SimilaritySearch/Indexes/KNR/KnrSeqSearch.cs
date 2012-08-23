// 
//  Copyright 2012  sadit
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.IO;
using System.Collections.Generic;
using natix.CompactDS;
using natix.Sets;
using natix.SortingSearching;

namespace natix.SimilaritySearch
{
	public class KnrSeqSearch : BasicIndex
	{
		public int K;
		public int MAXCAND;
		public Index R;
		public IRankSelectSeq SEQ;

		public override void Save (BinaryWriter Output)
		{
			base.Save(Output);
			Output.Write(this.K);
			Output.Write(this.MAXCAND);
			SpaceGenericIO.Save(Output, this.R.DB, false);
			var refs = this.R.DB;
			this.R.DB = new NullSpace();
			IndexGenericIO.Save(Output, this.R);
			this.R.DB = refs;
			RankSelectSeqGenericIO.Save(Output, this.SEQ);
		}

		public override void Load (BinaryReader Input)
		{
			base.Load(Input);
			this.K = Input.ReadInt32 ();
			this.MAXCAND = Input.ReadInt32 ();
			var refs = SpaceGenericIO.Load(Input, false);
			this.R = IndexGenericIO.Load(Input);
			this.R.DB = refs;
			this.SEQ = RankSelectSeqGenericIO.Load(Input);
		}

		public KnrSeqSearch () : base()
		{
		}

		public KnrSeqSearch (KnrSeqSearch other) : base()
		{
			this.DB = other.DB;
			this.K = other.K;
			this.MAXCAND = other.MAXCAND;
			this.R = other.R;
			this.SEQ = other.SEQ;
		}

		public void Build (MetricDB db, Index refs, int K=7, int maxcand=1024, SequenceBuilder seq_builder = null)
		{
			int n = db.Count;
			this.DB = db;
			this.R = refs;
			this.K = K;
			this.MAXCAND = maxcand;
			int[] G = new int[n * this.K];
			for (int i = 0; i < n; ++i) {
				if (i % 1000 == 0) {
					Console.WriteLine ("computing knr {0}/{1} (adv. {2:0.00}%, curr. time: {3})", i, n, i*100.0/n, DateTime.Now);
				}
				var u = this.DB[i];
				var useq = this.GetKnr(u);
				for (int j = 0; j < this.K; ++j) {
					G[i*this.K+j] = useq[j];
				}
			}
			if (seq_builder == null) {
				seq_builder = SequenceBuilders.GetSeqXLB_SArray64 (16);
			}
			this.SEQ = seq_builder(G, this.R.DB.Count);
		}

		public int[] GetKnr (object q)
		{
			var res = this.R.SearchKNN(q, this.K);
			var qseq = new  int[this.K];
			int i = 0;
			foreach (var s in res) {
				qseq[i] = s.docid;
				++i;
			}
			return qseq;
		}

		public IList<UInt16> GetStoredKnr (int docid)
		{
			var L = new ushort[this.K];
			for (int i = 0, start_pos = this.K * docid; i < this.K; ++i) {
				L [i] = (ushort)this.SEQ.Access (start_pos + i);
			}
			return L;
		}
		 
		/// <summary>
		/// Gets the candidates. 
		/// </summary>
		protected virtual IResult GetCandidatesSmallDB (IList<int> qseq, int maxcand)
		{
			var len_qseq = qseq.Count;
			var n = this.DB.Count;
			var A = new byte[this.DB.Count];
			for (int i = 0; i < len_qseq; ++i) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var count1 = rs.Count1;
				for (int j = 1; j <= count1; ++j) {
					var pos = rs.Select1 (j);
					if (pos % this.K == i) {
						var docid = pos / this.K;
						if (A [docid] == i) {
							A [docid] += 1;
						}
					}
				}
			}
			var res = new ResultTies (Math.Abs (maxcand), false);
			for (int i = 0; i < n; ++i) {
				if (A [i] == 0) {
					continue;
				}
				res.Push (i, -A [i]);
			}
			return res;
		}

		protected virtual IResult GetCandidates (IList<int> qseq, int maxcand)
		{
			var n = this.DB.Count;
			if (n < 500000) {
				return this.GetCandidatesSmallDB (qseq, maxcand);
			}
			var len_qseq = qseq.Count;
			var ialg = new BaezaYatesIntersection<int> (new DoublingSearch<int> ());
			IList<int> C = new SortedListRS (this.SEQ.Unravel (qseq [0]));
			int i = 1;
			while (i < len_qseq && C.Count > maxcand) {
				var rs = this.SEQ.Unravel (qseq [i]);
				var I = new ShiftedSortedListRS (rs, -i);
				var L = new List<IList<int>> () {C, I};
				var tmp = ialg.Intersection (L);
				++i;
				if (tmp.Count < maxcand) {
					break;
				}
				C = tmp;
			}
			var res = new ResultTies (int.MaxValue, false);
			foreach (var c in C) {
				if (c % this.K == 0) {
					res.Push (c / this.K, 0);
				}
			}
			return res;
		}

		public override IResult SearchKNN (object q, int knn, IResult res)
		{
			var qseq = this.GetKnr (q);
			// this.GetCandidatesIntersection (qseq, out C_docs, out C_sim);
			// this.GetCandidatesRelativeMatches (qseq, out C_docs, out C_sim);
			var C = this.GetCandidates (qseq, Math.Abs(this.MAXCAND));
			int maxcand = this.MAXCAND;
			if (maxcand < 0) {
				return C;
			} else {
				foreach (var p in C) {
					var docid = p.docid;
					double d = this.DB.Dist (q, this.DB [docid]);
					res.Push (docid, d);
					--maxcand;
					if (maxcand <= 0) {
						break;
					}
				}
			}
			return res;
		}

		public override IResult SearchRange (object q, double radius)
		{
			var qseq = this.GetKnr (q);
			// this.GetCandidatesIntersection (qseq, out C_docs, out C_sim);
			// this.GetCandidatesRelativeMatches (qseq, out C_docs, out C_sim);
			var C = this.GetCandidates (qseq, Math.Abs(this.MAXCAND));
			return this.FilterRadiusByRealDistances(q, radius, C, Math.Abs(this.MAXCAND));
		}
	}
}