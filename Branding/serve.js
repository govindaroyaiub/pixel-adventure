// Tiny static server for previewing Branding/ pages: node Branding/serve.js
const http = require("http");
const fs = require("fs");
const path = require("path");

const root = __dirname;
const port = 5173;
const types = { ".html": "text/html", ".png": "image/png", ".js": "text/javascript", ".css": "text/css" };

http.createServer((req, res) => {
  const url = decodeURIComponent(req.url.split("?")[0]);
  const file = path.join(root, url === "/" ? "high-devs-logo-concepts.html" : url);
  if (!file.startsWith(root)) { res.writeHead(403); return res.end(); }
  fs.readFile(file, (err, data) => {
    if (err) { res.writeHead(404); return res.end("not found"); }
    res.writeHead(200, { "Content-Type": types[path.extname(file)] || "application/octet-stream" });
    res.end(data);
  });
}).listen(port, () => console.log(`Branding preview on http://localhost:${port}`));
