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
	/// A collection of elements of type ContentBaseWireFormat
	/// </summary>
	public class ContentBaseWireFormatCollection: System.Collections.CollectionBase
	{
		/// <summary>
		/// Initializes a new empty instance of the ContentBaseWireFormatCollection class.
		/// </summary>
		public ContentBaseWireFormatCollection()
		{
			// empty
		}

		/// <summary>
		/// Initializes a new instance of the ContentBaseWireFormatCollection class, containing elements
		/// copied from an array.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the new ContentBaseWireFormatCollection.
		/// </param>
		public ContentBaseWireFormatCollection(ContentBaseWireFormat[] items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Initializes a new instance of the ContentBaseWireFormatCollection class, containing elements
		/// copied from another instance of ContentBaseWireFormatCollection
		/// </summary>
		/// <param name="items">
		/// The ContentBaseWireFormatCollection whose elements are to be added to the new ContentBaseWireFormatCollection.
		/// </param>
		public ContentBaseWireFormatCollection(ContentBaseWireFormatCollection items)
		{
			this.AddRange(items);
		}

		/// <summary>
		/// Adds the elements of an array to the end of this ContentBaseWireFormatCollection.
		/// </summary>
		/// <param name="items">
		/// The array whose elements are to be added to the end of this ContentBaseWireFormatCollection.
		/// </param>
		public virtual void AddRange(ContentBaseWireFormat[] items)
		{
			foreach (ContentBaseWireFormat item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds the elements of another ContentBaseWireFormatCollection to the end of this ContentBaseWireFormatCollection.
		/// </summary>
		/// <param name="items">
		/// The ContentBaseWireFormatCollection whose elements are to be added to the end of this ContentBaseWireFormatCollection.
		/// </param>
		public virtual void AddRange(ContentBaseWireFormatCollection items)
		{
			foreach (ContentBaseWireFormat item in items)
			{
				this.List.Add(item);
			}
		}

		/// <summary>
		/// Adds an instance of type ContentBaseWireFormat to the end of this ContentBaseWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The ContentBaseWireFormat to be added to the end of this ContentBaseWireFormatCollection.
		/// </param>
		public virtual void Add(ContentBaseWireFormat value)
		{
			this.List.Add(value);
		}

		/// <summary>
		/// Determines whether a specfic ContentBaseWireFormat value is in this ContentBaseWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The ContentBaseWireFormat value to locate in this ContentBaseWireFormatCollection.
		/// </param>
		/// <returns>
		/// true if value is found in this ContentBaseWireFormatCollection;
		/// false otherwise.
		/// </returns>
		public virtual bool Contains(ContentBaseWireFormat value)
		{
			return this.List.Contains(value);
		}

		/// <summary>
		/// Return the zero-based index of the first occurrence of a specific value
		/// in this ContentBaseWireFormatCollection
		/// </summary>
		/// <param name="value">
		/// The ContentBaseWireFormat value to locate in the ContentBaseWireFormatCollection.
		/// </param>
		/// <returns>
		/// The zero-based index of the first occurrence of the _ELEMENT value if found;
		/// -1 otherwise.
		/// </returns>
		public virtual int IndexOf(ContentBaseWireFormat value)
		{
			return this.List.IndexOf(value);
		}

		/// <summary>
		/// Inserts an element into the ContentBaseWireFormatCollection at the specified index
		/// </summary>
		/// <param name="index">
		/// The index at which the ContentBaseWireFormat is to be inserted.
		/// </param>
		/// <param name="value">
		/// The ContentBaseWireFormat to insert.
		/// </param>
		public virtual void Insert(int index, ContentBaseWireFormat value)
		{
			this.List.Insert(index, value);
		}

		/// <summary>
		/// Gets or sets the ContentBaseWireFormat at the given index in this ContentBaseWireFormatCollection.
		/// </summary>
		public virtual ContentBaseWireFormat this[int index]
		{
			get
			{
				return (ContentBaseWireFormat) this.List[index];
			}
			set
			{
				this.List[index] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific ContentBaseWireFormat from this ContentBaseWireFormatCollection.
		/// </summary>
		/// <param name="value">
		/// The ContentBaseWireFormat value to remove from this ContentBaseWireFormatCollection.
		/// </param>
		public virtual void Remove(ContentBaseWireFormat value)
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
		/// Type-specific enumeration class, used by ContentBaseWireFormatCollection.GetEnumerator.
		/// </summary>
		public class Enumerator: System.Collections.IEnumerator
		{
			private System.Collections.IEnumerator wrapped;

			/// <summary>
			/// 
			/// </summary>
			/// <param name="collection"></param>
			public Enumerator(ContentBaseWireFormatCollection collection)
			{
				this.wrapped = ((System.Collections.CollectionBase)collection).GetEnumerator();
			}

			/// <summary>
			/// 
			/// </summary>
			public ContentBaseWireFormat Current
			{
				get
				{
					return (ContentBaseWireFormat) (this.wrapped.Current);
				}
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return (ContentBaseWireFormat) (this.wrapped.Current);
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
		/// Returns an enumerator that can iterate through the elements of this ContentBaseWireFormatCollection.
		/// </summary>
		/// <returns>
		/// An object that implements System.Collections.IEnumerator.
		/// </returns>        
		public new virtual ContentBaseWireFormatCollection.Enumerator GetEnumerator()
		{
			return new ContentBaseWireFormatCollection.Enumerator(this);
		}
	}
}
