using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Globalization;
using System.IO;

namespace GK_proj4
{
    public class Point
    {
        public Vector<double> position;
        //public Vector<double> normalVector;
        //public System.Numerics.Vector3 fastPosition;
        //public System.Numerics.Vector3 fastNormal;
        public double X { get => position[0]; set => position[0] = value; }
        public double Y { get => position[1]; set => position[1] = value; }
        public double Z { get => position[2]; set => position[2] = value; }
        public double W { get => position[3]; set => position[3] = value; }
        public Point(double x, double y, double z = 0, double w = 1)
        {
            position = DenseVector.OfArray(new double[] { x, y, z, w });
            //normalVector = DenseVector.OfArray(new double[] { 0, 0, 0, 0 });
        }

        public Point()
        {
            position = DenseVector.OfArray(new double[] { 0, 0, 0, 0 });
        }

        public Point(Point copied)
        {
            position = copied.position.Clone();
            //normalVector = copied.normalVector.Clone();
        }
        public Point(Vector<double> vector)
        {
            if(vector.Count != 4)
            {
                throw new NotImplementedException();
            }
            position = vector;
        }

        public Point Transform(Matrix<double> transformMatrix)
        {
            Point result = new Point(this);
            result.position = transformMatrix * result.position;
            //result.normalVector = transformMatrix * result.normalVector;
            return result;
        }

        public void LoadFromStringArray(string[] data)
        {
            bool success;

            double x, y, z;

            success = double.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
            if (!success) throw new ArgumentException("Could not parse X parameter as double");

            success = double.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
            if (!success) throw new ArgumentException("Could not parse Y parameter as double");

            success = double.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out z);
            if (!success) throw new ArgumentException("Could not parse Z parameter as double");

            X = x;
            Y = y;
            Z = z;
            W = 1;
        }
    }

    public class NormalVector
    {
        public Vector<double> direction;
        public double X { get => direction[0]; set => direction[0] = value; }
        public double Y { get => direction[1]; set => direction[1] = value; }
        public double Z { get => direction[2]; set => direction[2] = value; }
        public double W { get => direction[3]; set => direction[3] = value; }

        public NormalVector()
        {
            direction = DenseVector.OfArray(new double[] { 0, 0, 0, 0 });
        }

        public NormalVector(Vector<double> normDir)
        {
            direction = normDir;
        }

        public NormalVector(double x, double y, double z)
        {
            direction = DenseVector.OfArray(new double[] { x, y, z, 0 }).Normalize(2);
        }

        public void LoadFromStringArray(string[] data)
        {
            bool success;
            double x, y, z;

            success = double.TryParse(data[1], NumberStyles.Any, CultureInfo.InvariantCulture, out x);
            if (!success) throw new ArgumentException("Could not parse X parameter as double");

            success = double.TryParse(data[2], NumberStyles.Any, CultureInfo.InvariantCulture, out y);
            if (!success) throw new ArgumentException("Could not parse Y parameter as double");

            success = double.TryParse(data[3], NumberStyles.Any, CultureInfo.InvariantCulture, out z);
            if (!success) throw new ArgumentException("Could not parse Z parameter as double");

            X = x;
            Y = y;
            Z = z;
            W = 0;
        }

        public NormalVector Transform(Matrix<double> transformMatrix)
        {
            return new NormalVector((transformMatrix * direction).Normalize(2));
        }
    }

    public class PointWithNormal
    {
        public Point position;
        public NormalVector normal;

        public PointWithNormal(Point p, NormalVector nv)
        {
            position = new Point(p);
            normal = new NormalVector(nv.direction);
        }
    }

    public class Model
    {
        public List<Point> points;
        public List<NormalVector> normalVectors;
        //public List<System.Numerics.Vector<int>> triangles;
        public List<MyVectorInt> triangles;

        public Model()
        {
            points = new List<Point>();
            normalVectors = new List<NormalVector>();
            triangles = new List<MyVectorInt>();
        }
    }

    public class ModelInstance
    {
        public Model model;
        public readonly ObjectLightProperties lightProperties;
        public Matrix<double> rotationMatrix;
        public Matrix<double> scaleMatrix;
        public Matrix<double> moveMatrix;

        public ModelInstance(Model mod, ObjectLightProperties lp = null)
        {
            model = mod;
            if (lp == null)
            {
                lightProperties = new ObjectLightProperties(MyColor.FromColor(System.Drawing.Color.Blue));
            }
            else
            {
                lightProperties = lp;
            }
            rotationMatrix = DenseMatrix.CreateIdentity(4);
            scaleMatrix = DenseMatrix.CreateIdentity(4);
            moveMatrix = DenseMatrix.CreateIdentity(4);
        }
        public void RotateAroundZ(double alpha)
        {
            alpha = -alpha;
            rotationMatrix *= DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(alpha), -Math.Sin(alpha), 0, 0 },
                {Math.Sin(alpha), Math.Cos(alpha), 0, 0 },
                {0, 0, 1, 0},
                {0, 0, 0, 1 }
            });
        }
        public void RotateAroundX(double alpha)
        {
            alpha = -alpha;
            rotationMatrix *= DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0, 0},
                {0, Math.Cos(alpha), -Math.Sin(alpha), 0 },
                {0, Math.Sin(alpha), Math.Cos(alpha), 0 },
                {0, 0, 0, 1 }
            });
        }
        public void RotateAroundY(double alpha)
        {
            //alpha = alpha;
            rotationMatrix *= DenseMatrix.OfArray(new double[,]
            {
                {Math.Cos(alpha), 0, -Math.Sin(alpha), 0 },
                {0, 1, 0, 0},
                {Math.Sin(alpha), 0 , Math.Cos(alpha), 0 },
                {0, 0, 0, 1 }
            });
        }
        public void MoveBy(double x, double y, double z)
        {
            moveMatrix += DenseMatrix.OfArray(new double[,]
            {
                {0, 0, 0, x},
                {0, 0, 0, y},
                {0, 0, 0, z},
                {0, 0, 0, 0 }
            });
        }

        public void MoveTo(double x, double y, double z)
        {
            moveMatrix = DenseMatrix.OfArray(new double[,]
            {
                {0, 0, 0, x},
                {0, 0, 0, y},
                {0, 0, 0, z},
                {0, 0, 0, 0 }
            });
        }
        public void ScaleBy(double x)
        {
            scaleMatrix *= DenseMatrix.OfArray(new double[,]
            {
                {x, 0, 0, 0},
                {0, x, 0, 0},
                {0, 0, x, 0},
                {0, 0, 0, 1 }
            });
        }
    }

    public class Triangle
    {
        public List<PointWithNormal> points;
        public Triangle(Point p1, Point p2, Point p3, NormalVector v1, NormalVector v2, NormalVector v3)
        {
            points = new List<PointWithNormal>
            {
                new PointWithNormal(p1, v1),
                new PointWithNormal(p2, v2),
                new PointWithNormal(p3, v3)
            };
        }
        public Triangle(PointWithNormal p1, PointWithNormal p2, PointWithNormal p3)
        {
            points = new List<PointWithNormal>
            {
                p1,
                p2,
                p3
            };
        }
    }

    // z pomocą: https://github.com/stefangordon/ObjParser
    public class ObjParser
    {
        Model model;
        public Model ParseFile(string path)
        {
            model = new Model();
            LoadObj(File.ReadAllLines(path));
            return model;
        }
        public void LoadObj(Stream data)
        {
            using (var reader = new StreamReader(data))
            {
                LoadObj(reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()));
            }
        }
        public void LoadObj(IEnumerable<string> data)
        {
            foreach (var line in data)
            {
                ProcessLine(line);
            }

            //updateSize();
        }
        public void ProcessLine(string line)
        {
            string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                switch (parts[0])
                {
                    case "v":
                        Point p = new Point();
                        p.LoadFromStringArray(parts);
                        model.points.Add(p);
                        //v.Index = points.Count();
                        break;
                    case "f":
                        MyVectorInt v = new MyVectorInt();
                        bool quadrangle = v.LoadFromStringArray(parts, new int[] { 0, 1, 2 });
                        //f.UseMtl = UseMtl;
                        model.triangles.Add(v);
                        if (quadrangle)
                        {
                            MyVectorInt v2 = new MyVectorInt();
                            v2.LoadFromStringArray(parts, new int[] { 2, 3, 0 });
                            model.triangles.Add(v2);
                        }
                        break;
                    case "vn":
                        NormalVector vn = new NormalVector();
                        vn.LoadFromStringArray(parts);
                        model.normalVectors.Add(vn);
                        
                        //vt.Index = TextureList.Count();
                        break;

                }
            }
        } 
    }
}
