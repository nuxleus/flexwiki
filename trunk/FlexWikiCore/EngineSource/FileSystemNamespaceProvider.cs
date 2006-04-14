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
        private const string c_namespace = "Namespace";
        private const string c_root = "Root";
        private const string c_contact = "Contact";
        private const string c_title = "Title";
        private const string c_description = "Description";

        private string _contact;
        private ArrayList _parameterDescriptors;
        private string _namespace;
        private string _namespaceDescription;
        private string _root;
        private string _title;

        public FileSystemNamespaceProvider()
        {
        }


        public string Contact
        {
            get
            {
                return _contact;
            }
            set
            {
                _contact = value;
            }
        }

        public string Description
        {
            get
            {
                return "single-namespace, content stored in file-system";
            }
        }

        public string Namespace
        {
            get
            {
                return _namespace;
            }
            set
            {
                _namespace = value;
            }
        }

        public string NamespaceDescription
        {
            get
            {
                return _namespaceDescription;
            }
            set
            {
                _namespaceDescription = value;
            }
        }

        public string OwnerMailingAddress
        {
            get
            {
                return Contact;
            }
        }

        public IList ParameterDescriptors
        {
            get
            {
                if (_parameterDescriptors != null)
                {
                    return ArrayList.ReadOnly(_parameterDescriptors);
                }
                _parameterDescriptors = new ArrayList();
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(c_namespace, "Namespace", "Name for the namespace", null, true));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(c_root, "Directory", @"Physical path to directory that holds this namespace.  Leave blank to accept the default (.\[NamespaceName]).", null, true));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(c_contact, "Contact", "eMail address for contact for this namespace", null, false));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(c_title, "Title", "A short title for this namespace", null, false));
                _parameterDescriptors.Add(new NamespaceProviderParameterDescriptor(c_description, "Description", "A description for the namespace (use Wiki formatting)", null, false));
                return ArrayList.ReadOnly(_parameterDescriptors);
            }
        }

        public string Root
        {
            get
            {
                return _root;
            }
            set
            {
                _root = value;
            }
        }

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }


        private string DefaultedRoot
        {
            get
            {
                if (Root != null && Root != "")
                    return Root;
                return @".\" + Namespace;
            }
        }


        public bool CanParameterBeEdited(string param)
        {
            return param != c_namespace;
        }

        public IList CreateNamespaces(Federation aFed)
        {
            string author = "";
            FileSystemStore store = new FileSystemStore();
            NamespaceManager manager = aFed.RegisterNamespace(store, Namespace);
            manager.WriteTopic(NamespaceManager.DefinitionTopicName, "");
            manager.SetTopicPropertyValue(NamespaceManager.DefinitionTopicName, "Contact", Contact, false, author);
            manager.SetTopicPropertyValue(NamespaceManager.DefinitionTopicName, "Title", Title, false, author);
            string defaultImportedNamespaces = System.Configuration.ConfigurationManager.AppSettings["DefaultImportedNamespaces"];
            if (defaultImportedNamespaces != null)
            {
                manager.SetTopicPropertyValue(NamespaceManager.DefinitionTopicName, "Import", defaultImportedNamespaces, false, author);
            }

            // whoever is last should write a new version
            manager.SetTopicPropertyValue(NamespaceManager.DefinitionTopicName, "Description", NamespaceDescription, true, author);

            ArrayList answer = new ArrayList();
            answer.Add(Namespace);
            return ArrayList.ReadOnly(answer);
        }

        public string GetParameter(string param)
        {
            if (param == c_namespace)
                return Namespace;
            else if (param == c_root)
                return Root;
            else if (param == c_contact)
                return Contact;
            else if (param == c_title)
                return Title;
            else if (param == c_description)
                return NamespaceDescription;
            else
                throw new Exception("Unknown parameter: " + param);
        }

        public void LoadNamespaces(Federation aFed)
        {
            FileSystemStore store = new FileSystemStore();
            aFed.RegisterNamespace(store, Namespace);
        }

        public void SavePersistentParametersToDefinition(NamespaceProviderDefinition def)
        {
            def.SetParameter(c_namespace, Namespace);
            def.SetParameter(c_root, DefaultedRoot);
        }

        public void SetParameter(string param, string val)
        {
            if (param == c_namespace)
                Namespace = val;
            else if (param == c_root)
                Root = val;
            else if (param == c_contact)
                Contact = val;
            else if (param == c_description)
                NamespaceDescription = val;
            else if (param == c_title)
                Title = val;
            else
                throw new Exception("Unknown parameter: " + param);
        }

        public IList ValidateAggregate(Federation aFed, bool isCreate)
        {
            // no errors
            return null;
        }

        public string ValidateParameter(Federation aFed, string param, string val, bool isCreate)
        {
            if (param == c_namespace)
            {
                if (val == "" || val == null)
                    return "Namespace can not be blank";
                if (isCreate && aFed.NamespaceManagerForNamespace(val) != null)
                    return "Namespace already exists";
                // TODO -- check other constraints (valid chars) for namespaces
            }

            return null;
        }

        public void UpdateNamespaces(Federation aFed)
        {
            // just kill the old one and reload a new one
            aFed.UnregisterNamespace(aFed.NamespaceManagerForNamespace(Namespace));
            LoadNamespaces(aFed);
        }

    }
}
