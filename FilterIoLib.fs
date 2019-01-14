// IO functions convenient for CLI filters
module FilterIoLib

open System

let readInput _ = Console.In.ReadLine()
let writeOutput(line: string) = Console.Out.WriteLine line

let writeError(line: string) =
    Console.Error.WriteLine line
    0 // return int code, as this is used in match stmt that expects int
