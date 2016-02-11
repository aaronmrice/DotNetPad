/// <reference path="~/Scripts/jquery-1.3.2-vsdoc.js" />


var CodeLinesHighlighted = new Hash();
var OutputLinesHighlighted = new Hash();

$(document).ready(function () {
	if (location.hash.length > 0) {
		var linesToHighlight = location.hash.replace("#", "").split(",");
		for (var i = 0; i < linesToHighlight.length; i += 1) {
			if (linesToHighlight[i].substr(0, 1) == "c") {
				CodeLinesHighlighted.setItem(Number(linesToHighlight[i].substr(1)), Number(linesToHighlight[i].substr(1)));
			}
			if (linesToHighlight[i].substr(0, 1) == "o") {
				OutputLinesHighlighted.setItem(Number(linesToHighlight[i].substr(1)), Number(linesToHighlight[i].substr(1)));
			}
		}
	}
	for (var line in CodeLinesHighlighted.items) {
		HighlightCodeLine(line);
	}
	for (var line in OutputLinesHighlighted.items) {
		HighlightOutputLine(line);
	}
	$(".code .linenumbers td").click(function () {
		var lineNumber = Number($(this).text()) - 1;
		SwapCodeHighlighting(lineNumber);
	});
	$(".output .linenumbers td").click(function () {
		var lineNumber = Number($(this).text()) - 1;
		SwapOutputHighlighting(lineNumber);
	});
	prettyPrint();
	$("pre br").after("\n").remove();
	$("pre span.pln:contains('\n')").before("\n").each(function () {
		var $this = $(this);
		var innerHtml = $this.html();
		innerHtml = innerHtml.replace(/[^\n]*\n/, "");
		$this.html(innerHtml);
	});

    $("textarea").tabby();
    $("#dissasembly-toggle").click(function () {
        $(".disassembly").toggle();
    });
    $(".disassembly").toggle();
});

function HighlightCodeLine(lineNumber) {
	var linesOfCode = $(".code pre.prettyprint").html().split("\n");
	var joiner = "\n";
	if (linesOfCode.length == 1) {
		linesOfCode = $(".code pre.prettyprint").html().split($("br").outerHTML());
		joiner = $("br").outerHTML();
	}
	linesOfCode[lineNumber] = "<span class=\"highlight\">" + linesOfCode[lineNumber] + "</span>";
	$(".code pre.prettyprint").html(linesOfCode.join(joiner));
}

function HighlightOutputLine(lineNumber) {
	var linesOfOutput = $(".output pre").html().split("\n");
	linesOfOutput[lineNumber] = "<span class=\"highlight\">" + linesOfOutput[lineNumber] + "</span>";
	$(".output pre").html(linesOfOutput.join("\n"));
}

function UnhighlightCodeLine(lineNumber) {
	var linesOfCode = $(".code pre.prettyprint").html().split("\n");
	var joiner = "\n";
	if (linesOfCode.length == 1) {
		linesOfCode = $(".code pre.prettyprint").html().split($("br").outerHTML());
		joiner = $("br").outerHTML();
	}
	linesOfCode[lineNumber] = linesOfCode[lineNumber].replace("<span class=\"highlight\">", "");
	linesOfCode[lineNumber] = linesOfCode[lineNumber].substr(0, linesOfCode[lineNumber].length - "</span>".length);
	$(".code pre.prettyprint").html(linesOfCode.join(joiner));
}

function UnhighlightOutputLine(lineNumber) {
	var linesOfOutput = $(".output pre").html().split("\n");
	linesOfOutput[lineNumber] = linesOfOutput[lineNumber].replace("<span class=\"highlight\">", "");
	linesOfOutput[lineNumber] = linesOfOutput[lineNumber].substr(0, linesOfOutput[lineNumber].length - "</span>".length);
	$(".output pre").html(linesOfOutput.join("\n"));
}

function SwapCodeHighlighting(lineNumber) {
	var linesOfCode = $(".code pre.prettyprint").html().split("\n");
	if (linesOfCode.length == 1) {
		linesOfCode = $(".code pre.prettyprint").html().split($("br").outerHTML());
	}
	if (linesOfCode[lineNumber].substr(0, "<span class=\"highlight\">".length) == "<span class=\"highlight\">") {
		UnhighlightCodeLine(lineNumber);
		CodeLinesHighlighted.removeItem(lineNumber);
	}
	else {
		HighlightCodeLine(lineNumber);
		CodeLinesHighlighted.setItem(lineNumber, lineNumber);
	}
	SetLocationHashValue();
}

function SwapOutputHighlighting(lineNumber) {
	var linesOfCode = $(".output pre").html().split("\n");
	if (linesOfCode.length == 1) {
		linesOfCode = $(".output pre").html().split($("br").outerHTML());
	}
	if (linesOfCode[lineNumber].substr(0, "<span class=\"highlight\">".length) == "<span class=\"highlight\">") {
		UnhighlightOutputLine(lineNumber);
		OutputLinesHighlighted.removeItem(lineNumber);
	}
	else {
		HighlightOutputLine(lineNumber);
		OutputLinesHighlighted.setItem(lineNumber, lineNumber);
	}
	SetLocationHashValue();
}

function SetLocationHashValue() {
	var newhash = "#";
	for (var key in CodeLinesHighlighted.items) {
		newhash += ",c" + key;
	}
	for (var key in OutputLinesHighlighted.items) {
		newhash += ",o" + key;
	}
	location.hash = newhash;
}
