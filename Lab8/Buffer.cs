using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Lab8
{
    class Buffer
    {
        private static int ProjMode = 0;
        public static Bitmap CreateZBuffer(int width, int height, List<Figure> figures, List<Color> colors, int projMode = 0)
        {
            ProjMode = projMode;

            Bitmap bitmap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    bitmap.SetPixel(i, j, Color.White);
                }
                    
            }
                

            float[,] zbuff = new float[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    zbuff[i, j] = float.MinValue;
                }
            }

            List<List<List<Point3D>>> rasterizedFigures = new List<List<List<Point3D>>>();
            for (int i = 0; i < figures.Count; i++)
            {
                rasterizedFigures.Add(MakeRasterization(figures[i]));
            }

            var centerX = width / 2;
            var centerY = height / 2;

            int ind = 0;
            for (int i = 0; i < rasterizedFigures.Count; i++)
            {
                var figureLeftX = rasterizedFigures[i].Where(face => face.Count != 0).Min(face => face.Min(vertex => vertex.X));
                var figureLeftY = rasterizedFigures[i].Where(face => face.Count != 0).Min(face => face.Min(vertex => vertex.Y));
                var figureRightX = rasterizedFigures[i].Where(face => face.Count != 0).Max(face => face.Max(vertex => vertex.X));
                var figureRightY = rasterizedFigures[i].Where(face => face.Count != 0).Max(face => face.Max(vertex => vertex.Y));
                var figureCenterX = (figureRightX - figureLeftX) / 2;
                var figureCenterY = (figureRightY - figureLeftY) / 2;

                Random r = new Random();

                for (int j = 0; j < rasterizedFigures[i].Count; j++)
                {
                    List<Point3D> curr = rasterizedFigures[i][j];
                    foreach (Point3D point in curr)
                    {
                        int x = (int)(point.X + centerX - figureCenterX);
                        int y = (int)(point.Y + centerY - figureCenterY);
                        if (x < width && y < height && x > 0 && y > 0)
                        {
                            if (point.Z > zbuff[x, y])
                            {
                                zbuff[x, y] = point.Z;
                                bitmap.SetPixel(x, y, colors[ind % colors.Count]);
                            }
                        }
                    }
                    ind++;
                }
            }
            return bitmap;
        }

        private static List<List<Point3D>> MakeRasterization(Figure fig)
        {
            List<List<Point3D>> list_rast = new List<List<Point3D>>();
            foreach (var surf in fig.Surfaces)
            {
                List<Point3D> cursurface = new List<Point3D>();
                List<Point3D> surfacePoints = new List<Point3D>();
                for (int i = 0; i < surf.Count; i++)
                {
                    surfacePoints.Add(fig.Vertexes[surf[i]]);
                }

                List<List<Point3D>> triangles = Triangulate(surfacePoints);
                foreach (List<Point3D> triangle in triangles)
                {
                    cursurface.AddRange(RasterizeTriangle(MakeProj(triangle)));
                }
                list_rast.Add(cursurface);
            }
            return list_rast;
        }


        private static List<Point3D> RasterizeTriangle(List<Point3D> points)
        {
            List<Point3D> res = new List<Point3D>();

            points.Sort((point1, point2) => point1.Y.CompareTo(point2.Y));
            var transform3DPoints = points.Select(point => (X: (int)Math.Round(point.X), Y: (int)Math.Round(point.Y), Z: (int)Math.Round(point.Z))).ToList();

            var xy0Toxy1 = Interpolate(transform3DPoints[0].Y, transform3DPoints[0].X, transform3DPoints[1].Y, transform3DPoints[1].X);
            var xy1Toxy2 = Interpolate(transform3DPoints[1].Y, transform3DPoints[1].X, transform3DPoints[2].Y, transform3DPoints[2].X);
            var xy0Toxy2 = Interpolate(transform3DPoints[0].Y, transform3DPoints[0].X, transform3DPoints[2].Y, transform3DPoints[2].X);

            var yz0Toyz1 = Interpolate(transform3DPoints[0].Y, transform3DPoints[0].Z, transform3DPoints[1].Y, transform3DPoints[1].Z);
            var yz1Toyz2 = Interpolate(transform3DPoints[1].Y, transform3DPoints[1].Z, transform3DPoints[2].Y, transform3DPoints[2].Z);
            var yz0Toyz2 = Interpolate(transform3DPoints[0].Y, transform3DPoints[0].Z, transform3DPoints[2].Y, transform3DPoints[2].Z);

            xy0Toxy1.RemoveAt(xy0Toxy1.Count - 1);
            List<int> xy02 = xy0Toxy1.Concat(xy1Toxy2).ToList();

            yz0Toyz1.RemoveAt(yz0Toyz1.Count - 1);
            List<int> yz02 = yz0Toyz1.Concat(yz1Toyz2).ToList();

            int middle = xy02.Count / 2;
            List<int> leftX, rightX, leftZ, rightZ;
            if (xy0Toxy2[middle] < xy02[middle])
            {
                leftX = xy0Toxy2;
                leftZ = yz0Toyz2;
                rightX = xy02;
                rightZ = yz02;
            }
            else
            {
                leftX = xy02;
                leftZ = yz02;
                rightX = xy0Toxy2;
                rightZ = yz0Toyz2;
            }

            int y0 = transform3DPoints[0].Y;
            int y2 = transform3DPoints[2].Y;

            for (int ind = 0; ind <= y2 - y0; ind++)
            {
                int XL = leftX[ind];
                int XR = rightX[ind];

                List<int> intCurrZ = Interpolate(XL, leftZ[ind], XR, rightZ[ind]);

                for (int x = XL; x < XR; x++)
                {
                    res.Add(new Point3D(x, y0 + ind, intCurrZ[x - XL]));
                }
            }

            return res;
        }


        private static List<List<Point3D>> Triangulate(List<Point3D> points)
        {
            if (points.Count == 3)
                return new List<List<Point3D>> { points };

            List<List<Point3D>> res = new List<List<Point3D>>();
            for (int i = 2; i < points.Count; i++)
            {
                res.Add(new List<Point3D> { points[0], points[i - 1], points[i] });
            }

            return res;
        }

        
        private static List<int> Interpolate(int y0, int x0, int y1, int x1)
        {
            if (y0 == y1)
            {
                return new List<int> { x0 };
            }
            List<int> res = new List<int>();

            float step = (x1 - x0) * 1.0f / (y1 - y0);
            float value = x0;
            for (int i = y0; i <= y1; i++)
            {
                res.Add((int)value);
                value += step;
            }

            return res;
        }

        public static List<Point3D> MakeProj(List<Point3D> init) => new Projection().ProjectZBuff(init, ProjMode);
    }
}
