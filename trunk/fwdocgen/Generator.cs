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
using System.Xml; 
using System.Xml.XPath; 
using System.Xml.Xsl; 
using System.Reflection; 
using System.Text; 

namespace FlexWiki.FwDocGen
{
  public sealed class Generator
  {
    public static void Run(Options options)
    {
      if (options.WriteTemplatesOnly)
      {
        WriteTemplates(options); 
      }
      else
      {
        Generate(options); 
      }
    }
    
    private static void WriteTemplates(Options options)
    {
      string templateDir = Directory.GetCurrentDirectory(); 
      if (options.TemplateDir != null)
      {
        templateDir = options.TemplateDir; 
      }

      if (!Directory.Exists(templateDir))
      {
        Directory.CreateDirectory(templateDir); 
      }

      WriteXslt("FlexWiki.FwDocGen.Generator.xslt", "generator.xslt", templateDir); 

    }

    private static void WriteXslt(string resourceName, string transformName, string templateDir)
    {
      Stream stmxslt = Assembly.GetExecutingAssembly().GetManifestResourceStream(
        resourceName); 

      try
      {
        XmlDocument doc = new XmlDocument(); 
        doc.Load(stmxslt);
  
        string path = Path.Combine(templateDir, transformName); 
        StreamWriter wtr = new StreamWriter(path, false,
          System.Text.Encoding.UTF8); 
        try
        {
          XmlTextWriter xmlwtr = new XmlTextWriter(wtr); 
          try
          {
            doc.WriteTo(xmlwtr);   
          }
          finally
          {
            xmlwtr.Close(); 
          }
        }
        finally
        {
          wtr.Close(); 
        }
      }
      finally
      {
        stmxslt.Close(); 
      }

    }

    private static void Generate(Options options)
    {
      if (!Directory.Exists(options.DocDir))
      {
        Directory.CreateDirectory(options.DocDir); 
      }

      if (!Directory.Exists(options.CommentsDir))
      {
        Directory.CreateDirectory(options.CommentsDir); 
      }

      XPathDocument input = new XPathDocument(options.InputPath); 
      XPathNavigator nav = input.CreateNavigator();

      Stream stmxslt = null;
      
      XmlResolver resolver = null; 
      if (options.Generator == null)
      {
        stmxslt = Assembly.GetExecutingAssembly().GetManifestResourceStream(
          "FlexWiki.FwDocGen.Generator.xslt"); 
        resolver = new ResourceResolver(); 
      }
      else
      {
        stmxslt = File.OpenRead(options.Generator);
        resolver = new FileResolver(Path.GetDirectoryName(Path.GetFullPath(options.Generator))); 
      }

      try
      {
        XmlReader rdr = new XmlTextReader(stmxslt); 

        XslTransform xslt = new XslTransform(); 
        xslt.Load(rdr, resolver, null); 

        MemoryStream buffer = new MemoryStream(); 
        TextWriter twoutput = new StreamWriter(buffer, Encoding.UTF8); 

        XsltArgumentList args = new XsltArgumentList(); 
        args.AddExtensionObject(FwXsltHelper.XmlNamespaceUri, new FwXsltHelper()); 
        args.AddParam("commentDir", "", options.CommentsDir); 
        args.AddParam("commentNamespace", "" ,options.CommentsNamespace); 
        args.AddParam("docDir", "", options.DocDir);
        args.AddParam("docNamespace", "", options.DocNamespace); 
        args.AddParam("verbosity", "", options.Verbose ? 4 : 0); 

        xslt.Transform(input, args, twoutput, resolver); 

        buffer.Position = 0L; 
    
        if (options.Verbose)
        {
          Console.WriteLine("XSLT completed with this output:"); 
          Console.WriteLine("================");
          TextReader tr = new StreamReader(buffer); 
          Console.WriteLine(tr.ReadToEnd()); 
          buffer.Position = 0L; 
          Console.WriteLine("================");
        }

        XPathDocument script = new XPathDocument(buffer); 

        XPathNavigator navScript = script.CreateNavigator(); 

        XmlNamespaceManager nsmgr = new XmlNamespaceManager(navScript.NameTable); 
        nsmgr.AddNamespace("fwdg", FwXsltHelper.XmlNamespaceUri); 
        XPathExpression expr = navScript.Compile("/fwdg:fileset/fwdg:file"); 
        expr.SetContext(nsmgr); 

        XPathNodeIterator iter = navScript.Select(expr);

        while (iter.MoveNext())
        {
          string path = (string) iter.Current.Evaluate("string(@path)"); 

#if false
          if (path.Length == 0)
          {
            System.Diagnostics.Debugger.Break(); 
          }
#endif

          path = Path.Combine(options.DocDir, path); 
          string contents = iter.Current.Value; 

          FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None); 
          try
          {
            TextWriter tw = new StreamWriter(fs, Encoding.UTF8); 
            try
            {
              tw.Write(contents);
              tw.Flush(); 
            }
            finally
            {
              tw.Close(); 
            }
          }
          finally
          {
            fs.Close(); 
          }
        }
     
      }
      finally
      {
        stmxslt.Close(); 
      }
     
    }

  }
}