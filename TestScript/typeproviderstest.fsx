#load "settings.fsx"
#r "QuickbaseTypeProvider.dll"
open Intuit.Quickbase.TypeProvider

let qb = new QuickbaseTable<TableId="bgpvar6v2", User=Settings.Credentials.user, Password=Settings.Credentials.pwd>()

let frstrow = qb.Records |> Seq.head

let v = frstrow.Label