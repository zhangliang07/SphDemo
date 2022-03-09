using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SphWpf {
  internal static class KenelFunction {
    public static double _h = 0.05f;
    static double _ad = 15.0f / (7 * Math.PI * _h * _h);


    public static double kenel(in Partical self, in Partical other) {
      double xdiff = self.posX - other.posX;
      double ydiff = self.posY - other.posY;
      double dis = Math.Sqrt(xdiff * xdiff + ydiff * ydiff) / _h;

      double w = 0.0f;
      if (dis < 1.0f)
        w = _ad * (2 / 3 - dis * dis + 0.5 * dis * dis * dis);
      else if (dis < 2.0f)
        w = _ad * 1 / 6 * Math.Pow((2 - dis), 3);
      return w;
    }


    public static Tuple<double, double> kenelDerivative(in Partical self, in Partical other) {
      double xdiff = self.posX - other.posX;
      double ydiff = self.posY - other.posY;
      double rh = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
      double dis = rh / _h;

      if (rh == 0.0)
        return new Tuple<double, double>(0, 0);

      if (dis > 2)
        return new Tuple<double, double>(0, 0);

      double drdx = 2 * (self.posX - other.posX) / (_h * rh);  //partial derivative of r by x
      double drdy = 2 * (self.posY - other.posY) / (_h * rh);  //partial derivative of r by y
      double dwdr = 0.0;
      if (dis < 1)
        dwdr = _ad * (3 / 2 * dis * dis - 2 * dis);   //partial derivative of w by r
      else if (dis < 2)
        dwdr = -_ad * 1 / 2 * (2 - dis) * (2 - dis);

      double dwdx = dwdr * drdx;
      double dwdy = dwdr * drdy;
      return new Tuple<double, double>(dwdx, dwdy);
    }
  }
}
