using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SphWpf {
  internal class Partical {
    public static double _c = 0.01f;
    public static double _initDensity = 1000.0f;
    public static double _viscosityNormal1 = 1;
    public static double _viscosityNormal2 = -1;
    public static double _viscosityShear1 = 1;
    public static double _viscosityShear2 = 1;

    public readonly int id = 0;
    public double posX = 0.0f;
    public double posY = 0.0f;
    public double velX = 0.0f;
    public double velY = 0.0f;
    public double mass = 0.001f;
    public double density = _initDensity;

    double pressure = 0f;
    double tauXX, tauXY, tauYY, tauYX = 0.0f;


    public Partical(int id, double x, double y, double mass) {
      this.id = id;
      posX = x;
      posY = y;
      this.mass = mass;
    }


    public double computeDensityAbsolute(in List<List<Partical>> neigborList) {
      double totalMaxx = 0;
      double totalVolumn = 0;
      foreach (var list in neigborList) {
        foreach (var point in list) {
          if (this.id == point.id) continue;

          double w = KenelFunction.kenel(this, point);
          totalMaxx += point.mass * w;
          totalVolumn += point.mass / point.density * w;
        }
      }

      if (totalVolumn < 1e-7) return this.density;
      return totalMaxx / totalVolumn;
    }


    public double computeDensity(in List<List<Partical>> neigborList) {
      double dpdt = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          if (this.id == point.id) continue;

          KenelFunction.kenelDerivative(this, point, out double dwdx, out double dwdy);
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
      double dVx_diff_dx = 0;
      double dVx_diff_dy = 0;
      double dVy_diff_dx = 0;
      double dVy_diff_dy = 0;
      foreach (var list in neigborList) {
        foreach (var point in list) {
          if (this.id == point.id) continue;

          KenelFunction.kenelDerivative(this, point, out double dwdx, out double dwdy);
          double vxdiff = point.mass / point.density * (point.velX - this.velX);
          double vydiff = point.mass / point.density * (point.velY - this.velY);
          dVx_diff_dx += vxdiff * dwdx;
          dVx_diff_dy += vxdiff * dwdy;
          dVy_diff_dx += vydiff * dwdx;
          dVy_diff_dy += vydiff * dwdy;
        }
      }

      //which one is currect?
      this.tauXX = _viscosityNormal1 * dVx_diff_dx + _viscosityNormal2 * dVy_diff_dx;
      this.tauYY = _viscosityNormal1 * dVy_diff_dy + _viscosityNormal2 * dVx_diff_dy;

      this.tauXY = _viscosityShear1 * dVx_diff_dy + _viscosityShear2 * dVy_diff_dy;
      this.tauYX = _viscosityShear1 * dVy_diff_dx + _viscosityShear2 * dVx_diff_dx;
    }


    public void computeVelocity(in List<List<Partical>> neigborList,
      out double dvxdt, out double dvydt) {
      dvxdt = 0;
      dvydt = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          if (this.id == point.id) continue;

          KenelFunction.kenelDerivative(this, point, out double dwdx, out double dwdy);
          double temp = point.density / (this.density * point.density);

          dvxdt += -temp * (this.pressure + point.pressure) * dwdx;
          dvxdt += temp * (this.tauXX + point.tauXX) * dwdx;
          dvxdt += temp * (this.tauXY + point.tauXY) * dwdy;

          dvydt += -temp * (this.pressure + point.pressure) * dwdy;
          dvydt += temp * (this.tauYY + point.tauYY) * dwdy;
          dvydt += temp * (this.tauYX + point.tauYX) * dwdx;
        }
      }
    }
  }
}
