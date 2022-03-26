using System;
using System.Collections.Generic;
using System.Drawing;
namespace GK_proj4
{
    public class ObjectLightProperties
    {
        public Color color;
        public MyColor mycolor;
        public double ka;
        public double kd;
        public double ks;
        public double ns; // n_shiny

        public ObjectLightProperties(MyColor col, double KA = 0.1, double KD = 0.2, double KS = 1, double NS = 20)
        {
            mycolor = col;
            ka = KA;
            kd = KD;
            ks = KS;
            ns = NS;
        }
    }
}
