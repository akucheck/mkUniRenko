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
    
    let currRow = String.concat "," lineArray
    currRow

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
// ========================================================
// formerly useful functions
// ========================================================
// let convertOhlcRowToTuple (currRow : OhlcRow) =
//     let uOpen = currRow.uOpen
//     let uHigh = currRow.uHigh
//     let uLow = currRow.uLow
//     let uClose = currRow.uClose
//     let direction = currRow.direction
//     (uOpen, uHigh, uLow, uClose, direction)
