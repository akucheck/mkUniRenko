// mkUniRenko.fs
(*
UniRenko bars are specfied by the the following parameters:
   Trend: number of ticks defining a trend target
Reversal: number of ticks defining a reversal target
  Offset: number of ticks for synthetic open

In addtion to the UniRenko parameters, the tickValue is passed as a 
command line parameter.  

usage: cat inFile | mkUniRenko 16 80 1 .25 outFile > output.txt

Expected input: the standard "shrink" format that contains one row for each
price change during the course of a session.  That data has been run through 
through the markLast to append a LastFlag:

dateTime, seqNum, volume, deltaFactor, occur, aggVol, aggDelta, LastFlag
2018-12-27T15:00:00,41,2491.50,1,-1,41,205,-5,X
2018-12-27T15:00:00,43,2491.25,1,-1,2,3,-2,X
2018-12-27T15:00:00,44,2491.00,1,-1,1,2,-1,X
2018-12-27T15:00:00,45,2491.25,1,1,1,2,1,X
2018-12-27T15:00:00,47,2491.50,1,1,2,3,0,X

Expected output: a UniRenko bar file
// uOpen, uHigh, uLow, uClose, seqNum1, seqNum2, seqNum3, seqNum4
2487.75,2492.00,2487.50,2487.50,41,110,916,916
2483.75,2488.00,2483.50,2483.50,925,1424,3114,3114
2479.75,2484.50,2479.50,2479.50,3120,3275,6368,6368
2499.75,2499.50,2477.75,2499.50,6375,56563,8894,56563
2503.25,2503.50,2499.25,2503.50,56582,56940,56582,56940

The seqNums above are the original tick sequence numbers for the given trades
that 
*)

// open System
open System.IO
open FilterIoLib
open MkUniRenkoTypes
open MkUniRenkoTargets
open MkUniRenkoUtils

let impossiblyHighValue = 100000.00 // a value we know will be above the range

// ========================================================
// All bar completion functions
// ========================================================
let (|PriorBarDnCurrBarDn|_|) (priorBarDir, currBarDir) =
    if (priorBarDir = "D" && currBarDir = "D") then Some()
    else None

let (|PriorBarDnCurrBarUp|_|) (priorBarDir, currBarDir) =
    if (priorBarDir = "D" && currBarDir = "U") then Some()
    else None

let (|PriorBarUpCurrBarUp|_|) (priorBarDir, currBarDir) =
    if (priorBarDir = "U" && currBarDir = "U") then Some()
    else None

let (|PriorBarUpCurrBarDn|_|) (priorBarDir, currBarDir) =
    if (priorBarDir = "U" && currBarDir = "D") then Some()
    else None

let complete (price : float) (seqNum : string) (tickVal : float) 
    (openBar : OhlcRow) (openParm : int) (nbDirection : string) 
    (_lastFlag : string) =
    let priorBarDir = openBar.direction
    let currBardir = nbDirection
    
    let openParmFactor = // sets direction of openParm based on prior bar
        match (priorBarDir, currBardir) with
        | PriorBarDnCurrBarUp -> 1
        | PriorBarUpCurrBarUp -> -1
        | PriorBarUpCurrBarDn -> -1
        | _ -> 1
    
    let barComplete = true
    
    let completedBarOpen =
        match (openBar.priorClose) with
        | 0.00 -> openBar.uOpen // handle first bar of session
        | _ -> 
            openBar.priorClose 
            + (tickVal * float openParm * float openParmFactor)
    
    let completedBarHigh = openBar.uHigh
    let completedBarLow = min completedBarOpen openBar.uLow // openBar.uLow
    let completedBarClose = price
    let completedBarDirection = "D"
    
    let completedBarOhlc =
        { uOpen = completedBarOpen
          uHigh = completedBarHigh
          uLow = completedBarLow
          uClose = completedBarClose
          direction = completedBarDirection
          priorClose = completedBarClose
          seqNum1 = openBar.seqNum1
          seqNum2 = openBar.seqNum2
          seqNum3 = openBar.seqNum3
          seqNum4 = seqNum }
    
    let newBarOpen = 0.00
    let newBarHigh = 0.00
    let newBarLow = impossiblyHighValue
    let newBarClose = 0.00
    let newBarDirection = nbDirection // based on whether up/dn target hit
    
    let newBarOhlc =
        { uOpen = newBarOpen
          uHigh = newBarHigh
          uLow = newBarLow
          uClose = newBarClose
          direction = newBarDirection
          priorClose = completedBarClose
          seqNum1 = "unassigned"
          seqNum2 = "unassigned"
          seqNum3 = "unassigned"
          seqNum4 = "unassigned" }
    (barComplete, completedBarOhlc, newBarOhlc)

// no-op func for catchall case in match that calls it
let incomplete (_price : float) (_tickVal : float) (openBar : OhlcRow) 
    (_direction : string) (_lastFlag : string) = (false, openBar, openBar)

let isBarComplete priceTargets theInputRow tickVal currBar openParm =
    let price = theInputRow.Price
    let seqNum = theInputRow.SeqNum
    let lastFlag = theInputRow.LastFlag
    let direction = currBar.direction
    match (priceTargets, price, seqNum, direction, lastFlag) with
    | DnTrdTarget -> complete price seqNum tickVal currBar openParm "D" lastFlag
    | UpTrdTarget -> complete price seqNum tickVal currBar openParm "U" lastFlag
    | DnRevTarget -> complete price seqNum tickVal currBar openParm "D" lastFlag
    | UpRevTarget -> complete price seqNum tickVal currBar openParm "U" lastFlag
    | LastRow -> complete price seqNum tickVal currBar openParm "L" lastFlag
    | _ -> incomplete price tickVal currBar "_" lastFlag

// active patterns and match functions for use below
let (|PriceGtHigh|_|) (price, high) =
    if (price > high) then Some()
    else None

let (|PriceLtLow|_|) (price, low) =
    if (price < low) then Some()
    else None

let checkForLowerLow theInputRow theBar =
    match (theInputRow.Price, theBar.uLow) with
    | PriceLtLow -> (theInputRow.Price, theInputRow.SeqNum)
    | _ -> (theBar.uLow, theBar.seqNum3)

let checkForHigherHigh theInputRow theBar =
    match (theInputRow.Price, theBar.uHigh) with
    | PriceGtHigh -> (theInputRow.Price, theInputRow.SeqNum)
    | _ -> (theBar.uHigh, theBar.seqNum2)

let setOpenIfNec theInputRow theBar =
    match (theBar.uOpen) with
    | 0.00 -> (theInputRow.Price, theInputRow.SeqNum)
    | _ -> (theBar.uOpen, theBar.seqNum1)

let setPriorClose theBar =
    match (theBar.priorClose) with
    | 0.00 -> theBar.uOpen
    | _ -> theBar.priorClose

// ========================================================
// main function to build bar, write to output file
// ========================================================
let buildBars (clParams : StreamWriter * int * int * int * float) 
    (barState : string) (line : string) =
    // unpack everything we need
    let outFile, trdParm, revParm, openParm, tickVal = clParams
    let theBar = deserializeOhlcRow barState
    let theInputRow = deserializeInputRow line
    // set values for bar from input data
    let priorClose = setPriorClose theBar
    let priceTargets = createPriceTargs priorClose tickVal trdParm revParm
    let nOpen, nSeqNum1 = setOpenIfNec theInputRow theBar
    // initialize low, high to same values as open 
    //  TODO: revisit this   
    // let initHigh, initSeqNum2 = nOpen, nSeqNum1
    // let initLow, initSeqNum3 = nOpen, nSeqNum1
    let nHigh, nSeqNum2 = checkForHigherHigh theInputRow theBar
    let nLow, nSeqNum3 = checkForLowerLow theInputRow theBar
    
    let currBar = //  assemble bar
        { uOpen = nOpen
          uHigh = nHigh
          uLow = nLow
          uClose = theBar.uClose
          direction = theBar.direction
          priorClose = theBar.priorClose
          seqNum1 = nSeqNum1
          seqNum2 = nSeqNum2
          seqNum3 = nSeqNum3
          seqNum4 = "unassigned" }
    
    let barIsComplete, completedBarOhlc, newBar = // have we met a target?
        isBarComplete priceTargets theInputRow tickVal currBar openParm
    let barToBeWritten = (serializeShortOhlcRow completedBarOhlc)
    if (barIsComplete) then outFile.WriteLine barToBeWritten
    let barState = // update accumulator state for next row of data
        match (barIsComplete) with
        | true -> newBar // create new bar, then rinse, repeat
        | _ -> currBar // keep going
    
    let newState = (serializeOhlcRow barState) + "1,2,3,4,5,6"
    newState

[<EntryPoint>]
let _main argv =
    let trdParm = int argv.[0]
    let revParm = int argv.[1]
    let openParm = int argv.[2]
    let tickVal = float argv.[3]
    let barFile = argv.[4]
    use outFile = new StreamWriter(barFile)
    let clParams = (outFile, trdParm, revParm, openParm, tickVal)
    // init state: uOpen, uHigh,uLow,uClose,direction, priorClose
    // seqNum1, role1, seqNum2, role2, seqNum3, role3
    let impossiblyHighValueForLow = impossiblyHighValue.ToString("F2")
    let initBarState = "0.00,0.00," + impossiblyHighValueForLow + ",0.00,X,0.00"
    let initConnectorState = "1,2,3,4,5,6"
    let initState = initBarState + "," + initConnectorState
    
    let _lines =
        Seq.initInfinite readInput
        |> Seq.takeWhile ((<>) null)
        |> Seq.scan (buildBars clParams) initState
        |> Seq.skip 1 // ignore init state
        |> Seq.iter writeOutput
    outFile.Flush |> ignore
    outFile.Close |> ignore
    0 // return an integer exit code 
