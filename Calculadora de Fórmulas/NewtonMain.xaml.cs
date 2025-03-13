using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Calculadora_de_Fórmulas
{
    public partial class NewtonMain : ContentPage
    {
        private List<Entry> xEntries = new List<Entry>();
        private List<Entry> fxEntries = new List<Entry>();
        private int currentRow = 1;
        private Dictionary<Entry, string> lastValidValues = new Dictionary<Entry, string>();

        public NewtonMain()
        {
            InitializeComponent();
            InitializeGrid();
            BindTextChangedEvents();
        }

        private void InitializeGrid()
        {
            // Agregar los primeros puntos x0, f(x0) y x1, f(x1) en el Grid
            AddPointToGrid("x0", "f(x0)");
            AddPointToGrid("x1", "f(x1)");
        }

        private void BindTextChangedEvents()
        {
            XValueEntry.TextChanged += OnEntryTextChanged;
            RealValueEntry.TextChanged += OnEntryTextChanged;
        }

        private void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                string newText = entry.Text;
                if (string.IsNullOrEmpty(newText) || newText.All(c => char.IsDigit(c) || c == '.'))
                {
                    return;
                }
                entry.Text = e.OldTextValue;
            }
        }

        private void AddPointToGrid(string xPlaceholder, string fxPlaceholder)
        {
            var xEntry = CreateEntry(xPlaceholder);
            var fxEntry = CreateEntry(fxPlaceholder);
            xEntry.TextChanged += OnEntryTextChanged;
            fxEntry.TextChanged += OnEntryTextChanged;
            AddControlsToGrid(xEntry, fxEntry);
            xEntries.Add(xEntry);
            fxEntries.Add(fxEntry);
            lastValidValues[xEntry] = "";
            lastValidValues[fxEntry] = "";
            currentRow++;
        }

        private Entry CreateEntry(string placeholder)
        {
            return new Entry
            {
                Placeholder = placeholder,
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                WidthRequest = 100,
                HorizontalOptions = LayoutOptions.Center
            };
        }

        private void AddControlsToGrid(Entry xEntry, Entry fxEntry)
        {
            PointsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            PointsGrid.Children.Add(xEntry);
            Grid.SetRow(xEntry, currentRow);
            Grid.SetColumn(xEntry, 0);
            PointsGrid.Children.Add(fxEntry);
            Grid.SetRow(fxEntry, currentRow);
            Grid.SetColumn(fxEntry, 1);
        }

        private void OnAddPointClicked(object sender, EventArgs e)
        {
            if (xEntries.Count >= 2)
            {
                int newIndex = xEntries.Count;
                AddPointToGrid($"x{newIndex}", $"f(x{newIndex})");
            }
        }

        private void OnRemovePointClicked(object sender, EventArgs e)
        {
            if (xEntries.Count > 2)
            {
                RemoveLastPointFromGrid();
            }
        }

        private void RemoveLastPointFromGrid()
        {
            int lastRow = xEntries.Count - 1;
            PointsGrid.Children.Remove(xEntries[lastRow]);
            PointsGrid.Children.Remove(fxEntries[lastRow]);
            xEntries.RemoveAt(lastRow);
            fxEntries.RemoveAt(lastRow);
            PointsGrid.RowDefinitions.RemoveAt(lastRow + 1);
            currentRow--;
        }

        private void OnCalculateClicked(object sender, EventArgs e)
        {
            try
            {
                List<double> xValues = new List<double>();
                List<double> fxValues = new List<double>();

                if (!ValidateEntries(xValues, fxValues))
                    return;

                if (!double.TryParse(XValueEntry.Text, out double x))
                {
                    ResultLabel.Text = "Valor de X no válido.";
                    return;
                }

                double result = NewtonInterpolation(xValues, fxValues, x);
                string resultText = FormatResult(result);

                if (double.TryParse(RealValueEntry.Text, out double realValue))
                {
                    // Calcular el error porcentual con la fórmula ajustada para permitir un valor negativo
                    double errorPercentage = ((realValue - result) / realValue) * 100;
                    resultText += $", Error porcentual = {errorPercentage:F10}%";
                }

                ResultLabel.Text = resultText;
            }
            catch
            {
                ResultLabel.Text = "Error en los datos.";
            }
        }

        private bool ValidateEntries(List<double> xValues, List<double> fxValues)
        {
            for (int i = 0; i < xEntries.Count; i++)
            {
                if (double.TryParse(xEntries[i].Text, out double xVal) && double.TryParse(fxEntries[i].Text, out double fxVal))
                {
                    xValues.Add(xVal);
                    fxValues.Add(fxVal);
                }
                else
                {
                    ResultLabel.Text = "Por favor, ingrese valores numéricos válidos.";
                    return false;
                }
            }
            return true;
        }

        private string FormatResult(double result)
        {
            return $"P(X) = {result:F10}";
        }

        private double NewtonInterpolation(List<double> x, List<double> fx, double X)
        {
            int n = x.Count;
            double result = fx[0];
            double term;

            // Crear un arreglo para las diferencias divididas
            List<List<double>> divDif = new List<List<double>>();

            // Inicializamos el primer término de las diferencias divididas
            divDif.Add(new List<double>(fx));

            // Calculamos las diferencias divididas
            for (int i = 1; i < n; i++)
            {
                List<double> newDivDiff = new List<double>();
                for (int j = 0; j < n - i; j++)
                {
                    double diff = (divDif[i - 1][j + 1] - divDif[i - 1][j]) / (x[j + i] - x[j]);
                    newDivDiff.Add(diff);
                }
                divDif.Add(newDivDiff);
            }

            // Ahora aplicamos las diferencias divididas en el polinomio de Newton
            for (int i = 1; i < n; i++)
            {
                term = divDif[i][0];
                for (int j = 0; j < i; j++)
                {
                    term *= (X - x[j]);
                }
                result += term;
            }

            return result;
        }
    }
}
