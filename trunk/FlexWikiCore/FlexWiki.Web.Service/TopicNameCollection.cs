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
	/// A collection of elements of type TopicName
	/// </summary>
	public class TopicNameCollection: System.Collections.CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the TopicNameCollection class.
		/// </summary>
		public TopicNameCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the TopicNameCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new TopicNameCollection.
		/// </param>
		public TopicNameCollection(TopicName[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the TopicNameCollection class, containing elements
		/// copied from another instance of TopicNameCollection
		/// </summary>
		/// <param name="items">
		/// The TopicNameCollection whose elements are to be added to the new TopicNameCollection.
		/// </param>
		public TopicNameCollection(TopicNameCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this TopicNameCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this TopicNameCollection.
		/// </param>
		public virtual void AddRange(TopicName[] items)
		{
			foreach (TopicName item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another TopicNameCollection to the end of this TopicNameCollection.
		/// </summary>
		/// <param name="items">
		/// The TopicNameCollection whose elements are to be added to the end of this TopicNameCollection.
		/// </param>
		public virtual void AddRange(TopicNameCollection items)
		{
			foreach (TopicName item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type TopicName to the end of this TopicNameCollection.
		/// </summary>
		/// <param name="value">
		/// The TopicName to be added to the end of this TopicNameCollection.
		/// </param>
		public virtual void Add(TopicName value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic TopicName value is in this TopicNameCollection.
		/// </summary>
		/// <param name="value">
		/// The TopicName value to locate in this TopicNameCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this TopicNameCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(TopicName value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this TopicNameCollection
		/// </summary>
		/// <param name="value">
		/// The TopicName value to locate in the TopicNameCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(TopicName value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the TopicNameCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the TopicName is to be inserted.
		/// </param>
		/// <param name="value">
		/// The TopicName to insert.
		/// </param>
		public virtual void Insert(int index, TopicName value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the TopicName at the given index in this TopicNameCollection.
		/// </summary>
		public virtual TopicName this[int index]
		{
			get
			{
				return (TopicName) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific TopicName from this TopicNameCollection.
		/// </summary>
		/// <param name="value">
		/// The TopicName value to remove from this TopicNameCollection.
		/// </param>
		public virtual void Remove(TopicName value)
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
		/// Type-specific enumeration class, used by TopicNameCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: System.Collections.IEnumerator
		{
			private System.Collections.IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(TopicNameCollection collection)
			{
				this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
			}

			/// <summary>
			/// 
			/// </summary>
			public TopicName Current
			{
				get
				{
					return (TopicName) (this.wrapped.Current);
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return (TopicName) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this TopicNameCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual TopicNameCollection.Enumerator GetEnumerator()
		{
			return new TopicNameCollection.Enumerator(this);
		}
	}
}
