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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for INamespaceProvider.
	/// </summary>
	public interface INamespaceProvider
	{
		/// <summary>
		/// Called when a provider should instantiate and register all of its namespaces in a federation.  
		/// Note that when a provider is first created (e.g., via the admin UI) that this function is not 
		/// called.
		/// </summary>
		/// <param name="aFed"></param>
		void LoadNamespaces(Federation aFed);
		/// <summary>
		/// Called when a provider is first created.  Must register all associated namespaces with the federation.
		/// Can also be used to create initial content in the namespaces.
		/// Answer a read-only IList of namespaces created (as strings).
		/// </summary>
		/// <param name="aFed"></param>
		IList CreateNamespaces(Federation aFed);
		/// <summary>
		/// Called when a provider definition is changed.  Should make sure the right changes happen in the federation
		/// to reflect the updated provider definition (e.g., removing/adding/updating namespace registrations).
		/// </summary>
		/// <param name="aFed"></param>
		void UpdateNamespaces(Federation aFed);

		void SavePersistentParametersToDefinition(NamespaceProviderDefinition def);


		string Description { get; }
		IList ParameterDescriptors { get; }
		string ValidateParameter(Federation aFed, string paramID, string proposedValue, bool isCreate);
		void SetParameter(string paramID, string proposedValue);
		string GetParameter(string paramID);
		bool CanParameterBeEdited(string paramID);
		IList ValidateAggregate(Federation aFed, bool isCreate);	
		string OwnerMailingAddress {get;}
	}
}
