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
using System.IO; 

using NUnit.Framework; 

namespace FlexWiki.FwDocGen.Tests
{
  [TestFixture]
  public class UtilitiesTests
  {
    [Test]
    public void ParseCommandLine1()
    {
      string[] args = new string[] 
        {
          "/docdir", "Docs", 
          "/docns",  "ProjectDocumentation", 
          "/commentdir", "Comments", 
          "/commentns", "ProjectComments", 
          "/generator", "generator.xslt",
          "input.xml"
        }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.CommentsDir = "Comments";
      expected.CommentsNamespace = "ProjectComments";
      expected.DocDir = "Docs";
      expected.DocNamespace = "ProjectDocumentation";
      expected.Generator = "generator.xslt";
      expected.InputPath = "input.xml";

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test]
    public void ParseCommandLine2()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/docdir", "Docs", 
          "/commentns", "ProjectComments", 
          "/commentdir", "Comments", 
          "input.xml"
        }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.CommentsDir = "Comments";
      expected.CommentsNamespace = "ProjectComments"; 
      expected.DocDir = "Docs";
      expected.DocNamespace = "ProjectDocumentation";
      expected.InputPath = "input.xml";

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test]
    public void ParseCommandLine3()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "/generator", "generator.xslt",
          "/commentdir", "Comments", 
          "/docdir", "Docs", 
          "input.xml"
        }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.DocDir = "Docs";
      expected.DocNamespace = "ProjectDocumentation";
      expected.InputPath = "input.xml";
      expected.Generator = "generator.xslt";
      expected.CommentsDir = "Comments";
      expected.CommentsNamespace = "ProjectComments";

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test] 
    public void ParseCommandLine4()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "/generator", "generator.xslt", 
          "/commentdir", "Comments", 
          "/docdir", "Docs", 
          "/templatedir", "Templates",
          "input.xml"
        }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.CommentsDir = "Comments";
      expected.CommentsNamespace = "ProjectComments";
      expected.DocDir = "Docs";
      expected.DocNamespace = "ProjectDocumentation";
      expected.InputPath = "input.xml";
      expected.Generator = "generator.xslt";
      expected.TemplateDir = "Templates";

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test] 
    public void ParseCommandLine5()
    {
      string[] args = new string[] 
        {
          "/writeonly", 
        }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.WriteTemplatesOnly = true; 

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test] 
    public void ParseCommandLine6()
    {
      string[] args = new string[] 
        {
          "/writeonly", 
          "/templatedir", "Templates"
      }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.WriteTemplatesOnly = true; 
      expected.TemplateDir = "Templates";

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test]
    public void ParseCommandLine7()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "/verbose", 
          "/generator", "generator.xslt",
          "/commentdir", "Comments", 
          "/docdir", "Docs", 
          "input.xml"
        }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.DocDir = "Docs";
      expected.DocNamespace = "ProjectDocumentation";
      expected.InputPath = "input.xml";
      expected.Generator = "generator.xslt";
      expected.CommentsDir = "Comments";
      expected.CommentsNamespace = "ProjectComments";
      expected.Verbose = true; 

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test]
    public void ParseCommandLine8()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "/generator", "generator.xslt",
          "/commentdir", "Comments", 
          "/docdir", "Docs", 
          "/verbose", 
          "input.xml"
        }; 

      Options actual = Utilities.ParseCommandLine(args); 

      Options expected = new Options(); 
      expected.DocDir = "Docs";
      expected.DocNamespace = "ProjectDocumentation";
      expected.InputPath = "input.xml";
      expected.Generator = "generator.xslt";
      expected.CommentsDir = "Comments";
      expected.CommentsNamespace = "ProjectComments";
      expected.Verbose = true; 

      AssertCommandLineParsedCorrectly(expected, actual); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "No options may be specified after the input file")]
    public void ParseCommandLineError1()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "input.xml",
          "/commentdir", "Comments", 
          "/docdir", "Docs"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/docdir argument is required when /writeonly is not specified")]
    public void ParseCommandLineError2()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/docns argument is required when /writeonly is not specified")]
    public void ParseCommandLineError3()
    {
      string[] args = new string[] 
        {
          "/docdir", "Docs", 
          "/commentdir", "Comments", 
          "/commentns", "ProjectComments", 
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/commentdir argument is required when /writeonly is not specified")]
    public void ParseCommandLineError4()
    {
      string[] args = new string[] 
        {
          "/docdir", "Docs",
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/commentns argument is required when /writeonly is not specified")]
    public void ParseCommandLineError5()
    {
      string[] args = new string[] 
        {
          "/docdir", "Docs",
          "/docns",  "ProjectDocumentation", 
          "/commentdir", "Comments", 
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "input file argument is required when /writeonly is not specified")]
    public void ParseCommandLineError7()
    {
      string[] args = new string[] 
        {
          "/docdir", "Docs",
          "/docns",  "ProjectDocumentation", 
          "/commentdir", "Comments", 
          "/commentns", "ProjectComments",
          "/generator", "generator.xslt"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "No options may be specified after the input file")]
    public void ParseCommandLineError8()
    {
      string[] args = new string[] 
        {
          "/docns",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "input.xml",
          "/commentdir", "Comments", 
          "/docdir", "Docs"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/docdir argument is not allowed when /writeonly is specified")]
    public void ParseCommandLineError9()
    {
      string[] args = new string[] 
        {
          "/docdir",  "ProjectDocumentation", 
          "/commentns", "ProjectComments", 
          "/writeonly",
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/docns argument is not allowed when /writeonly is specified")]
    public void ParseCommandLineError10()
    {
      string[] args = new string[] 
        {
          "/writeonly",
          "/docns", "ProjectDocumentation", 
          "/commentdir", "Comments", 
          "/commentns", "ProjectComments", 
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/commentdir argument is not allowed when /writeonly is specified")]
    public void ParseCommandLineError11()
    {
      string[] args = new string[] 
        {
          "/commentdir", "Comments", 
          "/writeonly",
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/commentns argument is not allowed when /writeonly is specified")]
    public void ParseCommandLineError12()
    {
      string[] args = new string[] 
        {
          "/writeonly",
          "/commentns", "ProjectComments", 
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "/generator argument is not allowed when /writeonly is specified")]
    public void ParseCommandLineError13()
    {
      string[] args = new string[] 
        {
          "/writeonly",
          "/generator", "generator.xslt",
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    [Test]
    [ExpectedException(typeof(ParseCommandLineException), 
       "input file argument is not allowed when /writeonly is specified")]
    public void ParseCommandLineError14()
    {
      string[] args = new string[] 
        {
          "/writeonly",
          "input.xml"
        }; 

      Options options = Utilities.ParseCommandLine(args); 
    }

    
    private void AssertCommandLineParsedCorrectly(Options expected, Options actual)
    {
      Assert.AreEqual(expected.CommentsDir, actual.CommentsDir, 
        "Checking that comment directory was correct");
      Assert.AreEqual(expected.CommentsNamespace, actual.CommentsNamespace, 
        "Checking that comment namespace was correct");
      Assert.AreEqual(expected.DocDir, actual.DocDir, 
        "Checking that document directory was correct");
      Assert.AreEqual(expected.DocNamespace, actual.DocNamespace, 
        "Checking that document namespace was correct");
      Assert.AreEqual(expected.Generator, actual.Generator, 
        "Checking that generator was correct");
      Assert.AreEqual(expected.InputPath, actual.InputPath, 
        "Checking that input path was correct");
      Assert.AreEqual(expected.TemplateDir, actual.TemplateDir, 
        "Checking that template directory was correct");
      Assert.AreEqual(expected.WriteTemplatesOnly, actual.WriteTemplatesOnly, 
        "Checking that write templates setting was correct");

    }



  }
}