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
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Web.Mail;
using System.Collections;

namespace FlexWiki.Newsletters
{
	/// <summary>
	/// provides methods to send email via smtp direct to mail server
	/// </summary>
  public class SmtpMail
  {
    /// <summary>
    /// Get / Set the name of the SMTP mail server
    /// </summary>
    private enum SMTPResponse: int
    {
      CONNECT_SUCCESS = 220,
      GENERIC_SUCCESS = 250,
      DATA_SUCCESS	= 354,
      AUTH_PROMPT = 334,
      AUTH_SUCCESS = 235,
      QUIT_SUCCESS	= 221
    }

    public StringWriter sw;

    string LogExit(string s)
    {
      if (sw != null)
        sw.WriteLine(s);
      return s;
    }


    public string Send(MailMessage message, string SmtpServer, string SMTPUser, string SMTPPassword)
    {
      return Send(message, SmtpServer, SMTPUser, SMTPPassword, null); 
    }
    
    // Added the ability to send attachments from in-memory representations, in addition to files
    // additioalAttachments should be a Hashtable containing a collection of byte[] keyed by strings
    // The string is the name, and the byte[] is the content. 
    public string Send(MailMessage message, string SmtpServer, string SMTPUser, string SMTPPassword, 
      Hashtable additionalAttachments)
    {
      if (sw != null)
        sw.WriteLine("Starting to send mail via {0}", SmtpServer);
      IPHostEntry IPhst = Dns.Resolve(SmtpServer);
      IPEndPoint endPt = new IPEndPoint(IPhst.AddressList[0], 25);
      Socket s= new Socket(endPt.AddressFamily, SocketType.Stream,ProtocolType.Tcp);
      s.Connect(endPt);
    		
      if(!Check_Response(s, SMTPResponse.CONNECT_SUCCESS))
      {				
        s.Close();
        return LogExit("Unable to connect to server");
      }
    			
      Senddata(s, string.Format("EHLO {0}\r\n", Dns.GetHostName() ));
      if(!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
      {
        s.Close();
        return  LogExit("HELO command failed");
      }

      if (SMTPPassword != null)
      {
        Senddata(s, string.Format("AUTH LOGIN\r\n"));
        if(!Check_Response(s, SMTPResponse.AUTH_PROMPT))
        {
          s.Close();
          return  LogExit("AUTH LOGIN command failed");
        }

        string user64 = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(SMTPUser));
        Senddata(s, string.Format(user64 + "\r\n"));
        if(!Check_Response(s, SMTPResponse.AUTH_PROMPT))
        {
          s.Close();
          return  LogExit("AUTH username failed");
        }
				
        string pw64 = System.Convert.ToBase64String(Encoding.ASCII.GetBytes(SMTPPassword));
        Senddata(s, string.Format(pw64 + "\r\n"), "[password  hidden]\n");
        if(!Check_Response(s, SMTPResponse.AUTH_SUCCESS))
        {
          s.Close();
          return  LogExit("AUTH password failed");
        }
      }

      Senddata(s, string.Format("MAIL From: {0}\r\n", message.From ));
      if(!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
      {
				
        s.Close();
        return  LogExit("MAIL command failed");
      }
    			
      string _To = message.To;
      string[] Tos= _To.Split(new char[] {';'});
      foreach (string To in Tos)
      {
        Senddata(s, string.Format("RCPT TO: {0}\r\n", To));
        if(!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
        {
          s.Close();
          return  LogExit("RCPT TO (" + To + ") failed");
        }
      }
    			
      if(message.Cc!=null)
      {
        Tos= message.Cc.Split(new char[] {';'});
        foreach (string To in Tos)
        {
          Senddata(s, string.Format("RCPT TO: {0}\r\n", To));
          if(!Check_Response(s, SMTPResponse.GENERIC_SUCCESS))
          {					
            s.Close();
            return  LogExit("RCPT TO (" + To + ") failed");
          }
        }
      }
    			
      StringBuilder Header=new StringBuilder();
      Header.Append("From: " + message.From + "\r\n");
      Tos= message.To.Split(new char[] {';'});
      Header.Append("To: ");
      for( int i=0; i< Tos.Length; i++)
      {
        Header.Append( i > 0 ? "," : "" );
        Header.Append(Tos[i]);
      }
      Header.Append("\r\n");
      if(message.Cc!=null)
      {
        Tos= message.Cc.Split(new char[] {';'});
        Header.Append("Cc: ");
        for( int i=0; i< Tos.Length; i++)
        {
          Header.Append( i > 0 ? "," : "" );
          Header.Append(Tos[i]);
        }
        Header.Append("\r\n");
      }
      Header.Append( "Date: " );
      Header.Append(DateTime.Now.ToString("ddd, d M y H:m:s z" ));
      Header.Append("\r\n");
      Header.Append("Subject: " + message.Subject+ "\r\n");
      Header.Append( "X-Mailer: FlexWiki\r\n" );
      string MsgBodyBeforePeriodEscaping = message.Body;
      string MsgBody = "";
      foreach (string each in MsgBodyBeforePeriodEscaping.Split(new char[]{'\n'}) )
      {
        string line = each;
        if (line.StartsWith("<h2"))
        {
          string sx = line;
        }

        line = line.Trim(new char[]{'\r'});
        if (line.StartsWith("."))
          MsgBody += ".";
        MsgBody += line + "\n";
      }

      if(!MsgBody.EndsWith("\r\n"))
        MsgBody+="\r\n";

      StringBuilder sb = new StringBuilder();

      bool hasMessageAttachments = message.Attachments.Count > 0;
      bool hasAdditionalAttachments = (additionalAttachments != null) && (additionalAttachments.Count > 0); 

      if(hasMessageAttachments || hasAdditionalAttachments)
      {
        Header.Append( "MIME-Version: 1.0\r\n" );
        Header.Append( "Content-Type: multipart/mixed; boundary=unique-boundary-1\r\n" );
        Header.Append("\r\n");
        Header.Append( "This is a multi-part message in MIME format.\r\n" );

        sb.Append("--unique-boundary-1\r\n");
        sb.Append("Content-Type: text/plain\r\n");
        sb.Append("Content-Transfer-Encoding: 7Bit\r\n");
        sb.Append("\r\n");
        sb.Append(MsgBody + "\r\n");
        sb.Append("\r\n");
      }
      else
      {
        Header.Append("Content-Type: text/html\r\n");
        sb.Append(MsgBody);
        sb.Append("\r\n");
      }

      if (hasMessageAttachments)
      {
    				
        foreach(object o in message.Attachments)
        {
          MailAttachment a = o as MailAttachment;
          byte[] binaryData;
          if(a!=null)
          {
            FileInfo f = new FileInfo(a.Filename);
            FileStream fs = f.OpenRead();
            binaryData = new Byte[fs.Length];
            long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
            fs.Close();

            AppendAttachment(sb, f.Name, binaryData); 
    					
          }
        }

      }

      if (hasAdditionalAttachments)
      {
        foreach (DictionaryEntry de in additionalAttachments)
        {
          int i = 0; 
          byte[] att = additionalAttachments[de.Key] as byte[]; 
          string name = de.Key as string; 
          if (name == null)
          {
            name = "attachment" + (++i).ToString(); 
          }
          if (att != null)
          {
            AppendAttachment(sb, name, att); 
          }
        }
      }

      MsgBody=sb.ToString();
    			
      Senddata(s, ("DATA\r\n"));
      if(!Check_Response(s, SMTPResponse.DATA_SUCCESS))
      {				
        s.Close();
        return  LogExit("DATA command failed");
      }
      Header.Append( "\r\n" );
      Header.Append( MsgBody );
      Header.Append( ".\r\n" );
//      Header.Append( "\r\n" );
//      Header.Append( "\r\n" );

      Senddata(s, Header.ToString());
      if(!Check_Response(s, SMTPResponse.GENERIC_SUCCESS ))
      {
				
        s.Close();
        return  LogExit("DATA sending failed");
      }			
    			
      Senddata(s, "QUIT\r\n");
      // I was seeing my SMTP server hang waiting for a response to the QUIT, so I added
      // this one-minute timeout.
      Check_Response(s, SMTPResponse.QUIT_SUCCESS, 60000);
      s.Close();    			
      return null;
    }

    private void AppendAttachment(StringBuilder sb, string name, byte[] binaryData)
    {
      sb.Append("--unique-boundary-1\r\n");
      sb.Append("Content-Type: application/octet-stream; file=" + name + "\r\n");
      sb.Append("Content-Transfer-Encoding: base64\r\n");
      sb.Append("Content-Disposition: attachment; filename=" + name + "\r\n");
      sb.Append("\r\n");
      string base64String = System.Convert.ToBase64String(binaryData, 0,binaryData.Length);
    						
      for(int i=0; i< base64String.Length ; )
      {
        int nextchunk=100;
        if(base64String.Length - (i + nextchunk ) <0)
          nextchunk = base64String.Length -i;
        sb.Append(base64String.Substring(i, nextchunk));
        sb.Append("\r\n");
        i+=nextchunk;
    						
      }
      sb.Append("\r\n");
    }

    private void Senddata(Socket s, string msg)
    {
      Senddata(s, msg, msg);
    }

    private void Senddata(Socket s, string msg, string logString)
    {
      if (sw != null && logString != null)
        sw.WriteLine(">>{0}", logString);
      byte[] _msg = Encoding.ASCII.GetBytes(msg);
      s.Send(_msg , 0, _msg .Length, SocketFlags.None);
    }

    private bool Check_Response(Socket s, SMTPResponse response_expected)
    {
      return Check_Response(s, response_expected, -1);
    }
   
    // I was seeing my SMTP server hang waiting for a response to the QUIT, so I added
    // this timeout. Pass -1 to wait forever. Timeout is in milliseconds. 
    private bool Check_Response(Socket s, SMTPResponse response_expected, int timeout)
    {
      string sResponse;
      int response;
      byte[] bytes = new byte[1024];

      int start = Environment.TickCount; 
      while (s.Available==0)
      {
        System.Threading.Thread.Sleep(101);

        if (timeout != -1)
        {
          if ((Environment.TickCount - start) > timeout)
          {
            // We timed out
            return false; 
          }
        }
      }

    			
      s.Receive(bytes, 0, s.Available, SocketFlags.None);
      sResponse = Encoding.ASCII.GetString(bytes);
      if (sw != null)
        sw.WriteLine(">>{0}", sResponse);
      response = Convert.ToInt32(sResponse.Substring(0,3));
      if(response != (int)response_expected)
        return false;
      return true;
    }
  }
}

