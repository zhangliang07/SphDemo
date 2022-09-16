using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SphWpf {
  public class Field {
    public int _threadCount = 6;
    public double _initialDeltaTime = 0.001;
    public bool _useAverageDensity = false;
    public double _stepTimeCoffient = 0.1;

    public double _gravityY = -9.8;

    public int _pointCountX = 60;
    public int _pointCountY = 60;
    public double _pointLocationX = 0.11;
    public double _pointLocationY = 0.11;

    public double _lowBorder = 0;
    public double _topBorder = 3;
    public double _leftBorder = 0;
    public double _rightBorder = 3;
    public int _wallThichnessNumber = 5;
    public double _borderStiffness = 1;
    public double _borderTemperature = 573.15;
    public double _borderThermalTransmissivity = 1e5;

    public double realTime = 0;
    double _deltaTime = 0.001;

    const double _interval = 0.02;

    ParticlesZone particalsZone;
    ParticlesZone wallParticalsZone;
    public List<Particle> particalList = new List<Particle>();
    public List<Particle> wallParticalList = new List<Particle>();

    List<List<Particle>> particalThreadList = new List<List<Particle>>();

    public Field() {
      particalsZone = new ParticlesZone(_leftBorder, _lowBorder, _rightBorder, _topBorder,
        2 * KernelFunction._h);
      wallParticalsZone = new ParticlesZone(_leftBorder, _lowBorder, _rightBorder, _topBorder,
        2 * KernelFunction._h);
    }


    public void putParticals() {
      _deltaTime = _initialDeltaTime;
      realTime = 0;
      particalList.Clear();
      Particle.totalCount = 0;

      //put particals
      for (int i = 0; i < _pointCountX; ++i) {
        for (int j = 0; j < _pointCountY; ++j) {
          particalList.Add(new Particle(i * _interval + _pointLocationX,
            j * _interval + _pointLocationY, Particle._initTemperature, false));
        }
      }

      particalThreadList.Clear();
      int count = (int)(particalList.Count / _threadCount) + 1;
      if(count < _threadCount) _threadCount = count;
      for (int i = 0; i < _threadCount; ++i) {
        int c = count;
        if (i * count + count > particalList.Count) c = particalList.Count - i * count;
        particalThreadList.Add(particalList.GetRange(i * count, c));
      }
      particalsZone.SortAllParticals(particalList);

      //put wallPartilcals
      wallParticalList.Clear();

      //top block of wall particles
      for (int i = 0; i < _wallThichnessNumber; ++i) {
        for (double currentX = _leftBorder; currentX <= _rightBorder; currentX += _interval) {
          wallParticalList.Add(new Particle(currentX, i * _interval, _borderTemperature, true));
          wallParticalList.Add(new Particle(currentX, _topBorder - (i + 1) * _interval, _borderTemperature, true));
        }
      }

      double top = _topBorder - _wallThichnessNumber * _interval;
      for (double currentY = _wallThichnessNumber * _interval; currentY < top; currentY += _interval) {
        for (int i = 0; i < _wallThichnessNumber; ++i) {
          wallParticalList.Add(new Particle(i * _interval, currentY, _borderTemperature, true));
          wallParticalList.Add(new Particle(_rightBorder - (i + 1) * _interval, currentY, _borderTemperature, true));
        }
      }
      wallParticalsZone.SortAllParticals(wallParticalList);
    }


    void computeDensity(object input) {
      List<Particle> pointList = input as List<Particle>;

      if (!_useAverageDensity) {
        foreach (var point in pointList) {
          var neighborList = particalsZone.GetNeigbors(point);
          neighborList.AddRange(wallParticalsZone.GetNeigbors(point));
          
          double dpdt = point.computeDensity(neighborList);
          point.density += dpdt * _deltaTime;
        }
      } else {
        foreach (var point in pointList) {
          var neighborList = particalsZone.GetNeigbors(point);
          neighborList.AddRange(wallParticalsZone.GetNeigbors(point));

          point.density = point.computeDensityAbsolute(neighborList);
        }
      }
    }


    void computePressrueAndViscous(object input) {
      List<Particle> pointList = input as List<Particle>;
      foreach (var point in pointList) {
        var neighborList = particalsZone.GetNeigbors(point);
        neighborList.AddRange(wallParticalsZone.GetNeigbors(point));

        point.computePressrue();
        point.computeViscousAndThermalGradient(neighborList);
        //point.computeThermalGradient(neighborList);
      }
    }


    struct MaxInfo {
      public double maxVelX;
      public double maxVelY;
      public double maxAccX;
      public double maxAccY;
      public double maxTemperature;
    }


    MaxInfo computeVelocityAndThermal(object input) {
      List<Particle> pointList = input as List<Particle>;

      MaxInfo maxInfo = new MaxInfo {
        maxVelX = 0,
        maxVelY = 0,
        maxAccX = 0,
        maxAccY = 0,
        maxTemperature = 0
      };

      double maxVelXAbs = 0;
      double maxVelYAbs = 0;
      double maxAccXAbs = 0;
      double maxAccYAbs = 0;
      foreach (var point in pointList) {
        var neighborList = particalsZone.GetNeigbors(point);
        neighborList.AddRange(wallParticalsZone.GetNeigbors(point));

        point.computeVelocityAndThermal(neighborList,
          out double dvxdt, out double dvydt, out double dTdt);
        dvydt += _gravityY;

        point.newPosX = point.posX + (point.velX + 0.5 * dvxdt * _deltaTime) * _deltaTime;
        point.newPosY = point.posY + (point.velY + 0.5 * dvydt * _deltaTime) * _deltaTime;
        point.velX += dvxdt * _deltaTime;
        point.velY += dvydt * _deltaTime;

        point.temperature += dTdt * _deltaTime;

        //checkBoundary(point);

        if (maxVelXAbs < Math.Abs(point.velX)) {
          maxVelXAbs = Math.Abs(point.velX);
          maxInfo.maxVelX = point.velX;
        }
        if (maxVelYAbs < Math.Abs(point.velY)) {
          maxVelYAbs = Math.Abs(point.velY);
          maxInfo.maxVelY = point.velY;
        }
        if (maxAccXAbs < Math.Abs(dvxdt)) {
          maxAccXAbs = Math.Abs(dvxdt);
          maxInfo.maxAccX = dvxdt;
        }
        if (maxAccYAbs < Math.Abs(dvydt)) {
          maxAccYAbs = Math.Abs(dvydt);
          maxInfo.maxAccY = dvydt;
        }
        if (maxInfo.maxTemperature < Math.Abs(point.temperature)) {
          maxInfo.maxTemperature = Math.Abs(point.temperature);
        }
      }

      return maxInfo;
    }


    public void updatePosition(object input) {
      List<Particle> pointList = input as List<Particle>;
      foreach (var point in pointList) {
        point.posX = point.newPosX;
        point.posY = point.newPosY;
      }
    }



    bool checkBoundary(Particle point) {
      bool result = false;

      //check boundary
      if (point.newPosX < _leftBorder) {
        point.newPosX = _leftBorder;
        if (point.velX < 0) point.velX = -point.velX;
        result = true;
      } else if (point.newPosX > _rightBorder) {
        point.newPosX = _rightBorder;
        if (point.velX > 0) point.velX = -point.velX;
        result = true;
      }

      if (point.newPosY < _lowBorder) {
        point.newPosY = _lowBorder;
        if (point.velY < 0) point.velY = -point.velY;
        result = true;
      } else if (point.newPosY > _topBorder) {;
        point.newPosY = _topBorder;
        if (point.velY > 0) point.velY = -point.velY;
        result = true;
      }

      if (result) {
        double dTdt = (_borderTemperature - point.temperature)
          * _borderThermalTransmissivity / (Particle._heatCapacity * point.density);
        point.temperature += dTdt * _deltaTime;
      }

      return result;
    }


    bool checkBoundary222(Particle point) {
      bool result = false;

      //check boundary
      if (point.newPosX < _leftBorder) {
        double newVelX2 = point.velX + _borderStiffness * (_leftBorder - point.newPosX) * _deltaTime;
        point.newPosX += (point.velX + newVelX2) / 2 * _deltaTime;
        point.velX = newVelX2;
        result = true;
      } else if (point.newPosX > _rightBorder) {
        double newVelX2 = point.velX + _borderStiffness * (_rightBorder - point.newPosX) * _deltaTime;
        point.newPosX += (point.velX + newVelX2) / 2 * _deltaTime;
        point.velX = newVelX2;
        result = true;
      }

      if (point.newPosY < _lowBorder) {
        double newVelY2 = point.velY + _borderStiffness * (_lowBorder - point.newPosY) * _deltaTime;
        point.newPosY += (point.velY + newVelY2) / 2 * _deltaTime;
        point.velY = newVelY2;
        result = true;
      } else if (point.newPosY > _topBorder) {
        double newVelY2 = point.velY + _borderStiffness * (_topBorder - point.newPosY) * _deltaTime;
        point.newPosY += (point.velY + newVelY2) / 2 * _deltaTime;
        point.velY = newVelY2;
        result = true;
      }

      if (result) {
        double dTdt = (_borderTemperature - point.temperature)
          * _borderThermalTransmissivity / (Particle._heatCapacity * point.density);
        point.temperature += dTdt * _deltaTime;
      }

      return result;
    }


    public string oneSetp() {
      Task[] taskList = new Task[particalThreadList.Count];
      //2. compute density and pressrue of every particals
      var action = new Action<object>(computeDensity);
      for (int i = 0; i < particalThreadList.Count; ++i) {
        taskList[i] = Task.Factory.StartNew((Action<object>)action, particalThreadList[i]);
      }
      Task.WaitAll(taskList);

      //3. compute pressrue and Viscous coefficient of every particals
      action = new Action<object>(computePressrueAndViscous);
      for (int i = 0; i < particalThreadList.Count; ++i) {
        taskList[i] = Task.Factory.StartNew((Action<object>)action, particalThreadList[i]);
      }
      Task.WaitAll(taskList);

      //4. compute velocity of every particals
      Task<MaxInfo>[] taskList2 = new Task<MaxInfo>[particalThreadList.Count];
      var fun = new Func<object, MaxInfo>(computeVelocityAndThermal);
      for (int i = 0; i < particalThreadList.Count; ++i) {
        taskList2[i] = Task.Factory.StartNew((Func<object, MaxInfo>)fun, particalThreadList[i]);
      }
      Task.WaitAll(taskList2);

      MaxInfo maxVelInfo = new MaxInfo {
        maxVelX = 0, maxVelY = 0, maxAccX = 0, maxAccY = 0, maxTemperature = 0 };
      for (int i = 0; i < particalThreadList.Count; ++i) {
        var info = taskList2[i].Result;
        if (Math.Abs(info.maxVelX) > Math.Abs(maxVelInfo.maxVelX)) maxVelInfo.maxVelX = info.maxVelX;
        if (Math.Abs(info.maxVelY) > Math.Abs(maxVelInfo.maxVelY)) maxVelInfo.maxVelY = info.maxVelY;
        if (Math.Abs(info.maxAccX) > Math.Abs(maxVelInfo.maxAccX)) maxVelInfo.maxAccX = info.maxAccX;
        if (Math.Abs(info.maxAccY) > Math.Abs(maxVelInfo.maxAccY)) maxVelInfo.maxAccY = info.maxAccY;
        if (info.maxTemperature > maxVelInfo.maxTemperature) maxVelInfo.maxTemperature = info.maxTemperature;
      }

      action = new Action<object>(updatePosition);
      for (int i = 0; i < particalThreadList.Count; ++i) {
        taskList[i] = Task.Factory.StartNew((Action<object>)action, particalThreadList[i]);
      }
      Task.WaitAll(taskList);


      particalsZone.SortAllParticals(particalList);

      //abjective h
      //double maxAcc = Math.Sqrt(maxVelInfo.maxAccX * maxVelInfo.maxAccX
      //  + maxVelInfo.maxAccY * maxVelInfo.maxAccY);
      //if (maxAcc != 0) _deltaTime = _stepTimeCoffient * Math.Sqrt(KenelFunction._h / maxAcc);
      //if (_deltaTime > 0.05) _deltaTime = 0.05;
      realTime += _deltaTime;

      return string.Format("∆t={0:g6}, maxVelX={1:g6}, maxVelY={2:g6}," +
        " maxAccX={3:g6}, maxAccY={4:g6}, maxT={5:g6}",
        _deltaTime, maxVelInfo.maxVelX, maxVelInfo.maxVelY,
        maxVelInfo.maxAccX, maxVelInfo.maxAccY, maxVelInfo.maxTemperature);
    }
  }
}
