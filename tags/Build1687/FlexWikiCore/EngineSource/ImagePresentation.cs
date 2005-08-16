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
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("ImagePresentation", "Presents an image")]
	public class ImagePresentation : PresentationPrimitive
	{

		private string _Title;
		private string _URL;
		private string _Height;
		private string _Width;
		private string _LinkToURL;

		public ImagePresentation(string title, string URL, string linkToURL, string height, string width)
		{
			Init(title, URL, linkToURL, height, width, null);
		}
		public ImagePresentation(string title, string URL, string linkToURL, string height, string width, string attributes)
		{
			Init(title, URL, linkToURL, height, width, attributes);
		}
		private void Init(string title, string URL, string linkToURL, string height, string width, string attributes)
		{
			_LinkToURL = linkToURL;
			_Title = title;
			_URL = URL;
			_Height = height;
			_Width = width;
			_attributes = attributes;
		}

		string LinkToURL
		{
			get
			{
				return _LinkToURL;
			}
		}

		string Title
		{
			get
			{
				return _Title;
			}
		}

		string URL
		{
			get
			{
				return _URL;
			}
		}

		string Height
		{
			get
			{
				return _Height;
			}
		}

		string Width
		{
			get
			{
				return _Width;
			}
		}

		public override void OutputTo(WikiOutput output)
		{
			output.WriteImage(Title, URL, LinkToURL, Height, Width, Attributes);
		}

	}
}
