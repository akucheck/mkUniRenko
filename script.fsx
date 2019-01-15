let barState = "0.00,0.00,0.00,0.00,X|s1,r1,s2,r2,s3,r3"
let testStr = "012345|67890"
let splitStringAtBar (line : string) =
    let firstPart = line.Substring(0, line.IndexOf("|"))
    let secondPart =
        line.Substring( 1 + line.IndexOf("|"), -1 + line.Length  - line.IndexOf("|"))
    (firstPart, secondPart)

let res1,res2 = splitStringAtBar testStr

testStr.Substring(7,5)
