# Travel Time Proof of Concept

A proof of concept project for investigating options for calculating travel times betwen points.


## google api  

- we are already paying for this
- be expensive for large numbers of requests
- caching can help, but beware the Google terms of use on this
- Returns travel times for all locations passed in
- we can filter on travel times and have all times visible on screen, even if one over one hour
- max 100 elements per call (GET) so large requests have to be batched
- slower
- Google unlikely to go away and my enhance their search further

## travel time

Fast API:
- returns quickly but is limited to returning times and fares.
- has limited travel modes (but nough for our current needs)
- returns travel times only for those within the search time

What about results over 1 hour? e.g. driving 50 mins, public transport not reachable
 - how to display?
 - search for a longer time to give a chance of more travel times, and filter?

TODO: Try driving+public_transport as third search - what results?

Will need to buy licenses - see https://www.traveltimeplatform.com/search/pricing
Basic (100K searches at £600 per month) should be fine for us. Each query by us is 2 searches, or 4 if we do a zero results search. We are currently below 10K user requests per month.


## For all

- can only use a single travel mode, so need to do two calls per search (run in parallel)
- approach is to filter locations based on route, then pass those to the API to get travel times
- What about the "zero results" search? Would be slower if getting times
  - in the last month 987 zero results shown out of 7803, about 12.5%


## General notes

Sample data taken from https://blog.traveltimeplatform.com/use-an-isochrone-api-and-algorithm-in-your-isochrone-app (raw https://gist.githubusercontent.com/LouisaKB/c6e9eb3f62e067ec0026d7646bcbf9ef/raw/4b5d2ff41e72d0e1eb4358850868b306a02c34b2/JSON%20response)

Google API documentation: https://developers.google.com/maps/documentation/distance-matrix/start

Convert json to C# class - https://app.quicktype.io/#l=cs&r=json2csharp

TravelTime Platform - 
Get keys https://docs.traveltimeplatform.com/overview/getting-keys

Login https://igeolise.3scale.net/admin

Sample search:
 https://app.traveltimeplatform.com/search/0_lng=-1.50812&0_tt=60&0_mode=driving&0_title=CV1%202WT%2C%20Coventry%2C%20England%2C%20United%20Kingdom&0_lat=52.40100&poi=Cinema

Travel time time filter:
  https://docs.traveltimeplatform.com/reference/time-filter

Travel time time filter (fast):
  https://docs.traveltimeplatform.com/reference/time-filter-fast

WITH polygons
 AS (SELECT 'p1' id, 
            geography::STGeomFromText('polygon ((-113.754429 52.471834 , 1 5, 5 5, -113.754429 52.471834))', 4326) poly
),
 points
 AS (SELECT [Postcode],[Location] as p FROM ProviderVenue)
 SELECT DISTINCT 
        points.Postcode, 
        points.p.STAsText() as Location
 FROM polygons
      RIGHT JOIN points ON polygons.poly.STIntersects(points.p) = 1
 WHERE polygons.id IS NOT NULL;

