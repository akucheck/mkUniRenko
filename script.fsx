let barState = "0.00,0.00,0.00,0.00,X|s1,r1,s2,r2,s3,r3"
let testStr = "012345|67890"
let splitStringAtBar (line : string) =
    let firstPart = line.Substring(0, line.IndexOf("|"))
    let secondPart =
        line.Substring( 1 + line.IndexOf("|"), -1 + line.Length  - line.IndexOf("|"))
    (firstPart, secondPart)

let res1,res2 = splitStringAtBar testStr

testStr.Substring(7,5)



let initConnectorState = "1,2,3,4,5,6"
//==============
let foo = 1
let bar = 2
let baz = 3
let bum = 4

// let val1, val2 =
//         if (foo = 0) then (bar, baz)
//         else (baz, bum)

let (val1, val2) = 
    match (foo) with 
    | 0 -> (bar, baz)
    | _ -> (baz, bum)