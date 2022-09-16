﻿using System;
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
    const double GAS_CONST = 2000;
    public const double H = 16;
    const double HSQ = H * H;
    const double MASS = 65;
    const double VISC = 250;
    const double DT = 0.0008;

    readonly static double POLY6 = 315d / (65d * Math.PI * Math.Pow(H, 9));
    readonly static double SPIKY_GRAD = -45d / (Math.PI * Math.Pow(H, 6));
    readonly static double VISC_LAP = 45d / (Math.PI * Math.Pow(H, 6));

    const double EPS = H;
    const double BOUND_DAMPING = -0.5;
    public const double VIEW_HEIGHT = 800;
    public const double VIEW_WIDTH = 1200;

    static int count = 0;

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
        _pressure = GAS_CONST * (_rho - REST_DENS);
      }
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

          double temp = 1d / r * MASS * (_pressure + it._pressure) / (2d * it._rho) * SPIKY_GRAD * Math.Pow(H - r, 2);
          fPressX += -diffX * temp;
          fPressX += -diffY * temp;

          temp = VISC * MASS * 1d / it._rho * VISC_LAP * (H - r);
          fViscX += diffX * temp;
          fviscY += diffY * temp;
        }
      }

      _fX = fPressX + fViscX;
      _fY = fPressY + fviscY + G * _rho;
    }


    public void integrate(in List<Particle> neighborList) {
      foreach (var it in neighborList) {
        _vX += DT * _fX / _rho;
        _vY += DT * _fY / _rho;
        _posX += DT * _vX;
        _posY += DT * _vY;

        if (_posX - H < 0.0) {
          _vX *= BOUND_DAMPING;
          _posX = H;
        } else if (_posX + H > VIEW_WIDTH) {
          _vX *= BOUND_DAMPING;
          _posX = VIEW_WIDTH - H;
        } else if (_posY - H < 0.0) {
          _vY *= BOUND_DAMPING;
          _vY += H;
          _posY = H;
        } else if (_posY + H > VIEW_HEIGHT) {
          _vY *= BOUND_DAMPING;
          _vY += H;
          _posY = VIEW_HEIGHT - H;
        }
      }
    }


    public void update(in List<Particle> neighborList) {
      computeDensityPressure(neighborList);
      computeForces(neighborList);
      integrate(neighborList);
    }
  }
}
