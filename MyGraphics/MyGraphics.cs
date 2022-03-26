using System;
using System.Drawing;
using System.Threading.Tasks;

namespace GK_proj4
{
    public class MyColor
    {
        public double R { get; set; }
        public double G { get; set; }
        public double B { get; set; }
        public MyColor(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }
        public MyColor(MyColor c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
        }
        public static MyColor FromColor(Color c)
        {
            double R = (double)c.R / 256;
            double G = (double)c.G / 256;
            double B = (double)c.B / 256;
            return new MyColor(R, G, B);
        }

        public Color ToColor()
        {
            byte RR = (byte)(Math.Min(R, 1) * 255);
            byte GG = (byte)(Math.Min(G, 1) * 255);
            byte BB = (byte)(Math.Min(B, 1) * 255);
            return Color.FromArgb(RR, GG, BB);
        }
        public MyColor Multiply(double mult)
        {
            return new MyColor(R * mult, G * mult, B * mult);
        }

        public static MyColor operator+(MyColor c1, MyColor c2)
        {
            return new MyColor(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);
        }
    }

    class MyGraphics
    {
        private readonly DirectBitmap bmp;

        private MyGraphics() { }
        public MyGraphics(DirectBitmap b)
        {
            bmp = b;
        }
        public void ClearBitmap()
        {
            Graphics gr = Graphics.FromImage(bmp.Bitmap);
            gr.Clear(Color.White);
            bmp.ResetZBuffer();//*/
        }

        static readonly Color blackColor = Color.Black;

        PolygonFiller pf = new PolygonFiller();
        FastPolygonFiller fpf = new FastPolygonFiller();

        public void RenderFigure(ModelInstance figure, Scene scene, int shadingMethod)
        {
                Graphics gr = Graphics.FromImage(bmp.Bitmap);

                int n = figure.model.triangles.Count;
                for (int j = 0; j < n; j++)
                {
                    MyVectorInt tr = figure.model.triangles[j];
                    PointWithNormal[] points = new PointWithNormal[3];
                    FastPointWithNormal[] fastpoints = new FastPointWithNormal[3];
                    for (int i = 0; i < 3; i++)
                    {
                        //points[i].position = figure.model.points[tr.vertices[i]].Transform(figure.transformMatrix);
                        points[i] = new PointWithNormal(figure.model.points[tr.vertices[i]].Transform(figure.moveMatrix * figure.scaleMatrix * figure.rotationMatrix),
                                                        figure.model.normalVectors[tr.normals[i]].Transform(figure.rotationMatrix)
                                                        );
                    }
                    RenderTriangle(new Triangle(points[0], points[1], points[2]), figure.lightProperties.color, scene, figure.lightProperties, shadingMethod);
                }
        }
        public void RenderTriangle(Triangle t, Color col, Scene scene, ObjectLightProperties lp, int shading)
        {
            if(shading == 0)
            {
                //Color c1 = pf.PhongFillTriangle(bmp, t, col, scene, lp);
                Color c2 = fpf.FastPhongFillTriangle(bmp, t, col, scene, lp);
                /*if(c1 != c2)
                {
                    c1 = pf.PhongFillTriangle(bmp, t, col, scene, lp);
                    c2 = fpf.FastPhongFillTriangle(bmp, t, col, scene, lp);
                }//*/
                return;
            }
            if(shading == 1)
            {
                pf.GouraudFillTriangle(bmp, t, col, scene, lp);
                return;
            }
            pf.ConstantFillTriangle(bmp, t, col, scene, lp);
        }
    }
}
