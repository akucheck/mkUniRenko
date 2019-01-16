module MkUniRenkoTargets

(*
There are 6 possible ways a UniRenko bar can close:
1. for 1st bar of session, only: upTrd or dnTrd target is met
2. if currently in a downTrend, a  dnTrd target is met
3. if currently in a upTrend,   an upTrd target is met
4. if currently in a upTrend,   a  dnRev target is met
5. if currently in a downTrend, an upRev target is met
6. the session ends
*)
let priceTargets priorClose (tickValue : float) (trendParm : int) 
    (reversalParm : int) =
    // compute all price targets based on clParams 
    let dnTrdTarget = priorClose - (tickValue * float trendParm)
    let upTrdTarget = priorClose + (tickValue * float trendParm)
    let dnRevTarget = priorClose - (tickValue * float reversalParm)
    let upRevTarget = priorClose + (tickValue * float reversalParm)
    (dnTrdTarget, upTrdTarget, dnRevTarget, upRevTarget)

// ========================================================
// active patterns for determining whether bar is complete
// ========================================================
let (|DnTrdTarget|_|) (priceTargets, price, seqNum, direction, lastFlag : string) =
    let dnTrdTarget, _, _, _ = priceTargets
    if (price = dnTrdTarget && direction <> "U") then Some()
    else None

let (|UpTrdTarget|_|) (priceTargets, price, seqNum, direction, lastFlag : string) =
    let _, upTrdTarget, _, _ = priceTargets
    if (price = upTrdTarget && direction <> "D") then Some()
    else None

let (|DnRevTarget|_|) (priceTargets, price, seqNum, direction, lastFlag : string) =
    let _, _, dnRevTarget, _ = priceTargets
    if (price = dnRevTarget && direction = "U") then Some()
    else None

let (|UpRevTarget|_|) (priceTargets, price, seqNum, direction, lastFlag : string) =
    let _, _, _, upRevTarget = priceTargets
    if (price = upRevTarget && direction = "D") then Some()
    else None

let (|LastRow|_|) (_priceTargets, _price, seqNum, _direction, lastFlag : string) =
    if (lastFlag = "L") then Some()
    else None
