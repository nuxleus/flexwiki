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
using System.IO;
using System.Threading;

using NUnit.Framework;

using FlexWiki;

namespace FlexWiki.BuildVerificationTests
{
  /// <summary>
  /// Tests that verify the /admin pages are working correctly
  /// </summary>
  [TestFixture]
  public class AdminUITests : UITests 
  {
    public AdminUITests() : base()
    {
    }

    [Test] public void CreateNamespaceProvider()
    {
      DocumentElement doc = TheBrowser.Navigate(TheLinkMaker.SiteURL() + "admin/EditProvider.aspx", true); 

		// System.Diagnostics.Debugger.Break();
		// Select the filesystem namespace provider
		SelectElement sel = (SelectElement)doc.GetElementByID("TypeName");
		int idx = 0;
		foreach (OptionElement each in sel.Options)
		{
			if (each.Text.IndexOf(typeof(FileSystemNamespaceProvider).Name) != -1)
			{
				sel.SelectedIndex = idx;
				break;
			}
			idx++;
		}


      InputElement next = doc.GetElementByName("next1") as InputElement; 
      next.Form.Submit(); 

      doc.WaitForNavigationComplete(); 
      
      InputElement ns = doc.GetElementByName("Namespace") as InputElement; 
      ns.Value = "NewNamespace"; 

      InputElement directory = doc.GetElementByName("Root") as InputElement; 
      directory.Value = Path.Combine(Path.GetFullPath(base.Root), "WikiBases\\NewNamespace"); 

      next = doc.GetElementByName("next2") as InputElement; 
      next.Form.Submit(); 

      doc.WaitForNavigationComplete(); 

      string contents = doc.Body.InnerHTML; 
      Assert.IsTrue(contents.IndexOf("The provider has been created") != -1, "Checking that namespace was created."); 
    }
  }
}