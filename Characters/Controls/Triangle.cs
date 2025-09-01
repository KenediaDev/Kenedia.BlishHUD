using Kenedia.Modules.Core.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using Point = Microsoft.Xna.Framework.Point;

namespace Kenedia.Modules.Characters.Controls
{
    public struct Triangle
    {
        public static Triangle Empty = new(new(0), new(0), new(0));

        public Triangle(Vector2 point1, Vector2 point2, Vector2 point3)
        {
            Point1 = point1;
            Point2 = point2;
            Point3 = point3;
        }

        public Vector2 Point1 { get; set; }

        public Vector2 Point2 { get; set; }

        public Vector2 Point3 { get; set; }

        public bool CompareTo(Triangle t)
        {
            return Point1.Equals(t.Point1) && Point2.Equals(t.Point2) && Point3.Equals(t.Point3);
        }

        public bool IsEmpty()
        {
            return Point1.Equals(Empty.Point1) && Point2.Equals(Empty.Point2) && Point3.Equals(Empty.Point3);
        }

        public List<Vector2> ToVectorList()
        {
            return new List<Vector2>()
                {
                    Point1,
                    Point2,
                    Point3,
                };
        }

        public bool PointInTriangle(Vector2 p)
        {
            // Compute vectors
            var v0 = Point3 - Point1;
            var v1 = Point2 - Point1;
            var v2 = p - Point1;

            // Compute dot products
            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            // Compute barycentric coordinates
            float denom = (dot00 * dot11) - (dot01 * dot01);
            if (Math.Abs(denom) < float.Epsilon)
                return false; // Triangle is degenerate

            float invDenom = 1f / denom;
            float u = ((dot11 * dot02) - (dot01 * dot12)) * invDenom;
            float v = ((dot00 * dot12) - (dot01 * dot02)) * invDenom;

            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        public bool Contains(Vector2 pt)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, Point1, Point2);
            d2 = Sign(pt, Point2, Point3);
            d3 = Sign(pt, Point3, Point1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return ((p1.X - p3.X) * (p2.Y - p3.Y)) - ((p2.X - p3.X) * (p1.Y - p3.Y));
        }

        public Point LowestRectPoint()
        {
            var min = new Vector2(Math.Min(Point1.X, Math.Min(Point2.X, Point3.X)), Math.Min(Point1.Y, Math.Min(Point2.Y, Point3.Y)));

            return new((int)min.X, (int)min.Y);
        }

        public List<PointF> DrawingPoints()
        {
            float diff_X = Point2.X - Point1.X;
            float diff_Y = Point2.Y - Point1.Y;
            int pointNum = Point2.ToPoint().Distance2D(Point1.ToPoint());

            float interval_X = diff_X / (pointNum + 1);
            float interval_Y = diff_Y / (pointNum + 1);

            var pointList = new List<PointF>();
            for (int i = 1; i <= pointNum; i++)
            {
                pointList.Add(new PointF(Point1.X + (interval_X * i), Point1.Y + (interval_Y * i)));
            }

            return pointList;
        }
    }
}
