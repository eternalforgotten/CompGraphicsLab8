using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Lab8
{
    public class Point3D
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float Z { get; set; } = 0;
        public Point3D(float a, float b, float c = 0)
        {
            X = a; Y = b; Z = c;
        }
        static public bool operator ==(Point3D a, Point3D b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        static public bool operator !=(Point3D a, Point3D b)
        {
            return !(a == b);
        }

        static public Point3D operator +(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        static public Point3D operator -(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public Point To2DPoint()
        {
            return new Point((int)X, (int)Y);
        }
        public void Normalize() {
            float len = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            X /= len; Y /= len; Z /= len;
        }
        public static Point3D VectorProduct(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.Y * p2.Z - p1.Z * p2.Y, p1.Z * p2.X - p1.X * p2.Z, p1.X * p2.Y - p1.Y * p2.X);
        }
        public static float ScalarProduct(Point3D p1, Point3D p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        }
        ~Point3D() { }
    }
    public class Edge
    {
        public Point3D From { get; set; }
        public Point3D To { get; set; }
        public Edge(Point3D a, Point3D b)
        {
            From = a; To = b;
        }
        public Edge(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            From = new Point3D(x1, y1, z1);
            To = new Point3D(x2, y2, z2);
        }
        ~Edge() { }
    }
    public class Polygon
    {
        public List<Point3D> points { get; set; }
        public Polygon()
        {
            points = new List<Point3D>();
        }
        public Polygon(List<Point3D> l)
        {
            points = l;
        }

        public void AddPoint(Point3D point)
        {
            points.Add(point);
        }

        public void AddPoint(float x, float y, float z)
        {
            points.Add(new Point3D(x, y, z));
        }
        ~Polygon() { }
    }
    public class Figure
    {
        public List<Point3D> Vertexes { get; set; } = new List<Point3D>();
        public List<Edge> Edges { get; } = new List<Edge>();
        public List<List<int>> Surfaces { get; } = new List<List<int>>();
        public Dictionary<int, List<int>> Adjacency { get; } = new Dictionary<int, List<int>>();

        public Point3D Center()
        {
            float x = Vertexes.Average(point => point.X);
            float y = Vertexes.Average(point => point.Y);
            float z = Vertexes.Average(point => point.Z);
            return new Point3D(x, y, z);
        }

        public Figure(List<Point3D> points)
        {
            Vertexes = points;
            int i = 0;
            foreach (Point3D point in points)
            {
                i++;
                Adjacency.Add(i, new List<int>());
            }
        }
        public Figure(Figure old)
        {
            Vertexes = new List<Point3D>(old.Vertexes);
            Edges = new List<Edge>(old.Edges);
            Adjacency = new Dictionary<int, List<int>>();
            foreach (var ad in old.Adjacency)
                Adjacency.Add(ad.Key, new List<int>(ad.Value));
            Surfaces = new List<List<int>>();
            foreach (var f in old.Surfaces)
                Surfaces.Add(new List<int>(f));
        }
        public Figure() { }

        public void AddEdges(int a, int b)
        {
            if (!Adjacency.ContainsKey(a))
                Adjacency.Add(a, new List<int> { b });
            else
                Adjacency[a].Add(b);
        }

        public void AddEdges(int a, List<int> l)
        {
            foreach (int b in l)
                AddEdges(a, b);
        }

        public void AddSurface(List<int> lst)
        {
            Surfaces.Add(lst);
        }

    }
}
