using System.Collections.Generic;



namespace SphWpf {
  public class Particle {
    public static double _c_stiffness = 10d;
    public static double _initmass = 0.001d;
    public static double _initDensity = 1000.0d;
    public static double _viscosityNormal1 = 1e6;
    public static double _viscosityNormal2 = -1e6;
    public static double _viscosityShear1 = 1e6;
    public static double _viscosityShear2 = 1e6;
    public static double _initTemperature = 293.15;
    public static double _heatCapacity = 4200;
    public static double _thermalTransmissivity = 0.59e9;

    public readonly int id = 0;
    public double posX = 0.0;
    public double posY = 0.0;
    public double velX = 0.0;
    public double velY = 0.0;
    public double density = _initDensity;
    public double temperature = _initTemperature;

    readonly double mass = 0.001d;

    double pressure = 0f;
    double tauXX, tauXY, tauYY, tauYX = 0.0;
    double temperatureGradientX, temperatureGradientY = 0.0;


    public Particle(int id, double x, double y) {
      this.id = id;
      posX = x;
      posY = y;
      this.mass = _initmass;
    }


    public double computeDensityAbsolute(in List<List<Particle>> neigborList) {
      double totalMaxx = 0;
      double totalVolumn = 0;
      foreach (var list in neigborList) {
        foreach (var point in list) {
          //if (this.id == point.id) continue;

          double w = KernelFunction.kernel(this, point);
          totalMaxx += point.mass * w;
          totalVolumn += point.mass / point.density * w;
        }
      }

      if (totalVolumn < 1e-7) return this.density;
      return totalMaxx / totalVolumn;
    }


    public double computeDensity(in List<List<Particle>> neigborList) {
      double dpdt = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          //if (this.id == point.id) continue;

          KernelFunction.kernelDerivative(this, point, out double dwdx, out double dwdy);
          dpdt += - this.density * point.mass / point.density
           * ((point.velX - this.velX) * dwdx + (point.velY - this.velY) * dwdy);
        }
      }

      return dpdt;
    }


    public void computePressrue() {
      //double B = 32;
      //this.pressure = B * (Math.Pow(this.density / _initDensity, 7) - 1);
      this.pressure = this.density * _c_stiffness * _c_stiffness;
    }


    public void computeViscousAndThermalGradient(in List<List<Particle>> neigborList) {
      double dVx_diff_dx = 0;
      double dVx_diff_dy = 0;
      double dVy_diff_dx = 0;
      double dVy_diff_dy = 0;

      double dT_diff_dx = 0;
      double dT_diff_dy = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          //if (this.id == point.id) continue;
          double temp = point.mass / point.density;

          KernelFunction.kernelDerivative(this, point, out double dwdx, out double dwdy);
          double vxdiff = temp * (point.velX - this.velX);
          double vydiff = temp * (point.velY - this.velY);
          dVx_diff_dx += vxdiff * dwdx;
          dVx_diff_dy += vxdiff * dwdy;
          dVy_diff_dx += vydiff * dwdx;
          dVy_diff_dy += vydiff * dwdy;

          double vTdiff = temp * (point.temperature - this.temperature);
          dT_diff_dx += vTdiff * dwdx;
          dT_diff_dy += vTdiff * dwdy;
        }
      }

      //which parameters is currect?
      this.tauXX = _viscosityNormal1 * dVx_diff_dx + _viscosityNormal2 * dVy_diff_dx;
      this.tauYY = _viscosityNormal1 * dVy_diff_dy + _viscosityNormal2 * dVx_diff_dy;

      this.tauXY = _viscosityShear1 * dVx_diff_dy + _viscosityShear2 * dVy_diff_dy;
      this.tauYX = _viscosityShear1 * dVy_diff_dx + _viscosityShear2 * dVx_diff_dx;

      this.temperatureGradientX = dT_diff_dx;
      this.temperatureGradientY = dT_diff_dy;
    }


    public void computeVelocityAndThermal(in List<List<Particle>> neigborList,
      out double dvxdt, out double dvydt, out double dTdt) {
      dvxdt = 0;
      dvydt = 0;
      dTdt = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          //if (this.id == point.id) continue;

          KernelFunction.kernelDerivative(this, point, out double dwdx, out double dwdy);
          double temp = point.mass / (this.density * point.density);

          dvxdt += -temp * (this.pressure + point.pressure) * dwdx;
          dvxdt += temp * (this.tauXX + point.tauXX) * dwdx;
          dvxdt += temp * (this.tauXY + point.tauXY) * dwdy;

          dvydt += -temp * (this.pressure + point.pressure) * dwdy;
          dvydt += temp * (this.tauYY + point.tauYY) * dwdy;
          dvydt += temp * (this.tauYX + point.tauYX) * dwdx;

          temp = temp * _thermalTransmissivity / _heatCapacity;
          dTdt += temp * (this.temperatureGradientX + point.temperatureGradientX) * dwdx;
          dTdt += temp * (this.temperatureGradientY + point.temperatureGradientY) * dwdy;
        }
      }
    }


    public void computeThermalGradient(in List<List<Particle>> neigborList) {
      double dT_diff_dx = 0;
      double dT_diff_dy = 0;
      foreach (var list in neigborList) {
        foreach (var point in list) {
          //if (this.id == point.id) continue;

          KernelFunction.kernelDerivative(this, point, out double dwdx, out double dwdy);
          double vTdiff = point.mass / point.density * (point.temperature - this.temperature);
          dT_diff_dx += vTdiff * dwdx;
          dT_diff_dy += vTdiff * dwdy;
        }
      }

      this.temperatureGradientX = dT_diff_dx;
      this.temperatureGradientY = dT_diff_dy;
    }


    public double computeThermal(in List<List<Particle>> neigborList) {
      double dTdt = 0;

      foreach (var list in neigborList) {
        foreach (var point in list) {
          //if (this.id == point.id) continue;

          KernelFunction.kernelDerivative(this, point, out double dwdx, out double dwdy);
          double temp = point.mass / (this.density * point.density) * _thermalTransmissivity / _heatCapacity;

          dTdt += temp * (this.temperatureGradientX + point.temperatureGradientX) * dwdx;
          dTdt += temp * (this.temperatureGradientY + point.temperatureGradientY) * dwdy;
        }
      }

      return dTdt;
    }
  }
}
