module MkUniRenkoTargets

let priceTargets uOpen (tickValue : float) (trendParm : int) 
    (reversalParm : int) =
    // compute all price targets based on clParams 
    let dnTrendTarget = uOpen - (tickValue * float trendParm)
    let upTrendTarget = uOpen + (tickValue * float trendParm)
    let dnReversalTarget = uOpen - (tickValue * float reversalParm)
    let upReversalTarget = uOpen + (tickValue * float reversalParm)
    (dnTrendTarget, upTrendTarget, dnReversalTarget, upReversalTarget)

// ========================================================
// active patterns for determining whether bar is complete
// ========================================================
let (|UpTrendTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let _dnTrendTarget, upTrendTarget, _dnReversalTarget, _upReversalTarget =
        priceTargets
    if (price = upTrendTarget && direction <> "D") then Some()
    else None

let (|DnReversalTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let _dnTrendTarget, _upTrendTarget, dnReversalTarget, _upReversalTarget =
        priceTargets
    if (price = dnReversalTarget && direction = "U") then Some()
    else None

let (|UpReversalTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let _dnTrendTarget, _upTrendTarget, _dnReversalTarget, upReversalTarget =
        priceTargets
    if (price = upReversalTarget && direction = "D") then Some()
    else None

let (|DnTrendTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let dnTrendTarget, _upTrendTarget, _dnReversalTarget, _upReversalTarget =
        priceTargets
    if (price = dnTrendTarget && direction <> "U") then Some()
    else None

let (|LastRow|_|) (_priceTargets, _price, _direction, lastFlag : string) =
    if (lastFlag = "L") then Some()
    else None
