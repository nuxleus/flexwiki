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
using System.Reflection; 
using System.IO; 

using NUnit.Framework; 

namespace FlexWiki.FwDocGen.Tests
{
  [TestFixture]
  public class GeneratorTests
  {
    private string cwd; 

    [SetUp]
    public void SetUp()
    {
      if (Directory.Exists("Test"))
      {
        Directory.Delete("Test", true); 
      }

      Directory.CreateDirectory("Test"); 
      cwd = Directory.GetCurrentDirectory(); 
      Directory.SetCurrentDirectory("Test"); 

      Stream stmin  = Assembly.GetExecutingAssembly().GetManifestResourceStream(
        "FlexWiki.FwDocGen.Tests.input.xml"); 
      Stream stmout = File.OpenWrite("input.xml"); 

      TextReader rdr = new StreamReader(stmin); 
      TextWriter wtr = new StreamWriter(stmout); 

      wtr.Write(rdr.ReadToEnd()); 

      wtr.Close(); 
      rdr.Close(); 
    }

    [TearDown]
    public void TearDown()
    {
      Directory.SetCurrentDirectory(cwd); 
    }

    [Test]
    public void Generate()
    {
      string[] args =
        {
          "/docdir", "Docs", 
          "/docns",  "ProjectDocumentation", 
          "/commentdir", "Comments", 
          "/commentns", "ProjectComments", 
          "input.xml"
        };
      Options options = Utilities.ParseCommandLine(args); 
 
      Generator.Run(options); 

      AssertGeneratedFiles(); 

    }

    private void WriteDefaultTemplatesToDirectory(string dir)
    {
      if (Directory.Exists(dir))
      {
        Directory.Delete(dir, true); 
      }

      Directory.CreateDirectory(dir); 

      string[] args = 
        {
          "/writeonly", 
          "/templatedir", dir
        }; 

      Options options = Utilities.ParseCommandLine(args); 
      Generator.Run(options); 
    }

    [Test]
    public void GenerateExplicitDefault()
    {
      WriteDefaultTemplatesToDirectory("Templates"); 

      string[] args =
        {
          "/docdir", "Docs", 
          "/docns",  "ProjectDocumentation", 
          "/commentdir", "Comments", 
          "/commentns", "ProjectComments", 
          "/generator", Path.Combine("Templates", "Generator.xslt"), 
          "input.xml"
        };
      Options options = Utilities.ParseCommandLine(args); 
 
      Generator.Run(options); 

      AssertGeneratedFiles(); 

    }

    [Test]
    [ExpectedException(typeof(FileNotFoundException))]
    public void GenerateExplicitMissingTemplate()
    {
      WriteDefaultTemplatesToDirectory("Templates"); 

      File.Delete(Path.Combine("Templates", "HomePage.xslt")); 

      string[] args =
        {
          "/docdir", "Docs", 
          "/docns",  "ProjectDocumentation", 
          "/commentdir", "Comments", 
          "/commentns", "ProjectComments", 
          "/generator", "blahblah", 
          "input.xml"
        };
      Options options = Utilities.ParseCommandLine(args); 
 
      Generator.Run(options); 

      AssertGeneratedFiles(); 

    }


    private void AssertGeneratedFiles()
    {
      Assert.IsTrue(Directory.Exists("Docs"), "Checking that doc dir exists"); 
      Assert.IsTrue(Directory.Exists("Comments"), 
        "Checking that comment dir exists");       

      Assert.IsTrue(File.Exists(Path.Combine("Docs", "HomePage.wiki")), 
        "Checking that the HomePage exists"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "ClassPageZ7wws14s.wiki")), 
        "Checking that ClassPage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "ConstructorPage6c3vjnuy.wiki")), 
        "Checking that ConstructorPage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "EventPage1yf2rhb4.wiki")), 
        "Checking that EventPage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "FieldPage2e7ec94n.wiki")), 
        "Checking that FieldPage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "MethodPage1gjtaxt1.wiki")), 
        "Checking that MethodPage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "NamespacePageJzb9n6v5.wiki")), 
        "Checking that NamespacePage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "OperatorPage4n221q8s.wiki")), 
        "Checking that Operator name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "PropertyPage4ewtx4tc.wiki")), 
        "Checking that PropertyPage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "StructurePage1spkq9xb.wiki")), 
        "Checking that StructurePage name hasn't changed"); 
      Assert.IsTrue(File.Exists(Path.Combine("Docs", "InterfacePageX697zgbg.wiki")), 
        "Checking that InterfacePage name hasn't changed"); 

      AssertDocumentationFiles("Namespace", 4); 
      AssertDocumentationFiles("Class", 68); 
      AssertDocumentationFiles("Structure", 3); 
      AssertDocumentationFiles("Interface", 12); 
      AssertDocumentationFiles("Enumeration", 5); 
      AssertDocumentationFiles("Field", 45); 
      AssertDocumentationFiles("Constructor", 77); 
      AssertDocumentationFiles("Property", 74); 
      AssertDocumentationFiles("Event", 21); 
      AssertDocumentationFiles("Method", 598); 
      AssertDocumentationFiles("Operator", 31); 

      Assert.AreEqual(0, Directory.GetFiles("Comments").Length, 
        "Checking that no comment files exist"); 

    }

    [Test]
    public void Write()
    {
      WriteDefaultTemplatesToDirectory("Templates"); 

      Assert.AreEqual(1, Directory.GetFiles("Templates", "*.xslt").Length, 
        "Checking that right number of templates were created"); 
      Assert.AreEqual(1, Directory.GetFiles("Templates").Length, 
        "Checking that no other files were created"); 

      Assert.IsTrue(File.Exists(Path.Combine("Templates", "Generator.xslt")), 
        "Checking that Generator.xslt was created"); 

    }

    private void AssertDocumentationFiles(string type, int number)
    {
      string[] files = Directory.GetFiles("Docs", type + "Page*.wiki"); 
      Assert.AreEqual(number, files.Length, 
        "Checking that the right number of " + type + " docs were generated"); 
    }
  }
}