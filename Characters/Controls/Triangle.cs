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

        public Triangle()
        {

        }

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

        public bool Contains(Vector2 pt)
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(pt, Point1, Point2);
            d2 = sign(pt, Point2, Point3);
            d3 = sign(pt, Point3, Point1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            return !(has_neg && has_pos);
        }

        float sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        public Point LowestRectPoint()
        {
            var max = new Vector2(Math.Max(Point1.X, Math.Max(Point2.X, Point3.X)), Math.Max(Point1.Y, Math.Max(Point2.Y, Point3.Y)));
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
