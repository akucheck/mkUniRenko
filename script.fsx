let barState = "0.00,0.00,0.00,0.00,X|s1,r1,s2,r2,s3,r3"
let testStr = "012345|67890"
let splitStringAtBar (line : string) =
    let firstPart = line.Substring(0, line.IndexOf("|"))
    let secondPart =
        line.Substring( 1 + line.IndexOf("|"), -1 + line.Length  - line.IndexOf("|"))
    (firstPart, secondPart)

let res1,res2 = splitStringAtBar testStr

testStr.Substring(7,5)

////////////
type ConnectorRow =
    { seqNum1 : int
      role1 : int
      seqNum2 : int
      role2 : int
      seqNum3 : int
      role3 : int }

let serializeConnectorRow (currRow : ConnectorRow) =
    let lineArray =
        [| currRow.seqNum1.ToString()
           currRow.role1.ToString()
           currRow.seqNum2.ToString()
           currRow.role2.ToString()
           currRow.seqNum3.ToString()
           currRow.role3.ToString() |]
    
    let currConnRow = String.concat "," lineArray
    currConnRow

let deserializeConnectorRow (line : string) =
    let lineArray = line.Split(',')
    
    let currRow =
        { seqNum1 = int lineArray.[0]
          role1 = int lineArray.[1]
          seqNum2 = int lineArray.[2]
          role2 = int lineArray.[3]
          seqNum3 = int lineArray.[4]
          role3 = int lineArray.[5] }
    currRow

let initConnectorState = "1,2,3,4,5,6"
let theConn = deserializeConnectorRow initConnectorState