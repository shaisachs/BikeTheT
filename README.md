# BikeTheT

An API which tells you if you can take your bike on the T - basically a codification of the [MBTA's bike policies](http://old.mbta.com/riding_the_t/bikes/). [Check it out on Mashape](https://market.mashape.com/shaisachs/bikethet), and read more about the project on [my blog](https://shaisachs.github.io/2017/12/01/bikethet.html?src=github)!

## Commuter Rail

```
GET /api/commuterRailTrains/{trainNum}

200 OK
{
  "trainNum": "123",
  "bikesAllowed": true
}
```

## Subway

```
GET /api/subwayLines/{green|blue|orange|red|mattapan}?direction={inbound|outbound}&timeOfTravel={YYYY-MM-DDTHH:MM:SS}&isWeekend={true|false|null}

200 OK
{
    "color": "red",
    "direction": "inbound",
    "isWeekend": null,
    "timeOfTravel": "2017-09-07T20:01:00",
    "bikesAllowed": true
}
```

The `isWeekend` parameter is optional and allows you to specify whether the travel is occurring on a weekend schedule, even though `timeOfTravel` indicates otherwise. Usually you would use this parameter to indicate holidays that occur on weekdays, when service sometimes operates on a weekend schedule:

```
GET /api/subwayLines/red?direction=inbound&timeOfTravel=2017-07-04T17:01:00&isWeekend=true

{
    "color": "red",
    "direction": "inbound",
    "isWeekend": true,
    "timeOfTravel": "2017-07-04T17:01:00",
    "bikesAllowed": true	
}
```
