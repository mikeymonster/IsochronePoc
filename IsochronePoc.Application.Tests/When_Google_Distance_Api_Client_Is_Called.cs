using System;
using FluentAssertions;
using System.Collections.Generic;
using System.Net.Http;
using IsochronePoc.Application.GoogleDistanceMatrixApi;
using Xunit;

namespace IsochronePoc.Application.Tests
{
    public class When_Google_Distance_Api_Client_Is_Called
    {
        [Fact]
        public void Then_Encoded_Polyline_Is_As_Expected_For_A_Single_Point()
        {
            var points = new List<Venue>
            {
                new Venue { Latitude = 52.400997M, Longitude = -1.508122M }
            };

            var apiClient = new GoogleDistanceMatrixApiClient(new HttpClient(),
                new ApiConfiguration
                {
                    GoogleMapsApiBaseUrl = "https://maps.googleapis.com/maps/api/"
                });

            var result = apiClient.EncodePolyline(points);

            result.Should().Be("gqy~HvpeH");
        }

        [Fact]
        public void Then_Encoded_Polyline_Is_As_Expected_For_Multiple_Points()
        {
            var points = new List<Venue>
            {
                new Venue { Latitude = 38.5M, Longitude = -120.2M },
                new Venue { Latitude = 40.7M, Longitude = -120.95M },
                new Venue { Latitude = 43.252M, Longitude = -126.453M }
            };

            var apiClient = new GoogleDistanceMatrixApiClient(new HttpClient(),
                new ApiConfiguration
                {
                    GoogleMapsApiBaseUrl = "https://maps.googleapis.com/maps/api/"
                });

            var result = apiClient.EncodePolyline(points);

            result.Should().Be("_p~iF~ps|U_ulLnnqC_mqNvxq`@");
        }

        [Fact]
        public void Then_Uri_Is_As_Expected_For_A_Single_Point_With_Standard_Encoding_And_Slash_In_Base_Uri()
        {
            var origin = new Venue
            {
                Latitude = 52.400997M,
                Longitude = -1.295653M
            };

            var destinations = new List<Venue>
            {
                new Venue
                {
                    Latitude = 51.742141M,
                    Longitude = -1.508122M
                }
            };

            var apiClient = new GoogleDistanceMatrixApiClient(new HttpClient(),
                new ApiConfiguration
                {
                    //Note: slash at end
                    GoogleMapsApiBaseUrl = "https://maps.googleapis.com/maps/api/",
                    GoogleMapsApiKey = "YOUR_API_KEY"
                });

            var result = apiClient.BuildUri(origin, destinations, "driving", false);

            result.Should().Be("https://maps.googleapis.com/maps/api/distancematrix/json?origins=52.400997,-1.295653&mode=driving&destinations=51.742141%2C-1.508122&key=YOUR_API_KEY");
        }

        [Fact]
        public void Then_Uri_Is_As_Expected_For_A_Single_Point_With_Standard_Encoding_And_No_Slash_In_Base_Uri()
        {
            var origin = new Venue
            {
                Latitude = 52.400997M,
                Longitude = -1.295653M
            };

            var destinations = new List<Venue>
            {
                new Venue
                {
                    Latitude = 51.742141M,
                    Longitude = -1.508122M
                }
            };

            var apiClient = new GoogleDistanceMatrixApiClient(new HttpClient(),
                new ApiConfiguration
                {
                    //Note: no slash at end
                    GoogleMapsApiBaseUrl = "https://maps.googleapis.com/maps/api",
                    GoogleMapsApiKey = "YOUR_API_KEY"
                });

            var result = apiClient.BuildUri(origin, destinations, "driving", false);

            result.Should().Be("https://maps.googleapis.com/maps/api/distancematrix/json?origins=52.400997,-1.295653&mode=driving&destinations=51.742141%2C-1.508122&key=YOUR_API_KEY");

            //https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=40.6655101,-73.89188969999998&destinations=40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626&key=YOUR_API_KEY
            //vs polyline:
            //https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=40.6655101,-73.89188969999998&destinations=enc:_kjwFjtsbMt%60EgnKcqLcaOzkGari%40naPxhVg%7CJjjb%40cqLcaOzkGari%40naPxhV:&key=YOUR_API_KEY
        }

        [Fact]
        public void Then_Uri_Is_As_Expected_For_Multiple_Points_With_Standard_Encoding()
        {
            var origin = new Venue
            {
                Latitude = 40.6655101M,
                Longitude = -73.89188969999998M
            };

            //40.6905615%2C-73.9976592
            //40.6905615%2C-73.9976592
            //40.6905615%2C-73.9976592
            //40.6905615%2C-73.9976592
            //40.6905615%2C-73.9976592
            //40.6905615%2C-73.9976592
            //40.659569%2C-73.933783
            //40.729029%2C-73.851524
            //40.6860072%2C-73.6334271
            //40.598566%2C-73.7527626
            //40.659569%2C-73.933783
            //40.729029%2C-73.851524
            //40.6860072%2C-73.6334271
            //40.598566%2C-73.7527626&key=YOUR_API_KEY


            var destinations = new List<Venue>
            {
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.659569M, Longitude = -73.933783M   },
                new Venue { Latitude = 40.729029M, Longitude = -73.851524M   },
                new Venue { Latitude = 40.6860072M, Longitude = -73.6334271M },
                new Venue { Latitude = 40.598566M, Longitude = -73.7527626M  },
                new Venue { Latitude = 40.659569M, Longitude = -73.933783M  },
                new Venue { Latitude = 40.729029M, Longitude = -73.851524M   },
                new Venue { Latitude = 40.6860072M, Longitude = -73.6334271M },
                new Venue { Latitude = 40.598566M, Longitude = -73.7527626M  }
            };

            var apiClient = new GoogleDistanceMatrixApiClient(new HttpClient(),
                new ApiConfiguration
                {
                    GoogleMapsApiBaseUrl = "https://maps.googleapis.com/maps/api/",
                    GoogleMapsApiKey = "YOUR_API_KEY"
                });

            var result = apiClient.BuildUri(origin, destinations, "driving", false);

            result.Should().Be("https://maps.googleapis.com/maps/api/distancematrix/json?origins=40.6655101,-73.89188969999998&mode=driving&destinations=40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626&key=YOUR_API_KEY");
            //https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=40.6655101,-73.89188969999998&destinations=40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626&key=YOUR_API_KEY
            //vs polyline:
            //https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=40.6655101,-73.89188969999998&destinations=enc:_kjwFjtsbMt%60EgnKcqLcaOzkGari%40naPxhVg%7CJjjb%40cqLcaOzkGari%40naPxhV:&key=YOUR_API_KEY
        }

        [Fact]
        public void Then_Uri_Is_As_Expected_For_Multiple_Points_With_Polyline_Encoding()
        {
            var origin = new Venue
            {
                Latitude = 40.6655101M,
                Longitude = -73.89188969999998M
            };

            var destinations = new List<Venue>
            {
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.6905615M, Longitude = -73.9976592M },
                new Venue { Latitude = 40.659569M, Longitude = -73.933783M   },
                new Venue { Latitude = 40.729029M, Longitude = -73.851524M   },
                new Venue { Latitude = 40.6860072M, Longitude = -73.6334271M },
                new Venue { Latitude = 40.598566M, Longitude = -73.7527626M  },
                new Venue { Latitude = 40.659569M, Longitude = -73.933783M  },
                new Venue { Latitude = 40.729029M, Longitude = -73.851524M   },
                new Venue { Latitude = 40.6860072M, Longitude = -73.6334271M },
                new Venue { Latitude = 40.598566M, Longitude = -73.7527626M  }
            };

            var apiClient = new GoogleDistanceMatrixApiClient(new HttpClient(),
                new ApiConfiguration
                {
                    GoogleMapsApiBaseUrl = "https://maps.googleapis.com/maps/api/",
                    GoogleMapsApiKey = "YOUR_API_KEY"
                });

            var result = apiClient.BuildUri(origin, destinations, "driving", true);

            result.Should().Be("https://maps.googleapis.com/maps/api/distancematrix/json?origins=40.6655101,-73.89188969999998&mode=driving&destinations=enc:_kjwFjtsbMt%60EgnKcqLcaOzkGari%40naPxhVg%7CJjjb%40cqLcaOzkGari%40naPxhV:&key=YOUR_API_KEY");
            //https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=40.6655101,-73.89188969999998&destinations=40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.6905615%2C-73.9976592%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626%7C40.659569%2C-73.933783%7C40.729029%2C-73.851524%7C40.6860072%2C-73.6334271%7C40.598566%2C-73.7527626&key=YOUR_API_KEY
            //vs polyline:
            //https://maps.googleapis.com/maps/api/distancematrix/json?units=imperial&origins=40.6655101,-73.89188969999998&destinations=enc:_kjwFjtsbMt%60EgnKcqLcaOzkGari%40naPxhVg%7CJjjb%40cqLcaOzkGari%40naPxhV:&key=YOUR_API_KEY
        }

        [Fact]
        public void TODO_Get_Lat_Long_For_Postcodes_And_Journey_Times()
        {
            throw new Exception("Do it");
        }
    }
}
