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
	/// A collection of elements of type ContentBase
	/// </summary>
	public class ContentBaseCollection: System.Collections.CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the ContentBaseCollection class.
		/// </summary>
		public ContentBaseCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the ContentBaseCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new ContentBaseCollection.
		/// </param>
		public ContentBaseCollection(ContentBase[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the ContentBaseCollection class, containing elements
		/// copied from another instance of ContentBaseCollection
		/// </summary>
		/// <param name="items">
		/// The ContentBaseCollection whose elements are to be added to the new ContentBaseCollection.
		/// </param>
		public ContentBaseCollection(ContentBaseCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this ContentBaseCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this ContentBaseCollection.
		/// </param>
		public virtual void AddRange(ContentBase[] items)
		{
			foreach (ContentBase item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another ContentBaseCollection to the end of this ContentBaseCollection.
		/// </summary>
		/// <param name="items">
		/// The ContentBaseCollection whose elements are to be added to the end of this ContentBaseCollection.
		/// </param>
		public virtual void AddRange(ContentBaseCollection items)
		{
			foreach (ContentBase item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type ContentBase to the end of this ContentBaseCollection.
		/// </summary>
		/// <param name="value">
		/// The ContentBase to be added to the end of this ContentBaseCollection.
		/// </param>
		public virtual void Add(ContentBase value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic ContentBase value is in this ContentBaseCollection.
		/// </summary>
		/// <param name="value">
		/// The ContentBase value to locate in this ContentBaseCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this ContentBaseCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(ContentBase value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this ContentBaseCollection
		/// </summary>
		/// <param name="value">
		/// The ContentBase value to locate in the ContentBaseCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(ContentBase value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the ContentBaseCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the ContentBase is to be inserted.
		/// </param>
		/// <param name="value">
		/// The ContentBase to insert.
		/// </param>
		public virtual void Insert(int index, ContentBase value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the ContentBase at the given index in this ContentBaseCollection.
		/// </summary>
		public virtual ContentBase this[int index]
		{
			get
			{
				return (ContentBase) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific ContentBase from this ContentBaseCollection.
		/// </summary>
		/// <param name="value">
		/// The ContentBase value to remove from this ContentBaseCollection.
		/// </param>
		public virtual void Remove(ContentBase value)
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
		/// Type-specific enumeration class, used by ContentBaseCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: System.Collections.IEnumerator
		{
			private System.Collections.IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(ContentBaseCollection collection)
			{
				this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
			}

			/// <summary>
			/// 
			/// </summary>
			public ContentBase Current
			{
				get
				{
					return (ContentBase) (this.wrapped.Current);
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return (ContentBase) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this ContentBaseCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual ContentBaseCollection.Enumerator GetEnumerator()
		{
			return new ContentBaseCollection.Enumerator(this);
		}
	}
}
