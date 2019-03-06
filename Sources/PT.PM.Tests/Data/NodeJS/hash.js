const crypto = require('crypto');
const fs = require('fs');
// const hash = crypto.createHash('sha256');
const hash = crypto.createHash('sha1');

const input = fs.createReadStream('test.js');
input.pipe(hash).pipe(process.stdout);


const fs = require('fs');
const hash = require('crypto').createHash('md5');

const input = fs.createReadStream('test.js');
input.pipe(hash).pipe(process.stdout);


var key = "whatewer";
var hmac = CryptoJS.algo.HMAC.create(CryptoJS.algo.MD5, key);
