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
	/// A collection of elements of type TopicNameWireFormat
	/// </summary>
	public class TopicNameWireFormatCollection: System.Collections.CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the TopicNameWireFormatCollection class.
		/// </summary>
		public TopicNameWireFormatCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the TopicNameWireFormatCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new TopicNameWireFormatCollection.
		/// </param>
		public TopicNameWireFormatCollection(TopicNameWireFormat[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the TopicNameWireFormatCollection class, containing elements
		/// copied from another instance of TopicNameWireFormatCollection
		/// </summary>
		/// <param name="items">
		/// The TopicNameWireFormatCollection whose elements are to be added to the new TopicNameWireFormatCollection.
		/// </param>
		public TopicNameWireFormatCollection(TopicNameWireFormatCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this TopicNameWireFormatCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this TopicNameWireFormatCollection.
		/// </param>
		public virtual void AddRange(TopicNameWireFormat[] items)
		{
			foreach (TopicNameWireFormat item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another TopicNameWireFormatCollection to the end of this TopicNameWireFormatCollection.
		/// </summary>
		/// <param name="items">
		/// The TopicNameWireFormatCollection whose elements are to be added to the end of this TopicNameWireFormatCollection.
		/// </param>
		public virtual void AddRange(TopicNameWireFormatCollection items)
		{
			foreach (TopicNameWireFormat item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type TopicNameWireFormat to the end of this TopicNameWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The TopicNameWireFormat to be added to the end of this TopicNameWireFormatCollection.
		/// </param>
		public virtual void Add(TopicNameWireFormat value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic TopicNameWireFormat value is in this TopicNameWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The TopicNameWireFormat value to locate in this TopicNameWireFormatCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this TopicNameWireFormatCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(TopicNameWireFormat value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this TopicNameWireFormatCollection
		/// </summary>
		/// <param name="value">
		/// The TopicNameWireFormat value to locate in the TopicNameWireFormatCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(TopicNameWireFormat value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the TopicNameWireFormatCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the TopicNameWireFormat is to be inserted.
		/// </param>
		/// <param name="value">
		/// The TopicNameWireFormat to insert.
		/// </param>
		public virtual void Insert(int index, TopicNameWireFormat value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the TopicNameWireFormat at the given index in this TopicNameWireFormatCollection.
		/// </summary>
		public virtual TopicNameWireFormat this[int index]
		{
			get
			{
				return (TopicNameWireFormat) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific TopicNameWireFormat from this TopicNameWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The TopicNameWireFormat value to remove from this TopicNameWireFormatCollection.
		/// </param>
		public virtual void Remove(TopicNameWireFormat value)
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
		/// Type-specific enumeration class, used by TopicNameWireFormatCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: System.Collections.IEnumerator
		{
			private System.Collections.IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(TopicNameWireFormatCollection collection)
			{
				this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
			}

			/// <summary>
			/// 
			/// </summary>
			public TopicNameWireFormat Current
			{
				get
				{
					return (TopicNameWireFormat) (this.wrapped.Current);
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return (TopicNameWireFormat) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this TopicNameWireFormatCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual TopicNameWireFormatCollection.Enumerator GetEnumerator()
		{
			return new TopicNameWireFormatCollection.Enumerator(this);
		}
	}
}
