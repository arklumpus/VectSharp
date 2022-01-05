/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2020-2022 Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectSharp
{
    //derived from https://www.particleincell.com/wp-content/uploads/2012/06/bezier-spline.js

    internal static class SmoothSpline
    {
        public static Point[] SmoothSplines(Point[] points)
        {
            double[] x = (from el in points select el.X).ToArray();
            double[] y = (from el in points select el.Y).ToArray();

            (double[] p1, double[] p2) px = ComputeControlPoints(x);
            (double[] p1, double[] p2) py = ComputeControlPoints(y);

            List<Point> tbr = new List<Point>();

            for (int i = 0; i < points.Length - 1; i++)
            {
                tbr.Add(new Point(x[i], y[i]));
                tbr.Add(new Point(px.p1[i], py.p1[i]));
                tbr.Add(new Point(px.p2[i], py.p2[i]));
            }

            tbr.Add(new Point(x[x.Length - 1], y[x.Length - 1]));

            return tbr.ToArray();
        }

        /*computes control points given knots K, this is the brain of the operation*/
        private static (double[] p1, double[] p2) ComputeControlPoints(double[] K)
        {
            int n = K.Length - 1;


            double[] p1 = new double[n];
            double[] p2 = new double[n];


            /*rhs vector*/
            double[] a = new double[n];
            double[] b = new double[n];
            double[] c = new double[n];
            double[] r = new double[n];

            /*left most segment*/
            a[0] = 0;
            b[0] = 2;
            c[0] = 1;
            r[0] = K[0] + 2 * K[1];

            /*internal segments*/
            for (int i = 1; i < n - 1; i++)
            {
                a[i] = 1;
                b[i] = 4;
                c[i] = 1;
                r[i] = 4 * K[i] + 2 * K[i + 1];
            }

            /*right segment*/
            a[n - 1] = 2;
            b[n - 1] = 7;
            c[n - 1] = 0;
            r[n - 1] = 8 * K[n - 1] + K[n];

            /*solves Ax=b with the Thomas algorithm (from Wikipedia)*/
            for (int i = 1; i < n; i++)
            {
                double m = a[i] / b[i - 1];
                b[i] = b[i] - m * c[i - 1];
                r[i] = r[i] - m * r[i - 1];
            }

            p1[n - 1] = r[n - 1] / b[n - 1];
            for (int i = n - 2; i >= 0; i--)
            {
                p1[i] = (r[i] - c[i] * p1[i + 1]) / b[i];
            }

            /*we have p1, now compute p2*/
            for (int i = 0; i < n - 1; i++)
            {
                p2[i] = 2 * K[i + 1] - p1[i + 1];
            }

            p2[n - 1] = 0.5 * (K[n] + p1[n - 1]);

            return (p1, p2);
        }

    }
}
