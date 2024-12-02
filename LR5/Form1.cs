using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LR5
{
    public partial class Form1 : Form
    {

        private Bitmap image;
        private double[,,] vertices;
        private const double PI = Math.PI;
        private const double STEP = 0.1;

       

        private double xOffset = 0;
        private double yOffset = 0;
        private double zOffset = 0;
        private double scaleFactor = 1;
        private double rotationX = 0;
        private double rotationY = 0;
        private double rotationZ = 0;
        bool flag = false;
        private Matrix4x4 translationMatrix = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        private Matrix4x4 scaleMatrix = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        private Matrix4x4 rotationMatrixX = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        private Matrix4x4 rotationMatrixY = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        private Matrix4x4 rotationMatrixZ = new Matrix4x4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);


        // Структура для матриц
        public struct Matrix4x4
        {
            public double m11, m12, m13, m14;
            public double m21, m22, m23, m24;
            public double m31, m32, m33, m34;
            public double m41, m42, m43, m44;

            public Matrix4x4(double m11, double m12, double m13, double m14,
                             double m21, double m22, double m23, double m24,
                             double m31, double m32, double m33, double m34,
                             double m41, double m42, double m43, double m44)
            {
                this.m11 = m11; this.m12 = m12; this.m13 = m13; this.m14 = m14;
                this.m21 = m21; this.m22 = m22; this.m23 = m23; this.m24 = m24;
                this.m31 = m31; this.m32 = m32; this.m33 = m33; this.m34 = m34;
                this.m41 = m41; this.m42 = m42; this.m43 = m43; this.m44 = m44;
            }

            public Points4D Multiply(Points4D point)
            {
                double newX = m11 * point.X + m12 * point.Y + m13 * point.Z + m14 * point.W;
                double newY = m21 * point.X + m22 * point.Y + m23 * point.Z + m24 * point.W;
                double newZ = m31 * point.X + m32 * point.Y + m33 * point.Z + m34 * point.W;
                double newW = m41 * point.X + m42 * point.Y + m43 * point.Z + m44 * point.W;
                return new Points4D(newX, newY, newZ, newW);
            }
        }

       
        public struct Points4D
        {
            public double X, Y, Z, W;
            public Points4D(double x, double y, double z, double w = 1)
            {
                X = x; Y = y; Z = z; W = w;
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            pictureBox1.Image = image;
            GenerateSurface();
            Render();
        }

        private void GenerateSurface()
        {
            int numT = (int)((1.4 * PI - 0.1 * PI) / STEP) + 1;
            int numO = (int)((1.5 * PI) / STEP) + 1;
            vertices = new double[numT, numO, 3];

            for (int i = 0; i < numT; i++)
            {
                double t = 0.1 * PI + i * STEP;
                for (int j = 0; j < numO; j++)
                {
                    double o = j * STEP;
                    double x = (1 + t - Math.Sin(t)) * Math.Cos(o);
                    double y = 1 - Math.Cos(t);
                    double z = -(1 + t - Math.Sin(t)) * Math.Sin(o);
                    vertices[i, j, 0] = x;
                    vertices[i, j, 1] = y;
                    vertices[i, j, 2] = z;
                }
            }
        }

        private Point Project(double x, double y, double z)
        {
           
            double scaleFactor = 40;
            if(!flag)
            {                         // Аксонометрическая проекция
                double radA = 30 * Math.PI / 180; // Преобразование градусов в радианы
                double radB = 30 * Math.PI / 180;

                double projectedX = x * Math.Cos(radA) + y * Math.Sin(radA);
                double projectedY = -x * Math.Sin(radA) * Math.Cos(radB) + y * Math.Cos(radA) * Math.Cos(radB) + z * Math.Sin(radB);
                projectedX *= scaleFactor;
                projectedY *= scaleFactor;

                return new Point((int)(projectedX + pictureBox1.Width / 2), (int)(-projectedY + pictureBox1.Height / 2));
                flag = true;
            }
            else
            {
                return new Point((int)(x * scaleFactor + pictureBox1.Width / 2), (int)(-y * scaleFactor + pictureBox1.Height / 2));
            }
           
          
        }

        private void Render()
        {
            Graphics g = Graphics.FromImage(image);
            g.Clear(Color.White);
            Pen pen = new Pen(Color.Black);

            double[,,] transformedVertices = new double[vertices.GetLength(0), vertices.GetLength(1), 3];
            for (int i = 0; i < vertices.GetLength(0); i++)
            {
                for (int j = 0; j < vertices.GetLength(1); j++)
                {
                    Points4D p = new Points4D(vertices[i, j, 0], vertices[i, j, 1], vertices[i, j, 2]);
                    p = translationMatrix.Multiply(p);
                    p = scaleMatrix.Multiply(p);
                    p = rotationMatrixX.Multiply(p);
                    p = rotationMatrixY.Multiply(p);
                    p = rotationMatrixZ.Multiply(p);

                    Point projectedPoint = Project(p.X, p.Y, p.Z);

                    // отрисовка линий вдоль t (константа o)
                    if (i < vertices.GetLength(0) - 1)
                    {
                        Points4D p2 = new Points4D(vertices[i + 1, j, 0], vertices[i + 1, j, 1], vertices[i + 1, j, 2]);
                        p2 = translationMatrix.Multiply(p2);
                        p2 = scaleMatrix.Multiply(p2);
                        p2 = rotationMatrixX.Multiply(p2);
                        p2 = rotationMatrixY.Multiply(p2);
                        p2 = rotationMatrixZ.Multiply(p2);
                        Point projectedPoint2 = Project(p2.X, p2.Y, p2.Z);
                        g.DrawLine(pen, projectedPoint, projectedPoint2);
                    }

                    // отрисовка линий вдоль o (константа t)
                    if (j < vertices.GetLength(1) - 1)
                    {
                        Points4D p2 = new Points4D(vertices[i, j + 1, 0], vertices[i, j + 1, 1], vertices[i, j + 1, 2]);
                        p2 = translationMatrix.Multiply(p2);
                        p2 = scaleMatrix.Multiply(p2);
                        p2 = rotationMatrixX.Multiply(p2);
                        p2 = rotationMatrixY.Multiply(p2);
                        p2 = rotationMatrixZ.Multiply(p2);
                        Point projectedPoint2 = Project(p2.X, p2.Y, p2.Z);
                        g.DrawLine(pen, projectedPoint, projectedPoint2);
                    }
                }
            }
            pictureBox1.Invalidate();
            g.Dispose();
        }
       
         private void UpdateTransformationMatrices()
    {
        translationMatrix = new Matrix4x4(1, 0, 0, xOffset, 0, 1, 0, yOffset, 0, 0, 1, zOffset, 0, 0, 0, 1);
        scaleMatrix = new Matrix4x4(scaleFactor, 0, 0, 0, 0, scaleFactor, 0, 0, 0, 0, scaleFactor, 0, 0, 0, 0, 1);

        double radX = rotationX * PI / 180;
        double radY = rotationY * PI / 180;
        double radZ = rotationZ * PI / 180;

        rotationMatrixX = new Matrix4x4(1, 0, 0, 0, 0, Math.Cos(radX), -Math.Sin(radX), 0, 0, Math.Sin(radX), Math.Cos(radX), 0, 0, 0, 0, 1);
        rotationMatrixY = new Matrix4x4(Math.Cos(radY), 0, Math.Sin(radY), 0, 0, 1, 0, 0, -Math.Sin(radY), 0, Math.Cos(radY), 0, 0, 0, 0, 1);
        rotationMatrixZ = new Matrix4x4(Math.Cos(radZ), -Math.Sin(radZ), 0, 0, Math.Sin(radZ), Math.Cos(radZ), 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);

    }

        
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            xOffset = trackBar1.Value; // Adjust the scaling as needed
            UpdateTransformationMatrices();
            Render();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            yOffset = trackBar2.Value;
            UpdateTransformationMatrices();
            Render();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            zOffset = trackBar3.Value;
            UpdateTransformationMatrices();
            Render();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            scaleFactor = trackBar4.Value / 10.0; // Adjust the scaling as needed
            UpdateTransformationMatrices();
            Render();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            rotationX = trackBar5.Value;
            UpdateTransformationMatrices();
            Render();
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            rotationY = trackBar6.Value;
            UpdateTransformationMatrices();
            Render();
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            rotationZ = trackBar7.Value;
            UpdateTransformationMatrices();
            Render();
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

    }
}
