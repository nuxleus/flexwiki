<%@ Page language="c#" Codebehind="WikiEdit.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.WikiEdit" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>
			<%= TheTopic.ToString() %>
			(edit)</title>
		<meta name="Robots" content="NOINDEX, NOFOLLOW">
		<%= InsertStylesheetReferences() %>   
		<script  type=""text/javascript"" language="javascript">

function CalcEditBoxHeight()
{
	var answer = CalcEditZoneHeight();
	return answer;
}

function CalcEditZoneHeight()
{
	var answer = MainHeight();
	return answer;
}

function ShowTip(tipid)
{
	var s = document.all(tipid);
	TipArea.innerHTML = s.innerHTML;
	TipArea.style.display = 'block';
}

function preview()
{
	var s = document.all("Text1").value;
	document.all("body").value = s;
	// window.open('preview.aspx', document.all("form2").target, '');
	window.open('about:blank', 'previewWindow');
	document.getElementById("Form2").submit();
}

function SetUserName()
{
	var r = document.getElementById("UserNameEntryField");
	if (r != null)
		document.getElementById("UserSuppliedName").value = r.value;
}

function Save()
{
	SetUserName();
	var r = document.all("ReturnTopic");
	if (r != null)
		r.value = ""; // prevent return action by emptying this out
	document.all("Form1").submit();
}

function SaveAndReturn()
{
	SetUserName();
	document.all("Form1").submit();
}

function search()
{
	window.open('search.aspx'); 
}


function MainHeight()
{
	var answer = document.body.clientHeight;
	var e;
	return answer;
}
			
function MainWidth()
{
	var answer = document.body.clientWidth;
	var e;
	
	e = document.getElementById("Sidebar");
	if (e != null)
		answer -= e.scrollWidth;
	return answer;
}


		</script>
		<style>

@media all
{
    tool\:tip   {
                behavior: url(tooltip_js.htc)
                }
}

.EditZone {
	background: lemonchiffon;
	overflow: hidden;
	height: expression(CalcEditZoneHeight());
	width: 100%;
}

.tip
{
    font-weight: bold;
}

.tipBody
{
	font: 8pt Verdana;
}

.TipArea
{
	margin: 3px;
	display: none;
	border: 1px solid #ffcc00;
	font: 8pt Verdana;
	padding: 4px;
}

.EditBox {
    font: 9pt Courier New;
    background: whitesmoke;
	height: expression(CalcEditBoxHeight());
	width: 100%;
}

		</style>
	</HEAD>
	<% DoPage(); %>
</HTML>
