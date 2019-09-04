Sample data taken from https://blog.traveltimeplatform.com/use-an-isochrone-api-and-algorithm-in-your-isochrone-app (raw https://gist.githubusercontent.com/LouisaKB/c6e9eb3f62e067ec0026d7646bcbf9ef/raw/4b5d2ff41e72d0e1eb4358850868b306a02c34b2/JSON%20response)

TravelTime Platform - 
Get keys https://docs.traveltimeplatform.com/overview/getting-keys

Login https://igeolise.3scale.net/admin

Application ID: 06082171
API key:  aad4f5a1c39529d2122a4224c4b952b4
nBZthg1LR6Eveep1


Sample search:
 https://app.traveltimeplatform.com/search/0_lng=-1.50812&0_tt=60&0_mode=driving&0_title=CV1%202WT%2C%20Coventry%2C%20England%2C%20United%20Kingdom&0_lat=52.40100&poi=Cinema

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

Convert json to C# class - https://app.quicktype.io/#l=cs&r=json2csharp
