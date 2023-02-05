using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;



namespace SphWpf2 {
  class Particle {
    const double G = -9.8;
    const double REST_DENS = 1000d;
    const double GAS_CONST = 20;
    const double H = 0.02;
    const double HSQ = H * H;
    const double MASS = 0.001;
    const double VISC = 0;    //1000
    const double DT = 0.001;

    readonly static double POLY6 = 315d / (65d * Math.PI * Math.Pow(H, 9));
    readonly static double SPIKY_GRAD = -45d / (Math.PI * Math.Pow(H, 6));
    readonly static double VISC_LAP = 45d / (Math.PI * Math.Pow(H, 6));

    const double BOUND_DAMPING = -0.5;
    public const double VIEW_HEIGHT = 0.5;
    public const double VIEW_WIDTH = 0.5;

    public static int count = 0;

    int _id;
    public double _posX, _posY;
    double _vX, _vY = 0;
    double _pressure = 0;
    double _fX, _fY = 0;
    double _rho = REST_DENS;

    public Particle(double x, double y) {
      _id = count;
      ++count;

      _posX = x;
      _posY = y;
    }


    public void computeDensityPressure(in List<Particle> neighborList) {
      _rho = 0;
      foreach (var it in neighborList) {
        double diffX = it._posX - _posX;
        double diffY = it._posY - _posY;
        double r2 = diffX * diffX + diffY * diffY;
        if (r2 < HSQ) {
          _rho += MASS * POLY6 * Math.Pow(HSQ - r2, 3);
        }
      }
      _pressure = GAS_CONST * (_rho - REST_DENS);
    }


    public void computeForces(in List<Particle> neighborList) {
      double fPressX = 0;
      double fPressY = 0;
      double fViscX = 0;
      double fviscY = 0;

      foreach (var it in neighborList) {
        if (_id == it._id) continue;

        double diffX = it._posX - _posX;
        double diffY = it._posY - _posY;
        double r2 = diffX * diffX + diffY * diffY;

        if (r2 < HSQ) {
          double r = Math.Sqrt(r2);

          double temp = 1d / r * MASS * (_pressure + it._pressure)
            / (2d * it._rho) * SPIKY_GRAD * Math.Pow(H - r, 2);
          fPressX += -diffX * temp;
          fPressY += -diffY * temp;


          temp = VISC * MASS * 1d / it._rho * VISC_LAP * (H - r);
          fViscX += (it._vX - _vX) * temp;
          fviscY += (it._vY - _vY) * temp;
        }
      }

      _fX = fPressX + fViscX;
      _fY = fPressY + fviscY + G * _rho;
    }


    public void integrate() {
      _vX += DT * _fX / _rho;
      _vY += DT * _fY / _rho;
      _posX += DT * _vX;
      _posY += DT * _vY;

      if (_posX < 0.0) {
        _vX *= BOUND_DAMPING;
        _posX = 0;
      } else if (_posX > VIEW_WIDTH) {
        _vX *= BOUND_DAMPING;
        _posX = VIEW_WIDTH;
      } else if (_posY < 0.0) {
        _vY *= BOUND_DAMPING;
        _posY = 0;
      } else if (_posY > VIEW_HEIGHT) {
        _vY *= BOUND_DAMPING;
        _posY = VIEW_HEIGHT;
      }
    }
  }
}
