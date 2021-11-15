using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab8
{
    class FigureRotation
    {

        static private Point3D ChangePoint(Point3D point, float[,] matrix)
        {
            var matrixPoint = Projection.MultMatrix(matrix, AffineChanges.MatrixColumnFromPoint3D(point));
            Point3D newPoint = new Point3D(matrixPoint[0, 0] / matrixPoint[3, 0], matrixPoint[1, 0] / matrixPoint[3, 0], matrixPoint[2, 0] / matrixPoint[3, 0]);
            return newPoint;
        }

        static private double TransformAngle(double angle)
        {
            return angle * Math.PI / 180;
        }


        static public List<Point3D> RotatePoints(List<Point3D> points, float angle, char axis)
        {
            float sin = (float)Math.Sin(TransformAngle(angle));
            float cos = (float)Math.Cos(TransformAngle(angle));
            float[,] matrix;
            switch (axis)
            {
                case 'x':
                    matrix = new float[,]{{ 1,  0,   0,  0},
                                          { 0, cos,-sin, 0},
                                          { 0, sin, cos, 0},
                                          { 0,  0,   0,  1}};
                    break;
                case 'y':
                    matrix = new float[,]{{ cos, 0, sin, 0},
                                          {  0,  1,  0,  0},
                                          {-sin, 0, cos, 0},
                                          {  0,  0,  0,  1}};
                    break;
                case 'z':
                    matrix = new float[,]{{ cos, -sin, 0, 0},
                                          { sin,  cos, 0, 0},
                                          {  0,    0,  1, 0},
                                          {  0,    0,  0, 1}};
                    break;
                default:
                    matrix = new float[,] {{ 1, 0, 0, 0 },
                                           { 0, 1, 0, 0 },
                                           { 0, 0, 1, 0 },
                                           { 0, 0, 0, 1 }};
                    break;
            }
            List<Point3D> res = new List<Point3D>();
            foreach (var pnt in points)
            {
                res.Add(ChangePoint(pnt, matrix));
            }
            return res;
        }


        static public Figure CreateRotationFigure(List<Point3D> points, int parts, char axis)
        {
            List<Point3D> res = new List<Point3D>(points);
            int length = points.Count;
            float angle = 360.0f / parts;
            for (int i = 1; i < parts; i++)
            {
                res.AddRange(RotatePoints(points, angle * i, axis));
            }

            Figure figure = new Figure(res);
            for (int i = 0; i < parts; i++)
            {
                for (int j = 0; j < length; j++)
                {
                    int current = i * length + j;
                    if ((current + 1) % length == 0)
                        figure.AddEdges(current, new List<int> { (current + length) % res.Count });
                    else
                    {
                        figure.AddEdges(current, new List<int> { current + 1, (current + length) % res.Count });
                        figure.AddSurface(new List<int> { current, current + 1, (current + 1 + length) % res.Count, (current + length) % res.Count });
                    }

                }
            }
            return figure;
        }
    }
}
