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
using System.Xml; 
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 

using FlexWiki.FwSyncLib;

namespace FlexWiki.FwSync
{
	public class FwEditServiceProxy : EditServiceProxy, IEditServiceProxy
	{
		public string Uri
		{
			get { return this.Url; }
			set { this.Url = value; }
		}

		ContentBase[] IEditServiceProxy.GetAllNamespaces()
		{
			return base.GetAllNamespaces();
		}

		AbsoluteTopicName[] IEditServiceProxy.GetAllTopics(ContentBase cb)
		{
			return base.GetAllTopics(cb); 
		}

		string IEditServiceProxy.GetTextForTopic(LocalTopic topic)
		{
			AbsoluteTopicName atn = new AbsoluteTopicName(); 
			atn.Namespace = topic.Namespace.Name; 
			atn.Name = topic.Name; 
			return base.GetTextForTopic(atn); 
		}

	}
}

