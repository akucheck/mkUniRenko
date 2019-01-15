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
let complete (price : float) (tickVal : float) (openBar : OhlcRow) 
    (openParm : int) (nbDirection : string) (_lastFlag : string) =
    let openParmFactor = // sets direction of openParm based on prior bar
        if openBar.direction = "U" then -1
        else 1
    
    let barComplete = true
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
          direction = completedBarDirection }
    
    let newBarOpen = price
    let newBarHigh = price
    let newBarLow = price
    let newBarClose = 0.00
    let newBarDirection = nbDirection // based on whether up/dn target hit
    
    let newBarOhlc =
        { uOpen = newBarOpen
          uHigh = newBarHigh
          uLow = newBarLow
          uClose = newBarClose
          direction = newBarDirection }
    (barComplete, completedBarOhlc, newBarOhlc)

// no-op func for catchall case in match that calls it
let incomplete (_price : float) (_tickVal : float) (openBar : OhlcRow) 
    (_direction : string) (_lastFlag : string) = (false, openBar, openBar)

let isBarComplete priceTargets price tickVal openBar openParm lastFlag =
    match (priceTargets, price, openBar.direction, lastFlag) with
    | DnTrdTarget -> complete price tickVal openBar openParm "D" lastFlag
    | UpTrdTarget -> complete price tickVal openBar openParm "U" lastFlag
    | DnRevTarget -> complete price tickVal openBar openParm "D" lastFlag
    | UpRevTarget -> complete price tickVal openBar openParm "U" lastFlag
    | LastRow -> complete price tickVal openBar openParm "U" lastFlag
    | _ -> incomplete price tickVal openBar "_" lastFlag

// ========================================================
// main function to build bar, write to output file
// ========================================================
let buildBars (clParams : StreamWriter * int * int * int * float) 
    (barState : string) (line : string) =
    // unpack everything we need
    let outFile, trendParm, reversalParm, openParm, tickVal = clParams
    let barInfo, connectorInfo = splitStringAtVerticalBar barState
    let theBar = deserializeOhlcRow barInfo
    // TODO: start using connectorInfo
    let theInputRow = deserializeInputRow line
    let priceTargets = // establish price targets for bar close
        priceTargets theBar.uOpen tickVal trendParm reversalParm
    
    // Until the first bar is complete, direction is unknown: "X"
    // after that, direction will always be "U" or "D"
    let newOpen = // uOpen = 0.00 only be true prior to reading 1st row of data
        if theBar.uOpen = 0.00 then theInputRow.Price
        else theBar.uOpen
    
    let newLow = // uLow = 0.00 only be true prior to reading 1st row of data
        if theBar.uLow = 0.00 then theInputRow.Price
        else theBar.uLow
    
    // update with new uHighs, uLows as we step thru data
    let newHigh = max theBar.uHigh theInputRow.Price
    let newLow = min newLow theInputRow.Price
    
    let openBar =
        { uOpen = newOpen
          uHigh = newHigh
          uLow = newLow
          uClose = theBar.uClose
          direction = theBar.direction }
    
    let barIsComplete, completedBarOhlc, newBar = // have we met a target?
        isBarComplete priceTargets theInputRow.Price tickVal openBar openParm 
            theInputRow.LastFlag
    if (barIsComplete) then 
        outFile.WriteLine // this line is the whole reason we are here
                          (serializeOhlcRowWoDirection completedBarOhlc)
    let barState =
        if (barIsComplete) then newBar // create new bar, then rinse, repeat
        else openBar // keep going
    
    let newState = (serializeOhlcRow barState) + "|" + connectorInfo
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
    // init state: uOpen, uHigh,uLow,uClose,direction, 
    // seqNum1, role1, seqNum2, role2, seqNum3, role3
    let initBarState = "0.00,0.00,0.00,0.00,X"
    let initConnectorState = "s1,r1,s2,r2,s3,r3"
    let initState = initBarState + "|" + initConnectorState
    
    let _lines =
        Seq.initInfinite readInput
        |> Seq.takeWhile ((<>) null)
        |> Seq.scan (buildBars clParams) initState
        |> Seq.skip 1 // ignore init state
        |> Seq.iter writeOutput
    outFile.Flush |> ignore
    outFile.Close |> ignore
    0 // return an integer exit code 
