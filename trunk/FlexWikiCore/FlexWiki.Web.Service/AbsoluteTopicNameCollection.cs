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
	/// A collection of elements of type AbsoluteTopicName
	/// </summary>
	public class AbsoluteTopicNameCollection: System.Collections.CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the AbsoluteTopicNameCollection class.
		/// </summary>
		public AbsoluteTopicNameCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the AbsoluteTopicNameCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new AbsoluteTopicNameCollection.
		/// </param>
		public AbsoluteTopicNameCollection(AbsoluteTopicName[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the AbsoluteTopicNameCollection class, containing elements
		/// copied from another instance of AbsoluteTopicNameCollection
		/// </summary>
		/// <param name="items">
		/// The AbsoluteTopicNameCollection whose elements are to be added to the new AbsoluteTopicNameCollection.
		/// </param>
		public AbsoluteTopicNameCollection(AbsoluteTopicNameCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this AbsoluteTopicNameCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this AbsoluteTopicNameCollection.
		/// </param>
		public virtual void AddRange(AbsoluteTopicName[] items)
		{
			foreach (AbsoluteTopicName item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another AbsoluteTopicNameCollection to the end of this AbsoluteTopicNameCollection.
		/// </summary>
		/// <param name="items">
		/// The AbsoluteTopicNameCollection whose elements are to be added to the end of this AbsoluteTopicNameCollection.
		/// </param>
		public virtual void AddRange(AbsoluteTopicNameCollection items)
		{
			foreach (AbsoluteTopicName item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type AbsoluteTopicName to the end of this AbsoluteTopicNameCollection.
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicName to be added to the end of this AbsoluteTopicNameCollection.
		/// </param>
		public virtual void Add(AbsoluteTopicName value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic AbsoluteTopicName value is in this AbsoluteTopicNameCollection.
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicName value to locate in this AbsoluteTopicNameCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this AbsoluteTopicNameCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(AbsoluteTopicName value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this AbsoluteTopicNameCollection
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicName value to locate in the AbsoluteTopicNameCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(AbsoluteTopicName value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the AbsoluteTopicNameCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the AbsoluteTopicName is to be inserted.
		/// </param>
		/// <param name="value">
		/// The AbsoluteTopicName to insert.
		/// </param>
		public virtual void Insert(int index, AbsoluteTopicName value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the AbsoluteTopicName at the given index in this AbsoluteTopicNameCollection.
		/// </summary>
		public virtual AbsoluteTopicName this[int index]
		{
			get
			{
				return (AbsoluteTopicName) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific AbsoluteTopicName from this AbsoluteTopicNameCollection.
		/// </summary>
		/// <param name="value">
		/// The AbsoluteTopicName value to remove from this AbsoluteTopicNameCollection.
		/// </param>
		public virtual void Remove(AbsoluteTopicName value)
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
		/// Type-specific enumeration class, used by AbsoluteTopicNameCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: System.Collections.IEnumerator
		{
			private System.Collections.IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(AbsoluteTopicNameCollection collection)
			{
				this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
			}

			/// <summary>
			/// 
			/// </summary>
			public AbsoluteTopicName Current
			{
				get
				{
					return (AbsoluteTopicName) (this.wrapped.Current);
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return (AbsoluteTopicName) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this AbsoluteTopicNameCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual AbsoluteTopicNameCollection.Enumerator GetEnumerator()
		{
			return new AbsoluteTopicNameCollection.Enumerator(this);
		}
	}
}
