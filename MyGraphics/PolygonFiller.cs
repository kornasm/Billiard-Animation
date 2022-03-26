using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GK_proj4
{
    class PointWithIdx
    {
        public Point p;
        public int idx;

        public PointWithIdx(Point point, int index)
        {
            p = point;
            idx = index;
        }
    }
    class AETEntry
    {
        public int yMax;
        public double x;
        public double oneOverM;

        public AETEntry(int y, double xx, double onem)
        {
            yMax = y;
            x = xx;
            oneOverM = onem;
        }
    }
    class AET
    {
        LinkedList<AETEntry> entries;
        public AET()
        {
            entries = new LinkedList<AETEntry>();
        }
        public void UpdateXs()
        {
            foreach (AETEntry en in entries)
            {
                en.x += en.oneOverM;
            }
        }
        public void DeleteEntries(int y)
        {
            if (entries.Count == 0)
            {
                return;
            }
            LinkedListNode<AETEntry> en = entries.First;
            LinkedListNode<AETEntry> next;
            while (en != null)
            {
                next = en.Next;
                if (en.Value.yMax < y)
                {
                    entries.Remove(en);
                }
                en = next;
            }
        }
        public void InsertEdge(Point from, Point to)
        {
            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double oneM = dx / dy;
            AETEntry toInsert = new AETEntry((int)to.Y, from.X + oneM, oneM);
            LinkedListNode<AETEntry> node = entries.First;
            bool inserted = false;
            while (node != null)
            {
                if (node.Value.x > toInsert.x)
                {
                    entries.AddBefore(node, toInsert);
                    inserted = true;
                    break;
                }
                node = node.Next;
            }
            if (!inserted)
            {
                entries.AddLast(toInsert);
            }
        }
        public List<int> GetX()
        {
            List<int> Xs = new List<int>();
            foreach (AETEntry en in entries)
            {
                Xs.Add((int)en.x);
            }
            return Xs;
        }
        public void Sort()
        {
            List<AETEntry> list = new List<AETEntry>(entries);
            list.Sort(Comparer<AETEntry>.Create((e1, e2) => e1.x.CompareTo(e2.x)));
            entries = new LinkedList<AETEntry>(list);
        }
    }
    class PolygonFiller
    {
        //const double maxval = (double)255 / 256;
        static int GetNextFromList(int ContainerSize, int idx)
        {
            return idx == ContainerSize - 1 ? 0 : idx + 1;
        }
        static int GetPrevFromList(int ContiainerSize, int idx)
        {
            return idx == 0 ? ContiainerSize - 1 : idx - 1;
        }

        public MyColor CalculateVertexColor(PointWithNormal v, ObjectLightProperties lp, Scene scene)
        {
            v.normal.direction = v.normal.direction.Normalize(2);
            MyColor result = new MyColor(0, 0, 0);
            result.R += scene.abientIlumination.R * lp.mycolor.R * lp.ka;
            result.G += scene.abientIlumination.G * lp.mycolor.G * lp.ka;
            result.B += scene.abientIlumination.B * lp.mycolor.B * lp.ka;

            Vector<double> toCam = (scene.cam.position - v.position.position).Normalize(2);
            foreach (LightSource ls in scene.lights)
            {
                Vector<double> toLight = (ls.position.position - v.position.position).Normalize(2);
                MyColor lightintensity = ls.GetLightIntensity(-toLight);
                double cos = Math.Max(v.normal.direction.PointwiseMultiply(toLight).Sum(), 0);
                double mult1 = cos * lp.kd;
                result.R += mult1 * lp.mycolor.R * lightintensity.R;
                result.G += mult1 * lp.mycolor.G * lightintensity.G;
                result.B += mult1 * lp.mycolor.B * lightintensity.B;

                Vector<double> reflection = (v.normal.direction * 2 * cos - toLight).Normalize(2);
                Vector<double> cosv = reflection.PointwiseMultiply(toCam);
                double cosr = Math.Max(cosv.Sum(), 0);
                cosr = Math.Pow(cosr, lp.ns);
                double mult2 = cosr * lp.ks;
                result.R += mult2 * lp.mycolor.R * lightintensity.R;
                result.G += mult2 * lp.mycolor.G * lightintensity.G;
                result.B += mult2 * lp.mycolor.B * lightintensity.B;//*/
            }

            return result;
        }

        public Task ConstantFillTriangle(DirectBitmap bmp, Triangle tr, Color col, Scene scene, ObjectLightProperties lp)
        {
            MyColor color1v1 = CalculateVertexColor(tr.points[0], lp, scene);
            MyColor color1v2 = CalculateVertexColor(tr.points[1], lp, scene);
            MyColor color1v3 = CalculateVertexColor(tr.points[2], lp, scene);
            MyColor result = (color1v1 + color1v2 + color1v3).Multiply((double)1 / 3);
            Color toFill = result.ToColor();
            Point p1 = tr.points[0].position;
            Point p2 = tr.points[1].position;
            Point p3 = tr.points[2].position;
            List<Point> points = new List<Point>();
            for (int i = 0; i < tr.points.Count; i++)
            {
                Vector<double> position = scene.projectionMatrix * scene.cam.viewMatrix * tr.points[i].position.position;
                int x = (int)(position[0] / position[3] * scene.screenWidth / 2 + scene.screenWidth / 2);
                int y = (int)(-position[1] / position[3] * scene.screenHeight / 2 + scene.screenHeight / 2);
                points.Add(new Point(x, y, position[2]));

            }
            int n = points.Count;
            PointWithIdx[] pointlist = new PointWithIdx[n];
            for (int i = 0; i < n; i++)
            {
                pointlist[i] = new PointWithIdx(points[i], i);
            }
            Array.Sort(pointlist, (x, y) => x.p.Y.CompareTo(y.p.Y));
            int yMin = (int)pointlist[0].p.Y, yMax = (int)pointlist[n - 1].p.Y;
            AET aet = new AET();
            int idx = 0;
            for (int scanY = yMin; scanY <= yMax; scanY++)
            {
                aet.DeleteEntries(scanY);
                aet.UpdateXs();
                while (pointlist[idx].p.Y < scanY)
                {
                    Point prev = points[GetPrevFromList(points.Count, pointlist[idx].idx)];
                    if (prev.Y > pointlist[idx].p.Y)
                    {
                        aet.InsertEdge(pointlist[idx].p, prev);
                    }
                    Point next = points[GetNextFromList(points.Count, pointlist[idx].idx)];
                    if (next.Y > pointlist[idx].p.Y)
                    {
                        aet.InsertEdge(pointlist[idx].p, next);
                    }
                    idx++;
                }
                if (scanY < 0)
                {
                    continue;
                }
                if (scanY > scene.screenHeight)
                {
                    break;
                }
                //
                List<int> xCoords = aet.GetX();
                int inside = 0;
                if (xCoords.Count == 0)
                {
                    continue;
                }
                int xMin = xCoords[0], xMax = xCoords[^1];
                int Xidx = 0;

                for (int x = xMin; x <= xMax; x++)
                {
                    if (Xidx < xCoords.Count)
                    {
                        while (xCoords[Xidx] < x)
                        {
                            inside = 1 - inside;
                            Xidx++;
                            if (Xidx >= xCoords.Count)
                            {
                                break;
                            }
                        }
                    }
                    if (x < 0)
                    {
                        continue;
                    }
                    if (x > scene.screenWidth)
                    {
                        x = xMax + 1;
                        continue;
                    }
                    if (inside == 1)
                    {
                        MyVector pointPosition = new MyVector(x, scanY, 0);
                        MyVector FromPointToP1 = (MyVector)points[0] - pointPosition;
                        MyVector FromPointToP2 = (MyVector)points[1] - pointPosition;
                        MyVector FromPointToP3 = (MyVector)points[2] - pointPosition;
                        double areaP1 = MyVector.PlainCrossProductValue(FromPointToP2, FromPointToP3);
                        double areaP2 = MyVector.PlainCrossProductValue(FromPointToP3, FromPointToP1);
                        double areaP3 = MyVector.PlainCrossProductValue(FromPointToP1, FromPointToP2);
                        double totalArea = areaP1 + areaP2 + areaP3;
                        areaP1 /= totalArea;
                        areaP2 /= totalArea;
                        areaP3 /= totalArea;

                        double zbuf1 = points[0].Z * (areaP1);
                        double zbuf2 = points[1].Z * (areaP2);
                        double zbuf3 = points[2].Z * (areaP3);
                        double zbufres = (zbuf1 + zbuf2 + zbuf3);

                        MyColor res = scene.ApplyFog(result, zbufres);
                        toFill = res.ToColor();
                        bmp.SetPixel(x, scanY, (float)zbufres, toFill);

                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task GouraudFillTriangle(DirectBitmap bmp, Triangle tr, Color col, Scene scene, ObjectLightProperties lp)
        {
            MyColor color1v1 = CalculateVertexColor(tr.points[0], lp, scene);
            MyColor color1v2 = CalculateVertexColor(tr.points[1], lp, scene);
            MyColor color1v3 = CalculateVertexColor(tr.points[2], lp, scene);
            Point p1 = tr.points[0].position;
            Point p2 = tr.points[1].position;
            Point p3 = tr.points[2].position;
            List<Point> points = new List<Point>();
            for (int i = 0; i < tr.points.Count; i++)
            {
                Vector<double> position = scene.projectionMatrix * scene.cam.viewMatrix * tr.points[i].position.position;
                int x = (int)(position[0] / position[3] * scene.screenWidth / 2 + scene.screenWidth / 2);
                int y = (int)(-position[1] / position[3] * scene.screenHeight / 2 + scene.screenHeight / 2);
                points.Add(new Point(x, y, position[2]));

            }
            int n = points.Count;
            PointWithIdx[] pointlist = new PointWithIdx[n];
            for (int i = 0; i < n; i++)
            {
                pointlist[i] = new PointWithIdx(points[i], i);
            }
            Array.Sort(pointlist, (x, y) => x.p.Y.CompareTo(y.p.Y));
            int yMin = (int)pointlist[0].p.Y, yMax = (int)pointlist[n - 1].p.Y;
            AET aet = new AET();
            int idx = 0;
            for (int scanY = yMin; scanY <= yMax; scanY++)
            {
                aet.DeleteEntries(scanY);
                aet.UpdateXs();
                while (pointlist[idx].p.Y < scanY)
                {
                    Point prev = points[GetPrevFromList(points.Count, pointlist[idx].idx)];
                    if (prev.Y > pointlist[idx].p.Y)
                    {
                        aet.InsertEdge(pointlist[idx].p, prev);
                    }
                    Point next = points[GetNextFromList(points.Count, pointlist[idx].idx)];
                    if (next.Y > pointlist[idx].p.Y)
                    {
                        aet.InsertEdge(pointlist[idx].p, next);
                    }
                    idx++;
                }
                if (scanY < 0)
                {
                    continue;
                }
                if (scanY > scene.screenHeight)
                {
                    break;
                }
                //
                List<int> xCoords = aet.GetX();
                int inside = 0;
                if (xCoords.Count == 0)
                {
                    continue;
                }
                int xMin = xCoords[0], xMax = xCoords[^1];
                int Xidx = 0;

                for (int x = xMin; x <= xMax; x++)
                {
                    if (Xidx < xCoords.Count)
                    {
                        while (xCoords[Xidx] < x)
                        {
                            inside = 1 - inside;
                            Xidx++;
                            if (Xidx >= xCoords.Count)
                            {
                                break;
                            }
                        }
                    }
                    if (x < 0)
                    {
                        continue;
                    }
                    if (x > scene.screenWidth)
                    {
                        x = xMax + 1;
                        continue;
                    }
                    if (inside == 1)
                    {
                        MyVector pointPosition = new MyVector(x, scanY, 0);
                        MyVector FromPointToP1 = (MyVector)points[0] - pointPosition;
                        MyVector FromPointToP2 = (MyVector)points[1] - pointPosition;
                        MyVector FromPointToP3 = (MyVector)points[2] - pointPosition;
                        double areaP1 = MyVector.PlainCrossProductValue(FromPointToP2, FromPointToP3);
                        double areaP2 = MyVector.PlainCrossProductValue(FromPointToP3, FromPointToP1);
                        double areaP3 = MyVector.PlainCrossProductValue(FromPointToP1, FromPointToP2);
                        double totalArea = areaP1 + areaP2 + areaP3;
                        areaP1 /= totalArea;
                        areaP2 /= totalArea;
                        areaP3 /= totalArea;

                        double zbuf1 = points[0].Z * (areaP1);
                        double zbuf2 = points[1].Z * (areaP2);
                        double zbuf3 = points[2].Z * (areaP3);
                        double zbufres = (zbuf1 + zbuf2 + zbuf3);

                        MyColor result = color1v1.Multiply(areaP1) + color1v2.Multiply(areaP2) + color1v3.Multiply(areaP3);
                        result = scene.ApplyFog(result, zbufres);
                        Color toFill = result.ToColor();

                        bmp.SetPixel(x, scanY, (float)zbufres, toFill);

                    }
                }
            }
            return Task.CompletedTask;
        }

        public Color PhongFillTriangle(DirectBitmap bmp, Triangle tr, Color col, Scene scene, ObjectLightProperties lp)
        {

            Point p1 = tr.points[0].position;
            Point p2 = tr.points[1].position;
            Point p3 = tr.points[2].position;
            List<Point> points = new List<Point>();
            for(int i = 0; i < tr.points.Count; i++)
            {
                Vector<double> position = scene.projectionMatrix * scene.cam.viewMatrix * tr.points[i].position.position;
                int x = (int)(position[0] / position[3] * scene.screenWidth / 2 + scene.screenWidth / 2);
                int y = (int)(-position[1] / position[3] * scene.screenHeight / 2 + scene.screenHeight / 2);
                points.Add(new Point(x, y, position[2]));

            }
            int n = points.Count;
            PointWithIdx[] pointlist = new PointWithIdx[n];
            for (int i = 0; i < n; i++)
            {
                pointlist[i] = new PointWithIdx(points[i], i);
            }
            Array.Sort(pointlist, (x, y) => x.p.Y.CompareTo(y.p.Y));
            int yMin = (int)pointlist[0].p.Y, yMax = (int)pointlist[n - 1].p.Y;
            AET aet = new AET();
            int idx = 0;
            if(yMax < 0)
            {
                //return Task.CompletedTask;
                return Color.Black;
            }
            for (int scanY = yMin; scanY <= yMax; scanY++)
            {
                aet.DeleteEntries(scanY);
                aet.UpdateXs();
                while (pointlist[idx].p.Y < scanY)
                {
                    Point prev = points[GetPrevFromList(points.Count, pointlist[idx].idx)];
                    if (prev.Y > pointlist[idx].p.Y)
                    {
                        aet.InsertEdge(pointlist[idx].p, prev);
                    }
                    Point next = points[GetNextFromList(points.Count, pointlist[idx].idx)];
                    if (next.Y > pointlist[idx].p.Y)
                    {
                        aet.InsertEdge(pointlist[idx].p, next);
                    }
                    idx++;
                }
                if (scanY < 0)
                {
                    continue;
                }
                if(scanY >= scene.screenHeight)
                {
                    break;
                }
                //
                List<int> xCoords = aet.GetX();
                int inside = 0;
                if (xCoords.Count == 0)
                {
                    continue;
                }
                int xMin = xCoords[0], xMax = xCoords[^1];
                int Xidx = 0;
                if(xMax < 0)
                {
                    continue;
                }

                for (int x = xMin; x <= xMax; x++)
                {
                    if (Xidx < xCoords.Count)
                    {
                        while (xCoords[Xidx] < x)
                        {
                            inside = 1 - inside;
                            Xidx++;
                            if (Xidx >= xCoords.Count)
                            {
                                break;
                            }
                        }
                    }
                    if (x < 0)
                    {
                        continue;
                    }
                    if (x >= scene.screenWidth)
                    {
                        x = xMax + 1;
                        continue;
                    }
                    if (inside == 1)
                    {
                        MyVector pointPosition = new MyVector(x, scanY, 0);
                        MyVector FromPointToP1 = (MyVector)points[0] - pointPosition;
                        MyVector FromPointToP2 = (MyVector)points[1] - pointPosition;
                        MyVector FromPointToP3 = (MyVector)points[2] - pointPosition;
                        double areaP1 = MyVector.PlainCrossProductValue(FromPointToP2, FromPointToP3);
                        double areaP2 = MyVector.PlainCrossProductValue(FromPointToP3, FromPointToP1);
                        double areaP3 = MyVector.PlainCrossProductValue(FromPointToP1, FromPointToP2);
                        double totalArea = areaP1 + areaP2 + areaP3;
                        areaP1 /= totalArea;
                        areaP2 /= totalArea;
                        areaP3 /= totalArea;

                        double zbuf1 = points[0].Z * (areaP1);
                        double zbuf2 = points[1].Z * (areaP2);
                        double zbuf3 = points[2].Z * (areaP3);
                        double zbufres = (zbuf1 + zbuf2 + zbuf3);

                        if (bmp.CheckZbuffer(x, scanY, zbufres) == false)
                        {
                            continue;
                        }//*/

                        double x1 = p1.X * (areaP1);
                        double x2 = p2.X * (areaP2);
                        double x3 = p3.X * (areaP3);
                        double xres = (x1 + x2 + x3);
                        double y1 = p1.Y * (areaP1);
                        double y2 = p2.Y * (areaP2);
                        double y3 = p3.Y * (areaP3);
                        double yres = (y1 + y2 + y3);
                        double z1 = p1.Z * (areaP1);
                        double z2 = p2.Z * (areaP2);
                        double z3 = p3.Z * (areaP3);
                        double zres = (z1 + z2 + z3);
                        double w1 = p1.W * (areaP1);
                        double w2 = p2.W * (areaP2);
                        double w3 = p3.W * (areaP3);
                        double wres = (w1 + w2 + w3);

                        MyColor mycol = new MyColor(0, 0, 0);
                        mycol.R += scene.abientIlumination.R * lp.mycolor.R * lp.ka;
                        mycol.G += scene.abientIlumination.G * lp.mycolor.G * lp.ka;
                        mycol.B += scene.abientIlumination.B * lp.mycolor.B * lp.ka;
                        Vector<double> normalinterpolated = (tr.points[0].normal.direction * areaP1 + tr.points[1].normal.direction * areaP2 + tr.points[2].normal.direction * areaP3).Normalize(2);
                        Vector<double> realposition = DenseVector.OfArray(new double[] { xres, yres, zres, 1 });
                        Vector<double> toCam = (scene.cam.position - realposition).Normalize(2);
                        foreach (LightSource ls in scene.lights)
                        {
                            Vector<double> toLight = (ls.position.position - realposition).Normalize(2);
                            MyColor lightintensity = ls.GetLightIntensity(-toLight);
                            double cos = Math.Max(normalinterpolated.PointwiseMultiply(toLight).Sum(), 0);
                            double mult1 = cos * lp.kd;
                            mycol.R += mult1 * lp.mycolor.R * lightintensity.R;
                            mycol.G += mult1 * lp.mycolor.G * lightintensity.G;
                            mycol.B += mult1 * lp.mycolor.B * lightintensity.B;

                            Vector<double> reflection = (normalinterpolated * 2 * cos - toLight).Normalize(2);
                            Vector<double> cosv = reflection.PointwiseMultiply(toCam);
                            //double cosr = Math.Max(reflection.PointwiseMultiply(toCam).Sum(), 0);
                            double cosr = Math.Max(cosv.Sum(), 0);
                            cosr = Math.Pow(cosr, lp.ns);
                            double mult2 = cosr * lp.ks;
                            mycol.R += mult2 * lp.mycolor.R * lightintensity.R;
                            mycol.G += mult2 * lp.mycolor.G * lightintensity.G;
                            mycol.B += mult2 * lp.mycolor.B * lightintensity.B;//*/
                        }

                        mycol = scene.ApplyFog(mycol, zbufres);
                        Color resultColor = mycol.ToColor();

                        bmp.SetPixel(x, scanY, (float)zbufres, resultColor);
                        return resultColor;

                    }
                }
            }
            //return Task.CompletedTask;
            return Color.Black;
        }
    }
}
