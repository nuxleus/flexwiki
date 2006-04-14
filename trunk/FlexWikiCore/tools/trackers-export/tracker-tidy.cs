using System; 
using System.IO; 
using System.Text; 

namespace Wangdera
{
  public class App
  {
    public static void Main(string[] args)
    {
      string tag = args[0];
      FileStream inputStream = new FileStream(args[1], FileMode.Open, FileAccess.Read, FileShare.None); 
      StreamReader inputReader = new StreamReader(inputStream); 

      string startTag = string.Format("<{0}>", tag); 
      string endTag = string.Format("</{0}>", tag); 

      string line; 
      bool inTag = false; 
      while ((line = inputReader.ReadLine()) != null)
      {
        int tagStart = line.IndexOf(startTag); 
        int tagEng = line.IndexOf(endTag); 
        int escapeStart = 0; 
        int escapeEnd = line.Length; 
      
        if (tagStart != -1)
        {
          inTag = true; 
          escapeStart = tagStart + startTag.Length; 
        }
      
        if (tagEng != -1)
        {
          escapeEnd = tagEng;
        }
      
        if (!inTag)
        {
          Console.WriteLine(line);
        }
        else
        {
          string beginning = line.Substring(0, escapeStart);
          string middle = line.Substring(escapeStart, escapeEnd - escapeStart);
          string end = line.Substring(escapeEnd, line.Length - escapeEnd); 
          Console.Write(beginning); 
          Console.Write(Escape(middle)); 
          Console.Write(end); 
        }

        if (tagEng != -1)
        {
          inTag = false;
        }

      }
    }  
  
    public static string Escape(string input)
    {
      StringBuilder builder = new StringBuilder(input); 
      builder.Replace("&", "&amp;"); 
      builder.Replace("<", "&lt;"); 
      return builder.ToString(); 
    }

  }
}