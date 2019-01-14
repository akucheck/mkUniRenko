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

open System
open System.IO
open FilterIoLib
open MkUniRenkoTypes
open MkUniRenkoTargets
open MkUniRenkoUtils

// ========================================================
// all bar completion functions
// ========================================================
let complete (price : float) (tickVal : float) 
    (currBarState : float * float * float * float * string) (openParm : int) 
    (nbDirection : string) (lastFlag : string) =
    let currOpen, currHigh, currLow, _currClose, direction = currBarState
    
    let openParmFactor = // sets direction of openParm based on prior bar
        if direction = "U" then -1
        else 1
    
    let barComplete = true
    let completedBarOpen =
        currOpen + (tickVal * float openParm * float openParmFactor)
    let completedBarHigh = currHigh
    let completedBarLow = currLow
    let completedBarClose = price
    let completedBarDirection = "D" // never used
    let completedBar =
        completedBarOpen, completedBarHigh, completedBarLow, completedBarClose, 
        completedBarDirection
    let newBarOpen = price
    let newBarHigh = price
    let newBarLow = price
    let newBarClose = 0.00
    let newBarDirection = nbDirection // based on whether up/dn target hit
    let newBar = newBarOpen, newBarHigh, newBarLow, newBarClose, newBarDirection
    (barComplete, completedBar, newBar)

// no-op func for catchall case in match that calls it
let incomplete (_price : float) (_tickVal : float) 
    (currBarState : float * float * float * float * string) 
    (_nbDirection : string) (lastFlag : string) =
    (false, currBarState, currBarState)

let isBarComplete priceTargets price tickVal barState openParm lastFlag =
    let _currOpen, _currHigh, _currLow, _currClose, direction = barState
    match (priceTargets, price, direction, lastFlag) with
    | DnTrdTarget -> complete price tickVal barState openParm "D" lastFlag
    | UpTrdTarget -> complete price tickVal barState openParm "U" lastFlag
    | DnRevTarget -> complete price tickVal barState openParm "D" lastFlag
    | UpRevTarget -> complete price tickVal barState openParm "U" lastFlag
    | LastRow -> complete price tickVal barState openParm "U" lastFlag
    | _ -> incomplete price tickVal barState "_" lastFlag

// ========================================================
// main function to build bar, write to output file
// ========================================================
let buildBars (clParams : StreamWriter * int * int * int * float) 
    (barState : string) (line : string) =
    // unpack everything we need
    let uOpen, uHigh, uLow, uClose, direction = unpackBarState (barState)
    let outFile, trendParm, reversalParm, openParm, tickVal = clParams
    let theInputRow = deserializeInputRow line
    
    // Until the first bar is complete, direction is unknown: "X"
    // after that, direction will always be "U" or "D"
    let newOpen = // uOpen = 0.00 only be true prior to reading 1st row of data
        if uOpen = 0.00 then theInputRow.Price
        else uOpen
    
    let newLow = // uLow = 0.00 only be true prior to reading 1st row of data
        if uLow = 0.00 then theInputRow.Price
        else uLow
    
    // update with uHighs, uLows as we step thru data
    let newHigh = max uHigh theInputRow.Price
    let newLow = min newLow theInputRow.Price
    let priceTargets = // establish price targets for bar close
        priceTargets uOpen tickVal trendParm reversalParm
    let openBar = // group the values into tuple to more easily pass around
        (newOpen, newHigh, newLow, uClose, direction)
    let barIsComplete, completedBar, newBar = // have we met a target?
        isBarComplete priceTargets theInputRow.Price tickVal openBar openParm 
            theInputRow.LastFlag
    // next line is the whole reason we are here:
    if (barIsComplete) then outFile.WriteLine(formatOutputBar completedBar)
    // TODO: change this to serialize ohlcRow  ^^^^^
    let barState =
        if (barIsComplete) then newBar // create new bar, then rinse, repeat
        else openBar // keep going
    
    let newState = packBarState barState
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
    let initState = "0.00,0.00,0.00,0.00,X" // uOpen, uHigh, uLow, uClose, direction
    
    let _lines =
        Seq.initInfinite readInput
        |> Seq.takeWhile ((<>) null)
        |> Seq.scan (buildBars clParams) initState
        |> Seq.skip 1 // ignore init state
        |> Seq.iter writeOutput
    outFile.Flush |> ignore
    outFile.Close |> ignore
    0 // return an integer exit code 
