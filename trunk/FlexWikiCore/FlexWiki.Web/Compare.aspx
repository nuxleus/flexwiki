<%@ Page language="c#" Codebehind="Compare.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Compare" %>
<%StartPage();%>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
  <HEAD>
		<TITLE><%= GetTitle() %></TITLE>
		<%= InsertStylesheetReferences() %>
	</HEAD>
	<body onload="focus();">
	<div id="content">
	  <a name="top" id="contentTop"></a>
	  	  <h1 class="firstHeading">Compare two versions</h1>
	  <div id="bodyContent">
		<% DoPage(); %>
		</div>
	</div>
	</body>
</HTML>
<%
	EndPage();
%>
