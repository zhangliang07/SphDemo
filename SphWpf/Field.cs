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
    public double _pointLocationX = 0.01;
    public double _pointLocationY = 0.01;

    public double _lowBound = 0;
    public double _upBound = 2.0;
    public double _leftBound = 0;
    public double _rightBound = 2.0;
    public double _BoundStiffness = 1e5;

    public double realTime = 0;
    double _deltaTime = 0.001;

    ParticalsZone particalsZone;
    public List<Partical> particalList = new List<Partical>();

    List<List<Partical>> particalThreadList = new List<List<Partical>>();

    public Field() {
      particalsZone = new ParticalsZone(_leftBound, _lowBound, _rightBound, _upBound,
        2 * KenelFunction._h);
    }


    public Field(int threadCount,
      double leftBound, double lowBound,
      double rightBound, double upBound, double h,
      double gravityY, double pointSize, int pointCountX, int pointCountY,
      double initDensity, double mass, double pressure, 
      double c, double viscosity,
      double deltaTime, bool adjectiveDeltaTime = true) {
      this._threadCount = threadCount;
      particalsZone = new ParticalsZone(leftBound, lowBound, rightBound, upBound, h);
    }


    public void putParticals() {
      _deltaTime = _initialDeltaTime;
      realTime = 0;
      particalList.Clear();

      int id = 1;
      for (int i = 0; i < _pointCountX; ++i) {
        for (int j = 0; j < _pointCountY; ++j) {
          particalList.Add(new Partical(id, i * 0.01 + _pointLocationX,
            j  * 0.01 + _pointLocationY));
          ++id;
        }
      }

      particalThreadList.Clear();
      int count = (int)(particalList.Count / _threadCount) + 1;
      for (int i = 0; i < _threadCount; ++i) {
        int c = count;
        if (i * count + count > particalList.Count) c = particalList.Count - i * count;
        particalThreadList.Add(particalList.GetRange(i * count, c));
      }

      particalsZone.SortAllParticals(particalList);
    }




    void computeDensity(object input) {
      List<Partical> pointList = input as List<Partical>;

      if (!_useAverageDensity) {
        foreach (var point in pointList) {
          var neighborList = particalsZone.GetNeigbors(point);
          double dpdt = point.computeDensity(neighborList);
          point.density += dpdt * _deltaTime;
        }
      } else {
        foreach (var point in pointList) {
          var neighborList = particalsZone.GetNeigbors(point);
          point.density = point.computeDensityAbsolute(neighborList);
        }
      }
    }


    void computePressrueAndViscous(object input) {
      List<Partical> pointList = input as List<Partical>;
      foreach (var point in pointList) {
        var neighborList = particalsZone.GetNeigbors(point);
        point.computePressrue();
        point.computeViscous(neighborList);
      }
    }


    struct MaxVelInfo {
      public double maxVelX;
      public double maxVelY;
      public double maxAccX;
      public double maxAccY;
    }


    MaxVelInfo computeVelocity(object input) {
      List<Partical> pointList = input as List<Partical>;

      double maxVelXAbs = 0;
      double maxVelYAbs = 0;
      double maxVelX = 0;
      double maxVelY = 0;
      double maxAccXAbs = 0;
      double maxAccYAbs = 0;
      double maxAccX = 0;
      double maxAccY = 0;
      foreach (var point in pointList) {
        var neighborList = particalsZone.GetNeigbors(point);
        point.computeVelocity(neighborList, out double dvxdt, out double dvydt);
        dvydt += _gravityY;

        point.velX = point.velX + dvxdt * _deltaTime;
        point.velY = point.velY + dvydt * _deltaTime;
        point.posX = point.posX + (point.velX + 0.5 * dvxdt * _deltaTime) * _deltaTime;
        point.posY = point.posY + (point.velY + 0.5 * dvydt * _deltaTime) * _deltaTime;

        checkBoundary(ref point.velX, ref point.velY, ref point.posX, ref point.posY);

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

      return new MaxVelInfo {
        maxVelX = maxVelX, maxVelY = maxVelY, maxAccX = maxAccX, maxAccY = maxAccY
      };
    }



    bool checkBoundary(ref double velX, ref double velY, ref double posX, ref double posY) {
      bool result = false;

      //check boundary
      if (posX < _leftBound) {
        //newPosX = _leftBound;
        //if (newVelX < 0) newVelX = -newVelX;
        double newVelX2 = velX + _BoundStiffness * (_leftBound - posX) * _deltaTime;
        posX += (velX + newVelX2) / 2 * _deltaTime;
        velX = newVelX2;
        result = true;
      } else if (posX > _rightBound) {
        //newPosX = _rightBound;
        //if (newVelX > 0) newVelX = -newVelX;
        double newVelX2 = velX + _BoundStiffness * (_rightBound - posX) * _deltaTime;
        posX += (velX + newVelX2) / 2 * _deltaTime;
        velX = newVelX2;
        result = true;
      }

      if (posY < _lowBound) {
        //newPosY = _lowBound;
        //if (newVelY < 0) newVelY = -newVelY;
        double newVelY2 = velY + _BoundStiffness * (_lowBound - posY) * _deltaTime;
        posY += (velY + newVelY2) / 2 * _deltaTime;
        velY = newVelY2;
        result = true;
      } else if (posY > _upBound) {
        //newPosY = _upBound;
        //if (newVelY > 0) newVelY = -newVelY;
        double newVelY2 = velY + _BoundStiffness * (_upBound - posY) * _deltaTime;
        posY += (velY + newVelY2) / 2 * _deltaTime;
        velY = newVelY2;
        result = true;
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
      Task<MaxVelInfo>[] taskList2 = new Task<MaxVelInfo>[particalThreadList.Count];
      var fun = new Func<object, MaxVelInfo>(computeVelocity);
      for (int i = 0; i < particalThreadList.Count; ++i) {
        taskList2[i] = Task.Factory.StartNew((Func<object, MaxVelInfo>)fun, particalThreadList[i]);
      }
      Task.WaitAll(taskList2);

      MaxVelInfo maxVelInfo = new MaxVelInfo{maxVelX = 0, maxVelY = 0, maxAccX = 0, maxAccY = 0};
      for (int i = 0; i < particalThreadList.Count; ++i) {
        var info = taskList2[i].Result;
        if (Math.Abs(info.maxVelX) > Math.Abs(maxVelInfo.maxVelX)) maxVelInfo.maxVelX = info.maxVelX;
        if (Math.Abs(info.maxVelY) > Math.Abs(maxVelInfo.maxVelY)) maxVelInfo.maxVelY = info.maxVelY;
        if (Math.Abs(info.maxAccX) > Math.Abs(maxVelInfo.maxAccX)) maxVelInfo.maxAccX = info.maxAccX;
        if (Math.Abs(info.maxAccY) > Math.Abs(maxVelInfo.maxAccY)) maxVelInfo.maxAccY = info.maxAccY;
      }

      particalsZone.SortAllParticals(particalList);

      //double maxAcc = Math.Sqrt(maxVelInfo.maxAccX * maxVelInfo.maxAccX
      //  + maxVelInfo.maxAccY * maxVelInfo.maxAccY);
      //if (maxAcc != 0) _deltaTime = _stepTimeCoffient * Math.Sqrt(KenelFunction._h / maxAcc);
      //if (_deltaTime > 0.05) _deltaTime = 0.05;
      realTime += _deltaTime;

      return string.Format("deltaTime = {0:g}, maxVelX = {1:g}, maxVelY = {2:g}, maxAccX = {3:g}, maxAccY = {4:g}",
        _deltaTime, maxVelInfo.maxVelX, maxVelInfo.maxVelY, maxVelInfo.maxAccX, maxVelInfo.maxAccY);
    }
  }
}
