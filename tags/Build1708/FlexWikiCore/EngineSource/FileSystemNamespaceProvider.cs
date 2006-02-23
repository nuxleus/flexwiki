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
	/// Summary description for FileSystemNamespaceProvider.
	/// </summary>
	public class FileSystemNamespaceProvider : INamespaceProvider
	{
		public FileSystemNamespaceProvider()
		{
		}

		public string OwnerMailingAddress
		{
			get
			{
				return Contact;
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
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(Names.Namespace, "Namespace", "Name for the namespace", null, true));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(Names.Root, "Directory", @"Physical path to directory that holds this namespace.  Leave blank to accept the default (.\[NamespaceName]).", null, true));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(Names.Contact, "Contact", "eMail address for contact for this namespace", null, false));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(Names.Title, "Title", "A short title for this namespace", null, false));
				_ParameterDescriptors.Add(new NamespaceProviderParameterDescriptor(Names.Description, "Description", "A description for the namespace (use Wiki formatting)", null, false));
				return ArrayList.ReadOnly(_ParameterDescriptors);
			}
		}

		struct Names
		{
			static public string Namespace = "Namespace";
			static public string Root = "Root";
			static public string Contact = "Contact";
			static public string Title = "Title";
			static public string Description = "Description";
		};

		public string Description
		{
			get
			{
				return "single-namespace, content stored in file-system";
			}
		}

		string _Namespace;
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

		string _Root;
		public string Root
		{
			get
			{
				return _Root;
			}
			set
			{
				_Root = value;
			}
		}

		string _Contact;
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

		string _Title;
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

		
		string _NamespaceDescription;
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
		
		public bool CanParameterBeEdited(string param)
		{
			return param != Names.Namespace;
		}

		public string GetParameter(string param)
		{
			if (param == Names.Namespace)
				return Namespace;
			else if (param == Names.Root)
				return Root;
			else if (param == Names.Contact)
				return Contact;
			else if (param == Names.Title)
				return Title;
			else if (param == Names.Description)
				return NamespaceDescription;
			else
				throw new Exception("Unknown parameter: " + param);
		}

		public void SetParameter(string param, string val)
		{
			if (param == Names.Namespace)
				Namespace = val;
			else if (param == Names.Root)
				Root = val;
			else if (param == Names.Contact)
				Contact = val;
			else if (param == Names.Description)
				NamespaceDescription = val;
			else if (param == Names.Title)
				Title = val;
			else
				throw new Exception("Unknown parameter: " + param);
		}

		public string ValidateParameter(Federation aFed, string param, string val, bool isCreate)
		{
			if (param == Names.Namespace)
			{
				if (val == "" || val == null)
					return "Namespace can not be blank";
				if (isCreate && aFed.ContentBaseForNamespace(val) != null)
					return "Namespace already exists";
				// TODO -- check other constraints (valid chars) for namespaces
			}

			return null;
		}

		public IList ValidateAggregate(Federation aFed, bool isCreate)
		{
			// no errors
			return null;
		}

		string DefaultedRoot
		{
			get
			{
				if (Root != null && Root != "")
					return Root;
				return @".\" + Namespace;
			}
		}

		public void LoadNamespaces(Federation aFed)
		{
			FileSystemStore store = new FileSystemStore(aFed, Namespace, DefaultedRoot);
			aFed.RegisterNamespace(store);
		}

		public IList CreateNamespaces(Federation aFed)
		{
			FileSystemStore store = new FileSystemStore(aFed, Namespace, DefaultedRoot);
			aFed.RegisterNamespace(store);
			store.WriteTopic(store.DefinitionTopicName.LocalName, "");
			store.SetFieldValue(store.DefinitionTopicName.LocalName, "Contact", Contact, false);
			store.SetFieldValue(store.DefinitionTopicName.LocalName, "Title", Title, false);
			string defaultImportedNamespaces = System.Configuration.ConfigurationSettings.AppSettings["DefaultImportedNamespaces"];
			if (defaultImportedNamespaces != null)
				store.SetFieldValue(store.DefinitionTopicName.LocalName, "Import", defaultImportedNamespaces, false);

			// whoever is last should write a new version
			store.SetFieldValue(store.DefinitionTopicName.LocalName, "Description", NamespaceDescription, true);

			ArrayList answer = new ArrayList();
			answer.Add(Namespace);
			return ArrayList.ReadOnly(answer);
		}

		public void UpdateNamespaces(Federation aFed)
		{
			// just kill the old one and reload a new one
			aFed.UnregisterNamespace(aFed.ContentBaseForNamespace(Namespace));
			LoadNamespaces(aFed);
		}

		public void SavePersistentParametersToDefinition(NamespaceProviderDefinition def)
		{
			def.SetParameter(Names.Namespace, Namespace);
			def.SetParameter(Names.Root, DefaultedRoot);
		}

	}
}
