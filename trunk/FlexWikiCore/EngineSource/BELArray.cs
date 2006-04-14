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

		[ExposedMethod( ExposedMethodFlags.NeedContext, "Convert this object to a Presentation")]
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

		[ExposedMethod("ToOneString", ExposedMethodFlags.Default, "Answer a single string that is the concatenation of all of the objects in the array converted to strings")]
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

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of objects in the array")]
		public int Count
		{
			get
			{
				return _Array.Count;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the item at the given index")]
		public IBELObject Item(int index)
		{
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException("Index (" + index + ") out of valid range: 0.." + Count.ToString());
			return (IBELObject)(Array[index]);
		}

		[ExposedMethod( ExposedMethodFlags.NeedContext, "Evalute the given block once for each object in the array; answer a new array containing the result of each block evaluation.")]
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

		[ExposedMethod( ExposedMethodFlags.NeedContext, "Evaluate the block for each item inthe array; answer a new Array that includes on those objects for which the block evaluated to true.")]
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
					throw new ExecutionException(ctx.CurrentLocation, "Select block must evaluate to a boolean.  Got " + BELType.BELTypeForType(objValue.GetType()).ExternalTypeName + " instead.");
				if (test.Value)
					answer.Add(each);
			}
			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a new array that is a copy of this array, but trimmed to have no more than the given number of objects.")]
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


		[ExposedMethod(ExposedMethodFlags.Default, "Sort the array; answer a new array (that is sorted)")]
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


		[ExposedMethod( ExposedMethodFlags.NeedContext, "Sort the array; evaluate the block for each object in the array to determine the object used to order that item in the sort")]
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

		[ExposedMethod(ExposedMethodFlags.Default, "Answer true if this object is empty, else false")]
		public bool IsEmpty
		{
			get
			{
				return Count == 0;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the unique elements in this array")]
		public ArrayList Unique()
		{
			Hashtable hashTable = new Hashtable();
			ArrayList uniqueElements = new ArrayList();
			foreach (object element in Array)
			{
				try
				{
					string key = element.ToString();
					hashTable.Add(key, element);
					uniqueElements.Add(element);
				}
				catch (ArgumentException)
				{
					// This is expected when a non-unique element is added for the second or subsequent times.
				}
			}
			return uniqueElements;
		}



	}
}
