var yaml = require("js-yaml");
var express = require("express");
var app = express();

app.get("/eval", function(req, res) {
	eval(req.query);
	eval(req.query.func);
});

app.get("/interval", function(req, res) {
	setInterval(req.body, 100);
	setInterval(req.body.code, 100);
	setInterval(req.query.func, 100);
});

app.get("/timeout", function(req, res) {
	setTimeout(req.param, 100);
	setTimeout(req.body.code, 100);
	setTimeout(req.param.func, 100);
});

app.get("/function", function(req, res) {
	new Function(req.query);
	new Function("x", "y", req.query.func);
});

app.get("/yaml", function(req, res) {
	require("js-yaml").load(req.body);
	yaml.load(req.param.yaml);
});

app.listen(3000);
