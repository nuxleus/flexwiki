#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Collections;
using System.Text;
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("Array", "Holds an ordered sequence of objects.")]
	public class BELArray : BELObject
	{
		public BELArray() : base()
		{
		}

		public BELArray(ArrayList list) : base()
		{
			_Array = new ArrayList();
			foreach (object x in list)
				Add(x);
		}

		public void Add(object arg)
		{
			_Array.Add(BELObject.ConvertToBELObjectIfNeeded(arg));
		}

		public ArrayList Array
		{
			get
			{
				return _Array;
			}
		}

		ArrayList _Array = new ArrayList();

		public override IOutputSequence ToOutputSequence()
		{
			CompositeWikiSequence seq = new CompositeWikiSequence();
			foreach (IWikiSequenceProducer each in _Array)
				seq.Add(each.ToOutputSequence());
			return seq;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Convert this object to a Presentation")]
		public IPresentation ToPresentation(ExecutionContext ctx)
		{
			return ToOutputSequence().ToPresentation(ctx.Presenter);
		}


		public override string ToString()
		{
			StringBuilder b = new StringBuilder();
			foreach (object each in _Array)
				b.Append(each.ToString());
			return b.ToString();
		}

		[ExposedMethod("ToOneString", ExposedMethodFlags.CachePolicyNone, "Answer a single string that is the concatenation of all of the objects in the array converted to strings")]
		public string ToOneString
		{
			get
			{
				StringBuilder b = new StringBuilder();
				foreach (IBELObject each in _Array)
					b.Append(each.ToString());
				return b.ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the number of objects in the array")]
		public int Count
		{
			get
			{
				return _Array.Count;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the item at the given index")]
		public IBELObject Item(int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException("Index (" + index + ") out of valid range: 0.." + Count.ToString());
			return (IBELObject)(Array[index]);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Evalute the given block once for each object in the array; answer a new array containing the result of each block evaluation.")]
		public BELArray Collect(ExecutionContext ctx, Block block)
		{
			BELArray answer = new BELArray();
			
			foreach (IBELObject each in Array)
			{
				ArrayList parms = new ArrayList();
				parms.Add(each);
				answer.Add(block.Value(ctx, parms));
			}
			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Evaluate the block for each item inthe array; answer a new Array that includes on those objects for which the block evaluated to true.")]
		public BELArray Select(ExecutionContext ctx, Block block)
		{
			BELArray answer = new BELArray();
			
			foreach (IBELObject each in Array)
			{
				ArrayList parms = new ArrayList();
				parms.Add(each);
				IBELObject objValue = block.Value(ctx, parms);
				BELBoolean test = objValue as BELBoolean;
				if (test == null)
					throw new ExecutionException("Select block must evaluate to a boolean.  Got " + BELType.BELTypeForType(objValue.GetType()).ExternalTypeName + " instead.");
				if (test.Value)
					answer.Add(each);
			}
			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a new array that is a copy of this array, but trimmed to have no more than the given number of objects.")]
		public BELArray Snip(int maxCount)
		{
			BELArray answer = new BELArray();			
			foreach (IBELObject each in Array)
			{
				answer.Add(each);
				if (answer.Count >= maxCount)
					return answer;
			}
			return answer;
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Sort the array; answer a new array (that is sorted)")]
		public BELArray Sort()
		{
			ArrayList answer = new ArrayList(Array);
			try 
			{
				answer.Sort();
			}
			catch (InvalidOperationException e)
			{
				throw e.InnerException;
			}

			return new BELArray(answer);
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Sort the array; evaluate the block for each object in the array to determine the object used to order that item in the sort")]
		public BELArray SortBy(ExecutionContext ctx, Block block)
		{
			ArrayList answer = new ArrayList(Array);
			try 
			{
				answer.Sort(new CustomSorter(block, ctx));
			}
			catch (InvalidOperationException e)
			{
				throw e.InnerException;
			}

			return new BELArray(answer);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if this object is empty, else false")]
		public bool IsEmpty
		{
			get
			{
				return Count == 0;
			}
		}

		class DefaultSorter : IComparer
		{
			#region IComparer Members

			public DefaultSorter()
			{
			}

			public int Compare(object x, object y)
			{
				IComparable	xc = x as IComparable;
				if (xc == null)
					throw new ExecutionException("Can not compare objects of type " + BELType.ExternalTypeNameForType(x.GetType()));
				IComparable	yc = y as IComparable;
				if (yc == null)
					throw new ExecutionException("Can not compare objects of type " + BELType.ExternalTypeNameForType(y.GetType()));

				return xc.CompareTo(yc);
			}

			#endregion
		}


		class CustomSorter : IComparer
		{
			#region IComparer Members

			Block block;
			ExecutionContext ctx;

			public CustomSorter(Block b, ExecutionContext c)
			{
				block = b;
				ctx = c;
			}

			public int Compare(object x, object y)
			{
				ArrayList a1 = new ArrayList();
				a1.Add(x);
				IBELObject xKey = block.Value(ctx, a1);

				ArrayList a2 = new ArrayList();
				a2.Add(y);
				IBELObject yKey = block.Value(ctx, a2);

				IComparable	xc = xKey as IComparable;
				if (xc == null)
					throw new ExecutionException("Can not compare objects of type " + BELType.ExternalTypeNameForType(xKey.GetType()));
				IComparable	yc = yKey as IComparable;
				if (yc == null)
					throw new ExecutionException("Can not compare objects of type " + BELType.ExternalTypeNameForType(yKey.GetType()));

				int a = xc.CompareTo(yc);
				return a;
			}

			#endregion
		}


	}
}
