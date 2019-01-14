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

let complete (price : float) (tickValue : float) 
    (currBarState : float * float * float * float * string) (offsetParm: int) (nbDirection: string) =
    let currOpen, currHigh, currLow, _currClose, direction = currBarState
    let offsetParmFactor =
        if direction = "U" then -1
        else 1
    let barComplete = true
    let completedBarOpen = currOpen + (tickValue * float offsetParm * float offsetParmFactor) 
    let completedBarHigh = currHigh
    let completedBarLow = currLow
    let completedBarClose = price
    let completedBarDirection = "D" // never really needed
    let completedBar =
        completedBarOpen, completedBarHigh, completedBarLow, completedBarClose, 
        completedBarDirection
    let newBarOpen = price 
    let newBarHigh = price
    let newBarLow = price
    let newBarClose = 0.00
    let newBarDirection = nbDirection // "D" // determines whether dnTrend or upReversal is next target
    let newBar = newBarOpen, newBarHigh, newBarLow, newBarClose, newBarDirection
    (barComplete, completedBar, newBar)

let incomplete (_price : float) (_tickValue : float) 
    (currBarState : float * float * float * float * string) (_nbDirection: string) =
    (false, currBarState, currBarState)

let (|DnTrendTarget|_|) (priceTargets, price, direction) =
    let dnTrendTarget, _upTrendTarget, _dnReversalTarget, _upReversalTarget = priceTargets
    if (price = dnTrendTarget && direction <> "U") then Some()
    else None

let (|UpTrendTarget|_|) (priceTargets, price, direction) =
    let _dnTrendTarget, upTrendTarget, _dnReversalTarget, _upReversalTarget = priceTargets
    if (price = upTrendTarget && direction <> "D") then Some()
    else None

let (|DnReversalTarget|_|) (priceTargets, price, direction) =
    let _dnTrendTarget, _upTrendTarget, dnReversalTarget, _upReversalTarget = priceTargets
    if (price = dnReversalTarget && direction = "U") then Some()
    else None

let (|UpReversalTarget|_|) (priceTargets, price, direction) =
    let _dnTrendTarget, _upTrendTarget, _dnReversalTarget, upReversalTarget = priceTargets
    if (price = upReversalTarget && direction = "D") then Some()
    else None

let unpackState (barState : string) =
    let barStateArray = barState.Split(',') // unpack current accumulator state
    let uOpen = float barStateArray.[0]
    let uHigh = float barStateArray.[1]
    let uLow = float barStateArray.[2]
    let uClose = float barStateArray.[3]
    let direction = barStateArray.[4]
    (uOpen, uHigh, uLow, uClose, direction)

let packState (newOpen : float, newHigh : float, newLow : float, 
               newClose : float, newDirection) =
    let barStateArray =
        [| newOpen.ToString("F2")
           newHigh.ToString("F2")
           newLow.ToString("F2")
           newClose.ToString("F2")
           newDirection.ToString() |]
    
    let newState = barStateArray |> String.concat (",")
    newState

let priceTargets uOpen (tickValue : float) (trendParm : int) 
    (reversalParm : int) =
    let dnTrendTarget = uOpen - (tickValue * float trendParm)
    let upTrendTarget = uOpen + (tickValue * float trendParm)
    let dnReversalTarget = uOpen - (tickValue * float reversalParm)
    let upReversalTarget = uOpen + (tickValue * float reversalParm)
    (dnTrendTarget, upTrendTarget, dnReversalTarget, upReversalTarget)

let isBarComplete priceTargets price tickValue currBarState offsetParm =
    let _currOpen, _currHigh, _currLow, _currClose, direction = currBarState
    match (priceTargets, price, direction) with
    | DnTrendTarget -> complete price tickValue currBarState offsetParm "D"
    | UpTrendTarget -> complete price tickValue currBarState offsetParm "U"
    | DnReversalTarget -> complete price tickValue currBarState offsetParm "D"
    | UpReversalTarget -> complete price tickValue currBarState offsetParm "U"
    | _ -> incomplete price tickValue currBarState "_"

let formatOutputBar (completedBar : float * float * float * float * string) =
    let completedBarOpen, completedBarHigh, completedBarLow, completedBarClose, 
        _completedBarDirection = completedBar
    let outputString =
        completedBarOpen.ToString("F2") + "," + completedBarHigh.ToString("F2") 
        + "," + completedBarLow.ToString("F2") + "," 
        + completedBarClose.ToString("F2")
    outputString

let buildBars (clParams : StreamWriter * int * int * int * float) 
    (barState : string) (line : string) =
    // unpack everything we need
    let uOpen, uHigh, uLow, uClose, direction = unpackState (barState)
    let outFile, trendParm, reversalParm, offsetParm, tickValue = clParams
    let theInputRow = deserializeInputRow line
    
    // Until the first bar is complete, direction is unknown: "X"
    // after that, direction will always be "U" or "D"
    // uOpen = uLow = 0.00 will only be true for 1st input row of data
    let newOpen =
        if uOpen = 0.00 then theInputRow.Price
        else uOpen
    
    let newLow =
        if uLow = 0.00 then theInputRow.Price
        else uLow
    
    // update new uHighs, uLows
    let newHigh = max uHigh theInputRow.Price
    let newLow = min newLow theInputRow.Price
    let priceTargets = // establish price targets for bar close
        priceTargets uOpen tickValue trendParm reversalParm
    let openBar = // tuple values to more easily pass around
        (newOpen, newHigh, newLow, uClose, direction)
    let barIsComplete, completedBar, newBar = // have we met a trend target?
        isBarComplete
            priceTargets theInputRow.Price tickValue openBar offsetParm
    // next line it the whole reason we are here:
    if (barIsComplete) then outFile.WriteLine(formatOutputBar completedBar) 
    let barState =
        if (barIsComplete) then newBar
        else openBar
    
    let newState = packState barState
    // remember to check for LAST! TODO 
    newState

[<EntryPoint>]
let _main argv =
    let trendParm = int argv.[0]
    let reversalParm = int argv.[1]
    let offsetParm = int argv.[2]
    let tickValue = float argv.[3]
    let barFile = argv.[4]
    use outFile = new StreamWriter(barFile)
    let clParams = (outFile, trendParm, reversalParm, offsetParm, tickValue)
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
