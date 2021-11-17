using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab8
{
    class Camera
    {
        public Point3D Position = new Point3D(0, 0, 0);
        public Point3D Focus = new Point3D(0, 0, 0);
        public Point3D Offset = new Point3D(0, 0, 0);
        public double AngleX = 0;
        public double AngleY = 0;


        public void MoveCamera(int dx, int dy, int dz)
        {
            Position += new Point3D(dx, dy, dz);
            Focus += new Point3D(dx, dy, 0);
        }

        public void RotateCamera(double ax, double ay)
        {
            AngleX += ax; AngleY += ay;
            if (AngleX > 360) AngleX -= 360;
            else if (AngleX < 0) AngleX += 360;
        }
    }
}
