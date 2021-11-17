using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Math;
using System.IO;
using Newtonsoft.Json;

namespace Lab8
{
    public partial class Form1 : Form
    {
        Graphics g;
        Pen pen;
        Projection projection;
        Camera camera = new Camera();
        Figure curFigure;
        private List<Point3D> rotationPoints;
        private List<Figure> allFigures = new List<Figure>();
        private List<Color> colors;

        public Form1()
        {
            InitializeComponent();
            textBox15.Text = "105";
            textBox16.Text = "20";
            textBox17.Text = "63";
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            pen = new Pen(Color.BlueViolet, 2);
            projection = new Projection();
            rotationPoints = new List<Point3D>();
            projBox.SelectedIndex = 0;
            radioButton1.Checked = true;
            this.KeyDown += new KeyEventHandler(movement);


            colors = new List<Color>();
            Random r = new Random(Environment.TickCount);
            for (int i = 0; i < 100; ++i)
                colors.Add(Color.FromArgb(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255)));

            camera.Position = new Point3D(int.Parse(textBox15.Text), int.Parse(textBox16.Text), int.Parse(textBox17.Text));
            camera.Focus = new Point3D(0, 0, 1000);
            camera.Offset = new Point3D(pictureBox1.Width / 2, pictureBox1.Height / 2);
            
        }
        private void DrawSurfaces(List<List<int>> visibleSurfaces)
        {
            DeletePic();
            g.Clear(Color.White);
            List<Edge> edges = projection.ProjectWithEdges(curFigure, projBox.SelectedIndex);

            var centerX = pictureBox1.Width / 2;
            var centerY = pictureBox1.Height / 2;

            var figureLeftX = edges.Min(e => e.From.X < e.To.X ? e.From.X : e.To.X);
            var figureLeftY = edges.Min(e => e.From.Y < e.To.Y ? e.From.Y : e.To.Y);
            var figureRightX = edges.Max(e => e.From.X > e.To.X ? e.From.X : e.To.X);
            var figureRightY = edges.Max(e => e.From.Y > e.To.Y ? e.From.Y : e.To.Y);
            var figureCenterX = (figureRightX - figureLeftX) / 2;
            var figureCenterY = (figureRightY - figureLeftY) / 2;

            var fixX = centerX - figureCenterX + (figureLeftX < 0 ? Math.Abs(figureLeftX) : -Math.Abs(figureLeftX));
            var fixY = centerY - figureCenterY + (figureLeftY < 0 ? Math.Abs(figureLeftY) : -Math.Abs(figureLeftY));
            List<Point3D> points = projection.ProjectWithPoints(curFigure, projBox.SelectedIndex);

            foreach (List<int> surface in visibleSurfaces)
            {
                var p1 = points[surface[0]].To2DPoint();
                var p2 = points[surface[surface.Count - 1]].To2DPoint();
                g.DrawLine(pen, p1.X + centerX - figureCenterX, p1.Y + centerY - figureCenterY, p2.X + centerX - figureCenterX, p2.Y + centerY - figureCenterY);
                for (var i = 1; i < surface.Count; i++)
                {
                    p1 = points[surface[i - 1]].To2DPoint();
                    p2 = points[surface[i]].To2DPoint();
                    g.DrawLine(pen, p1.X + centerX - figureCenterX, p1.Y + centerY - figureCenterY, p2.X + centerX - figureCenterX, p2.Y + centerY - figureCenterY);

                }
            }
            pictureBox1.Invalidate();
        }

        private void Draw()
        {
            if (!checkBox3.Checked)
            {
                DeletePic();
                foreach (var figure in allFigures)
                {

                    g.Clear(Color.White);
                    List<Edge> edges = projection.ProjectWithEdges(curFigure, projBox.SelectedIndex);

                    var centerX = pictureBox1.Width / 2;
                    var centerY = pictureBox1.Height / 2;

                    var figureLeftX = edges.Min(e => e.From.X < e.To.X ? e.From.X : e.To.X);
                    var figureLeftY = edges.Min(e => e.From.Y < e.To.Y ? e.From.Y : e.To.Y);
                    var figureRightX = edges.Max(e => e.From.X > e.To.X ? e.From.X : e.To.X);
                    var figureRightY = edges.Max(e => e.From.Y > e.To.Y ? e.From.Y : e.To.Y);


                    var figureCenterX = (figureRightX - figureLeftX) / 2;
                    var figureCenterY = (figureRightY - figureLeftY) / 2;


                    foreach (Edge line in edges)
                    {
                        var point1 = line.From.To2DPoint();
                        var point2 = line.To.To2DPoint();
                        g.DrawLine(pen, point1.X + centerX - figureCenterX, point1.Y + centerY - figureCenterY, point2.X + centerX - figureCenterX, point2.Y + centerY - figureCenterY);
                    }
                    pictureBox1.Invalidate();
                }
            }
            else DrawCam();
        }
        public void DrawCam()
        {
            g.Clear(Color.White);
            Point3D p1 = camera.Position, p2 = camera.Focus;
            double dist = Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y) + (p1.Z - p2.Z) * (p1.Z - p2.Z));
            double angleBeta = (180 - camera.AngleX) / 2;
            double angleGamma = 90 - angleBeta;
            float l = (float)(dist * Math.Sqrt(2 * (1 - Math.Cos(camera.AngleX * Math.PI / 180))));
            float x = (float)(l * Math.Cos(angleGamma * Math.PI / 180));
            float z = (float)(l * Math.Sin(angleGamma * Math.PI / 180));
            Point3D currentPosition = p1 + new Point3D(x, 0, z);
            
            Point3D Z = p2 - currentPosition; Z.Normalize();
            Point3D X = Point3D.VectorProduct(new Point3D(0, 1, 0), Z); X.Normalize();
            Point3D Y = Point3D.VectorProduct(Z, X); Y.Normalize();

            float[,] matrixV =
                 {
                     { X.X, X.Y, X.Z, -Point3D.ScalarProduct(X, currentPosition) },
                     { Y.X, Y.Y, Y.Z, -Point3D.ScalarProduct(Y, currentPosition) },
                     { Z.X, Z.Y, Z.Z, -Point3D.ScalarProduct(Z, currentPosition) },
                     { 0, 0, 0, 1 }
                 };

            foreach (var fig in allFigures)
            {
                Figure current = new Figure(fig);
                AffineChanges.RecreateFigure(current, matrixV);
                foreach (var line in projection.ProjectWithEdges(current, 0))
                    g.DrawLine(pen, new PointF(line.From.X + camera.Offset.X, line.From.Y + camera.Offset.Y),
                                    new PointF(line.To.X + camera.Offset.X, line.To.Y + camera.Offset.Y));
            }
            pictureBox1.Invalidate();
        }

        private void movement(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString().CompareTo("Up") == 0)
            {
                camera.MoveCamera(0, 0, -4);
                Draw();
            }

            if (e.KeyCode.ToString().CompareTo("Down") == 0)
            {
                camera.MoveCamera(0, 0, 4);
                Draw();
            }
            if (e.KeyCode.ToString().CompareTo("W") == 0)
            {
                camera.MoveCamera(0, -4, 0);
                Draw();
            }
            if (e.KeyCode.ToString().CompareTo("S") == 0)
            {
                camera.MoveCamera(0, 4, 0);
                Draw();
            }
            if (e.KeyCode.ToString().CompareTo("D") == 0)
            {
                camera.MoveCamera(4, 0, 0);
                Draw();
            }
            if (e.KeyCode.ToString().CompareTo("A") == 0)
            {
                camera.MoveCamera(-4, 0, 0);
                Draw();
            }
            if (e.KeyCode.ToString().CompareTo("Left") == 0)
            { 
                camera.RotateCamera(-2, 0);
                Draw();
            }
            if (e.KeyCode.ToString().CompareTo("Right") == 0)
            {
                camera.RotateCamera(2, 0);
                Draw();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Point3D start = new Point3D(0, 0, 0);
            float len = 150;

            List<Point3D> points = new List<Point3D>
            {
                start,
                new Point3D(len, 0, 0),
                new Point3D(len, 0, len),
                new Point3D(0, 0, len),

                new Point3D(0, len, 0),
                new Point3D(len, len, 0),
                new Point3D(len, len, len),
                new Point3D(0, len, len)
            };

            curFigure = new Figure(points);
            curFigure.AddEdges(0, new List<int> { 1, 4 });
            curFigure.AddEdges(1, new List<int> { 2, 5 });
            curFigure.AddEdges(2, new List<int> { 6, 3 });
            curFigure.AddEdges(3, new List<int> { 7, 0 });
            curFigure.AddEdges(4, new List<int> { 5 });
            curFigure.AddEdges(5, new List<int> { 6 });
            curFigure.AddEdges(6, new List<int> { 7 });
            curFigure.AddEdges(7, new List<int> { 4 });

            curFigure.AddSurface(new List<int> { 0, 1, 2, 3 });
            curFigure.AddSurface(new List<int> { 1, 2, 6, 5 });
            curFigure.AddSurface(new List<int> { 0, 3, 7, 4 });
            curFigure.AddSurface(new List<int> { 4, 5, 6, 7 });
            curFigure.AddSurface(new List<int> { 2, 3, 7, 6 });
            curFigure.AddSurface(new List<int> { 0, 1, 5, 4 });

            allFigures.Add(curFigure);
            Draw();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Point3D start = new Point3D(0, 0, 0);
            float len = 150;

            List<Point3D> points = new List<Point3D>
            {
                start,
                new Point3D(len, 0, len),
                new Point3D(len, len, 0),
                new Point3D(0, len, len),
            };

            curFigure = new Figure(points);
            curFigure.AddEdges(0, new List<int> { 1, 3, 2 });
            curFigure.AddEdges(1, new List<int> { 3 });
            curFigure.AddEdges(2, new List<int> { 1, 3 });


            curFigure.AddSurface(new List<int> { 0, 1, 2 });
            curFigure.AddSurface(new List<int> { 0, 1, 3 });
            curFigure.AddSurface(new List<int> { 0, 2, 3 });
            curFigure.AddSurface(new List<int> { 1, 2, 3 });

            allFigures.Add(curFigure);
            Draw();
        }
       
        private void button4_Click(object sender, EventArgs e)
        {
            Point3D start = new Point3D(0, 0, 0);
            float len = 150;

            List<Point3D> points = new List<Point3D>
            {
                start,
                new Point3D(len , len , 0),
                new Point3D(-len, len , 0),
                new Point3D(0, len , -len ),
                new Point3D(0, len , len ),
                new Point3D(0,  2 *len, 0),
            };

            curFigure = new Figure(points);
            curFigure.AddEdges(0, new List<int> { 1, 3, 2, 4 });
            curFigure.AddEdges(5, new List<int> { 1, 3, 2, 4 });
            curFigure.AddEdges(1, new List<int> { 3 });
            curFigure.AddEdges(3, new List<int> { 2 });
            curFigure.AddEdges(2, new List<int> { 4 });
            curFigure.AddEdges(4, new List<int> { 1 });

            curFigure.AddSurface(new List<int> { 0, 1, 3 });
            curFigure.AddSurface(new List<int> { 0, 1, 4 });
            curFigure.AddSurface(new List<int> { 0, 2, 3 });
            curFigure.AddSurface(new List<int> { 0, 2, 4 });
            curFigure.AddSurface(new List<int> { 5, 1, 3 });
            curFigure.AddSurface(new List<int> { 5, 1, 4 });
            curFigure.AddSurface(new List<int> { 5, 2, 3 });
            curFigure.AddSurface(new List<int> { 5, 2, 4 });

            allFigures.Add(curFigure);
            Draw();
        }



        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            allFigures.Clear();
            pictureBox1.Invalidate();
            rotationPoints.Clear();
        }

        private void DeletePic()
        {
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
        }


        private void button5_Click(object sender, EventArgs e)
        {
            float x = float.Parse(textBox1.Text);
            float y = float.Parse(textBox2.Text);
            float z = float.Parse(textBox3.Text);
            AffineChanges.Translate(curFigure, x, y, z);
            Draw();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            float x = float.Parse(textBox1.Text) / 100;
            float y = float.Parse(textBox2.Text) / 100;
            float z = float.Parse(textBox3.Text) / 100;
            AffineChanges.Scale(curFigure, x, y, z);
            Draw();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            float x = float.Parse(textBox1.Text);
            float y = float.Parse(textBox2.Text);
            float z = float.Parse(textBox3.Text);
            AffineChanges.Rotate(curFigure, x, y, z);
            Draw();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            AffineChanges.Reflect(curFigure, "xy");
            Draw();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            AffineChanges.Reflect(curFigure, "yz");
            Draw();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            AffineChanges.Reflect(curFigure, "xz");
            Draw();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            float a = float.Parse(textBox4.Text) / 100;
            AffineChanges.ScaleCenter(curFigure, a);
            Draw();
        }
        private void rotateBtn_Click(object sender, EventArgs e)
        {
            if (rotateOX.Checked)
            {
                AffineChanges.RotateCentral(curFigure, (float)rotateAngle.Value, 0, 0);
            }
            else if (rotateOY.Checked)
            {
                AffineChanges.RotateCentral(curFigure, 0, (float)rotateAngle.Value, 0);
            }
            else if (rotateOZ.Checked)
            {
                AffineChanges.RotateCentral(curFigure, 0, 0, (float)rotateAngle.Value);
            }
            else if (rotateOwn.Checked)

            {
                Edge ed = new Edge(float.Parse(rX1.Text), float.Parse(rY1.Text), float.Parse(rZ1.Text),
                    float.Parse(rX2.Text), float.Parse(rY2.Text), float.Parse(rZ2.Text));
                AffineChanges.RotateFigureAboutLine(curFigure, (float)rotateAngle.Value, ed);
            }
            Draw();
        }

        private void projBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (curFigure != null)
                Draw();
        }

        private void rotateOX_Click(object sender, EventArgs e)
        {
            rotateOY.Checked = rotateOZ.Checked = rotateOwn.Checked = false;
        }

        private void rotateOY_Click(object sender, EventArgs e)
        {
            rotateOX.Checked = rotateOZ.Checked = rotateOwn.Checked = false;
        }

        private void rotateOZ_Click(object sender, EventArgs e)
        {
            rotateOY.Checked = rotateOX.Checked = rotateOwn.Checked = false;
        }
        private void rotateOwn_Click(object sender, EventArgs e)
        {
            rotateOY.Checked = rotateOZ.Checked = rotateOX.Checked = false;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fName = saveFileDialog1.FileName;
                File.WriteAllText(fName, JsonConvert.SerializeObject(curFigure, Formatting.Indented), Encoding.UTF8);
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string fName = openFileDialog1.FileName;
                if (File.Exists(fName))
                {
                    curFigure = JsonConvert.DeserializeObject<Figure>(File.ReadAllText(fName, Encoding.UTF8));
                    Draw();
                }
            }
        }



        private void button15_Click(object sender, EventArgs e)
        {

            int count = int.Parse(textBox5.Text);
            char axis;
            if (radioButton1.Checked)
            {
                axis = 'x';
            }
            else if (radioButton2.Checked)
            {
                axis = 'y';
            }
            else
            {
                axis = 'z';
            }
            curFigure = FigureRotation.CreateRotationFigure(rotationPoints, count, axis);
            allFigures.Clear();
            allFigures.Add(curFigure);
            Draw();
        }


        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = radioButton3.Checked = false;
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = radioButton3.Checked = false;
        }

        private void radioButton3_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = radioButton2.Checked = false;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            float x = float.Parse(textBox6.Text);
            float y = float.Parse(textBox7.Text);
            float z = float.Parse(textBox8.Text);

            rotationPoints.Add(new Point3D(x, y, z));
        }
        delegate float func(float x, float y);
        private void button16_Click(object sender, EventArgs e)
        {
            float x0 = 0, y0 = 0, x1 = 0, y1 = 0, count = 0;
            try
            {
                float.TryParse(textBox9.Text, out x0);
                float.TryParse(textBox10.Text, out y0);
                float.TryParse(textBox11.Text, out x1);
                float.TryParse(textBox12.Text, out y1);
                float.TryParse(textBox13.Text, out count);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Неверное значение для графика");
                return;
            }
            func f;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    f = (x, y) => (float)(Cos(x + y));
                    break;
                case 1:
                    f = (x, y) => (float)(Sin(x + y));
                    break;
                default:
                    MessageBox.Show("График не выбран");
                    return;
            }
            float dx = (x1 - x0) / count;
            float dy = (y1 - y0) / count;
            float curx, cury = y0;

            List<Point3D> points = new List<Point3D>();
            for (int i = 0; i <= count; i++)
            {
                curx = x0;
                for (int j = 0; j <= count; j++)
                {
                    points.Add(new Point3D(curx, cury, f(curx, cury)));
                    curx += dx;
                }
                cury += dy;
            }
            Figure figure = new Figure(points);
            int n = (int)count + 1;
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    if (j != n - 1)
                        figure.AddEdges(i * n + j, i * n + j + 1);
                    if (i != n - 1)
                        figure.AddEdges(i * n + j, (i + 1) * n + j);
                    if (i != n - 1 && j != n - 1)
                        figure.AddSurface(new List<int> { i * n + j, i * n + j + 1, (i + 1) * n + j, (i + 1) * n + j + 1 });
                }

            AffineChanges.ScaleCenter(figure, 40);
            AffineChanges.RotateCentral(figure, 60, 0, 0);
            curFigure = figure;
            allFigures.Add(curFigure);
            Draw();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        

        private void ColorizedZBuffer(List<Color> colors)
        {
            Bitmap bmp = Buffer.CreateZBuffer(pictureBox1.Width, pictureBox1.Height, allFigures, colors, projBox.SelectedIndex);
            pictureBox1.Image = bmp;
            pictureBox1.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
            if (checkBox1.Checked)
            {
                Draw();
                ColorizedZBuffer(colors);
            }
            else
            {
                Draw();
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            float x = float.Parse(textBox15.Text);
            float y = float.Parse(textBox16.Text);
            float z = float.Parse(textBox17.Text);
            if (checkBox2.Checked)
            {
                DrawSurfaces(RemoveInvisibleSurfaces.RemoveSurfaces(curFigure, new Point3D(x, y, z)));
            }
            else
            {
                Draw();
            }
        }
    }
}