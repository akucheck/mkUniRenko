module MkUniRenkoTargets

// ========================================================
// all active patterns for determining whether bar is complete
// ========================================================
let (|UpTrendTarget|_|) (priceTargets, price, direction) =
    let _dnTrendTarget, upTrendTarget, _dnReversalTarget, _upReversalTarget =
        priceTargets
    if (price = upTrendTarget && direction <> "D") then Some()
    else None

let (|DnReversalTarget|_|) (priceTargets, price, direction) =
    let _dnTrendTarget, _upTrendTarget, dnReversalTarget, _upReversalTarget =
        priceTargets
    if (price = dnReversalTarget && direction = "U") then Some()
    else None

let (|UpReversalTarget|_|) (priceTargets, price, direction) =
    let _dnTrendTarget, _upTrendTarget, _dnReversalTarget, upReversalTarget =
        priceTargets
    if (price = upReversalTarget && direction = "D") then Some()
    else None

let (|DnTrendTarget|_|) (priceTargets, price, direction) =
    let dnTrendTarget, _upTrendTarget, _dnReversalTarget, _upReversalTarget =
        priceTargets
    if (price = dnTrendTarget && direction <> "U") then Some()
    else None