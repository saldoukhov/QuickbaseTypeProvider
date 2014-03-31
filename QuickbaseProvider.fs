namespace QuickbaseTypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

type QuickbaseTableData(tableId) = 
    let data = [ "a"; "b"; "c" ] |> Seq.toList
    member __.Records = data

[<TypeProvider>]
type public QuickbaseProvider(cfg : TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()
    let ns = "Intuit.Quickbase.TypeProvider"
    let asm = System.Reflection.Assembly.GetExecutingAssembly()
    let quickbaseType = ProvidedTypeDefinition(asm, ns, "QuickbaseTable", Some(typeof<obj>))
    
    let buildTypes (typeName : string) (args : obj []) = 
        let tableIdParameter = "12345"

        let provider = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<QuickbaseTableData>)

        let recordType = ProvidedTypeDefinition("Record", Some typeof<string>)

        let recNumProp = ProvidedProperty("RecordNumber", typeof<string>, GetterCode = fun [ record ] -> <@@ (%%record:string) @@>)

        recordType.AddMember(recNumProp)

        let ctor1 = ProvidedConstructor([], InvokeCode = fun [] -> <@@ QuickbaseTableData(tableIdParameter) @@>)
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
    
    let parameters = [ ProvidedStaticParameter("TableId", typeof<string>) ]
    do quickbaseType.DefineStaticParameters(parameters, buildTypes)
    do this.AddNamespace(ns, [quickbaseType])

[<TypeProviderAssembly>]
do ()
