﻿using System;
using System.Collections.Generic;
using System.Linq;
using TravelService.Models;
using TravelService.Services;
using GeoCalculator = Geolocation.GeoCalculator;

namespace TravelService.Impl
{
    public class UserRouteTracer : IUserRouteTracer
    {
        private static readonly NormalDistributionTable NormalDistributionTable = new NormalDistributionTable(2);

        private const int DecimalPlaces = 6;
        private const double PointDistance = 4;
        private const double StandardRouteWidth = 20;
        private const double AllowedDeviation = 25;
        private const double StandardConfidence = 0.95;
        private const uint NumParticles = 500;
        private const double EarthRadius = 6371 * 1000;

        private static readonly Random random = new Random();

        public TraceMeasures TraceUserWithParticles(Itinerary itinerary, TraceLocation location)
        {
            var polyLine = itinerary.Legs.Where(d => null != d.Geometry).SelectMany(d => d.Geometry).ToList();
            if (polyLine.Count < 2)
            {
                throw new Exception("Itinerary must have at least two points");
            }
            var standardDeviation = GetStandardDeviation(location.Accuracy);
            var z = NormalDistributionTable.ReverseLookUpD(StandardConfidence);
            var radiusStandardConfidence = z * standardDeviation;
            var denseLine = InsertPointsBetween(polyLine).ToList();
            double countOnPath = 0;
            double routeWidth = Math.Max(AllowedDeviation, (4 * radiusStandardConfidence) / 3.0);
            for (int i = 0; i < NumParticles; i++)
            {
                var distance = CoordinatePolyLineDistance(denseLine,
                    GetNormalDistributedParticle(standardDeviation, location.Coordinate));
                if (distance <= routeWidth)
                {
                    countOnPath++;
                }
            }
            return new TraceMeasures
            {
                ConfidenceOnRoute = countOnPath / NumParticles,
                RouteWidth = routeWidth,
                PositionOnRoute = GetPostitionOnRoute(itinerary, denseLine, location)
            };
        }

        public PositionOnRoute GetPostitionOnRoute(Itinerary itinerary, List<Coordinate> denseLine, TraceLocation location)
        {
            var positionOnRoute = denseLine
                .Select(v => new { v, Distance = GetDistance(v, location.Coordinate) })
                .OrderBy(v => v.Distance)
                .Select(v => v.v)
                .First();
            var positionIndex = denseLine.IndexOf(positionOnRoute);
            foreach (var leg in itinerary.Legs.Where(l => null != l.Geometry))
            {
                var itStart = denseLine.Where(c => c.Lat == leg.Geometry[0].Lat && c.Lng == leg.Geometry[0].Lng)
                    .First();
                var itStartIndex = denseLine.IndexOf(itStart);
                var itEnd = denseLine.Where(c => c.Lat == leg.Geometry[leg.Geometry.Length - 1].Lat && c.Lng == leg.Geometry[leg.Geometry.Length - 1].Lng)
                    .First();
                var itEndIndex = denseLine.IndexOf(itEnd);
                if (itStartIndex <= positionIndex && positionIndex <= itEndIndex)
                {
                    double length = 0;
                    double wayLength = 0;
                    for (int i = itStartIndex; i < itEndIndex; i++)
                    {
                        var dist = GetDistance(denseLine[i], denseLine[i + 1]);
                        length += dist;
                        if (i < positionIndex)
                        {
                            wayLength += dist;
                        }
                    }
                    TimeSpan delay;
                    if (length > 0)
                    {
                        var pointTime = leg.StartTime + (wayLength) * ((leg.EndTime - leg.StartTime) / (length));
                        delay = location.Timestamp - pointTime;
                    }
                    else
                    {
                        delay = location.Timestamp - leg.EndTime;
                    }
                    return new PositionOnRoute()
                    {
                        LegIndex = Array.IndexOf(itinerary.Legs, leg),
                        LocationOnRoute = positionOnRoute,
                        Delay = delay
                    };
                }
            }
            throw new Exception("Position on route not found");
        }

        private double CoordinatePolyLineDistance(List<Coordinate> denseLine, Coordinate x3)
        {
            return denseLine.Select(v => GetDistance(v, x3)).OrderBy(v => v).First();
        }

        private Coordinate GetNormalDistributedParticle(double standardDeviation, Coordinate center)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2);
            double randNormal =
                         0 + standardDeviation * randStdNormal;

            var bearing = random.NextDouble() * 360;

            var lat = Math.Asin(Math.Sin(Radians(center.Lat)) * Math.Cos(randNormal / EarthRadius) +
                    Math.Cos(Radians(center.Lat)) * Math.Sin(randNormal / EarthRadius) * Math.Cos(Radians(bearing)));
            var lng = Radians(center.Lng) + Math.Atan2(Math.Sin(Radians(bearing)) * Math.Sin(randNormal / EarthRadius)
                * Math.Cos(Radians(center.Lat)),
                                     Math.Cos(randNormal / EarthRadius) - Math.Sin(Radians(center.Lat)) * Math.Sin(lat));

            return new Coordinate(Degrees(lat), Degrees(lng));
        }

        public TraceMeasures TraceUserOnItinerary(Itinerary itinerary, TraceLocation location)
        {
            var polyLine = itinerary.Legs.Where(d => null != d.Geometry).SelectMany(d => d.Geometry).ToList();
            if (polyLine.Count < 2)
            {
                throw new Exception("Itinerary must have at least two points");
            }
            var denseLine = InsertPointsBetween(polyLine).ToList();
            var standardDeviation = GetStandardDeviation(location.Accuracy);
            var z = NormalDistributionTable.ReverseLookUpD(StandardConfidence);
            var radiusStandardConfidence = z * standardDeviation;

            var polyPointsInConfidenceRadius =
                GetPolyPointsInConfidenceRadius(denseLine, radiusStandardConfidence, location.Coordinate);
            if (!polyPointsInConfidenceRadius.Any(v => v.InConfidenceRadius))
            {
                return new TraceMeasures
                {
                    ConfidenceOnRoute = 0,
                    RouteWidth = 0.01
                };
            }
            var circleIntersection = PolyLineLengthInCircle(polyPointsInConfidenceRadius, location.Coordinate, radiusStandardConfidence);

            double confidenceRadius = radiusStandardConfidence;
            double confidence = StandardConfidence;

            var smallestDistanceToCircle = circleIntersection.InsidePoints.Where(i =>
                circleIntersection.EnterPoints.All(e => GetDistance(e, i.Point) > StandardRouteWidth) &&
                circleIntersection.ExitPoints.All(e => GetDistance(e, i.Point) > StandardRouteWidth))
                .OrderBy(v => v.Distance).FirstOrDefault();
            if (null != smallestDistanceToCircle && smallestDistanceToCircle.Distance < StandardRouteWidth)
            {
                var increaseCircle = StandardRouteWidth - smallestDistanceToCircle.Distance;
                var increaseClamped = Math.Min(increaseCircle, 3 * standardDeviation - confidenceRadius);
                confidenceRadius += increaseClamped;
                confidence = 2 * NormalDistributionTable.LookupZ(confidenceRadius / standardDeviation) - 1;
                circleIntersection.Length += 2 * increaseClamped; //todo
            }
            var confidenceOnRoute = (circleIntersection.Length * StandardRouteWidth / (Math.PI * Math.Pow(confidenceRadius, 2))) * confidence;
            return new TraceMeasures
            {
                ConfidenceOnRoute = confidenceOnRoute,
                RouteWidth = StandardRouteWidth
            };
        }

        private List<PolyPointWithDistance> GetPolyPointsInConfidenceRadius(List<Coordinate> polyLine,
            double radius, Coordinate center)
        {
            return polyLine.Select(point =>
            {
                var distance = GetDistance(point, center);
                return new PolyPointWithDistance
                {
                    Point = point,
                    Distance = distance,
                    InConfidenceRadius = distance < radius
                };
            }).ToList();
        }

        private IEnumerable<PolyPointWithDistance> PointsInCircle(List<PolyPointWithDistance> polyLine)
        {
            for (int i = 1; i < polyLine.Count - 1; i++)
            {
                if (polyLine[i - 1].InConfidenceRadius)
                {
                    if (polyLine[i].InConfidenceRadius == true)
                    {
                        if (polyLine[i + 1].InConfidenceRadius == true)
                        {
                            yield return polyLine[i];
                        }
                    }
                }
            }
        }

        private class PolyLineCircleIntersection
        {
            public List<Coordinate> EnterPoints { get; set; }
            public List<Coordinate> ExitPoints { get; set; }
            public List<PolyPointWithDistance> InsidePoints { get; set; }
            public double Length { get; set; }
            public PolyLineCircleIntersection()
            {
                EnterPoints = new List<Coordinate>();
                ExitPoints = new List<Coordinate>();
                InsidePoints = new List<PolyPointWithDistance>();
            }
        }

        private PolyLineCircleIntersection PolyLineLengthInCircle(List<PolyPointWithDistance> polyLine, Coordinate center, double radius)
        {
            var res = new PolyLineCircleIntersection();
            var stack = new Stack<PolyPointWithDistance>();
            var reversed = new List<PolyPointWithDistance>(polyLine);
            reversed.Reverse();
            foreach (var c in reversed)
            {
                stack.Push(c);
            }
            double length = 0;
            PolyPointWithDistance before = stack.Pop();
            while (stack.TryPop(out PolyPointWithDistance current))
            {
                if (!before.InConfidenceRadius)
                {
                    if (current.InConfidenceRadius)
                    {
                        var intersection = GetIntersection(before.Point, current.Point, center, radius);
                        length += GetDistance(intersection, current.Point);
                        res.EnterPoints.Add(intersection);
                    }
                    else
                    {
                        // out to out
                    }
                }
                else
                {
                    if (current.InConfidenceRadius)
                    {
                        length += GetDistance(before.Point, current.Point);
                        res.InsidePoints.Add(current);
                    }
                    else
                    {
                        var intersection = GetIntersection(before.Point, current.Point, center, radius);
                        length += GetDistance(current.Point, intersection);
                        res.ExitPoints.Add(intersection);
                    }
                }
                before = current;
            }
            res.Length = length;
            return res;
        }

        private Coordinate GetIntersection(Coordinate outside, Coordinate inside, Coordinate center, double radius)
        {
            return MidPoint(outside, inside);
        }

        private class PolyPointWithDistance
        {
            public Coordinate Point { get; set; }
            public double Distance { get; set; }
            public bool InConfidenceRadius { get; set; }
        }

        private void PolyPointsInCircle(IEnumerable<Coordinate> polyLine)
        {

        }

        private double GetStandardDeviation(TraceLocationAccuracy accuracy)
        {
            var z = NormalDistributionTable.ReverseLookUpD(accuracy.Confidence);
            return accuracy.Radius / z;
        }

        public IEnumerable<Coordinate> InsertPointsBetween(List<Coordinate> polyLine)
        {
            var stack = new Stack<Coordinate>();
            polyLine.Reverse();
            foreach (var c in polyLine)
            {
                stack.Push(c);
            }
            Coordinate before = stack.Pop();
            yield return before;
            while (stack.TryPop(out Coordinate current))
            {
                var distance = GetDistance(before, current);
                if (distance > PointDistance)
                {
                    stack.Push(current);
                    stack.Push(MidPoint(before, current));
                }
                else
                {
                    before = current;
                    yield return current;
                }
            }
        }

        private double GetDistance(Coordinate c1, Coordinate c2)
        {
            return GeoCalculator.GetDistance(new Geolocation.Coordinate()
            {
                Latitude = c1.Lat,
                Longitude = c1.Lng
            }, new Geolocation.Coordinate()
            {
                Latitude = c2.Lat,
                Longitude = c2.Lng
            }, DecimalPlaces, Geolocation.DistanceUnit.Meters);
        }

        private double Radians(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        public double Degrees(double radians)
        {
            double degrees = (180 / Math.PI) * radians;
            return (degrees);
        }

        public Coordinate MidPoint(Coordinate c1, Coordinate c2)
        {
            double dLon = Radians(c2.Lng - c1.Lng);
            var lat1 = Radians(c1.Lat);
            var lat2 = Radians(c2.Lat);
            var lon1 = Radians(c1.Lng);
            double Bx = Math.Cos(lat2) * Math.Cos(dLon);
            double By = Math.Cos(lat2) * Math.Sin(dLon);
            double lat3 = Math.Atan2(Math.Sin(lat1) + Math.Sin(lat2), Math.Sqrt((Math.Cos(lat1) + Bx) * (Math.Cos(lat1) + Bx) + By * By));
            double lon3 = lon1 + Math.Atan2(By, Math.Cos(lat1) + Bx);
            return new Coordinate(Degrees(lat3), Degrees(lon3));
        }
    }

    public class NormalDistributionTable
    {
        private readonly uint _precision;
        private readonly double _increment;
        private double[] _table;
        public NormalDistributionTable(uint decimalPoints)
        {
            _precision = decimalPoints;
            int values = 1 + (int)Math.Pow(10, decimalPoints) * 3;
            _increment = 3 / (double)values;
            double val = 0;
            _table = new double[values];
            for (var i = 0; i < values; i++)
            {
                _table[i] = Phi(val);
                val += _increment;
            }
        }

        public double ReverseLookUpD(double percentage)
        {
            if (percentage < 0 || percentage > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(percentage));
            }
            for (int i = _table.Length - 1; i >= 0; i--)
            {
                if ((2 * _table[i] - 1) <= percentage)
                {
                    return _increment * i;
                }
            }
            return 0;
        }

        public double LookupZ(double z)
        {
            if (z < 0 || z > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(z));
            }
            var index = (int)(Math.Round(z, (int)_precision) * (int)Math.Pow(10, _precision));
            return _table[index];
        }

        private double Phi(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x) / Math.Sqrt(2.0);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }
    }
}
