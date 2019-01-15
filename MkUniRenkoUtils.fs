module MkUniRenkoUtils

let splitStringAtVerticalBar (line : string) =
    let firstPart = line.Substring(0, line.IndexOf("|"))
    let secondPart =
        line.Substring( 1 + line.IndexOf("|"), -1 + line.Length  - line.IndexOf("|"))
    (firstPart, secondPart)

// ========================================================
// formerly useful functions
// ========================================================
// let unpackBarState (barState : string) =
//     let barStateArray = barState.Split(',') // unpack current accumulator state
//     let uOpen = float barStateArray.[0]
//     let uHigh = float barStateArray.[1]
//     let uLow = float barStateArray.[2]
//     let uClose = float barStateArray.[3]
//     let direction = barStateArray.[4]
//     (uOpen, uHigh, uLow, uClose, direction)

// let packBarState (newOpen : float, newHigh : float, newLow : float, 
//                   newClose : float, newDirection) =
//     let barStateArray =
//         [| newOpen.ToString("F2")
//            newHigh.ToString("F2")
//            newLow.ToString("F2")
//            newClose.ToString("F2")
//            newDirection.ToString() |]
    
//     let newState = barStateArray |> String.concat (",")
//     newState


// let formatOutputBar (completedBar : float * float * float * float * string) =
//     let completedBarOpen, completedBarHigh, completedBarLow, completedBarClose, 
//         _completedBarDirection = completedBar
//     let outputString =
//         completedBarOpen.ToString("F2") + "," + completedBarHigh.ToString("F2") 
//         + "," + completedBarLow.ToString("F2") + "," 
//         + completedBarClose.ToString("F2")
//     outputString
