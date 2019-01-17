Purpose: Build Higher TimeFrame UniRenko Bars
===============
UniRenko bars are specfied by the the following parameters:

1. **Trend**: number of ticks defining a trend target
2. **Reversal**: number of ticks defining a reversal target
3. **Offset**: number of ticks for synthetic open

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

// uOpen, uHigh, uLow, uClose, seqNum1, seqNum2, seqNum3
2487.75,2492.00,2487.50,2487.50,41,110,916,916
2483.75,2488.00,2483.50,2483.50,925,1424,3114,3114
2479.75,2484.50,2479.50,2479.50,3120,3275,6368,6368
2499.75,2499.50,2477.75,2499.50,6375,56563,8894,56563
2503.25,2503.50,2499.25,2503.50,56582,56940,56582,56940

The seqNums above are the original tick sequence numbers for the given trades.

For the purposes of the discussion below we will use the following UniRenko parameters.

1. Trend=16 ticks
2. Reversal= 80 ticks
3. Offset= 1 tick

The data we are looking at is: 12/28/18.
The instrument is ES.
The format is "shrink". We will refer to this below as sourceData.
The sourceData has the following format:

dateTime, seqNum, volume, deltaFactor, occur, aggVol, aggDelta

- 2018-12-27T15:00:00,41,2491.50,1,-1,41,205,-5
- 2018-12-27T15:00:00,43,2491.25,1,-1,2,3,-2
- 2018-12-27T15:00:00,44,2491.00,1,-1,1,2,-1
- 2018-12-27T15:00:00,45,2491.25,1,1,1,2,1
- 2018-12-27T15:00:00,47,2491.50,1,1,2,3,0

Hours
----

Each 24-hour globex session is looked at in isolation. The implication of this 
approach is that at the end of a day any partial bar is closed, without regard 
to meeting its price target. Likewise, at the the start of a new session a new 
bar is always started. All times referenced at PT.  The globex sessions start 
at 15:00:00 and end at 13:59:59 PT the following day. Additionally, there is a
brief halt at 13:14:59 and a resumption at 13:30:00.

UniRenko Bars
-------

Bars are typically decribed by the canonical OHLC values: Open High, Low, 
and Close.  UniRenko bars have slighly different rules, so we will use a slightly
different nomenclature when we refer to them individually: uOpen, uHigh, uLow, 
and uClose. For the sake of brevity, "OHLC" will still be used to refer to them 
collectively.

**How do we want to represent this new bar information?**

If we add bar information to the sourceData, we pollute its information space.  
 
We will certainly want to use this same sourceData to create many other 
periodicities.  It is very likely a better option to create a new file that 
only references to this sourceData, without changing it.  This has the additional 
space-saving advantage of not replicating the sourceData every time we want to 
create a new periodicity. Further, the sourceData preserves the complete price 
movement history.  Creating a system that simply refers back to this history 
permits us to retain all the benefits this format implies, particularly the 
ability to reliably backtest.

Will we represent bar data with a single row for each bar? If we define the bar 
comprehensively in a single row we can omit barNumbers, as they are implied.  

The first four columns will be the Open, High, Low, and Close. 
We also need to refer to the sourceData somehow.  The sourceData has a 
**seqNum**, which is session-unique, and we will use this. So we will add four more columns: the seqNums for the ticks that represent the Open, High, Low, and Close.


Let's step through the sourceData and build the first bar.  We will prepend 
line numbers for this discussion. They do not exist in the sourceData.

The day starts with:

1. 2018-12-27T15:00:00,41,2491.50,1,-1,41,205,-5

By definition, this is the uOpen of the 1st bar of the day. We note it as such.

Since we have no way of knowing whether the first bar will be an upBar or a dnBar, 
the uOpen is also potentially the uHigh or uLow, as well.  Therefore, we must 
note this at the same time. 

Next, we must compute target values, based on the Trend parameter 
(since the 1st bar of a session can never be a Reversal bar, it must be a 
Trend bar): 

- price +/- Trend paremeters = target value
- 2491.50 + 16 ticks = 2495.50 (upTrend)
- 2491.50 - 16 ticks = 2487.50 (dnTrend)

Whichever price target we arrive at first determines the direction of this 
1st bar of the chart. In the meantime, in order to identify the the 
uHigh (if this becomes a dnBar) and the uLow (if this becomes an upBar) 
we will track new highs/lows:

- 2018-12-27T15:00:00,41,2491.50,1,-1,41,205,-5
- 2018-12-27T15:00:00,43,2491.25,1,-1,2,3,-2  // potential low
- 2018-12-27T15:00:00,44,2491.00,1,-1,1,2,-1  // new potential low
- 2018-12-27T15:00:00,49,2491.75,1,1,2,3,2    // potential high
- 2018-12-27T15:00:04,110,2492.00,1,1,5,7,6   // new potential high
- .
- .
- .
- .
- .
- 2018-12-27T15:00:10,173,2490.75,1,-1,1,2,-1 // new pot low
.
.
.

until, at row 217, we hit one of our targets:

- 2018-12-27T15:02:22,916,2487.50,3,-1,4,10,-7 // low target hit!

At this point we know the uHigh occured 3x, the low kept getting lower until 
the target was hit. If we wanted to create a our 1st row to represent this
1st bar, we have:

- 2018-12-27T15:00:00,41,2491.50,1,-1,41,205,-5 // uOpen
- 2018-12-27T15:00:04,110,2492.00,1,1,5,7,6 // also rows: 32, 35)
- 2018-12-27T15:02:22,916,2487.50,3,-1,4,10,-7 uC // target hit: uLow and uClose 


Note: For both Trend and Reversal bars, the Low (in a dnBar) or High (in an upBar) 
will always = the Close.  

Conclusion
==========
For perspective, the tickData we started with for this day comprises about 75MB.
The **priceChangeOnly** sourceData ("shrink" format) referenced here squeezed that down for this day to 103,189 rows of priceChange data, occupying 4.9MB. This UniRenko bar 
file will contain 30 rows and about 1KB on disk. This file can be used to 
generate any indicators desired based on this periodicity. Backtesting will, 
of course, require the sourceData that it references.




