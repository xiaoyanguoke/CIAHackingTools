/**
 * @description Polyfill for element based media-queries.
 * @version 0.4.0
 * @author Marc J. Schmidt
 * @license MIT, http://www.opensource.org/licenses/MIT
 * @url http://marcj.github.io/css-element-queries/
 *
 * The MIT License (MIT)
 *
 * Copyright (c) 2013 Marc J. Schmidt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
(function(f,e){"function"===typeof define&&define.amd?define(e):"object"===typeof exports?module.exports=e():f.ResizeSensor=e()})(this,function(){function f(c,d){var b=Object.prototype.toString.call(c),e=0,f=c.length;if("[object Array]"===b||"[object NodeList]"===b||"[object HTMLCollection]"===b||"[object Object]"===b||"undefined"!==typeof jQuery&&c instanceof jQuery||"undefined"!==typeof Elements&&c instanceof Elements)for(;e<f;e++)d(c[e]);else d(c)}if("undefined"===typeof window)return null;var e=
window.requestAnimationFrame||window.mozRequestAnimationFrame||window.webkitRequestAnimationFrame||function(c){return window.setTimeout(c,20)},g=function(c,d){function b(){var a=[];this.add=function(b){a.push(b)};var b,c;this.call=function(){b=0;for(c=a.length;b<c;b++)a[b].call()};this.remove=function(e){var d=[];b=0;for(c=a.length;b<c;b++)a[b]!==e&&d.push(a[b]);a=d};this.length=function(){return a.length}}function v(a,b){return a.currentStyle?a.currentStyle[b]:window.getComputedStyle?window.getComputedStyle(a,
null).getPropertyValue(b):a.style[b]}function h(a,c){if(!a.resizedAttached)a.resizedAttached=new b,a.resizedAttached.add(c);else if(a.resizedAttached){a.resizedAttached.add(c);return}a.resizeSensor=document.createElement("div");a.resizeSensor.className="resize-sensor";a.resizeSensor.style.cssText="position: absolute; left: 0; top: 0; right: 0; bottom: 0; overflow: hidden; z-index: -1; visibility: hidden;";a.resizeSensor.innerHTML='<div class="resize-sensor-expand" style="position: absolute; left: 0; top: 0; right: 0; bottom: 0; overflow: hidden; z-index: -1; visibility: hidden;"><div style="position: absolute; left: 0; top: 0; transition: 0s;"></div></div><div class="resize-sensor-shrink" style="position: absolute; left: 0; top: 0; right: 0; bottom: 0; overflow: hidden; z-index: -1; visibility: hidden;"><div style="position: absolute; left: 0; top: 0; transition: 0s; width: 200%; height: 200%"></div></div>';
a.appendChild(a.resizeSensor);"static"==v(a,"position")&&(a.style.position="relative");var d=a.resizeSensor.childNodes[0],f=d.childNodes[0],g=a.resizeSensor.childNodes[1],n,k,l,m,p=a.offsetWidth,q=a.offsetHeight,r=function(){f.style.width="100000px";f.style.height="100000px";d.scrollLeft=1E5;d.scrollTop=1E5;g.scrollLeft=1E5;g.scrollTop=1E5};r();var h=function(){k=0;n&&(p=l,q=m,a.resizedAttached&&a.resizedAttached.call())},t=function(){l=a.offsetWidth;m=a.offsetHeight;(n=l!=p||m!=q)&&!k&&(k=e(h));
r()},u=function(a,b,c){a.attachEvent?a.attachEvent("on"+b,c):a.addEventListener(b,c)};u(d,"scroll",t);u(g,"scroll",t)}f(c,function(a){h(a,d)});this.detach=function(a){g.detach(c,a)}};g.detach=function(c,d){f(c,function(b){if(b.resizedAttached&&"function"==typeof d&&(b.resizedAttached.remove(d),b.resizedAttached.length()))return;b.resizeSensor&&(b.contains(b.resizeSensor)&&b.removeChild(b.resizeSensor),delete b.resizeSensor,delete b.resizedAttached)})};return g});

(function(){var f="discoveryRestUrl methodId apiKey clientId useCors showTitle defaultScopes extraDescription params".split(" "),g=!1;
function k(){if(!g){g=!0;for(var d=document.querySelectorAll(".apis-explorer"),a={},e=0;e<d.length;a={b:a.b,c:a.c,a:a.a},e++){a.b=d[e];a.b.textContent="";a.a=document.createElement("iframe");a.c=[];f.forEach(function(a){return function(b){var h=a.b.getAttribute(l(b));null!=h&&a.c.push(b+"="+encodeURIComponent(h))}}(a));var c=window.location.search.match(/[&?](authuser)=(\d+)/i);c&&a.c.push(c[1]+"="+c[2]);a.a.src="https://explorer.apis.google.com/embedded.html?"+a.c.join("&");a.a.frameBorder="0";a.a.onload=
function(a){return function(){a.a.contentWindow.postMessage("apix_frame_enable","https://explorer.apis.google.com")}}(a);a.a.width=a.b.offsetWidth;c=function(a){return function(){setTimeout(function(){window.requestAnimationFrame(function(){var b=a.b.offsetWidth;a.a.width!=b&&(a.a.width=b,a.a.contentWindow.postMessage({apix_embedder_event:"resized"},"https://explorer.apis.google.com"))})})}}(a);a.b.appendChild(a.a);new ResizeSensor(a.b,c);window.addEventListener("resize",c);window.addEventListener("message",
function(a){return function(b){"https://explorer.apis.google.com"===b.origin&&b.source===a.a.contentWindow&&"object"===typeof b.data&&"apix_event"in b.data&&("resize"==b.data.apix_event?a.a.height=b.data.height+"px":console.debug("Unknown event",b.data))}}(a))}}}function l(d){return"data-"+d.replace(/[A-Z]/g,function(a){return"-"+a.toLowerCase()})}window.addEventListener("load",k);"complete"==document.readyState&&k();}).call(this);
