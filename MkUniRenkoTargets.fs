module MkUniRenkoTargets

let priceTargets uOpen (tickValue : float) (trendParm : int) 
    (reversalParm : int) =
    // compute all price targets based on clParams 
    let dnTrdTarget = uOpen - (tickValue * float trendParm)
    let upTrdTarget = uOpen + (tickValue * float trendParm)
    let dnRevTarget = uOpen - (tickValue * float reversalParm)
    let upRevTarget = uOpen + (tickValue * float reversalParm)
    (dnTrdTarget, upTrdTarget, dnRevTarget, upRevTarget)

// ========================================================
// active patterns for determining whether bar is complete
// ========================================================
let (|UpTrdTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let _dnTrdTarget, upTrdTarget, _dnRevTarget, _upRevTarget =
        priceTargets
    if (price = upTrdTarget && direction <> "D") then Some()
    else None

let (|DnRevTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let _dnTrdTarget, _upTrdTarget, dnRevTarget, _upRevTarget =
        priceTargets
    if (price = dnRevTarget && direction = "U") then Some()
    else None

let (|UpRevTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let _dnTrdTarget, _upTrdTarget, _dnRevTarget, upRevTarget =
        priceTargets
    if (price = upRevTarget && direction = "D") then Some()
    else None

let (|DnTrdTarget|_|) (priceTargets, price, direction, lastFlag : string) =
    let dnTrdTarget, _upTrdTarget, _dnRevTarget, _upRevTarget =
        priceTargets
    if (price = dnTrdTarget && direction <> "U") then Some()
    else None

let (|LastRow|_|) (_priceTargets, _price, _direction, lastFlag : string) =
    if (lastFlag = "L") then Some()
    else None
