using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace SphWpf {
  internal class ParticalsZone {
    List<Partical>[,] zones;

    readonly double _lowBound;
    readonly double _upBound;
    readonly double _leftBound;
    readonly double _rightBound;
    readonly double _h;

    readonly int xiMax = 0;
    readonly int yiMax = 0;

    public ParticalsZone(double leftBound, double lowBound,
      double rightBound, double upBound, double h) {
      _leftBound = leftBound;
      _lowBound = lowBound;
      _rightBound = rightBound;
      _upBound = upBound;
      _h = h;

      xiMax = (int)((rightBound - leftBound) / h) + 1;
      yiMax = (int)((upBound - lowBound) / h) + 1;
      zones = new List<Partical>[xiMax, yiMax];
      for(int i = 0; i < xiMax; ++i) {
        for (int j = 0; j < yiMax; ++j) {
          zones[i, j] = new List<Partical>();
        }
      }
    }


    public void SortAllParticals(in List<Partical> particalList) {
      foreach (var it in zones) {
        it.Clear();
      }

      foreach (var point in particalList) {
        int xi = (int)((point.posX - _leftBound) / _h);
        int yi = (int)((point.posY - _lowBound) / _h);
        if (xi <0) xi = 0;
        if (xi >= xiMax) xi = xiMax - 1;
        if (yi < 0) yi = 0;
        if (yi >= yiMax) yi = yiMax - 1;
        zones[xi, yi].Add(point);
      }
    }


    public List<List<Partical>> GetNeigbors(in Partical partical) {
      int xi = (int)((partical.posX - _leftBound) / _h);
      int yi = (int)((partical.posY - _lowBound) / _h);

      List<List<Partical>> list = new List<List<Partical>>();

      int x = xi;
      int y = yi;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      x = xi - 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      x = xi + 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      x = xi;
      y = yi - 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      y = yi + 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      x = xi - 1;
      y = yi - 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      x = xi + 1;
      y = yi - 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      x = xi - 1;
      y = yi + 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      x = xi + 1;
      y = yi + 1;
      if (x >= 0 && x < xiMax && y >= 0 && y < yiMax) list.Add(zones[x, y]);

      return list;
    }


    void computeLocation(in Partical partical, out int xi, out int yi) {
      xi = (int)((partical.posX - _leftBound) / _h);
      yi = (int)((partical.posY - _lowBound) / _h);
    }
  }
}
