using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace SphWpf {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    double pointSize = 5;
    BackgroundWorker backgroundWorker;
    MainLoop mainLoop = null;
    int step = 0;
    bool pause = false;

    struct PointDD {
      public double X;
      public double Y;
    }
    PointDD[] pointList;
    Ellipse[] ellipses;
    Rectangle boundary;


    public MainWindow() {
      InitializeComponent();
      backgroundWorker = new BackgroundWorker();
      backgroundWorker.WorkerReportsProgress = true;
      backgroundWorker.WorkerSupportsCancellation = true;
      backgroundWorker.DoWork += DoWork;
      backgroundWorker.ProgressChanged += ProgressChanged;

      intiParticals();
    }


    void intiParticals() {
      step = 1;
      mainLoop = new MainLoop();
      mainLoop.putParticals();

      map.Children.Clear();
      map.Background = Brushes.LightGreen;
      boundary = new Rectangle();
      boundary.Stroke = Brushes.Red;
      boundary.StrokeThickness = 3;
      map.Children.Add(boundary);
      pointList = new PointDD[mainLoop.pointCountX * mainLoop.pointCountY];
      ellipses = new Ellipse[mainLoop.pointCountX * mainLoop.pointCountY];
      for (int i = 0; i < ellipses.Length; ++i) {
        var it = new Ellipse();
        it.Stroke = Brushes.Blue;
        it.StrokeThickness = 2;
        it.Height = pointSize;
        it.Width = pointSize;
        ellipses[i] = it;
        map.Children.Add(it);
      }

      var particalList = mainLoop.particalList;
      for (int j = 0; j < particalList.Count; ++j) {
        pointList[j].X = particalList[j].posX;
        pointList[j].Y = particalList[j].posY;
      }
      drawParticals();
      listBox.Items.Add("set particals");
    }


    private void button_Click(object sender, RoutedEventArgs e) {
      button.IsEnabled = false;
      button2.IsEnabled = false;
      pause = false;

      backgroundWorker.RunWorkerAsync();
    }



    void DoWork(object sender, DoWorkEventArgs e) {
      for (; step < 1000; ++step) {
        var time_start = DateTimeOffset.Now;
        string info = mainLoop.oneSetp();
        info = string.Format("step {0:d}, cost time: {1:g}, ",
          step, (DateTimeOffset.Now - time_start).TotalSeconds) + info;

        var particalList = mainLoop.particalList;
        if (step % 1 == 0) {
          for (int j = 0; j < particalList.Count; ++j) {
            pointList[j].X = particalList[j].posX;
            pointList[j].Y = particalList[j].posY;
          }
          backgroundWorker.ReportProgress(step, info);
        }

        if (backgroundWorker.CancellationPending) break;
      }
    }


    void ProgressChanged(object sender, ProgressChangedEventArgs e) {
      listBox.Items.Add((string)e.UserState);
      listBox.ScrollIntoView(listBox.Items[listBox.Items.Count -1]);

      drawParticals();
    }


    void drawParticals() {
      double height = mainLoop._upBound - mainLoop._lowBound;
      double weight = mainLoop._rightBound - mainLoop._leftBound;
      double windowH = map.ActualHeight;
      double windowW = map.ActualWidth;

      boundary.Width = windowW;
      boundary.Height = windowH;
      //map.Children.Clear();
      for (int i = 0; i < pointList.Length; ++i) {
        Canvas.SetLeft(ellipses[i], pointList[i].X * windowW / weight);
        Canvas.SetBottom(ellipses[i], pointList[i].Y * windowH / height);
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
      Parameters parameters = new Parameters();
      parameters.ShowDialog();
    }
  }
}
