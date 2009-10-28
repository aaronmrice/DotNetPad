/// <reference path="~/Scripts/jquery-1.3.2-vsdoc.js" />

$(document).ready(function() {
	prettyPrint();
	if(location.hash && location.hash.length > 0) {
		$(".code td.highlight").attr("class","");
		$(location.hash).attr("class","highlight");
	}
	$(".linenumbers td a").click(function() {
		var lineNumber = $(this).attr("href");
		$(".code td.highlight").attr("class","");
		$(lineNumber).attr("class", "highlight");
	});
});