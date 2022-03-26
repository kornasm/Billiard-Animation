using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK_proj4
{
    public class MyVectorInt
    {
        public int[] vertices;
        public int[] normals;
        public MyVectorInt()
        {
            vertices = new int[3] { 0, 0, 0 };
            normals = new int[3] { 0, 0, 0 };
        }
        public MyVectorInt(int a, int b, int c, int d, int e, int f)
        {
            vertices = new int[3] { a, b, c };
            normals = new int[3] { d, e, f };
        }

        public bool LoadFromStringArray(string[] data, int[] indexes)
        {
            //if (data.Length < MinimumDataLength)
            //    throw new ArgumentException("Input array must be of minimum length " + MinimumDataLength, "data");

            //if (!data[0].ToLower().Equals(Prefix))
            //    throw new ArgumentException("Data prefix must be '" + Prefix + "'", "data");

            int vcount = data.Count() - 1;
            vertices = new int[3];
            normals = new int[3];

            bool success;
            for (int i = 0; i < 3; i++)
            {
                string[] parts = data[indexes[i] + 1].Split('/');

                int vindex;
                success = int.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
                //if (!success) throw new ArgumentException("Could not parse parameter as int");
                vertices[i] = vindex - 1;

                if (parts.Count() > 1)
                {
                    success = int.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out vindex);
                    if (success)
                    {
                        normals[i] = vindex - 1;
                    }
                }
            }
            return vcount > 3;
        }
    }
    public class MyVector
    {
        public double X;
        public double Y;
        public double Z;
        public double Length
        {
            get => Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public MyVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public MyVector(Point p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        public double AngleTo(MyVector a)
        {
            return X * a.X + Y * a.Y + Z * a.Z;
        }

        public MyVector CreateVersor()
        {
            double length = Math.Sqrt(X * X + Y * Y + Z * Z);
            return new MyVector(X / length, Y / length, Z / length);
        }

        public MyVector ScaleBy(double scale)
        {
            return new MyVector(X * scale, Y * scale, Z * scale);
        }

        public static MyVector operator -(MyVector v1, MyVector v2)
        {
            return new MyVector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }
        public static MyVector operator +(MyVector v1, MyVector v2)
        {
            return new MyVector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
        }

        public static double ScalarProduct(MyVector v1, MyVector v2)
        {
            return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
        }

        public static double CrossProductValue(MyVector a, MyVector b)
        {
            MyVector vec = new MyVector(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
            return vec.Length;
        }

        public static double PlainCrossProductValue(MyVector a, MyVector b)
        {
            return (a.X * b.Y) - (a.Y * b.X);
        }

        public static double Cos(MyVector v1, MyVector v2)
        {
            return Math.Max(ScalarProduct(v1, v2) / v1.Length / v2.Length, 0);
        }

        public static explicit operator MyVector(Point p)
        {
            return new MyVector(p);
        }
    }
}
