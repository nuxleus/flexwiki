using System;
using System.IO;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for IHTMLRenderable.
	/// </summary>
	public interface IHTMLRenderable
	{
		void RenderToHTML(TextWriter output);
	}
}
