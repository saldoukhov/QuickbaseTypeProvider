﻿module Quickbase

open System.Net.Http
open System.Net.Http.Headers
open Http
open Xml

type QuickBaseFieldType = 
    | Text
    | Float

type QuickBaseField = 
    { Label : string
      Type : QuickBaseFieldType }

type Quickbase() = 
    let client = new HttpClient()
    let mutable ticket = null
    let qbGetStringFromUri uri = getStringAsync uri client
    let envelope query = sprintf "<qdbapi><ticket>%s</ticket>%s</qdbapi>" ticket query
    
    let qbCall (api : string) (query : string) tableId = 
        async { 
            let uri = sprintf "https://intuitcorp.quickbase.com/db/%s" tableId
            let content = new StringContent(envelope query)
            content.Headers.ContentType <- MediaTypeHeaderValue.Parse("application/xml")
            content.Headers.Add("QUICKBASE-ACTION", api)
            let! response = client.PostAsync(uri, content) |> Async.AwaitTask
            let! stream = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
            let xml = stream |> loadXml
            return xml
        }
    
    member this.Authenticate userName password = 
        async { 
            let uri = 
                sprintf "https://intuitcorp.quickbase.com/db/main?act=API_Authenticate&username=%s&password=%s" userName 
                    password
            let! xml = qbGetStringFromUri uri
            ticket <- xml
                      |> parseXml
                      |> elementValue "ticket"
            return ticket
        }
    
    member this.GetSchema tableId = 
        async { 
            let! xml = qbCall "API_GetSchema" "" tableId
            let label el = el |> mapToElement "label"
            return xml
                   |> descendants "field"
                   |> Seq.map (fun x -> 
                          { Label = x |> elementValue "label"
                            Type = 
                                match x |> attributeValue "field_type" with
                                | "float" -> Float
                                | _ -> Text })
        }
    
    member this.GetData tableId = async { 
                                      //            let! xml = qbCall "API_DoQuery" "<fmt>structured</fmt>" tableId
                                      let! xml = qbCall "API_DoQuery" "" tableId
                                      return xml |> descendants "record" }
