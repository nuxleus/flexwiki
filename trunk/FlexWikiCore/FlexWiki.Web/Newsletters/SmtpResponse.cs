using System;

namespace FlexWiki.Web.Newsletters
{
  /// <summary>
  /// Get / Set the name of the SMTP mail server
  /// </summary>
  internal enum SmtpResponse: int
  {
    CONNECT_SUCCESS = 220,
    GENERIC_SUCCESS = 250,
    DATA_SUCCESS	= 354,
    AUTH_PROMPT = 334,
    AUTH_SUCCESS = 235,
    QUIT_SUCCESS	= 221
  }
}
