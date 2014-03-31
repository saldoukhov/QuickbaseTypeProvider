module Xml

open System.IO
open System.Xml.Linq

let xname n = XName.op_Implicit (n)
let parseXml s = XElement.Parse s
let loadXml (stream : Stream) = XElement.Load stream
let descendants s (e : XElement) = e.Descendants(xname s)
let element s (e : XElement) = e.Element(xname s)
let elementValue s e = (element s e).Value

