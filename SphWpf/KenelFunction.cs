using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SphWpf {
  internal static class KenelFunction {
    public static double _h = 0.05d;
    static double _ad = 15.0d / (7 * Math.PI * _h * _h);


    public static double kenel(in Partical self, in Partical other) {
      double xdiff = self.posX - other.posX;
      double ydiff = self.posY - other.posY;
      double dis = Math.Sqrt(xdiff * xdiff + ydiff * ydiff) / _h;

      double w = 0.0d;
      if (dis < 1.0d)
        w = _ad * (2d / 3d - dis * dis + 0.5 * dis * dis * dis);
      else if (dis < 2.0d)
        w = _ad * 1 / 6 * Math.Pow((2d - dis), 3);
      return w;
    }


    public static void kenelDerivative(in Partical self, in Partical other,
      out double dwdx, out double dwdy) {
      double xdiff = self.posX - other.posX;
      double ydiff = self.posY - other.posY;
      double rh = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
      double dis = rh / _h;

      if (rh < 1e-7 || dis > 2d) {
        dwdx = 0;
        dwdy = 0;
        return;
      }

      double drdx = 2 * xdiff / (_h * rh);  //partial derivative of r by x
      double drdy = 2 * ydiff / (_h * rh);  //partial derivative of r by y
      double dwdr = 0.0;
      if (dis < 1.0)
        dwdr = _ad * (1.5d * dis * dis - 2d * dis);   //partial derivative of w by r
      else if (dis < 2.0)
        dwdr = -_ad * 0.5d * (2d - dis) * (2d - dis);

      dwdx = dwdr * drdx;
      dwdy = dwdr * drdy;
    }
  }
}
