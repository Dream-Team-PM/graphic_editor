
﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GeometryLib
{
    public interface IShape
    {
        /// <summary>Центр массы / опорная точка.</summary>
        Vector2 Center { get; }

        /// <summary>Переместить фигуру на дельту.</summary>
        void Move(Vector2 delta);

        /// <summary>Масштабировать относительно центра массы. factor > 0</summary>
        void Scale(double factor);

        /// <summary>Повернуть относительно центра массы на angleRadians.</summary>
        void Rotate(double angleRadians);

        /// <summary>Попадает ли точка в фигуру (с допуском eps).</summary>
        bool IsIn(Vector2 point, double eps = 1e-9);
    }
  
    public static class GeometryUtils
    {
        public static Vector2 RotatePoint(Vector2 p, Vector2 center, double angleRadians)
        {
            double c = Math.Cos(angleRadians);
            double s = Math.Sin(angleRadians);
            var dx = p - center;
            var x = (float)(dx.X * c - dx.Y * s);
            var y = (float)(dx.X * s + dx.Y * c);
            return center + new Vector2(x, y);
        }

        public static Vector2 ScalePoint(Vector2 p, Vector2 center, double factor)
        {
            var dx = p - center;
            return center + (dx * (float)factor);
        }

        // Ray-casting algorithm (winding can be used as well).
        public static bool PointInPolygon(IReadOnlyList<Vector2> poly, Vector2 point, double eps = 1e-9)
        {
            bool inside = false;
            int n = poly.Count;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                var pi = poly[i];
                var pj = poly[j];

                // Edge intersects horizontal ray right of point?
                bool intersect = ((pi.Y > point.Y) != (pj.Y > point.Y)) &&
                                 (point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y + (float)0.0) + pi.X);
                if (intersect) inside = !inside;
            }
            return inside;
        }
    }
}