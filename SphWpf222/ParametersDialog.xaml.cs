using System;
using System.Collections.Generic;
using System.Windows;

namespace SphWpf {
  /// <summary>
  /// Interaction logic for ParametersDialog.xaml
  /// </summary>
  public partial class ParametersDialog : Window {
    const double _KelvinTemperature = 273.15;


    Field field = null;
    class DataItem {
      public string name { get; }
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
      parameters.Add(new DataItem("delta time between steps (s)", field._initialDeltaTime));
      parameters.Add(new DataItem("left border of the field (m)", field._leftBorder));
      parameters.Add(new DataItem("right border of the field (m)", field._rightBorder));
      parameters.Add(new DataItem("low border of the field (m)", field._lowBorder));
      parameters.Add(new DataItem("up border of the field (m)", field._upBorder));
      parameters.Add(new DataItem("particles left position (m)", field._pointLocationX));
      parameters.Add(new DataItem("particles right position (m)", field._pointLocationY));
      parameters.Add(new DataItem("particles count along X", field._pointCountX));
      parameters.Add(new DataItem("particles count along Y", field._pointCountY));
      parameters.Add(new DataItem("gravity along Y (m/s^2)", field._gravityY));
      parameters.Add(new DataItem("search radius of kernel finction (m), h", KernelFunction._h));
      parameters.Add(new DataItem("mass of each particle (kg)", Particle._initmass));
      parameters.Add(new DataItem("initial density of each particle (kg/m^2)", Particle._initDensity));
      parameters.Add(new DataItem("stiffness of the liquid, c", Particle._c_stiffness));
      parameters.Add(new DataItem("coefficient 1 of normal viscosity, u1", Particle._viscosityNormal1));
      parameters.Add(new DataItem("coefficient 2 of normal viscosity, u2", Particle._viscosityNormal2));
      parameters.Add(new DataItem("coefficient 3 of shear viscosity, u3", Particle._viscosityShear1));
      parameters.Add(new DataItem("coefficient 4 of shear viscosity, u4", Particle._viscosityShear2));
      parameters.Add(new DataItem("initial temperature of each particle (℃)", Particle._initTemperature - _KelvinTemperature));
      parameters.Add(new DataItem("heat capacity of the liquid (J/(kg*K)), C", Particle._heatCapacity));
      parameters.Add(new DataItem("thermal transmissivity of the liquid (W/(m*K)), λ", Particle._thermalTransmissivity));
      parameters.Add(new DataItem("stiffness of the borders (N/m)", field._borderStiffness));
      parameters.Add(new DataItem("temperature of the borders (℃)", field._borderTemperature - _KelvinTemperature));
      parameters.Add(new DataItem("thermal transmissivity of the borders (W/(s*K))", field._borderThermalTransmissivity));
      parameters.Add(new DataItem("particle size on screen", MainWindow._pointSize));
      parameters.Add(new DataItem("lower temperature on screen for blue (℃)", MainWindow._lowTemperature - _KelvinTemperature));
      parameters.Add(new DataItem("higher temperature on screen for red (℃)", MainWindow._highTemperature - _KelvinTemperature));
    }


    private void Ok_Click(object sender, RoutedEventArgs e) {
      field._useAverageDensity = (bool)checkBox.IsChecked;
      try {
        if (parameters[0].value < 1) parameters[0].value = 1;
        field._threadCount = (int)parameters[0].value;

        field._initialDeltaTime = parameters[1].value;
        field._leftBorder = parameters[2].value;
        field._rightBorder = parameters[3].value;
        field._lowBorder = parameters[4].value;
        field._upBorder = parameters[5].value;
        field._pointLocationX = parameters[6].value;
        field._pointLocationY = parameters[7].value;
        field._pointCountX = (int)parameters[8].value;
        field._pointCountY = (int)parameters[9].value;
        field._gravityY = parameters[10].value;
        KernelFunction._h = parameters[11].value;
        Particle._initmass = parameters[12].value;
        Particle._initDensity = parameters[13].value;
        Particle._c_stiffness = parameters[14].value;
        Particle._viscosityNormal1 = parameters[15].value;
        Particle._viscosityNormal2 = parameters[16].value;
        Particle._viscosityShear1 = parameters[17].value;
        Particle._viscosityShear2 = parameters[18].value;
        Particle._initTemperature = parameters[19].value + _KelvinTemperature;
        Particle._heatCapacity = parameters[20].value;
        Particle._thermalTransmissivity = parameters[21].value;
        field._borderStiffness = parameters[22].value;
        field._borderTemperature = parameters[23].value + _KelvinTemperature;
        field._borderThermalTransmissivity = parameters[24].value;

        if (parameters[20].value < 1) parameters[25].value = 1;
        MainWindow._pointSize = parameters[25].value;
        MainWindow._lowTemperature = parameters[26].value + _KelvinTemperature;
        MainWindow._highTemperature= parameters[27].value + _KelvinTemperature;
      } catch (Exception) {
        MessageBox.Show("A parameter is not a number.", "error",
          MessageBoxButton.OK, MessageBoxImage.Error);
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
