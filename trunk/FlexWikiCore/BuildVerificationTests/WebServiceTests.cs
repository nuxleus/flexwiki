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
using System.Configuration; 

using NUnit.Framework; 

namespace FlexWiki.BuildVerificationTests
{
  [TestFixture]
  public class WebServiceTests
  {
    private EditServiceProxy proxy = new EditServiceProxy(); 

    [SetUp]
    public void SetUp()
    {
      // Get the base URL from the config file and gen up the web service endpoint
      proxy.Url = ConfigurationSettings.AppSettings["InstallationUri"] + "EditService.asmx"; 
    }

    [Test]
    public void CanEdit()
    {
      string visitorIdentityString = proxy.CanEdit(); 
      Assert.IsNotNull(visitorIdentityString, "Checking that CanEdit returns a non-null string"); 
    }

  }
}

