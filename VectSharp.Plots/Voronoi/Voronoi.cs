// Adapted from RafaelKuebler/DelaunayVoronoi, Copyright (c) 2018 Rafael Kübler da Silva

/*
    VectSharp - A light library for C# vector graphics.
    Copyright (C) 2023 Giorgio Bianchini, University of Bristol

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

namespace VectSharp.Plots.Voronoi
{
    internal class Voronoi
    {
        public static List<double[][]> GetVoronoiCells(IReadOnlyList<IReadOnlyList<double>> points, double minX, double minY, double maxX, double maxY)
        {
            double rangeX = maxX - minX;
            double rangeY = maxY - minY;

            minX -= rangeX * 0.01;
            minY -= rangeY * 0.01;

            maxX += rangeX * 0.01;
            maxY += rangeY * 0.01;

            List<Vertex> vertices = new List<Vertex>(points.Count + 4)
            {
                new Vertex(0, 0),
                new Vertex(0, maxY),
                new Vertex(maxX, maxY),
                new Vertex(maxX, 0)
            };

            for (int i = 0; i < points.Count; i++)
            {
                double x = points[i][0] - minX;
                double y = points[i][1] - minY;

                vertices.Add(new Vertex(x, y));
            }

            Delaunay delaunay = new Delaunay(maxX, maxY, vertices[0], vertices[1], vertices[2], vertices[3]);

            List<Triangle> triangulation = delaunay.BowyerWatson(vertices).ToList();

            List<double[][]> tbr = new List<double[][]>(vertices.Count - 4);

            for (int i = 4; i < vertices.Count; i++)
            {
                List<Vertex> voronoiVertices = new List<Vertex>();

                foreach (Triangle triangle in vertices[i].AdjacentTriangles)
                {
                    voronoiVertices.Add(triangle.Circumcenter);
                }

                voronoiVertices.Sort((x, y) => Math.Sign(Math.Atan2(x.X - vertices[i].X, x.Y - vertices[i].Y) - Math.Atan2(y.X - vertices[i].X, y.Y - vertices[i].Y)));

                double[][] voronoiVerticesData  = new double[voronoiVertices.Count][];

                for (int j = 0; j < voronoiVertices.Count; j++)
                {
                    voronoiVerticesData[j] = new double[] { voronoiVertices[j].X + minX, voronoiVertices[j].Y + minY };
                }

                tbr.Add(voronoiVerticesData);
            }

            return tbr;
        }
    }
}
