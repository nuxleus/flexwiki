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
using System.Security.Cryptography; 

namespace FlexWiki.FwDocGen
{
  public sealed class Utilities
  {
    private static SHA1Managed provider = new SHA1Managed(); 
    private static char[] lookupTable = 
      {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 
        'j', 'k', 'm', 'n', 'p', 'q', 'r', 's', 
        't', 'u', 'v', 'w', 'x', 'y', 'z', '1', 
        '2', '3', '4', '5', '6', '7', '8', '9'
      };

    private const string USAGE = 
@"fwdocgen /docdir <docdir> /docns <docns> /commentdir <commentdir> 
    /commentns <commentns> [ /generator <generator> ] [ /verbose ] <input> 

fwdocgen /writeonly [ /templatedir <templatedir> ]
    

/docdir       Specifies the directory to which to output documentation files
/docns        Specifies the FlexWiki namespace to use for doc files
/commentdir   Specifies the directory to which to output comments files
/commentns    Specifies the FlexWiki namespace to use for comment files.
/generator    Optional. If present, specifies the XSLT that will be run 
              to generate the script that will drive the generation process.
              If not present, the default transform is used. 
/verbose      Print extra debugging information to the console. Note: this 
              produces a *lot* of extra info. 
/writeonly    Specifies that rather than generating documentation, the
              program should dump the default set of XSLT templates to 
              the <templatedir> (if specified) or the current directory
              (if templatedir is not specified). If this parameter is 
              specified, all other parameters are optional. 
/templatedir  Optional. If not specified, the current directory will be used. 

<input>       An ndoc-generated XML file containing the classes, namespaces,
              methods, etc. for which you are generation documentation.
              Not required when /writeonly is specified. 
<generator>   The relative or absolute path to the XSLT that will be run
              against the input document to create the script that will 
              drive the generation process
";

    private Utilities() {} 

    public static string Copyright
    {
      get
      {
        return "fwdocgen Version " + 
          System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + 
          " Copyright (c) 2004 Microsoft Corporation.\n Written by Craig Andera candera@wangdera.com"; 
      }
    }

    public static string Usage
    {
      get { return USAGE; }
    }

    public static Options ParseCommandLine(string[] args)
    {
      Options options = new Options(); 
      for (int i = 0; i < args.Length; ++i)
      {
        string arg = args[i].ToUpper(); 

        if (arg.Equals("/DOCDIR"))
        {
          options.DocDir = args[++i]; 
        }
        else if (arg.Equals("/DOCNS"))
        {
          options.DocNamespace = args[++i];
        }
        else if (arg.Equals("/COMMENTDIR"))
        {
          options.CommentsDir = args[++i]; 
        }
        else if (arg.Equals("/COMMENTNS"))
        {
          options.CommentsNamespace = args[++i]; 
        }
        else if (arg.Equals("/TEMPLATEDIR"))
        {
          options.TemplateDir = args[++i]; 
        }
        else if (arg.Equals("/GENERATOR"))
        {
          options.Generator = args[++i]; 
        }
        else if (arg.Equals("/WRITEONLY"))
        {
          options.WriteTemplatesOnly = true; 
        }
        else if (arg.Equals("/VERBOSE"))
        {
          options.Verbose = true; 
        }
        else if (arg.Equals("/DEBUG"))
        {
          options.Debug = true; 
        }
        else
        {
          options.InputPath = args[i]; 
          if (i < args.Length - 1)
          {
            throw new ParseCommandLineException(
              "No options may be specified after the input file"
              ); 
          }
        }
      }

      if (options.WriteTemplatesOnly)
      {
        if (options.DocDir != null)
        {
          throw new ParseCommandLineException("/docdir argument is not allowed when /writeonly is specified"); 
        }
        else if (options.DocNamespace != null)
        {
          throw new ParseCommandLineException("/docns argument is not allowed when /writeonly is specified"); 
        }
        else if (options.CommentsDir != null)
        {
          throw new ParseCommandLineException("/commentdir argument is not allowed when /writeonly is specified"); 
        }
        else if (options.CommentsNamespace != null)
        {
          throw new ParseCommandLineException("/commentns argument is not allowed when /writeonly is specified"); 
        }
        else if (options.Generator != null)
        {
          throw new ParseCommandLineException("/generator argument is not allowed when /writeonly is specified"); 
        }
        else if (options.InputPath != null)
        {
          throw new ParseCommandLineException("input file argument is not allowed when /writeonly is specified"); 
        }
      }
      else
      {
        if (options.DocDir == null)
        {
          throw new ParseCommandLineException("/docdir argument is required when /writeonly is not specified"); 
        }
        else if (options.DocNamespace == null)
        {
          throw new ParseCommandLineException("/docns argument is required when /writeonly is not specified"); 
        }
        else if (options.CommentsDir == null)
        {
          throw new ParseCommandLineException("/commentdir argument is required when /writeonly is not specified"); 
        }
        else if (options.CommentsNamespace == null)
        {
          throw new ParseCommandLineException("/commentns argument is required when /writeonly is not specified"); 
        }
        else if (options.InputPath == null)
        {
          throw new ParseCommandLineException("input file argument is required when /writeonly is not specified"); 
        }
      }
      

      return options;
    }

    public static string HashFromID(string id)
    {
      byte[] buf = System.Text.Encoding.UTF8.GetBytes(id); 
      byte[] hash = provider.ComputeHash(buf); 

      string s = ""; 
      for (byte b = 0; b < 8; ++b)
      {
        byte index = (byte) (hash[b] & 0x1f); 
        char letter = lookupTable[index]; 
        if (b == 0)
        {
          s += letter.ToString().ToUpper(); 
        }
        else
        {
          s += letter; 
        }
      }

      return s; 

    }

  }
}
