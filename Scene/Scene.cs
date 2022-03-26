using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GK_proj4
{
    class Scene
    {
        double n = 1, f = 100, e = 1, fov = 60;
        double a;
        public int screenWidth;
        public int screenHeight;
        public List<LightSource> lights;
        public List<ModelInstance> objects;
        public List<Camera> cameras;

        private double maxFogDistance = 60;

        public Camera cam;
        public Matrix<double> projectionMatrix;
        //public Matrix<double> invProjMatrix;
        public MyColor abientIlumination;

        public Scene(int height, int width)
        {
            screenHeight = height;
            screenWidth = width;
            a = (double)height / width;
            fov = (Math.PI / 180) * 60;
            e = 1 / Math.Tan(fov / 2);//*/
            lights = new List<LightSource>();
            objects = new List<ModelInstance>();
            cameras = new List<Camera>();
            abientIlumination = new MyColor(1, 1, 1);
            cam = new Camera();
            cameras.Add(cam);
            projectionMatrix = DenseMatrix.OfArray(new double[,]
            {
                {e, 0, 0, 0 },
                {0, e/a, 0, 0 },
                {0, 0, (-1) * (f + n) / (f - n), (-2) * f * n / (f - n) },
                {0, 0, -1, 0 }
            });
            //invProjMatrix = projectionMatrix.Inverse();
        }

        public void AddLight(LightSource l)
        {
            lights.Add(l);
        }

        public void AddObject(ModelInstance obj)
        {
            objects.Add(obj);
        }

        public void AddCamera(Camera cam)
        {
            cameras.Add(cam);
        }

        public void SetScreenSize(int heigth, int width)
        {
            screenHeight = heigth;
            screenWidth = width;
            a = (double)heigth / width;
            UpdateProjMatrix();   
        }

        public void UpdateFov(double value)
        {
            fov = (Math.PI / 180) * value;
            e = 1 / Math.Tan(fov / 2);//*/
            UpdateProjMatrix();
        }

        private void UpdateProjMatrix()
        {
            projectionMatrix = DenseMatrix.OfArray(new double[,]
            {
                {e, 0, 0, 0 },
                {0, e/a, 0, 0 },
                {0, 0, (-1) * (f + n) / (f - n), (-2) * f * n / (f - n) },
                {0, 0, -1, 0 }
            });
        }

        public MyColor ApplyFog(MyColor col, double distance)
        {
            MyColor result = new MyColor(0, 0, 0);
            double white = Math.Pow(distance / maxFogDistance, 1);
            white = Math.Min(white, 1);
            //double ratioR = col.R / white;
            result.R = col.R + white - white * col.R;
            result.G = col.G + white - white * col.G;
            result.B = col.B + white - white * col.B;
            return result;
        }

        private int dir = 1;
        public void ChangeFog(double value)
        {
            maxFogDistance = value;
        }

        public void SwitchCamera(int idx)
        {
            if(idx < cameras.Count)
                cam = cameras[idx];
        }
    }
}
