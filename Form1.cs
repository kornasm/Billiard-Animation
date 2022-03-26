using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GK_proj4
{
    public partial class Form1 : Form
    {
        int shadingMethod = 0;
        
        private readonly Timer t;
        private DirectBitmap bmp;

        Scene scene;
        Model sphere, table, floor, leg;
        ModelInstance whiteBall, tablegreen ,floorbrown, leg1, leg2, leg3, leg4, yellowBall, redBall, blackBall;
        SpotLight rotatingLight;
        public Form1()
        {
            InitializeComponent();

            scene = new Scene(pictureBox.Height, pictureBox.Width);
            ObjParser objParser = new ObjParser();
            sphere = objParser.ParseFile("../../../Models/sphere.obj"); 
            table = objParser.ParseFile("../../../Models/table.obj");
            floor = objParser.ParseFile("../../../Models/floor.obj");
            leg = objParser.ParseFile("../../../Models/tableleg.obj");

            whiteBall = new ModelInstance(sphere, new ObjectLightProperties(MyColor.FromColor(Color.White), 0.2, 0.8, 1, 5));
            yellowBall = new ModelInstance(sphere, new ObjectLightProperties(MyColor.FromColor(Color.Yellow), 0.2, 0.6, 1, 5));
            blackBall = new ModelInstance(sphere, new ObjectLightProperties(MyColor.FromColor(Color.FromArgb(10, 10, 10)), 0.2, 0.6, 15, 5));
            redBall = new ModelInstance(sphere, new ObjectLightProperties(MyColor.FromColor(Color.Red), 0.2, 0.6, 1, 5));
            tablegreen = new ModelInstance(table, new ObjectLightProperties(MyColor.FromColor(Color.FromArgb(20, 240, 20)), 0.2, 0.6, 0, 1));
            floorbrown = new ModelInstance(floor, new ObjectLightProperties(MyColor.FromColor(Color.Brown), 0.05, 0.7, 0, 1));
            leg1 = new ModelInstance(leg, new ObjectLightProperties(MyColor.FromColor(Color.Brown), 0.2, 0.7, 0.6, 3));
            leg2 = new ModelInstance(leg, new ObjectLightProperties(MyColor.FromColor(Color.Brown), 0.2, 0.7, 0.6, 3));
            leg3 = new ModelInstance(leg, new ObjectLightProperties(MyColor.FromColor(Color.Brown), 0.2, 0.7, 0.6, 3));
            leg4 = new ModelInstance(leg, new ObjectLightProperties(MyColor.FromColor(Color.Brown), 0.2, 0.7, 0.6, 3));

            whiteBall.ScaleBy((double)1 / 12);
            whiteBall.MoveBy(0, 0, 0.5);
            yellowBall.ScaleBy((double)1 / 12);
            yellowBall.MoveBy(-4, -2, 0.5);
            redBall.ScaleBy((double)1 / 12);
            redBall.MoveBy(-2, 2, 0.5);
            blackBall.ScaleBy((double)1 / 12);
            blackBall.MoveBy(0, -4.5, 0.5);
            leg1.MoveBy(8, 3.5, 0);
            leg2.MoveBy(8, -3.5, 0);
            leg3.MoveBy(-8, -3.5, 0);
            leg4.MoveBy(-8, 3.5, 0);

            scene.AddObject(whiteBall);
            scene.AddObject(yellowBall);
            scene.AddObject(redBall);
            
            scene.AddObject(blackBall);
            scene.AddObject(tablegreen);
            scene.AddObject(leg1);
            scene.AddObject(leg2);
            scene.AddObject(leg3);
            scene.AddObject(leg4);
            scene.AddObject(floorbrown);//*/

            scene.AddCamera(new CameraFollowing(0, 7, 15, whiteBall));

            scene.AddLight(new PointLight(new Point(-6, 0, 3), Color.White));
            scene.AddLight(new SpotLight(new Point(5, -5, 5), new Point(5, 0, 0), Color.White, 5));
            rotatingLight = new SpotLight(new Point(0, 4, 1), new Point(3, 4, 0), Color.White, 20);
            scene.AddLight(rotatingLight);

            bmp = new DirectBitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = bmp.Bitmap;
            hScrollBar1.Value = 70;
            hScrollBar1_Scroll(null, null);
            t = new Timer();
            t.Interval = 50;
            t.Tick += Tick;
            t.Start();

            UpdateTransformMatrix(0);
        }
        
        private double alph = 0;

        private void buttonStillCam_Click(object sender, EventArgs e)
        {
            scene.cam = scene.cameras[0];
        }

        private void buttonFollowObjectCam_Click(object sender, EventArgs e)
        {
            if(scene.cameras.Count >= 2)
            {
                scene.cam = scene.cameras[1];
            }
        }

        private void Tick(object sender, EventArgs e)
        {
            alph += Math.PI / 90;
            UpdateTransformMatrix(alph);
            Draw();
        }
        public void Draw()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            MyGraphics mgr = new MyGraphics(bmp);
            Graphics gr = Graphics.FromImage(bmp.Bitmap);
            mgr.ClearBitmap();
            scene.cam.UpdateViewMatrix();
            foreach(ModelInstance mi in scene.objects)
            {
                mgr.RenderFigure(mi, scene, shadingMethod);
            }
            //RenderFigure(tablegreen);
            //RenderFigure(spherewhite);
            sw.Stop();            
            long milis = sw.ElapsedMilliseconds;
            this.Text = (Math.Round(1000d / milis, 2)).ToString();
            this.Refresh();
        }

        /*public void RenderFigure(ModelInstance figure)
        {
            MyGraphics mgr = new MyGraphics(bmp);
            Graphics gr = Graphics.FromImage(bmp.Bitmap);

            /*Parallel.ForEach(figure.model.triangles, tr =>
            {
                PointWithNormal[] points = new PointWithNormal[3];
                for (int i = 0; i < 3; i++)
                {
                    //points[i].position = figure.model.points[tr.vertices[i]].Transform(figure.transformMatrix);
                    points[i] = new PointWithNormal(figure.model.points[tr.vertices[i]].Transform(figure.transformMatrix),
                                                    figure.model.normalVectors[tr.normals[i]].Transform(figure.transformMatrix)
                                                    );
                }
                mgr.RenderTriangle(new Triangle(points[0], points[1], points[2]), figure.lightProperties.color, scene, figure.lightProperties);
            });
            int n = figure.model.triangles.Count;
            for(int j = 0; j < n; j++)
            {
                MyVectorInt tr = figure.model.triangles[j];
                PointWithNormal[] points = new PointWithNormal[3];
                for (int i = 0; i < 3; i++)
                {
                    //points[i].position = figure.model.points[tr.vertices[i]].Transform(figure.transformMatrix);
                    points[i] = new PointWithNormal(figure.model.points[tr.vertices[i]].Transform(figure.moveMatrix * figure.scaleMatrix * figure.rotationMatrix),
                                                    figure.model.normalVectors[tr.normals[i]].Transform(figure.rotationMatrix)
                                                    );
                }
                if(j % 20 == 0)
                {
                    int a = 0;
                    a++;
                }
                mgr.RenderTriangle(new Triangle(points[0], points[1], points[2]), figure.lightProperties.color, scene, figure.lightProperties, shadingMethod);
            }
        }//*/

        private void buttonConstantShading_Click(object sender, EventArgs e)
        {
            shadingMethod = 2;
        }

        private void buttonGouraudShading_Click(object sender, EventArgs e)
        {
            shadingMethod = 1;
        }

        private void buttonPhongShading_Click(object sender, EventArgs e)
        {
            shadingMethod = 0;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            t.Stop();
            bmp.Dispose();
            bmp = new DirectBitmap(pictureBox.Width, pictureBox.Height);
            pictureBox.Image = bmp.Bitmap;
            scene.SetScreenSize(pictureBox.Height, pictureBox.Width);
            t.Start();
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            scene.ChangeFog(hScrollBar2.Value);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs ee)
        {
            scene.UpdateFov(hScrollBar1.Value);
            label1.Text = hScrollBar1.Value.ToString();
        }

        public double dist = 0, maxDist = 7, add = 0.1;
        private void UpdateTransformMatrix(double alpha)
        {
            if(dist >= maxDist)
            {
                add = -0.1;
            }
            if(dist < -maxDist)
            {
                add = 0.1;
            }
            dist += add;
            whiteBall.MoveBy(add, 0, 0);
            whiteBall.RotateAroundY((- 1) * add);
            rotatingLight.ChangeTarget(new Point(3 * Math.Cos(3 * alpha), 4 + 3 * Math.Sin(3 * alpha), 0));
        }
    }
}
