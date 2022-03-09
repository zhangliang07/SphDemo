using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SphWpf {
  internal class Partical {
    public static double _c = 0.01f;
    public static double _initDensity = 1000.0f;
    public static double _viscosity = 1; // 0.001f

    public readonly int id = 0;
    public double posX = 0.0f;
    public double posY = 0.0f;
    public double velX = 0.0f;
    public double velY = 0.0f;
    public double mass = 0.001f;
    public double density = _initDensity;

    double pressure = 1000f;
    double tauXX, tauXY, tauYY, tauYX = 0.0f;


    public Partical(int id, double x, double y, double mass) {
      this.id = id;
      posX = x;
      posY = y;
      this.mass = mass;
    }


    public double computeDensity(in List<List<Partical>> neigborList) {
      double dpdt = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          Tuple<double, double> ddd = KenelFunction.kenelDerivative(this, point);
          double dwdx = ddd.Item1;
          double dwdy = ddd.Item2;
          dpdt += this.density * point.mass / point.density
           * ((this.velX - point.velX) * dwdx + (this.velY - point.velY) * dwdy);
        }
      }

      return dpdt;
    }


    public void computePressrue() {
      //double B = 32;
      //this.pressure = B * (Math.Pow(this.density / _initDensity, 7) - 1);
      this.pressure = this.density * _c * _c;
    }


    public void computeViscous(in List<List<Partical>> neigborList) {
      double dvdx = 0;
      double dvdy = 0;
      foreach (var list in neigborList) {
        foreach (var point in list) {
          Tuple<double, double> ddd = KenelFunction.kenelDerivative(this, point);
          dvdx += point.mass / point.density * point.velX * ddd.Item1;
          dvdy += point.mass / point.density * point.velY * ddd.Item2;
        }
      }


      //which one is currect?
      //this.tauXX = _viscosity * (-1/2 * (dvdx + dvdy));
      //this.tauYY = _viscosity * (-1/2 * (dvdx + dvdy));
      this.tauXX = _viscosity * (2 * dvdx - 1 / 2 * (dvdx + dvdy));
      this.tauYY = _viscosity * (2 * dvdy - 1 / 2 * (dvdx + dvdy));

      this.tauXY = _viscosity * (dvdx + dvdy);
      this.tauYX = _viscosity * (dvdx + dvdy);
    }


    public Tuple<double, double> computeVelocity(in List<List<Partical>> neigborList) {
      double dvxdt = 0;
      double dvydt = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          Tuple<double, double> ddd = KenelFunction.kenelDerivative(this, point);
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
      }

      return new Tuple<double, double>(dvxdt, dvydt);
    }
  }
}
