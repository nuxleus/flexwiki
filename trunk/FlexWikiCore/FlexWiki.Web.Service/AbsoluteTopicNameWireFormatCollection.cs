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

namespace FlexWiki.Web.Services
{
	/// <summary>
	/// A collection of elements of type AbsoluteTopicNameWireFormat
	/// </summary>
	public class AbsoluteTopicNameWireFormatCollection: System.Collections.CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the AbsoluteTopicNameWireFormatCollection class.
		/// </summary>
		public AbsoluteTopicNameWireFormatCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the AbsoluteTopicNameWireFormatCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new AbsoluteTopicNameWireFormatCollection.
		/// </param>
		public AbsoluteTopicNameWireFormatCollection(AbsoluteTopicNameWireFormat[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the AbsoluteTopicNameWireFormatCollection class, containing elements
		/// copied from another instance of AbsoluteTopicNameWireFormatCollection
		/// </summary>
		/// <param name="items">
		/// The AbsoluteTopicNameWireFormatCollection whose elements are to be added to the new AbsoluteTopicNameWireFormatCollection.
		/// </param>
		public AbsoluteTopicNameWireFormatCollection(AbsoluteTopicNameWireFormatCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this AbsoluteTopicNameWireFormatCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this AbsoluteTopicNameWireFormatCollection.
		/// </param>
		public virtual void AddRange(AbsoluteTopicNameWireFormat[] items)
		{
			foreach (AbsoluteTopicNameWireFormat item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another AbsoluteTopicNameWireFormatCollection to the end of this AbsoluteTopicNameWireFormatCollection.
		/// </summary>
		/// <param name="items">
		/// The AbsoluteTopicNameWireFormatCollection whose elements are to be added to the end of this AbsoluteTopicNameWireFormatCollection.
		/// </param>
		public virtual void AddRange(AbsoluteTopicNameWireFormatCollection items)
		{
			foreach (AbsoluteTopicNameWireFormat item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type AbsoluteTopicNameWireFormat to the end of this AbsoluteTopicNameWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicNameWireFormat to be added to the end of this AbsoluteTopicNameWireFormatCollection.
		/// </param>
		public virtual void Add(AbsoluteTopicNameWireFormat value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic AbsoluteTopicNameWireFormat value is in this AbsoluteTopicNameWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicNameWireFormat value to locate in this AbsoluteTopicNameWireFormatCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this AbsoluteTopicNameWireFormatCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(AbsoluteTopicNameWireFormat value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this AbsoluteTopicNameWireFormatCollection
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicNameWireFormat value to locate in the AbsoluteTopicNameWireFormatCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(AbsoluteTopicNameWireFormat value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the AbsoluteTopicNameWireFormatCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the AbsoluteTopicNameWireFormat is to be inserted.
		/// </param>
		/// <param name="value">
		/// The AbsoluteTopicNameWireFormat to insert.
		/// </param>
		public virtual void Insert(int index, AbsoluteTopicNameWireFormat value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the AbsoluteTopicNameWireFormat at the given index in this AbsoluteTopicNameWireFormatCollection.
		/// </summary>
		public virtual AbsoluteTopicNameWireFormat this[int index]
		{
			get
			{
				return (AbsoluteTopicNameWireFormat) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific AbsoluteTopicNameWireFormat from this AbsoluteTopicNameWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicNameWireFormat value to remove from this AbsoluteTopicNameWireFormatCollection.
		/// </param>
		public virtual void Remove(AbsoluteTopicNameWireFormat value)
		{
			this.List.Remove(value);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Sort()
		{
			this.InnerList.Sort();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="comparer"></param>
		public void Sort(IComparer comparer)
		{
			this.InnerList.Sort(comparer);
		}

		/// <summary>
		/// Type-specific enumeration class, used by AbsoluteTopicNameWireFormatCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: System.Collections.IEnumerator
		{
			private System.Collections.IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(AbsoluteTopicNameWireFormatCollection collection)
			{
				this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
			}

			/// <summary>
			/// 
			/// </summary>
			public AbsoluteTopicNameWireFormat Current
			{
				get
				{
					return (AbsoluteTopicNameWireFormat) (this.wrapped.Current);
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return (AbsoluteTopicNameWireFormat) (this.wrapped.Current);
				}
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public bool MoveNext()
			{
				return this.wrapped.MoveNext();
			}

			/// <summary>
			/// 
			/// </summary>
			public void Reset()
			{
				this.wrapped.Reset();
			}
		}

		/// <summary>
		/// Returns an enumerator that can iterate through the elements of this AbsoluteTopicNameWireFormatCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual AbsoluteTopicNameWireFormatCollection.Enumerator GetEnumerator()
		{
			return new AbsoluteTopicNameWireFormatCollection.Enumerator(this);
		}
	}
}
