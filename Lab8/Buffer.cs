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
        public static Bitmap CreateZBuffer(int width, int height, List<Figure> scene, List<Color> colors, int projMode = 0)
        {
            ProjMode = projMode;

            Bitmap bitmap = new Bitmap(width, height);
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    bitmap.SetPixel(i, j, Color.White);

            float[,] zbuff = new float[width, height];
            for (int i = 0; i < width; i++)
                for (int j = 0; j < height; j++)
                    zbuff[i, j] = float.MinValue;

            List<List<List<Point3D>>> rasterizedScene = new List<List<List<Point3D>>>();
            for (int i = 0; i < scene.Count; i++)
            {
                rasterizedScene.Add(MakeRasterization(scene[i]));
            }

            var centerX = width / 2;
            var centerY = height / 2;

            int ind = 0;
            for (int i = 0; i < rasterizedScene.Count; i++)
            {
                var figureLeftX = rasterizedScene[i].Where(face => face.Count != 0).Min(face => face.Min(vertex => vertex.X));
                var figureLeftY = rasterizedScene[i].Where(face => face.Count != 0).Min(face => face.Min(vertex => vertex.Y));
                var figureRightX = rasterizedScene[i].Where(face => face.Count != 0).Max(face => face.Max(vertex => vertex.X));
                var figureRightY = rasterizedScene[i].Where(face => face.Count != 0).Max(face => face.Max(vertex => vertex.Y));
                var figureCenterX = (figureRightX - figureLeftX) / 2;
                var figureCenterY = (figureRightY - figureLeftY) / 2;

                Random r = new Random();

                for (int j = 0; j < rasterizedScene[i].Count; j++)
                {
                    List<Point3D> curr = rasterizedScene[i][j];
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
            var rpoints = points.Select(point => (X: (int)Math.Round(point.X), Y: (int)Math.Round(point.Y), Z: (int)Math.Round(point.Z))).ToList();

            var x01 = Interpolate(rpoints[0].Y, rpoints[0].X, rpoints[1].Y, rpoints[1].X);
            var x12 = Interpolate(rpoints[1].Y, rpoints[1].X, rpoints[2].Y, rpoints[2].X);
            var x02 = Interpolate(rpoints[0].Y, rpoints[0].X, rpoints[2].Y, rpoints[2].X);

            var z01 = Interpolate(rpoints[0].Y, rpoints[0].Z, rpoints[1].Y, rpoints[1].Z);
            var z12 = Interpolate(rpoints[1].Y, rpoints[1].Z, rpoints[2].Y, rpoints[2].Z);
            var z02 = Interpolate(rpoints[0].Y, rpoints[0].Z, rpoints[2].Y, rpoints[2].Z);

            x01.RemoveAt(x01.Count - 1);
            List<int> x012 = x01.Concat(x12).ToList();

            z01.RemoveAt(z01.Count - 1);
            List<int> z012 = z01.Concat(z12).ToList();

            int middle = x012.Count / 2;
            List<int> leftX, rightX, leftZ, rightZ;
            if (x02[middle] < x012[middle])
            {
                leftX = x02;
                leftZ = z02;
                rightX = x012;
                rightZ = z012;
            }
            else
            {
                leftX = x012;
                leftZ = z012;
                rightX = x02;
                rightZ = z02;
            }

            int y0 = rpoints[0].Y;
            int y2 = rpoints[2].Y;

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
