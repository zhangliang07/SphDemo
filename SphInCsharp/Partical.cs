using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SphInCsharp {
  internal class Partical {
    public static double _h = 10.0f;
    static double _ad = 15.0f / (7 * Math.PI * _h * _h);

    public static double _c = 10f;
    public static double _initDensity = 1000.0f;
    public static double _viscosity = 1e10; // 0.001f
    public static double _gravityY = -9.8f;

    public double posX = 0.0f;
    public double posY = 0.0f;
    public double velX = 0.0f;
    public double velY = 0.0f;
    public double mass = 100.0f;
    public double density = _initDensity;

    double pressure = 100f;
    double tauXX, tauXY, tauYY, tauYX = 0.0f;


    public Partical(double x, double y, double mass) {
      posX = x;
      posY = y;
      this.mass = mass;
    }


    double kenel(in Partical other) {
      double xdiff = this.posX - other.posX;
      double ydiff = this.posY - other.posY;
      double dis = Math.Sqrt(xdiff * xdiff + ydiff * ydiff) / _h;

      double w = 0.0f;
      if (dis < 1.0f)
        w = _ad * (2 / 3 - dis * dis + 0.5 * dis * dis * dis);
      else if (dis < 2.0f)
        w = _ad * 1 / 6 * Math.Pow((2 - dis), 3);
      return w;
    }


    Tuple<double, double> kenelDerivative(in Partical other) {
      double xdiff = this.posX - other.posX;
      double ydiff = this.posY - other.posY;
      double rh = Math.Sqrt(xdiff * xdiff + ydiff * ydiff);
      double dis = rh / _h;

      if (rh == 0.0) 
        return new Tuple<double, double>(0, 0);

      if(dis > 2)
        return new Tuple<double, double>(0, 0);

      double drdx = 2 * (this.posX - other.posX) / (_h * rh);  //partial derivative of r by x
      double drdy = 2 * (this.posY - other.posY) / (_h * rh);  //partial derivative of r by y
      double dwdr = 0.0;
      if (dis < 1)
        dwdr = _ad * (3 / 2 * dis * dis - 2 * dis);   //partial derivative of w by r
      if (dis < 2)
        dwdr = -_ad * 1 / 2 * (2 - dis) * (2 - dis);

      double dwdx = dwdr * drdx;
      double dwdy = dwdr * drdy;
      return new Tuple<double, double>(dwdx, dwdy);
    }


    public double computeDensity(in List<Partical> neigborList) {
      double dpdt = 0;

      foreach (var point in neigborList) {
        Tuple<double, double> ddd = kenelDerivative(point);
        double dwdx = ddd.Item1;
        double dwdy = ddd.Item2;
        dpdt += this.density * point.mass / point.density
         * ((this.velX - point.velX) * dwdx + (this.velY - point.velY) * dwdy);
      }

      return dpdt;
    }


    public void computePressrue() {
      double B = 32;
      //this.pressure = B * (Math.Pow(this.density / _initDensity, 7) - 1);
      this.pressure = this.density * _c * _c;
    }


    public void computeViscous(in List<Partical> neigborList) {
      double dvdx = 0;
      double dvdy = 0;
      foreach(var point in neigborList) {
        Tuple<double, double> ddd = kenelDerivative(point);
        dvdx += point.mass / point.density * point.velX * ddd.Item1;
        dvdy += point.mass / point.density * point.velY * ddd.Item2;
      }


      //which one is currect?
      this.tauXX = _viscosity * (-2 / 3 * (dvdx + dvdy));
      this.tauYY = _viscosity * (-2 / 3 * (dvdx + dvdy));
      //this.tauXX = _viscosity * (2 * dvdx - 2 / 3 * (dvdx + dvdy));
      //this.tauYY = _viscosity * (2 * dvdy - 2 / 3 * (dvdx + dvdy));

      this.tauXY = _viscosity * (dvdx + dvdy);
      this.tauYX = _viscosity * (dvdx + dvdy);
    }


    public Tuple<double, double> computeVelocity(in List<Partical> neigborList) {
      double dvxdt = 0;
      double dvydt = 0;

      foreach(var point in neigborList) {
        Tuple<double, double> ddd = kenelDerivative(point);
        double dwdx = ddd.Item1;
        double dwdy = ddd.Item2;
        double temp = point.density / (this.density * point.density);

        dvxdt += -temp * (this.pressure + point.pressure) * dwdx;
        dvxdt += temp * (this.tauXX + point.tauXX) * dwdx;
        dvxdt += temp * (this.tauXY + point.tauXY) * dwdy;

        dvydt += -temp * (this.pressure + point.pressure) * dwdy;
        dvydt += temp * (this.tauYY + point.tauYY) * dwdy;
        dvydt += temp * (this.tauYX + point.tauYX) * dwdx;
      }

      return new Tuple<double, double>(dvxdt, dvydt);
    }
  }
}
