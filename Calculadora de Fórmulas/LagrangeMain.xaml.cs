using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Calculadora_de_Fórmulas
{
    public partial class LagrangeMain : ContentPage
    {
        private List<Entry> xEntries = new List<Entry>();
        private List<Entry> fxEntries = new List<Entry>();
        private int currentRow = 1; // Comenzamos desde la fila 1 (después de las etiquetas)
        private Dictionary<Entry, string> lastValidValues = new Dictionary<Entry, string>();

        public LagrangeMain()
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
            // Vincular eventos de validación a las entradas
            XValueEntry.TextChanged += OnEntryTextChanged;
            RealValueEntry.TextChanged += OnEntryTextChanged;
        }

        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is Entry entry)
            {
                // Si el campo está vacío, no restauramos el valor anterior
                if (string.IsNullOrEmpty(entry.Text))
                {
                    lastValidValues[entry] = "";
                    return;
                }

                // Si el valor ingresado es un número válido, guardamos ese valor
                if (double.TryParse(entry.Text, out _))
                {
                    lastValidValues[entry] = entry.Text; // Guardar el último valor válido
                }
                else
                {
                    // Si no es válido, restauramos el valor anterior
                    entry.Text = lastValidValues[entry];
                }
            }
        }

        private void AddPointToGrid(string xPlaceholder, string fxPlaceholder)
        {
            // Crear las entradas para x y f(x)
            var xEntry = CreateEntry(xPlaceholder);
            var fxEntry = CreateEntry(fxPlaceholder);

            // Asociar eventos de cambio de texto
            xEntry.TextChanged += OnEntryTextChanged;
            fxEntry.TextChanged += OnEntryTextChanged;

            // Añadir los controles al Grid
            AddControlsToGrid(xEntry, fxEntry);

            // Guardar las referencias de las entradas
            xEntries.Add(xEntry);
            fxEntries.Add(fxEntry);

            // Inicializar los valores válidos
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

            // Añadir las entradas al Grid
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
                // Agregar un nuevo punto x, f(x)
                int newIndex = xEntries.Count;
                AddPointToGrid($"x{newIndex}", $"f(x{newIndex})");
            }
        }

        private void OnRemovePointClicked(object sender, EventArgs e)
        {
            if (xEntries.Count > 2)
            {
                // Eliminar el último punto x, f(x)
                RemoveLastPointFromGrid();
            }
        }

        private void RemoveLastPointFromGrid()
        {
            int lastRow = xEntries.Count - 1;

            // Eliminar las entradas del Grid
            PointsGrid.Children.Remove(xEntries[lastRow]);
            PointsGrid.Children.Remove(fxEntries[lastRow]);

            // Eliminar las referencias de las listas
            xEntries.RemoveAt(lastRow);
            fxEntries.RemoveAt(lastRow);

            // Eliminar la definición de la fila
            PointsGrid.RowDefinitions.RemoveAt(lastRow + 1);

            currentRow--;
        }

        private void OnCalculateClicked(object sender, EventArgs e)
        {
            try
            {
                List<double> xValues = new List<double>();
                List<double> fxValues = new List<double>();

                // Validar los valores de X y f(X)
                if (!ValidateEntries(xValues, fxValues))
                    return;

                // Validar el valor de X para la interpolación
                if (!double.TryParse(XValueEntry.Text, out double x))
                {
                    ResultLabel.Text = "Valor de X no válido.";
                    return;
                }

                // Realizar la interpolación de Lagrange
                double result = LagrangeInterpolation(xValues, fxValues, x);
                string resultText = FormatResult(result);

                // Si se proporcionó un valor real, calcular el error porcentual
                if (double.TryParse(RealValueEntry.Text, out double realValue))
                {
                    double errorPercentage = Math.Abs((realValue - result) / realValue) * 100;
                    resultText += $", Error porcentual: {errorPercentage:F10}%";
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
                    ResultLabel.Text = "Por favor ingrese valores numéricos válidos.";
                    return false;
                }
            }
            return true;
        }

        private string FormatResult(double result)
        {
            string polynomialDegree = $"P{Math.Max(1, xEntries.Count - 1)}";
            return $"{polynomialDegree} es: {result:F10}";
        }

        private double LagrangeInterpolation(List<double> x, List<double> fx, double X)
        {
            double result = 0;
            int n = x.Count;

            for (int i = 0; i < n; i++)
            {
                double term = fx[i];
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        term *= (X - x[j]) / (x[i] - x[j]);
                    }
                }
                result += term;
            }
            return result;
        }
    }
}
