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

    BackgroundWorker backgroundWorker;
    Field mainLoop = new Field();
    int step = 0;
    bool pause = false;

    struct PointDD {
      public double X;
      public double Y;
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

      boundary = new Rectangle();
      boundary.Stroke = Brushes.Red;
      boundary.StrokeThickness = 3;
      map.Children.Add(boundary);
      Canvas.SetLeft(boundary, 50);
      Canvas.SetBottom(boundary, 50);

      pointList = new PointDD[mainLoop._pointCountX * mainLoop._pointCountY];
      ellipses = new Ellipse[mainLoop._pointCountX * mainLoop._pointCountY];
      for (int i = 0; i < ellipses.Length; ++i) {
        var it = new Ellipse();
        it.Stroke = Brushes.Blue;
        it.StrokeThickness = 2;
        it.Height = _pointSize;
        it.Width = _pointSize;
        ellipses[i] = it;
        map.Children.Add(it);
      }

      var particalList = mainLoop.particalList;
      for (int j = 0; j < particalList.Count; ++j) {
        pointList[j].X = particalList[j].posX;
        pointList[j].Y = particalList[j].posY;
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
            pointList[j].X = particalList[j].posX;
            pointList[j].Y = particalList[j].posY;
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

      double height = mainLoop._upBound - mainLoop._lowBound;
      double weight = mainLoop._rightBound - mainLoop._leftBound;
      double windowH = map.ActualHeight - 100;
      double windowW = map.ActualWidth - 100;

      boundary.Width = windowW;
      boundary.Height = windowH;
      //map.Children.Clear();
      for (int i = 0; i < pointList.Length; ++i) {
        Canvas.SetLeft(ellipses[i], (pointList[i].X - mainLoop._leftBound) * windowW / weight + 50);
        Canvas.SetBottom(ellipses[i], (pointList[i].Y - mainLoop._lowBound) * windowH / height + 50);
      }
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
