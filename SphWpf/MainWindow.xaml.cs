using System;
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

    BackgroundWorker backgroundWorker;
    Field mainLoop = new Field();
    int step = 0;
    bool pause = false;

    struct PointDD {
      public double X;
      public double Y;
      public double temperature;
    }
    PointDD[] pointList;
    Ellipse[] ellipses;
    Rectangle boundary;

    Mutex drawMutex = new Mutex();
    int lastDrawTime = Environment.TickCount;
    Mutex writeMutex = new Mutex();
    int lastWriteTime = Environment.TickCount;
    string bufferText;


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


    void initParticals() {
      step = 1;
      mainLoop.putParticals();

      map.Children.Clear();
      //map.Background = Brushes.LightYellow;
      double hue = 240.0 * (1.0 - (mainLoop._borderTemperature- _lowTemperature)
        / (_highTemperature - _lowTemperature));
      if (hue > 240) hue = 240;
      if (hue < 0) hue = 0;

      boundary = new Rectangle();
      boundary.Stroke = new SolidColorBrush(HslToRgb((float)hue, 1.0f, 0.4f));
      boundary.StrokeThickness = 3;
      map.Children.Add(boundary);
      Canvas.SetLeft(boundary, 50);
      Canvas.SetBottom(boundary, 50);

      pointList = new PointDD[mainLoop._pointCountX * mainLoop._pointCountY];
      ellipses = new Ellipse[mainLoop._pointCountX * mainLoop._pointCountY];
      for (int i = 0; i < ellipses.Length; ++i) {
        var it = new Ellipse();
        it.Stroke = new SolidColorBrush(Colors.Blue);
        it.StrokeThickness = 2;
        it.Height = _pointSize;
        it.Width = _pointSize;
        ellipses[i] = it;
        map.Children.Add(it);
      }

      var particalList = mainLoop.particalList;
      for (int j = 0; j < particalList.Count; ++j) {
        Particle partical = particalList[j];

        pointList[j].X = partical.posX;
        pointList[j].Y = partical.posY;
        pointList[j].temperature = partical.temperature;
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
      for (; step < 10000; ++step) {
        string info = mainLoop.oneSetp();

        int time = Environment.TickCount - lastDrawTime;
        lastDrawTime = Environment.TickCount;
        if (time < 20) {
          Thread.Sleep(20 - time);
        }

        if (drawMutex.WaitOne(0)) {
          info = string.Format("\nstep {0:d}, cost time: {1:g}, ",
            step, 0.001 * time) + info;
          var particalList = mainLoop.particalList;
          for (int j = 0; j < particalList.Count; ++j) {
            Particle partical = particalList[j];

            pointList[j].X = partical.posX;
            pointList[j].Y = partical.posY;
            pointList[j].temperature = partical.temperature;
          }
          drawMutex.ReleaseMutex();
          backgroundWorker.ReportProgress(step, info);
        }

        if (backgroundWorker.CancellationPending) {
          e.Cancel = true;
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
      textBlock.Text = String.Format("real time: {0:g8} s", mainLoop.realTime);

      double height = mainLoop._upBorder - mainLoop._lowBorder;
      double weight = mainLoop._rightBorder - mainLoop._leftBorder;
      double windowH = map.ActualHeight - 100;
      double windowW = map.ActualWidth - 100;

      boundary.Width = windowW;
      boundary.Height = windowH;
      //map.Children.Clear();
      for (int i = 0; i < pointList.Length; ++i) {
        double hue = 240.0 * (1.0 - (pointList[i].temperature - _lowTemperature) / (_highTemperature - _lowTemperature));
        if (hue > 240) hue = 240;
        if (hue < 0) hue = 0;
        ((SolidColorBrush)ellipses[i].Stroke).Color = HslToRgb((float)hue, 1.0f, 0.4f);
        Canvas.SetLeft(ellipses[i], (pointList[i].X - mainLoop._leftBorder) * windowW / weight + 50);
        Canvas.SetBottom(ellipses[i], (pointList[i].Y - mainLoop._lowBorder) * windowH / height + 50);
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


    private void button2_Click(object sender, RoutedEventArgs e) {
      ParametersDialog parameters = new ParametersDialog(mainLoop);
      if ((bool)parameters.ShowDialog()) {
        initParticals();
      }
    }
  }
}
