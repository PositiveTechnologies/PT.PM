var fs = require("fs");
var express = require("express");
var app = express();

app.get("/stream", function(req, res) {
	var stream = fs.createReadStream(req.query.path);
});

app.get("/file", function(req, res) {
	fs.readFile(req.param.path, (err, data) => {
		if (err) throw err;
	 	console.log(data);
	});
});

app.listen(3000);
