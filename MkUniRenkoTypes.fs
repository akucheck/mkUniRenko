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

type OhlcRow =
    { uOpen : float
      uHigh : float
      uLow : float
      uClose : float
      direction : string }

let deserializeOhlcRow (bar : string) =
    let barArray = bar.Split(',')
    
    let currOhlcRow =
        { uOpen = float barArray.[0]
          uHigh = float barArray.[1]
          uLow = float barArray.[2]
          uClose = float barArray.[3]
          direction = barArray.[4] }
    currOhlcRow

let serializeOhlcRow (currRow : OhlcRow) =
    let lineArray =
        [| currRow.uOpen.ToString("F2")
           currRow.uHigh.ToString("F2")
           currRow.uLow.ToString("F2")
           currRow.uClose.ToString("F2")
           currRow.direction.ToString() |]
    
    let currOhlcRow = String.concat "," lineArray
    currOhlcRow

let serializeOhlcRowWoDirection (currRow : OhlcRow) =
    let lineArray =
        [| currRow.uOpen.ToString("F2")
           currRow.uHigh.ToString("F2")
           currRow.uLow.ToString("F2")
           currRow.uClose.ToString("F2") |]
    
    let currOhlcRow = String.concat "," lineArray
    currOhlcRow

let convertOhlcRowToTuple (currRow : OhlcRow) =
    let uOpen = currRow.uOpen
    let uHigh = currRow.uHigh
    let uLow = currRow.uLow
    let uClose = currRow.uClose
    let direction = currRow.direction
    (uOpen, uHigh, uLow, uClose, direction)

type UniRenkoRow =
    { SeqNum1 : int
      UniRole1 : int
      SeqNum2 : int
      UniRole2 : int
      SeqNum3 : int
      UniRole3 : int }

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
