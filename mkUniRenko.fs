// mkUniRenko
// usage: 
(*

expected input:
a standard "shrink" format that has been run through markLast to append
a LastFlag
dateTime, seqNum, volume, deltaFactor, occur, aggVol, aggDelta, LastFlag
2018-12-27T15:00:00,41,2491.50,1,-1,41,205,-5,X
2018-12-27T15:00:00,43,2491.25,1,-1,2,3,-2,X
2018-12-27T15:00:00,44,2491.00,1,-1,1,2,-1,X
2018-12-27T15:00:00,45,2491.25,1,1,1,2,1,X
2018-12-27T15:00:00,47,2491.50,1,1,2,3,0,X

expected output: a UniRenko bar file
// seqNum1, barRole1, seqNum2, barRole2, seqNum3, barRole3
41,8,110,4,916,3
.
.
.
.

*)

// open System
open System.IO
open FilterIoLib
open MkUniRenkoTypes
open MkUniRenkoTargets
open MkUniRenkoUtils

// ========================================================
// all bar completion functions
// ========================================================
let complete (price : float) (seqNum : string) (tickVal : float) 
    (openBar : OhlcRow) (openParm : int) (nbDirection : string) 
    (_lastFlag : string) =
    let openParmFactor = // sets direction of openParm based on prior bar
        match (openBar.direction) with
        | "U" -> -1
        | _ -> 1
    
    let barComplete = true
    //
    let completedBarOpen =
        openBar.uOpen + (tickVal * float openParm * float openParmFactor)
    let completedBarHigh = openBar.uHigh
    let completedBarLow = openBar.uLow
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
    
    let newBarOpen = 0.00 //latest changes
    let newBarHigh = 0.00
    let newBarLow = 0.00
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

let isBarComplete priceTargets price seqNum tickVal openBar openParm lastFlag =
    match (priceTargets, price, seqNum, openBar.direction, lastFlag) with
    | DnTrdTarget -> complete price seqNum tickVal openBar openParm "D" lastFlag
    | UpTrdTarget -> complete price seqNum tickVal openBar openParm "U" lastFlag
    | DnRevTarget -> complete price seqNum tickVal openBar openParm "D" lastFlag
    | UpRevTarget -> complete price seqNum tickVal openBar openParm "U" lastFlag
    | LastRow -> complete price seqNum tickVal openBar openParm "L" lastFlag
    | _ -> incomplete price tickVal openBar "_" lastFlag

// active patterns for use below
let (|PriceGtHigh|_|) (price, high) =
    if (price > high) then Some()
    else None

let (|PriceLtLow|_|) (price, low) =
    if (price < low) then Some()
    else None
// ========================================================
// main function to build bar, write to output file
// ========================================================
let buildBars (clParams : StreamWriter * int * int * int * float) 
    (barState : string) (line : string) =
    // unpack everything we need
    let outFile, trendParm, reversalParm, openParm, tickVal = clParams
    let theBar = deserializeOhlcRow barState
    let theInputRow = deserializeInputRow line
    
    let priorClose = // needed for priceTarget calc 
        match (theBar.priorClose) with
        | 0.00 -> theBar.uOpen
        | _ -> theBar.priorClose
    
    let priceTargets = // establish price targets for bar close
        priceTargets priorClose tickVal trendParm reversalParm

    // now record Open, Low, check for higherHigh, lowerLow
    // and assemble bar
    //
    let nOpen, nSeqNum1 = 
        match (theBar.uOpen) with
        | 0.00 -> (theInputRow.Price, theInputRow.SeqNum)
        | _ -> (theBar.uOpen, theBar.seqNum1)

    // set low to ridiculously high value to make sure we get a new low
    let newLow = 
        match (theBar.uLow) with
        | 0.00 -> 100000.00
        | _ -> theBar.uLow
 
    // check for HH
    let nHigh, nSeqNum2 =
        match (theInputRow.Price, theBar.uHigh) with
        | PriceGtHigh -> (theInputRow.Price, theInputRow.SeqNum)
        | _ -> (theBar.uHigh, theBar.seqNum2)
    
    // check for LL
    let nLow, nSeqNum3 =
        match (theInputRow.Price, newLow) with
        | PriceLtLow -> (theInputRow.Price, theInputRow.SeqNum)
        | _ -> (theBar.uLow, theBar.seqNum3)
    
    let openBar =
        { uOpen = nOpen
          uHigh = nHigh
          uLow = nLow
          uClose = theBar.uClose
          // Until the first bar is complete, direction is unknown: "X".
          // After that, direction will always be "U" or "D"
          // until last row of data, then "L"
          direction = theBar.direction
          priorClose = theBar.priorClose
          seqNum1 = nSeqNum1
          seqNum2 = nSeqNum2
          seqNum3 = nSeqNum3
          seqNum4 = "unassigned" }
    
    // have we met a target?
    let barIsComplete, completedBarOhlc, newBar = 
        isBarComplete priceTargets theInputRow.Price theInputRow.SeqNum tickVal 
            openBar openParm theInputRow.LastFlag
    // if we have met target we will need a bar...
    let barToBeWritten = (serializeOhlcRowWoDirection completedBarOhlc)
    // next line is the whole reason we are here
    if (barIsComplete) then outFile.WriteLine barToBeWritten
    let barState = // create accumulator state on to next row of data
        match (barIsComplete) with
        | true -> newBar // create new bar, then rinse, repeat
        | _ -> openBar // keep going
    
    let newState = (serializeOhlcRow barState) + "1,2,3,4,5,6"
    newState

[<EntryPoint>]
let _main argv =
    let trendParm = int argv.[0]
    let reversalParm = int argv.[1]
    let openParm = int argv.[2]
    let tickVal = float argv.[3]
    let barFile = argv.[4]
    use outFile = new StreamWriter(barFile)
    let clParams = (outFile, trendParm, reversalParm, openParm, tickVal)
    // init state: uOpen, uHigh,uLow,uClose,direction, priorClose
    // seqNum1, role1, seqNum2, role2, seqNum3, role3
    let initBarState = "0.00,0.00,0.00,0.00,X,0.00"
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
