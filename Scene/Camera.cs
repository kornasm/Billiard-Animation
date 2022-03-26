using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GK_proj4
{
    public class Camera
    {
        public static Vector<double> UpVector = DenseVector.OfArray(new double[] { 0, 0, 1, 0 });
        public Matrix<double> viewMatrix;
        public Matrix<double> invViewMatrix;
        public Vector<double> position;
        public Vector<double> target;
        public System.Numerics.Vector4 pos;
        public System.Numerics.Vector4 targ;

        public Camera()
        {
            position = DenseVector.OfArray(new double[] { 15, 12, 7, 1 });
            pos = new System.Numerics.Vector4((float)position[0], (float)position[1], (float)position[2], 1);
            target = DenseVector.OfArray(new double[] { 0, 0, 1, 1 });
            //pos = new System.Numerics.Vector4(3, (float)0, (float)0, 0);
        }
        private Camera(double x, double y, double z, double a = 0, double b = 0, double c = 0)
        {
            position = DenseVector.OfArray(new double[] { x, y, z, 1 });
            target = DenseVector.OfArray(new double[] { a, b, c, 1 });
            UpdateViewMatrix();
        }

        private Vector<double> CrossProduct(Vector<double> v1, Vector<double> v2)
        {
            return DenseVector.OfArray(new double[] { v1[1] * v2[2] - v1[2] * v2[1], -v1[0] * v2[2] + v1[2] * v2[0], v1[0] * v2[1] - v1[1] * v2[0] });
        }

        public void ChangePosition(double x, double y, double z)
        {
            position = DenseVector.OfArray(new double[] { x, y, z, 1 });
            pos = new System.Numerics.Vector4((float)x, (float)y, (float)z, 1);
            UpdateViewMatrix();
        }

        public virtual void ChangeTarget(double x, double y, double z)
        {
            target = DenseVector.OfArray(new double[] { x, y, z, 1 });
            targ = new System.Numerics.Vector4((float)x, (float)y, (float)z, 1);
            UpdateViewMatrix();
        }

        public virtual void UpdateViewMatrix()
        {
            Vector<double> zAxis = (position - target).Normalize(2);
            Vector<double> xAxis = CrossProduct(UpVector, zAxis).Normalize(2);
            Vector<double> yAxis = CrossProduct(zAxis, xAxis).Normalize(2);
            invViewMatrix = DenseMatrix.OfArray(new double[,]
            {
                {xAxis[0], yAxis[0], zAxis[0], position[0] },
                {xAxis[1], yAxis[1], zAxis[1], position[1] },
                {xAxis[2], yAxis[2], zAxis[2], position[2] },
                {0, 0, 0, 1 }
            });
            viewMatrix = invViewMatrix.Inverse();
        }
    }

    public class CameraFollowing: Camera
    {
        ModelInstance followedObject;

        public CameraFollowing(int x, int y, int z, ModelInstance followedobject)
        {
            position = DenseVector.OfArray(new double[] { x, y, z, 1 });
            pos = new System.Numerics.Vector4((float)position[0], (float)position[1], (float)position[2], 1);
            followedObject = followedobject;
            ChangeTarget(0, 0, 0);
        }

        public override void ChangeTarget(double x, double y, double z)
        {
            target = DenseVector.OfArray(new double[] { followedObject.moveMatrix.At(0, 3), followedObject.moveMatrix.At(1, 3), followedObject.moveMatrix.At(2, 3), 1 });
            UpdateViewMatrix();
        }

        public override void UpdateViewMatrix()
        {
            target = DenseVector.OfArray(new double[] { followedObject.moveMatrix.At(0, 3), followedObject.moveMatrix.At(1, 3), followedObject.moveMatrix.At(2, 3), 1 });
            base.UpdateViewMatrix();
        }
    }

    
}
