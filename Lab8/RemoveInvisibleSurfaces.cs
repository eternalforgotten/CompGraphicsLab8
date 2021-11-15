using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab8
{
    class RemoveInvisibleSurfaces
    {
        private static Point3D Normalize(List<int> surface, List<Point3D> vertexes)
        {
            Point3D firstVec = vertexes[surface[1]] - vertexes[surface[0]];
            Point3D secondVec = vertexes[surface[2]] - vertexes[surface[1]];
            if (secondVec.X == 0 && secondVec.Y == 0 && secondVec.Z == 0)
            {
                secondVec = vertexes[surface[3]] - vertexes[surface[2]];
            }
            return VectoredMultiplication(firstVec, secondVec);
        }

        public static List<List<int>> RemoveSurfaces(Figure figure, Point3D offset)
        {
            List<List<int>> res = new List<List<int>>();
            Point3D pointOfView = figure.Center() - offset;
            foreach (var surface in figure.Surfaces)
            {
                Point3D normalized = Normalize(surface, figure.Vertexes);
                var scalarMultiplication = normalized.X * pointOfView.X + normalized.Y * pointOfView.Y + normalized.Z * pointOfView.Z;
                var mult = Math.Sqrt(normalized.X * normalized.X + normalized.Y * normalized.Y + normalized.Z * normalized.Z) * Math.Sqrt(pointOfView.X * pointOfView.X + pointOfView.Y * pointOfView.Y + pointOfView.Z * pointOfView.Z);
                var cos = 0.0;
                if (mult != 0)
                    cos = scalarMultiplication / mult;
                if (cos > 0)
                    res.Add(surface);
            }
            return res;
        }
        private static Point3D VectoredMultiplication(Point3D vector1, Point3D vector2)
        {
            float x = vector1.Y * vector2.Z - vector1.Z * vector2.Y;
            float y = vector1.Z * vector2.X - vector1.X * vector2.Z;
            float z = vector1.X * vector2.Y - vector1.Y * vector2.X;
            return new Point3D(x, y, z);
        }
    }
}
