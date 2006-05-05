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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace FlexWiki.Web
{
    /// <summary>
    /// Summary description for Rename.
    /// </summary>
    public class Rename : BasePage
    {
        private void Page_Load(object sender, System.EventArgs e)
        {
            // Put user code to initialize the page here
        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.Page_Load);
        }
        #endregion

        protected QualifiedTopicRevision AbsTopicName
        {
            get
            {
                return new QualifiedTopicRevision(Request.QueryString["topic"]);
            }
        }

        protected bool IsTopicReadOnly
        {
            get
            {
                NamespaceManager storeManager = Federation.NamespaceManagerForTopic(AbsTopicName);
                if (storeManager == null)
                    return true;
                return !storeManager.IsExistingTopicWritable(AbsTopicName.LocalName);

            }

        }

        protected string Fixup
        {
            get
            {
                return Request.Form["Fixup"];
            }
        }

        protected string LeaveRedirect
        {
            get
            {
                return Request.Form["LeaveRedirect"];
            }
        }

        protected string NewName
        {
            get
            {
                return Request.Form["NewName"];
            }
        }

        protected string OldName
        {
            get
            {
                return Request.Form["OldName"];
            }
        }

        protected string Namespace
        {
            get
            {
                return Request.Form["Namespace"];
            }
        }

        protected void PerformRename()
        {
            QualifiedTopicRevision oldName = new QualifiedTopicRevision(OldName, Namespace);
            NamespaceManager storeManager = Federation.NamespaceManagerForNamespace(Namespace);

            string defaultNamespace = DefaultNamespace;
            string oldAppearsAs = (oldName.Namespace == defaultNamespace) ? oldName.LocalName : oldName.QualifiedName;
            string newName = NewName;
            string newAppearsAs = (oldName.Namespace == defaultNamespace) ? newName : Namespace + "." + newName;

            if (storeManager == null)
            {
                Response.Write("<b>No namespace was specified. Please try again.</b>");
                return;
            }

            if (newName == null || newName.Length == 0)
            {
                Response.Write("<b>No name was specified. Please try again.</b>");
                return;
            }

            // See if the new name already exists
            if (storeManager.TopicExists(newName, ImportPolicy.DoNotIncludeImports))
            {
                Response.Write("<b>Topic (" + newName + ") already exists.  Choose another name...</b>");
                return;
            }

            bool fixup = Fixup == "on";
            bool fixupDisabled = false;

            try
            {
                fixupDisabled = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["DisableRenameFixup"]);
            }
            catch
            {
            }

            if (fixupDisabled)
            {
                fixup = false;
            }

            ReferenceFixupPolicy fixupPolicy = ReferenceFixupPolicy.DoNotFixReferences;

            if (fixup)
            {
                fixupPolicy = ReferenceFixupPolicy.FixReferences; 
            }

            RenameTopicDetails results = storeManager.RenameTopic(new UnqualifiedTopicName(oldName.LocalName), new UnqualifiedTopicName(newName), 
                fixupPolicy, VisitorIdentityString);

            Response.Write("Renamed <i>" + oldAppearsAs + "</i> to <i>" + newName + "</i><br/>");
            Response.Write("<br/>");
            foreach (TopicName topic in results.UpdatedReferenceTopics)
            {
                Response.Write(String.Format("Topic {0} had its references updated. <br>", topic.QualifiedName));
            }
            bool redir = LeaveRedirect == "on";
            if (redir)
            {
                DateTime ts = DateTime.Now.ToLocalTime();
                storeManager.WriteTopicAndNewVersion(oldName.LocalName,
                    "Redirect: " + newName + @"

This page was automatically generated when this topic (" + oldName.LocalName + ") was renamed to " + newName + " on " +
                    ts.ToShortDateString() + " at " + ts.ToShortTimeString() + " by " + VisitorIdentityString + "." +
                    "\nPlease update references to point to the new topic.", 
                    VisitorIdentityString);
            }

        }

        protected void DoLeftBorder()
        {
            Response.Write("<td width='140' valign='top' class='BorderLeft'></td>");
        }
        protected void DoRightBorder()
        {
            string rightBorder = Federation.GetTopicFormattedBorder(GetTopicVersionKey(), Border.Right); // topic, Border.Right);
            rightBorder = "<td width='140' valign='top' class='BorderRight'>" + rightBorder + "</td>";
            Response.Write(rightBorder);
        }
    }
}
