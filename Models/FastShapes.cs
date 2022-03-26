using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace GK_proj4
{
    public class FastPoint
    {
        public System.Numerics.Vector4 position;
        public System.Numerics.Vector4 normalVector;
        public double X { get => position.X; set => position.X = (float)value; }
        public double Y { get => position.Y; set => position.Y = (float)value; }
        public double Z { get => position.Z; set => position.Z = (float)value; }
        public double W { get => position.W; set => position.W = (float)value; }

        public FastPoint(Point p)
        {
            position = new System.Numerics.Vector4((float)p.position[0], (float)p.position[1], (float)p.position[2], 1);
        }

        public FastPoint(FastPoint p)
        {
            position = new Vector4(p.position.X, p.position.Y, p.position.Z, 1);
        }//*/

        public FastPoint(float x, float y, float z)
        {
            position = new System.Numerics.Vector4(x, y, z, 1);
            normalVector = new System.Numerics.Vector4(0, 0, 0, 0);
        }
    }

    public class FastNormalVector
    {
        public Vector4 direction;
        public float X { get => direction.X; set => direction.X = value; }
        public float Y { get => direction.Y; set => direction.Y = value; }
        public float Z { get => direction.Z; set => direction.Z = value; }
        public float W { get => direction.W; set => direction.W = value; }

        public FastNormalVector()
        {
            direction = new Vector4(0, 0, 0, 0);
        }

        public FastNormalVector(Vector4 normDir)
        {
            direction = normDir;
        }

        public FastNormalVector(NormalVector nv)
        {
            direction = new Vector4((float)nv.X, (float)nv.Y, (float)nv.Z, 0);
        }

        public FastNormalVector(double x, double y, double z)
        {
            //direction = DenseVector.OfArray(new double[] { x, y, z, 0 }).Normalize(2);
            direction = Vector4.Normalize(new Vector4((float)x, (float)y, (float)z, 0));
        }
    }

    public class FastPointWithNormal
    {
        public FastPoint position;
        public FastNormalVector normal;

        public FastPointWithNormal(FastPoint p, FastNormalVector nv)
        {
            position = new FastPoint(p);
            normal = new FastNormalVector(nv.direction);
        }
    }

}
