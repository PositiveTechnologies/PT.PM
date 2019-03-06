if ('development' == app.get('env')) {
    console.log("No TLS validation");
    process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";
}

var http = require('http');
var curl = require('node-curl');

http.createServer(function(request,response){

    var url = 'https://url';
    url += request.url;

    console.log(url);


    curl(url,
        { 
            SSL_VERIFYPEER : 0
        },
        function(err){
            response.end(this.body);
        })

}).listen(8000);
