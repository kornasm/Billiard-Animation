using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MathNet.Numerics.LinearAlgebra;

namespace GK_proj4
{
    abstract class LightSource
    {
        public Point position;
        public FastPoint fastPosition;
        //public Color color;
        public MyColor mycolor;
        public abstract MyColor GetLightIntensity(Vector<double> pointPosition);
        public abstract MyColor GetFastLightIntensity(System.Numerics.Vector4 pointPosition);
        //public abstract MyColor GetFastLightIntenity(FastPoint p);
        protected double c1 = 0.1;
        protected double c2 = 0.2;
        protected double c3 = 0.3;
        protected double intensity = 8;

        protected LightSource(Color c)
        {
            mycolor = MyColor.FromColor(c);

        }

        public LightSource(Point p, Color c)
        {
            position = p;
            fastPosition = new FastPoint(position);
            //color = c;
            mycolor = MyColor.FromColor(c);
        }
    }

    class PointLight: LightSource
    {
        public PointLight(Point p, Color c) :base(p, c){ }
        public override MyColor GetLightIntensity(Vector<double> pointPosition)
        {
            double dist = (position.position - pointPosition).L2Norm();
            return new MyColor(mycolor).Multiply(intensity / (c1 * dist * dist + c2 * dist + c3));
        }
        public override MyColor GetFastLightIntensity(System.Numerics.Vector4 pointPosition)
        {
            float dist = (fastPosition.position - pointPosition).Length();
            return new MyColor(mycolor).Multiply(intensity / (c1 * dist * dist + c2 * dist + c3));
        }
    }

    class SpotLight: LightSource
    {
        private Vector<double> direction;
        private System.Numerics.Vector4 fastdirection;
        private double p;

        public SpotLight(Point source, Point target, Color c, double pp = 2) : base(source, c)
        {
            direction = (target.position - source.position).Normalize(2);
            fastdirection = new System.Numerics.Vector4((float)direction[0], (float)direction[1], (float)direction[2], (float)direction[3]);
            p = pp;
        }

        public override MyColor GetLightIntensity(Vector<double> normalizedDirectionToPoint)
        {
            double cos = Math.Max(direction.PointwiseMultiply(normalizedDirectionToPoint).Sum(), 0);
            cos = Math.Pow(cos, p);
            return mycolor.Multiply(cos);
        }

        //public override MyC
        public override MyColor GetFastLightIntensity(System.Numerics.Vector4 normalizedDirectionToPoint)
        {
            System.Numerics.Vector4 vec = fastdirection * normalizedDirectionToPoint;
            double cos = Math.Max(vec.X + vec.Y + vec.Z + vec.W, 0);
            cos = Math.Pow(cos, p);
            return new MyColor(mycolor).Multiply(cos);
        }

        public void ChangeTarget(Point target)
        {
            direction = (target.position - position.position).Normalize(2);
            fastdirection = new System.Numerics.Vector4((float)direction[0], (float)direction[1], (float)direction[2], (float)direction[3]);
        }
    }
    class DirectionalLight : LightSource
    {
        //private Vector<double> direction;
        private System.Numerics.Vector4 fastdirection;
        private double p;

        public DirectionalLight(Vector<double> dir, Color c): base(c)
        {
            position = new Point(-dir.Normalize(2) * 1000); 
        }

        public override MyColor GetLightIntensity(Vector<double> normalizedDirectionToPoint)
        {
            //double cos = Math.Max(direction.PointwiseMultiply(normalizedDirectionToPoint).Sum(), 0);
            //cos = Math.Pow(cos, p);
            //return mycolor.Multiply(cos);
            return null;
        }
        public override MyColor GetFastLightIntensity(System.Numerics.Vector4 normalizedDirectionToPoint)
        {
            return null;
        }
    }
}
