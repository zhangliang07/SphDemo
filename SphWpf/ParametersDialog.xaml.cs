using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SphWpf {
  /// <summary>
  /// Interaction logic for ParametersDialog.xaml
  /// </summary>
  public partial class ParametersDialog : Window {

    Field field = null;
    class DataItem {
      public string name { get;}
      public double value { get; set; }

      public DataItem(string name, double value) {
        this.name = name;
        this.value = value;
      }
    }
    List<DataItem> parameters = new List<DataItem>();


    public ParametersDialog(in Field field) {
      InitializeComponent();

      this.field = field;
      dataGrid.ItemsSource = parameters;
      GetParemeters();
    }


    void GetParemeters() {
      checkBox.IsChecked = field._useAverageDensity;
      parameters.Clear();
      parameters.Add(new DataItem("thread count for calculation", field._threadCount));
      parameters.Add(new DataItem("delta time between steps", field._initialDeltaTime));
      parameters.Add(new DataItem("left bound of the feild", field._leftBound));
      parameters.Add(new DataItem("right bound of the feild", field._rightBound));
      parameters.Add(new DataItem("low bound of the feild", field._lowBound));
      parameters.Add(new DataItem("up bound of the feild", field._upBound));
      parameters.Add(new DataItem("particals left position", field._pointLocationX));
      parameters.Add(new DataItem("particals right position", field._pointLocationY));
      parameters.Add(new DataItem("particals count along X", field._pointCountX));
      parameters.Add(new DataItem("particals count along Y", field._pointCountY));
      parameters.Add(new DataItem("gravity along Y", field._gravityY));
      parameters.Add(new DataItem("search radius of kenel finction", KenelFunction._h));
      parameters.Add(new DataItem("mass of each partical", Partical._initmass));
      parameters.Add(new DataItem("initial density of each partical", Partical._initDensity));
      parameters.Add(new DataItem("stiffness of the liquid, c", Partical._c));
      parameters.Add(new DataItem("coefficient 1 of normal viscosity, u1", Partical._viscosityNormal1));
      parameters.Add(new DataItem("coefficient 2 of normal viscosity, u2", Partical._viscosityNormal2));
      parameters.Add(new DataItem("coefficient 3 of shear viscosity, u3", Partical._viscosityShear1));
      parameters.Add(new DataItem("coefficient 4 of shear viscosity, u4", Partical._viscosityShear2));
      parameters.Add(new DataItem("stiffness of the boundary", field._BoundStiffness));
    }


    private void Ok_Click(object sender, RoutedEventArgs e) {
      field._useAverageDensity = (bool)checkBox.IsChecked;

      try {
        field._threadCount = (int)parameters[0].value;
        field._initialDeltaTime = parameters[1].value;
        field._leftBound = parameters[2].value;
        field._rightBound = parameters[3].value;
        field._lowBound = parameters[4].value;
        field._upBound = parameters[5].value;
        field._pointLocationX = parameters[6].value;
        field._pointLocationY = parameters[7].value;
        field._pointCountX = (int)parameters[8].value;
        field._pointCountY = (int)parameters[9].value;
        field._gravityY = parameters[10].value;
        KenelFunction._h = parameters[11].value;
        Partical._initmass = parameters[12].value;
        Partical._initDensity = parameters[13].value;
        Partical._c = parameters[14].value;
        Partical._viscosityNormal1 = parameters[15].value;
        Partical._viscosityNormal2 = parameters[16].value;
        Partical._viscosityShear1 = parameters[17].value;
        Partical._viscosityShear2 = parameters[18].value;
        field._BoundStiffness = parameters[19].value;
      } catch (Exception ex) {
        MessageBox.Show("A parameter is not a number.", "error", MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      DialogResult = true;
      Close();
    }


    private void Cancel_Click(object sender, RoutedEventArgs e) {
      DialogResult = false;
      Close();
    }
  }
}
