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
using System.Text;
using System.Net.Mail; 

namespace FlexWiki.Web.Newsletters
{
  internal class DaemonBasedDeliveryBoy : IDeliveryBoy
  {
    private StringWriter _log;
    private string _smtpHost;
    private string _smtpUser;
    private string _smtpPassword;
    private bool _sendAsAttachments; 

    internal DaemonBasedDeliveryBoy(string smtpHost, string smtpUser, string smtpPassword)
      : this(smtpHost, smtpUser, smtpPassword, false)
    {
    }

    internal DaemonBasedDeliveryBoy(string smtpHost, string smtpUser, string smtpPassword, bool sendAsAttachments)
    {
      _smtpHost = smtpHost;
      _smtpUser = smtpUser;
      _smtpPassword = smtpPassword;
      _sendAsAttachments = sendAsAttachments; 
    }

    

    public StringWriter Log
    {
      get { return _log; }
      set { _log = value; }
    }

    public void Deliver(string to, string from, string subject, string body)
    {
      SmtpMail mailer = new SmtpMail();
      StringWriter smtpLog = new StringWriter(new StringBuilder());
      mailer.sw = smtpLog;
      MailMessage message = new MailMessage(from, to);
      message.Subject = subject;
      if (!_sendAsAttachments)
      {
        message.Body = body;
        message.IsBodyHtml = true;
      }
      else
      {
        message.Body = "The wiki has been updated. See the attached files for changes."; 
      }

      string success = null;
      try
      {
        if (!_sendAsAttachments)
        { 
          success = mailer.Send(message, _smtpHost, _smtpUser, _smtpPassword);
        }
        else
        {
          Hashtable atts = new Hashtable(); 
          atts.Add("Changes.htm", System.Text.Encoding.UTF8.GetBytes(body)); 
          success = mailer.Send(message, _smtpHost, _smtpUser, _smtpPassword, atts); 
        }
      }
      catch (Exception e)
      {
        Log.WriteLine("Exception while sending to host '" + _smtpHost + "': " + e.ToString());
      }
      if (success == null)
      {
        Log.WriteLine("SMTP deliver succeeded to: " + to);
      }
      else
      {
        Log.WriteLine("SMTP deliver failed to: " + to);
        Log.Write(smtpLog.ToString());
      }
    }
  }
}
