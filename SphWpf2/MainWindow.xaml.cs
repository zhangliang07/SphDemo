using SphWpf2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;



namespace SphWpf {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public static double _pointSize = 5;
    public static double _lowTemperature = 293.15;
    public static double _highTemperature = 373.15;
    public static int _maxStepCount = 10000;

    List<Particle> particleList = new List<Particle>();

    BackgroundWorker backgroundWorker;
    int step = 0;
    bool pause = false;
    bool oneStep = false;

    struct PointDD {
      public double X;
      public double Y;
      //public double temperature;
    }
    PointDD[] pointList;
    Ellipse[] ellipses;
    Rectangle boundary;

    Mutex drawMutex = new Mutex();
    int lastDrawTime = Environment.TickCount;
    Mutex writeMutex = new Mutex();
    int lastWriteTime = Environment.TickCount;
    string bufferText;


    static Random rand = new Random();


    public MainWindow() {
      InitializeComponent();
      backgroundWorker = new BackgroundWorker();
      backgroundWorker.WorkerReportsProgress = true;
      backgroundWorker.WorkerSupportsCancellation = true;
      backgroundWorker.DoWork += DoWork;
      backgroundWorker.ProgressChanged += ProgressChanged;
      backgroundWorker.RunWorkerCompleted += RunWorkerCompleted;

      Loaded += MainWindow_Loaded;
    }


    private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
      initParticals();
    }


    static double NormalDistribution() {
      double u1, u2, v1 = 0, v2 = 0, s = 0;
      while (s > 1 || s == 0) {
        u1 = rand.NextDouble();
        u2 = rand.NextDouble();
        v1 = 2 * u1 - 1;
        v2 = 2 * u2 - 1;
        s = v1 * v1 + v2 * v2;
      }
      return Math.Sqrt(-2 * Math.Log(s) / s) * v1;
    }


    void initParticals() {
      //put
      const double span = 14;
      Particle.count = 0;
      particleList.Clear();

      for (int i = 0; i < 10; ++i) {
        for (int j = 0; j < 10; ++j) {
          //particleList.Add(new Particle((i + 1) * span + rand.NextDouble(), (j + 1) * span));
          particleList.Add(new Particle((i + 3) * span, (j + 3) * span));
        }
      }


      step = 0;

      map.Children.Clear();
      //map.Background = Brushes.LightYellow;
      boundary = new Rectangle();
      boundary.Stroke = Brushes.Blue;
      boundary.StrokeThickness = 3;
      map.Children.Add(boundary);
      Canvas.SetLeft(boundary, 50);
      Canvas.SetBottom(boundary, 50);

      //set the particles
      pointList = new PointDD[particleList.Count];
      ellipses = new Ellipse[particleList.Count];
      for (int i = 0; i < ellipses.Length; ++i) {
        var it = new Ellipse();
        it.Stroke = new SolidColorBrush(Colors.Blue);
        it.StrokeThickness = 2;
        it.Height = _pointSize;
        it.Width = _pointSize;
        ellipses[i] = it;
        map.Children.Add(it);
      }

      for (int j = 0; j < particleList.Count; ++j) {
        Particle partical = particleList[j];

        pointList[j].X = partical._posX;
        pointList[j].Y = partical._posY;
      }
      drawParticals();

      textBox.Text = "set particals\n";
    }


    private void button_Click(object sender, RoutedEventArgs e) {
      button.IsEnabled = false;
      button1.Content = "Pause";
      button1.IsEnabled = true;
      button2.IsEnabled = false;
      pause = false;

      initParticals();

      backgroundWorker.RunWorkerAsync();
    }



    void DoWork(object sender, DoWorkEventArgs e) {
      for (; step < _maxStepCount; ++step) {
        foreach (var it in particleList) {
          it.computeDensityPressure(particleList);
        }
        foreach (var it in particleList) {
          it.computeForces(particleList);
        }
        foreach (var it in particleList) {
          it.integrate();
        }

        int time = Environment.TickCount - lastDrawTime;
        lastDrawTime = Environment.TickCount;
        if (time < 20) {
          Thread.Sleep(20 - time);
        }

        if (drawMutex.WaitOne(0)) {
          for (int j = 0; j < particleList.Count; ++j) {
            Particle partical = particleList[j];

            pointList[j].X = partical._posX;
            pointList[j].Y = partical._posY;
          }
          drawMutex.ReleaseMutex();
          backgroundWorker.ReportProgress(step, null);
        }

        if (backgroundWorker.CancellationPending || oneStep) {
          e.Cancel = true;
          oneStep= false;
          break;
        }
      }
    }


    void ProgressChanged(object sender, ProgressChangedEventArgs e) {
      if (drawMutex.WaitOne(0)) {
        drawParticals();
        drawMutex.ReleaseMutex();
      }

      if (Environment.TickCount - lastWriteTime > 500) {
        lastWriteTime = Environment.TickCount;
        writeMutex.WaitOne();
        textBox.AppendText(bufferText);
        bufferText = "";
        writeMutex.ReleaseMutex();
        textBox.ScrollToEnd();
      } else {
        writeMutex.WaitOne();
        bufferText += (string)e.UserState;
        writeMutex.ReleaseMutex();
      }
    }


    private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      button.IsEnabled = true;
      button2.IsEnabled = true;

      if (!e.Cancelled) button1.IsEnabled = false;
    }


    void drawParticals() {
      textBlock.Text = String.Format("step: {0} s", step);

      double windowH = map.ActualHeight - 100;
      double windowW = map.ActualWidth - 100;

      boundary.Width = windowW;
      boundary.Height = windowH;
      //map.Children.Clear();
      for (int i = 0; i < pointList.Length; ++i) {
        Canvas.SetLeft(ellipses[i], (pointList[i].X - 0) * windowW / Particle.VIEW_WIDTH + 50);
        Canvas.SetBottom(ellipses[i], (pointList[i].Y - 0) * windowH / Particle.VIEW_HEIGHT + 50);
      }
    }


    //https://blog.csdn.net/ls9512/article/details/50001753
    public static Color HslToRgb(float H, float S, float L) {
      float R, G, B;
      float temp1, temp2, temp3;

      if (L < 0.5f) {
        temp2 = L * (1.0f + S);
      } else {
        temp2 = L + S - L * S;
      }
      temp1 = 2.0f * L - temp2;
      H /= 360.0f;
      // R
      temp3 = H + 1.0f / 3.0f;
      if (temp3 < 0) temp3 += 1.0f;
      if (temp3 > 1) temp3 -= 1.0f;
      if (6.0 * temp3 < 1) R = temp1 + (temp2 - temp1) * 6.0f * temp3;
      else if (2.0 * temp3 < 1) R = temp2;
      else if (3.0 * temp3 < 2) R = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f;
      else R = temp1;

      // G
      temp3 = H;
      if (temp3 < 0) temp3 += 1.0f;
      if (temp3 > 1) temp3 -= 1.0f;
      if (6.0 * temp3 < 1) G = temp1 + (temp2 - temp1) * 6.0f * temp3;
      else if (2.0 * temp3 < 1) G = temp2;
      else if (3.0 * temp3 < 2) G = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f;
      else G = temp1;
      // B
      temp3 = H - 1.0f / 3.0f;
      if (temp3 < 0) temp3 += 1.0f;
      if (temp3 > 1) temp3 -= 1.0f;
      if (temp3 < 0) temp3 += 1.0f;
      if (temp3 > 1) temp3 -= 1.0f;
      if (6.0 * temp3 < 1) B = temp1 + (temp2 - temp1) * 6.0f * temp3;
      else if (2.0 * temp3 < 1) B = temp2;
      else if (3.0 * temp3 < 2) B = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - temp3) * 6.0f;
      else B = temp1;

      return Color.FromScRgb(1, R, G, B);
    }


    private void button1_Click(object sender, RoutedEventArgs e) {
      if (pause) {
        backgroundWorker.RunWorkerAsync();
        button.IsEnabled = false;
        button2.IsEnabled = false;
        button1.Content = "Pause";
        pause = false;
      } else {
        backgroundWorker.CancelAsync();
        button.IsEnabled = true;
        button2.IsEnabled = true;
        button1.Content = "Resume";
        pause = true;
      }
    }


    private void button2_Click(object sender, RoutedEventArgs e) { }


    private void button3_Click(object sender, RoutedEventArgs e) {
      oneStep = true;
      backgroundWorker.RunWorkerAsync();
    }
  }
}
