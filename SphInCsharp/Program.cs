using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plotly.NET;


namespace SphInCsharp {
  internal class Program {
    static double _gravityY = -9.8;

    static double _lowBound = 0;
    static double _upBound = 200;
    static double _leftBound = 0;
    static double _rightBound = 200;
    static double _deltaTime = 0.01;

    static List<Partical> particalList = new List<Partical>();
    static GenericChart.GenericChart chart;

    static void Main(string[] args) {
      Console.WriteLine("set particals");
      putParticals();

      for(int i = 1; i < 10000; ++i){
        Console.Write("step " + i + ", deltaTime = " + _deltaTime);
        var time_start = DateTimeOffset.Now;
        oneSetp(out double maxVelX, out double maxVelY);
        Console.WriteLine(", cost time: {0:f}, maxVelX = {1:f}, maxVelY = {2:f}",
          (DateTimeOffset.Now - time_start).TotalSeconds, maxVelX, maxVelY);


        if(i % 10 == 0) {
          drawImage();
          Console.WriteLine("press any key to continue");
          var key = Console.ReadKey();
          if (key.Key == ConsoleKey.Escape) break; 
        }
      }
    }


    static void drawImage() {
      var xList = new double[particalList.Count];
      var yList = new double[particalList.Count];
      for(int i = 0; i < particalList.Count; ++i) {
        xList[i] = particalList[i].posX;
        yList[i] = particalList[i].posY;
      }
      chart = Chart2D.Chart.Point<double, double, string>(x: xList, y: yList);
      var config = Config.init(StaticPlot: true);
      chart.WithConfig(config);
      chart.Show();
    }


    static void putParticals() {
      for (int i = 1; i < 50; ++i) {
        for (int j = 1; j < 50; ++j) {
          particalList.Add(new Partical(i, j, 0.01));
        }
      }
      //drawImage();
    }


    static void oneSetp(out double maxVelX, out double maxVelY) {
      //2. compute density and pressrue of every particals
      foreach(var point in particalList){
        double dpdt = point.computeDensity(particalList);
        point.density += dpdt * _deltaTime;
      }

      //3. compute pressrue and Viscous coefficient of every particals
      foreach (var point in particalList) {
        point.computePressrue();
        point.computeViscous(particalList);
      }

      //4. compute velocity of every particals
      maxVelX = 0;
      maxVelY = 0;
      double maxVelX__ = 0;
      double maxVelY__ = 0;
      double maxAccX = 0;
      double maxAccY = 0;
      foreach (var point in particalList){
        Tuple<double, double> ddd = point.computeVelocity(particalList);
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

        if (maxVelX__ < Math.Abs(point.velX)) {
          maxVelX__ = Math.Abs(point.velX);
          maxVelX = point.velX;
        }
        if (maxVelY__ < Math.Abs(point.velY)) {
          maxVelY__ = Math.Abs(point.velY);
          maxVelY = point.velY;
        }


        if (maxAccX < Math.Abs(dvxdt)) maxAccX = Math.Abs(dvxdt);
        if (maxAccY < Math.Abs(dvydt)) maxAccY = Math.Abs(dvydt);
        double maxAcc = Math.Sqrt(maxAccX * maxAccX + maxAccY * maxAccY);
        if (maxAcc != 0) _deltaTime = Partical._h / maxAcc;
        //if (_deltaTime > 0.05) _deltaTime = 0.05;
      }
    }








  }
}
