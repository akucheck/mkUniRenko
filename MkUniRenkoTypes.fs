module MkUniRenkoTypes

type InputRow =
    { DateTime : string
      SeqNum : string
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
          SeqNum = string lineArray.[1]
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
      direction : string
      priorClose : float
      seqNum1 : string
      seqNum2 : string
      seqNum3 : string
      seqNum4 : string }

let deserializeOhlcRow (bar : string) =
    let barArray = bar.Split(',')
    
    let currOhlcRow =
        { uOpen = float barArray.[0]
          uHigh = float barArray.[1]
          uLow = float barArray.[2]
          uClose = float barArray.[3]
          direction = barArray.[4]
          priorClose = float barArray.[5]
          seqNum1 = barArray.[6]
          seqNum2 = barArray.[7]
          seqNum3 = barArray.[8]
          seqNum4 = barArray.[9] }
    currOhlcRow

let serializeOhlcRow (currRow : OhlcRow) =
    let lineArray =
        [| currRow.uOpen.ToString("F2")
           currRow.uHigh.ToString("F2")
           currRow.uLow.ToString("F2")
           currRow.uClose.ToString("F2")
           currRow.direction
           currRow.priorClose.ToString("F2")
           currRow.seqNum1
           currRow.seqNum2
           currRow.seqNum3
           currRow.seqNum4 |]
    
    let currOhlcRow = String.concat "," lineArray
    currOhlcRow

let serializeOhlcRowWoDirection (currRow : OhlcRow) =
    let lineArray =
        [| currRow.uOpen.ToString("F2")
           currRow.uHigh.ToString("F2")
           currRow.uLow.ToString("F2")
           currRow.uClose.ToString("F2")
           currRow.seqNum1
           currRow.seqNum2
           currRow.seqNum3
           currRow.seqNum4 |]
    
    let currOhlcRow = String.concat "," lineArray
    currOhlcRow
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
