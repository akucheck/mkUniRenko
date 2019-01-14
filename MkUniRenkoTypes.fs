module MkUniRenkoTypes

type InputRow =
    { DateTime : string
      SeqNum : int
      Price : float
      Volume : int
      DeltaFactor : int
      Occur : int
      AggVol : int
      AggDelta : int
      LastFlag : string }

type UniRenkoRow =
    { SeqNum1 : int
      UniRole1 : int
      SeqNum2 : int
      UniRole2 : int
      SeqNum3 : int
      UniRole3 : int }

let deserializeInputRow (line : string) =
    let lineArray = line.Split(',')
    
    let currInputRow =
        { DateTime = lineArray.[0]
          SeqNum = int lineArray.[1]
          Price = float lineArray.[2]
          Volume = int lineArray.[3]
          DeltaFactor = int lineArray.[4]
          Occur = int lineArray.[5]
          AggVol = int lineArray.[6]
          AggDelta = int lineArray.[7]
          LastFlag = lineArray.[8] }
    currInputRow

let serializeUniRenkoRow (currRow : UniRenkoRow) =
    let lineArray =
        [| currRow.SeqNum1.ToString()
           currRow.UniRole1.ToString()
           currRow.SeqNum2.ToString()
           currRow.UniRole2.ToString()
           currRow.SeqNum3.ToString()
           currRow.UniRole3.ToString() |]
    
    let currUniRenkoRow = String.concat "," lineArray
    currUniRenkoRow
