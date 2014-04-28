namespace QuickbaseTypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Xml.Linq
open Quickbase
open Xml

type QuickbaseRecord = 
    { Xml : XElement }
    with member this.StringVal (name: string) = this.Xml |> elementValue (name.ToLower()) 
         member this.FloatVal (name: string) = this.Xml |> elementValue (name.ToLower()) |> System.Double.Parse

type QuickbaseTableData(tableId, user, password) = 
    let quickbase = new Quickbase()
    let ticket = quickbase.Authenticate user password |> Async.RunSynchronously
    let xml = quickbase.GetData tableId |> Async.RunSynchronously
    let data = xml |> Seq.map (fun x -> { Xml = x })
    member __.Records = data

[<TypeProvider>]
type public QuickbaseProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()
    let ns = "Intuit.Quickbase.TypeProvider"
    let asm = System.Reflection.Assembly.GetExecutingAssembly()
    let quickbaseType = ProvidedTypeDefinition(asm, ns, "QuickbaseTable", Some(typeof<obj>))
    
    let buildTypes (typeName : string) (args : obj []) = 
        let tableIdParameter = args.[0] :?> string
        let userParameter = args.[1] :?> string
        let passwordParameter = args.[2] :?> string

        let provider = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<QuickbaseTableData>)

        let recordType = ProvidedTypeDefinition("Record", Some typeof<QuickbaseRecord>, HideObjectMethods = true)

        let quickbase = new Quickbase()
        let ticket = quickbase.Authenticate userParameter passwordParameter |> Async.RunSynchronously
        let schema = quickbase.GetSchema tableIdParameter |> Async.RunSynchronously
        schema |> Seq.iter (fun x -> 
            let propName = x.Label.Replace(' ', '_')
            let recNumProp = ProvidedProperty(propName, 
                                match x.Type with
                                | Text -> typeof<string>
                                | Float -> typeof<float>
                                , 
                                GetterCode = match x.Type with
                                                | Text -> fun [ record ] -> <@@ (%%record:QuickbaseRecord).StringVal propName @@>
                                                | Float -> fun [ record ] -> <@@ (%%record:QuickbaseRecord).FloatVal propName @@>)
            recordType.AddMember(recNumProp)
        )

        let ctor1 = ProvidedConstructor([], InvokeCode = fun [] -> <@@ QuickbaseTableData(tableIdParameter, userParameter, passwordParameter) @@>)
//        let ctor2 = ProvidedConstructor([ProvidedParameter("TableId", typeof<string>)], InvokeCode = fun [tableId] -> <@@ QuickbaseTableData(%%tableId) @@>)

        let recordsProperty = 
            ProvidedProperty
                ("Records", typedefof<seq<_>>.MakeGenericType(recordType), 
                 GetterCode = fun [ tableId ] -> <@@ (%%tableId : QuickbaseTableData).Records @@>)

        provider.AddMember(ctor1)
//        provider.AddMember(ctor2)
        provider.AddMember(recordsProperty)
        provider.AddMember(recordType)

        provider
    
    let parameters = [ 
        ProvidedStaticParameter("TableId", typeof<string>);
        ProvidedStaticParameter("User", typeof<string>);
        ProvidedStaticParameter("Password", typeof<string>) 
        ]
    do quickbaseType.DefineStaticParameters(parameters, buildTypes)
    do this.AddNamespace(ns, [quickbaseType])

[<TypeProviderAssembly>]
do ()
