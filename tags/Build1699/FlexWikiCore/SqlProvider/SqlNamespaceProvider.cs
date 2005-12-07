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
using FlexWiki;

namespace FlexWiki.SqlProvider
{
	/// <summary>
	/// Summary description for SqlNamespaceProvider.
	/// </summary>
	public class SqlNamespaceProvider: INamespaceProvider
	{
		struct ConfigurationParameterNames
		{
			static public string Namespace = "Namespace";
			static public string ConnectionString = "ConnectionString";
			static public string Contact = "Contact";
			static public string Title = "Title";
			static public string Description = "Description";
		};
		
		public string OwnerMailingAddress
		{
			get
			{
				return Contact;
			}
		}

		public SqlNamespaceProvider()
		{
		}

		/// <summary>
		/// Called when a provider should instantiate and register all of its namespaces in a federation.  
		/// Note that when a provider is first created (e.g., via the admin UI) that this function is not 
		/// called.
		/// </summary>
		/// <param name="aFed"></param>
		public void LoadNamespaces(Federation aFed)
		{
			SqlStore store = new SqlStore(aFed, Namespace, ConnectionString);
			aFed.RegisterNamespace(store);
		}

		/// <summary>
		/// Called when a provider is first created.  Must register all associated namespaces with the federation.
		/// Can also be used to create initial content in the namespaces.
		/// </summary>
		/// <param name="aFed"></param>
		public IList CreateNamespaces(Federation aFed)
		{
			ArrayList answer = new ArrayList();

			if( SqlNamespaceProvider.Exists(Namespace, ConnectionString) )
			{
				throw new Exception("Namespace with the specified name already exists.");
			}

			this.CreateNamespace(Namespace, ConnectionString);

			SqlStore store = new SqlStore(aFed, Namespace, ConnectionString);
			aFed.RegisterNamespace(store);
			store.WriteTopic(store.DefinitionTopicName.LocalName, "");
			store.SetFieldValue(store.DefinitionTopicName.LocalName, "Contact", Contact, false);
			store.SetFieldValue(store.DefinitionTopicName.LocalName, "Title", Title, false);
			string defaultImportedNamespaces = System.Configuration.ConfigurationSettings.AppSettings["DefaultImportedNamespaces"];
			if (defaultImportedNamespaces != null)
				store.SetFieldValue(store.DefinitionTopicName.LocalName, "Import", defaultImportedNamespaces, false);

			// whoever is last should write a new version
			store.SetFieldValue(store.DefinitionTopicName.LocalName, "Description", NamespaceDescription, true);
			answer.Add(Namespace);
			return ArrayList.ReadOnly(answer);
		}

		/// <summary>
		/// Called when a provider definition is changed.  Should make sure the right changes happen in the federation
		/// to reflect the updated provider definition (e.g., removing/adding/updating namespace registrations).
		/// </summary>
		/// <param name="aFed"></param>
		public void UpdateNamespaces(Federation aFed)
		{
			// just kill the old one and reload a new one
			aFed.UnregisterNamespace(aFed.ContentBaseForNamespace(Namespace));
			LoadNamespaces(aFed);
		}

		/// <summary>
		/// Method responsible to write the namespace config information to the  
		/// NamespaceMapFile.
		/// </summary>
		/// <param name="def"></param>
		public void SavePersistentParametersToDefinition(NamespaceProviderDefinition def)
		{
			def.SetParameter(ConfigurationParameterNames.Namespace, Namespace);
			def.SetParameter(ConfigurationParameterNames.ConnectionString, ConnectionString);
		}

		public string Description
		{
			get
			{
				return "Single-namespace, content stored in Sql Database.";
			}
		}

		string _Namespace = string.Empty;
		public string Namespace
		{
			get
			{
				return _Namespace;
			}
			set
			{
				_Namespace = value;
			}
		}

		string _ConnectionString = "Data Source=[Sql Server Machine Name];Initial Catalog=FlexwikiSqlStore;Integrated Security=true;";
		public string ConnectionString
		{
			get
			{
				return _ConnectionString;
			}
			set
			{
				_ConnectionString = value;
			}
		}

		string _Contact = string.Empty;
		public string Contact
		{
			get
			{
				return _Contact;
			}
			set
			{
				_Contact = value;
			}
		}

		string _Title = string.Empty;
		public string Title
		{
			get
			{
				return _Title;
			}
			set
			{
				_Title = value;
			}
		}

		
		string _NamespaceDescription = string.Empty;
		public string NamespaceDescription
		{
			get
			{
				return _NamespaceDescription;
			}
			set
			{
				_NamespaceDescription = value;
			}
		}

		public ArrayList _ParameterDescriptors;
		public IList ParameterDescriptors
		{
			get
			{
				if (_ParameterDescriptors != null)
					return ArrayList.ReadOnly(_ParameterDescriptors);
				_ParameterDescriptors = new ArrayList();
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Namespace, "Namespace", "Name for the namespace", null, true));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.ConnectionString, "ConnectionString", "ConnectionString to the database. Format: Data Source=[Sql Server Machine Name];Initial Catalog=FlexwikiSqlStore;Integrated Security=true;", null, true));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Contact, "Contact", "eMail address for contact for this namespace", null, false));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Title, "Title", "A short title for this namespace", null, false));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(ConfigurationParameterNames.Description, "Description", "A description for the namespace (use Wiki formatting)", null, false));
				return ArrayList.ReadOnly(_ParameterDescriptors);
			}
		}


		/// <summary>
		/// Validate the parameters for creating the Namespace.
		/// </summary>
		/// <param name="param">Parameters name</param>
		/// <param name="val">Parameter value to validate.</param>
		/// <returns>returns null to indicate success otherwise returns 
		/// the error message to be displayed.</returns>
		public string ValidateParameter(Federation aFed, string param, string val, bool isCreate)
		{
			if (param == ConfigurationParameterNames.Namespace)
			{
				// Would need to be consistent with the namespace 
				// names for the FileSystemNameSpaceProvider
				if (val == "" || val == null)
					return "Namespace can not be blank";
				if (isCreate && aFed.ContentBaseForNamespace(val) != null)
					return "Namespace already exists";
			}
			else if (param == ConfigurationParameterNames.ConnectionString)
			{
				if (val == "")
					return "ConnectionString can not be null or blank";
			}
			return null;
		}

		public void SetParameter(string param, string val)
		{
			if (param == ConfigurationParameterNames.Namespace)
				Namespace = val;
			else if (param == ConfigurationParameterNames.ConnectionString)
				ConnectionString = val;
			else if (param == ConfigurationParameterNames.Contact)
				Contact = val;
			else if (param == ConfigurationParameterNames.Description)
				NamespaceDescription = val;
			else if (param == ConfigurationParameterNames.Title)
				Title = val;
			else
				throw new Exception("Unknown parameter: " + param);
		}

		public string GetParameter(string param)
		{
			if (param == ConfigurationParameterNames.Namespace)
				return Namespace;
			else if (param == ConfigurationParameterNames.ConnectionString)
				return ConnectionString;
			else if (param == ConfigurationParameterNames.Contact)
				return Contact;
			else if (param == ConfigurationParameterNames.Title)
				return Title;
			else if (param == ConfigurationParameterNames.Description)
				return NamespaceDescription;
			else
				throw new Exception("Unknown parameter: " + param);
		}

		/// <summary>
		/// Indicates if a namespace parameter can be edited.
		/// </summary>
		/// <param name="param">Parameter name.</param>
		/// <returns>Boolean value indicates if the parameter can be edited.</returns>
		public bool CanParameterBeEdited(string param)
		{
			// Except for Namespace name all values can be edited.
			return param != ConfigurationParameterNames.Namespace;
		}

		public IList ValidateAggregate(Federation aFed, bool isCreate)
		{
			return null;
		}

		/// <summary>
		/// Check if the Namespace already exists in the database.
		/// </summary>
		/// <param name="ns">Namespace to check for existence.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Boolean vaue indicating if the namespace exists or not.</returns>
		public static bool Exists(string ns, string connectionString)
		{
			// Check if the namespace already exists in the database
			return SqlHelper.NamespaceExists(ns, connectionString);
		}

		/// <summary>
		/// Create the specified namespace.
		/// </summary>
		/// <param name="ns">Namespace to check for existence.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		private void CreateNamespace(string ns, string connectionString)
		{
			// Create a new namespace.
			SqlHelper.CreateNamespace(ns, connectionString);
		}
	}
}
