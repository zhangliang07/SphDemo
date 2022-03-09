using System;
using System.Collections.Generic;



namespace SphWpf {
  internal class MainLoop {
    static readonly double _gravityY = -9.8;

    public readonly int pointCountX = 50;
    public readonly int pointCountY = 100;
    public readonly double _lowBound = 0;
    public readonly double _upBound = 2.0;
    public readonly double _leftBound = 0;
    public readonly double _rightBound = 2.0;
    double _deltaTime = 0.001;

    ParticalsZone particalsZone;
    public List<Partical> particalList = new List<Partical>();
    int step = 0;


    public MainLoop() {
      particalsZone = new ParticalsZone(_leftBound, _lowBound, _rightBound, _upBound, KenelFunction._h);
    }


    public MainLoop(double leftBound, double lowBound,
      double rightBound, double upBound, double h,
      double gravityY, double pointSize, int pointCountX, int pointCountY,
      double initDensity, double mass, double pressure, 
      double c, double viscosity,
      double deltaTime, bool adjectiveDeltaTime = true) {
      particalsZone = new ParticalsZone(leftBound, lowBound, rightBound, upBound, h);
    }


    public void putParticals() {
      int id = 1;
      for (int i = 1; i < pointCountX; ++i) {
        for (int j = 1; j < pointCountY; ++j) {
          particalList.Add(new Partical(id, _leftBound + i * 0.01, _lowBound+ j * 0.01, 0.001));
          ++id;
        }
      }

      particalsZone.SortAllParticals(particalList);
    }


    public string oneSetp() {
      //2. compute density and pressrue of every particals
      foreach (var point in particalList) {
        var neighborList = particalsZone.GetNeigbors(point);
        double dpdt = point.computeDensity(neighborList);
        point.density += dpdt * _deltaTime;
      }

      //3. compute pressrue and Viscous coefficient of every particals
      foreach (var point in particalList) {
        var neighborList = particalsZone.GetNeigbors(point);
        point.computePressrue();
        point.computeViscous(neighborList);
      }

      //4. compute velocity of every particals
      double maxVelXAbs = 0;
      double maxVelYAbs = 0;
      double maxVelX = 0;
      double maxVelY = 0;
      double maxAccXAbs = 0;
      double maxAccYAbs = 0;
      double maxAccX = 0;
      double maxAccY = 0;
      foreach (var point in particalList) {
        var neighborList = particalsZone.GetNeigbors(point);
        Tuple<double, double> ddd = point.computeVelocity(neighborList);
        double dvxdt = ddd.Item1;
        double dvydt = ddd.Item2 + _gravityY;

        double newVelX = point.velX + dvxdt * _deltaTime;
        double newVelY = point.velY + dvydt * _deltaTime;
        double newPosX = point.posX + (newVelX + point.velX) / 2 * _deltaTime;
        double newPosY = point.posY + (newVelY + point.velY) / 2 * _deltaTime;

        //check boundary
        if (newPosX < _leftBound) {
          newPosX = _leftBound;
          if (newVelX < 0) newVelX = -newVelX;
        } else if (newPosX > _rightBound) {
          newPosX = _rightBound;
          if (newVelX > 0) newVelX = -newVelX;
        }

        if (newPosY > _upBound) {
          newPosY = _upBound;
          if (newVelY > 0) newVelY = -newVelY;
        } else if (newPosY < _lowBound) {
          newPosY = _lowBound;
          if (newVelY < 0) newVelY = -newVelY;
        }

        point.posX = newPosX;
        point.posY = newPosY;
        point.velX = newVelX;
        point.velY = newVelY;

        if (maxVelXAbs < Math.Abs(point.velX)) {
          maxVelXAbs = Math.Abs(point.velX);
          maxVelX = point.velX;
        }
        if (maxVelYAbs < Math.Abs(point.velY)) {
          maxVelYAbs = Math.Abs(point.velY);
          maxVelY = point.velY;
        }
        if (maxAccXAbs < Math.Abs(dvxdt)) {
          maxAccXAbs = Math.Abs(dvxdt);
          maxAccX = dvxdt;
        }
        if (maxAccYAbs < Math.Abs(dvydt)) {
          maxAccYAbs = Math.Abs(dvydt);
          maxAccY = dvydt;
        }
      }

      particalsZone.SortAllParticals(particalList);

      double maxAcc = Math.Sqrt(maxAccX * maxAccX + maxAccY * maxAccY);
      if (maxAcc != 0) _deltaTime = Math.Sqrt(KenelFunction._h / maxAcc);
      //if (_deltaTime > 0.05) _deltaTime = 0.05;

      ++step;
      return string.Format("deltaTime = {0:g}, maxVelX = {1:g}, maxVelY = {2:g}, maxAccX = {3:g}, maxAccY = {4:g}",
        _deltaTime, maxVelX, maxVelY, maxAccX, maxAccY);
    }
  }
}
