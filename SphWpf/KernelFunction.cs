﻿using System;
using System.Runtime.CompilerServices;


namespace SphWpf {
  internal static class KernelFunction {
    public static double _h = 0.04d;
    static double _ad = 15d / (7d * Math.PI * _h * _h);


    public static void update() {
      _ad = 15d / (7d * Math.PI * _h * _h);
      //_ad = 1;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double kernel(in Particle self, in Particle other) {
      double xdiff = self.posX - other.posX;
      double ydiff = self.posY - other.posY;
      double dis = Math.Sqrt(xdiff * xdiff + ydiff * ydiff) / _h;

      double w;
      if (dis < 1.0d)
        w = _ad * (2d / 3d - dis * dis + 0.5 * dis * dis * dis);
      else if (dis < 2.0d)
        w = _ad * 1d / 6d * Math.Pow((2d - dis), 3);
      else w = 0;

      return w;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void kernelDerivative(in Particle self, in Particle other,
      out double dwdx, out double dwdy) {
      double h = _h;
      double xdiff = self.posX - other.posX;
      double ydiff = self.posY - other.posY;
      double rh = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);

      double dis = rh / h;
      if (rh < 1e-7 || dis >= 2.0) {
        dwdx = 0;
        dwdy = 0;
        return;
      }

      //double drdx = 2 * xdiff / (h * rh);  //partial derivative of r by x
      //double drdy = 2 * ydiff / (h * rh);  //partial derivative of r by y
      double dwdr;
      if (dis < 1.0)
        dwdr = _ad * (1.5d * dis * dis - 2d * dis);   //partial derivative of w by r
      else
        dwdr = -_ad * 0.5d * (2d - dis) * (2d - dis);
        
      //dwdx = dwdr * drdx;
      //dwdy = dwdr * drdy;
      dwdx = dwdr * 2 * xdiff / (h * rh);
      dwdy = dwdr * 2 * ydiff / (h * rh);
    }
  }
}
