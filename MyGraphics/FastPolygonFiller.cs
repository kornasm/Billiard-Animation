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
    class FastPointWithIdx
    {
        public FastPoint p;
        public int idx;

        public FastPointWithIdx(FastPoint point, int index)
        {
            p = point;
            idx = index;
        }
    }
    class FastAETEntry
    {
        public int yMax;
        public double x;
        public double oneOverM;

        public FastAETEntry(int y, double xx, double onem)
        {
            yMax = y;
            x = xx;
            oneOverM = onem;
        }
    }
    class FastAET
    {
        LinkedList<FastAETEntry> entries;
        public FastAET()
        {
            entries = new LinkedList<FastAETEntry>();
        }
        public void UpdateXs()
        {
            foreach (FastAETEntry en in entries)
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
            LinkedListNode<FastAETEntry> en = entries.First;
            LinkedListNode<FastAETEntry> next;
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
        public void InsertEdge(FastPoint from, FastPoint to)
        {
            double dx = to.X - from.X;
            double dy = to.Y - from.Y;
            double oneM = dx / dy;
            FastAETEntry toInsert = new FastAETEntry((int)to.Y, from.X + oneM, oneM);
            LinkedListNode<FastAETEntry> node = entries.First;
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
            foreach (FastAETEntry en in entries)
            {
                Xs.Add((int)en.x);
            }
            return Xs;
        }
        public void Sort()
        {
            List<FastAETEntry> list = new List<FastAETEntry>(entries);
            list.Sort(Comparer<FastAETEntry>.Create((e1, e2) => e1.x.CompareTo(e2.x)));
            entries = new LinkedList<FastAETEntry>(list);
        }
    }
    class FastPolygonFiller
    {
        const double maxval = (double)255 / 256;
        //readonly static Vector ToEye = new Vector(0, 0, 1);
        static int GetNextFromList(int ContainerSize, int idx)
        {
            return idx == ContainerSize - 1 ? 0 : idx + 1;
        }
        static int GetPrevFromList(int ContiainerSize, int idx)
        {
            return idx == 0 ? ContiainerSize - 1 : idx - 1;
        }

        public Color FastPhongFillTriangle(DirectBitmap bmp, Triangle tr, Color col, Scene scene, ObjectLightProperties lp)
        {

            FastPoint p1 = new FastPoint(tr.points[0].position);
            FastPoint p2 = new FastPoint(tr.points[1].position);
            FastPoint p3 = new FastPoint(tr.points[2].position);
            FastNormalVector n1 = new FastNormalVector(tr.points[0].normal);
            FastNormalVector n2 = new FastNormalVector(tr.points[1].normal);
            FastNormalVector n3 = new FastNormalVector(tr.points[2].normal);
            List<FastPoint> points = new List<FastPoint>();
            for (int i = 0; i < tr.points.Count; i++)
            {
                Vector<double> position = scene.projectionMatrix * scene.cam.viewMatrix * tr.points[i].position.position;
                int x = (int)(position[0] / position[3] * scene.screenWidth / 2 + scene.screenWidth / 2);
                int y = (int)(-position[1] / position[3] * scene.screenHeight / 2 + scene.screenHeight / 2);
                points.Add(new FastPoint(new Point(x, y, position[2])));

            }
            int n = points.Count;
            FastPointWithIdx[] pointlist = new FastPointWithIdx[n];
            for (int i = 0; i < n; i++)
            {
                pointlist[i] = new FastPointWithIdx(points[i], i);
            }
            Array.Sort(pointlist, (x, y) => x.p.Y.CompareTo(y.p.Y));
            int yMin = (int)pointlist[0].p.Y, yMax = (int)pointlist[n - 1].p.Y;
            FastAET FastAET = new FastAET();
            int idx = 0;
            if (yMax < 0)
            {
                //return Task.CompletedTask;
                return Color.Black;
            }
            for (int scanY = yMin; scanY <= yMax; scanY++)
            {
                FastAET.DeleteEntries(scanY);
                FastAET.UpdateXs();
                while (pointlist[idx].p.Y < scanY)
                {
                    FastPoint prev = points[GetPrevFromList(points.Count, pointlist[idx].idx)];
                    if (prev.Y > pointlist[idx].p.Y)
                    {
                        FastAET.InsertEdge(pointlist[idx].p, prev);
                    }
                    FastPoint next = points[GetNextFromList(points.Count, pointlist[idx].idx)];
                    if (next.Y > pointlist[idx].p.Y)
                    {
                        FastAET.InsertEdge(pointlist[idx].p, next);
                    }
                    idx++;
                }
                if (scanY < 0)
                {
                    continue;
                }
                if (scanY >= scene.screenHeight)
                {
                    break;
                }
                //FastAET.Sort();
                List<int> xCoords = FastAET.GetX();
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
                    if (x >= scene.screenWidth)
                    {
                        x = xMax + 1;
                        continue;
                    }
                    if (inside == 1)
                    {
                        System.Numerics.Vector4 pointPosition = new System.Numerics.Vector4(x, scanY, 0, 0);
                        System.Numerics.Vector4 FromPointToP1 = points[0].position - pointPosition;
                        System.Numerics.Vector4 FromPointToP2 = points[1].position - pointPosition;
                        System.Numerics.Vector4 FromPointToP3 = points[2].position - pointPosition;
                        float areaP1 = FromPointToP2.X * FromPointToP3.Y - FromPointToP2.Y * FromPointToP3.X;
                        float areaP2 = FromPointToP3.X * FromPointToP1.Y - FromPointToP3.Y * FromPointToP1.X;
                        float areaP3 = FromPointToP1.X * FromPointToP2.Y - FromPointToP1.Y * FromPointToP2.X;
                        float totalArea = areaP1 + areaP2 + areaP3;
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
                        float xres = (float)(x1 + x2 + x3);
                        double y1 = p1.Y * (areaP1);
                        double y2 = p2.Y * (areaP2);
                        double y3 = p3.Y * (areaP3);
                        float yres = (float)(y1 + y2 + y3);
                        double z1 = p1.Z * (areaP1);
                        double z2 = p2.Z * (areaP2);
                        double z3 = p3.Z * (areaP3);
                        float zres = (float)(z1 + z2 + z3);

                        MyColor mycol = new MyColor(0, 0, 0);
                        mycol.R += scene.abientIlumination.R * lp.mycolor.R * lp.ka;
                        mycol.G += scene.abientIlumination.G * lp.mycolor.G * lp.ka;
                        mycol.B += scene.abientIlumination.B * lp.mycolor.B * lp.ka;

                        System.Numerics.Vector4 normalinterpolated = System.Numerics.Vector4.Normalize(n1.direction * areaP1 + n2.direction * areaP2 + n3.direction * areaP3);
                        System.Numerics.Vector4 realposition = new System.Numerics.Vector4(xres, yres, zres, 1);
                        System.Numerics.Vector4 toCam = System.Numerics.Vector4.Normalize(scene.cam.pos - realposition);
                        foreach (LightSource ls in scene.lights)
                        {
                            System.Numerics.Vector4 toLight = System.Numerics.Vector4.Normalize(ls.fastPosition.position - realposition);
                            MyColor lightintensity = ls.GetFastLightIntensity(-toLight);
                            //double cos = Math.Max(normalinterpolated.PointwiseMultiply(toLight).Sum(), 0);
                            System.Numerics.Vector4 multiplied = (normalinterpolated * toLight);
                            float cos = Math.Max(multiplied.X + multiplied.Y + multiplied.Z + multiplied.W, 0);
                            double mult1 = cos * lp.kd;
                            mycol.R += mult1 * lp.mycolor.R * lightintensity.R;
                            mycol.G += mult1 * lp.mycolor.G * lightintensity.G;
                            mycol.B += mult1 * lp.mycolor.B * lightintensity.B;

                            System.Numerics.Vector4 reflection = System.Numerics.Vector4.Normalize(normalinterpolated * 2 * cos - toLight);
                            System.Numerics.Vector4 cosv = reflection * toCam;
                            float cosr = Math.Max(cosv.X + cosv.Y + cosv.Z + cosv.W, 0);
                            cosr = (float)Math.Pow(cosr, lp.ns);
                            double mult2 = cosr * lp.ks;
                            mycol.R += mult2 * lp.mycolor.R * lightintensity.R;
                            mycol.G += mult2 * lp.mycolor.G * lightintensity.G;
                            mycol.B += mult2 * lp.mycolor.B * lightintensity.B;//*/
                        }

                        mycol = scene.ApplyFog(mycol, zbufres);
                        Color resultColor = mycol.ToColor();

                        bmp.SetPixel(x, scanY, (float)zbufres, resultColor);
                        //return resultColor;

                    }
                }
            }
            //return Task.CompletedTask;
            return Color.Black;
        }
    }
}
