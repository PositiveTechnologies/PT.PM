var express = require("express");
var app = express();

app.get("/redirect", function(req, res) {
	res.redirect(req.query.path);
});

app.get("/send", function(req, res) {
	res.send(req.body.data);
});

app.get("/set", function(req, res) {
	res.set("Header", req.param.data);
});

app.listen(3000);
