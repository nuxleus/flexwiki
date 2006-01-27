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

namespace FlexWiki
{

	/// <summary>
	/// This class accepts notice of various changes from callers and aggregates them into a FederationUpdate.  
	/// It also allows callers and push and pop contexts.  When the last context is popped off, an actual 
	/// event is generated with the batched up changes.
	/// </summary>

	public class FederationUpdateGenerator 
	{
		public delegate void GenerationCompleteEventHandler(object sender, GenerationCompleteEventArgs e);

		/// <summary>
		/// This event is fired when the last generation context is popped off.  In its arguments will be 
		/// a FederationUpdate object that contains all the accumulated changes that happened during the generation.
		/// </summary>
		public event GenerationCompleteEventHandler GenerationComplete;

		// Invoke the GenerationComplete event; called whenever generation is complete change
		protected virtual void OnGenerationComplete(GenerationCompleteEventArgs e) 
		{
			if (GenerationComplete != null)
				GenerationComplete(this, e);
		}

		void ThrowMissingContext()
		{
			throw new Exception("FederationUpdateGenerator can not process changes with an empty generation context stack.  This is a coding error.");
		}

		public void RecordPropertyChange(AbsoluteTopicName topic, string propertyName, FederationUpdate.PropertyChangeType aType)
		{
			if (_Update == null)
				ThrowMissingContext();
			_Update.RecordPropertyChange(topic, propertyName, aType);
		}

		public void RecordCreatedTopic(AbsoluteTopicName name)
		{
			if (_Update == null)
				ThrowMissingContext();
			_Update.RecordCreatedTopic(name);
		}

		public void RecordDeletedTopic(AbsoluteTopicName name)
		{
			if (_Update == null)
				ThrowMissingContext();
			_Update.RecordDeletedTopic(name);
		}

		public void RecordUpdatedTopic(AbsoluteTopicName name)
		{
			if (_Update == null)
				ThrowMissingContext();
			_Update.RecordUpdatedTopic(name);
		}

		public void RecordNamespaceListChanged()
		{
			if (_Update == null)
				ThrowMissingContext();
			_Update.RecordNamespaceListChanged();
		}

		public void RecordFederationPropertiesChanged()
		{
			if (_Update == null)
				ThrowMissingContext();
			_Update.RecordFederationPropertiesChanged();
		}

		int _Depth = 0;

		FederationUpdate _Update = null;

		/// <summary>
		/// Push a generation context.  If this is the outermost context, start a new FederationUpdate to collect all the changes
		/// </summary>
		public void Push()
		{
			if (_Depth == 0)
				_Update = new FederationUpdate();
			_Depth++;
		}

		/// <summary>
		/// Pop a generation context.  If we're poping the last context, fire the GenerationComplete event with the FederationUpdate batch of changes
		/// </summary>
		public void Pop()
		{
			if (_Depth <= 0)
				return;
			_Depth--;
			if (_Depth == 0)
				OnGenerationComplete(new GenerationCompleteEventArgs(_Update));
		}

	}
}
