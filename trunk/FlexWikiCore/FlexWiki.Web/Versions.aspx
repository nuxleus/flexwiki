<%@ Page language="c#" Codebehind="Versions.aspx.cs" AutoEventWireup="false" Inherits="FlexWiki.Web.Versions" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<TITLE><%= GetTitle() %></TITLE>
		<meta name="Robots" content="NOINDEX, NOFOLLOW">
		<%= InsertStylesheetReferences() %>
<script  type="text/javascript" language="javascript">
function PageInit() {
    hf = document.getElementById('topicversions');
    if(!hf) return;
    lis = hf.getElementsByTagName('li');
    for (i=0;i<lis.length;i++) {
        inputs=lis[i].getElementsByTagName('input');
        if(inputs[0] && inputs[1]) {
                inputs[0].onclick = diffcheck;
                inputs[1].onclick = diffcheck;
        }
    }
    diffcheck();
}
function diffcheck() { 
    var dli = false; // the li where the diff radio is checked
    var oli = false; // the li where the oldid radio is checked
    hf = document.getElementById('topicversions');
    if(!hf) return;
    lis = hf.getElementsByTagName('li');
    for (i=0;i<lis.length;i++) {
        inputs=lis[i].getElementsByTagName('input');
        if(inputs[1] && inputs[0]) {
            if(inputs[1].checked || inputs[0].checked) { // this row has a checked radio button
                if(inputs[1].checked && inputs[0].checked && inputs[0].value == inputs[1].value) return false;
                if(oli) { // it's the second checked radio
                    if(inputs[1].checked) {
                    oli.className = "selected";
                    return false 
                    }
                } else if (inputs[0].checked) {
                    return false;
                }
                if(inputs[0].checked) dli = lis[i];
                if(!oli) inputs[0].style.visibility = 'hidden';
                if(dli) inputs[1].style.visibility = 'hidden';
                lis[i].className = "selected";
                oli = lis[i];
            }  else { // no radio is checked in this row
                if(!oli) inputs[0].style.visibility = 'hidden';
                else inputs[0].style.visibility = 'visible';
                if(dli) inputs[1].style.visibility = 'hidden';
                else inputs[1].style.visibility = 'visible';
                lis[i].className = "";
            }
        }
    }
}
</script>

	</HEAD>
	<body class='Dialog' onload="PageInit();">
	
	<div id='StaticTopicBar' class='StaticTopicBar' style='display: block'><%= TheTopic.ToString() %> </div>
	<h3>Previous Versions</h3>
	<p>Select differences: Select the radio boxes of any versions and press 'Enter' or press the button below or press key [alt-v] to compare these versions.<br />
	Legend: (Current) = shows difference with current version, (Previos) = shows difference with preceding version</p>
	<form action="<%= LinkToCompare() %>" method="GET">
	<input type="hidden" name="topic" value="<%=TheTopic.Fullname%>" />
	<input type="submit" accesskey="v" class="standardsButton" title="Show the differences between two selected versions of this topics. [alt-v]" value="Compare selected versions" />
		<ul id="topicversions">
				<asp:PlaceHolder ID="phResult" Runat="server" />
		</ul>
	</form>
	</body>
</HTML>
