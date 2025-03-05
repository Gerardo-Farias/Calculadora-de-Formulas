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

        public LagrangeMain()
        {
            InitializeComponent();

            // Agregar los primeros puntos x0, f(x0) y x1, f(x1) en el Grid
            AddPointToGrid("x0", "f(x0)");
            AddPointToGrid("x1", "f(x1)");
        }

        private void AddPointToGrid(string xPlaceholder, string fxPlaceholder)
        {
            // Crear las entradas
            Entry xEntry = new Entry { Placeholder = xPlaceholder, BackgroundColor = Colors.White, WidthRequest = 100 };
            Entry fxEntry = new Entry { Placeholder = fxPlaceholder, BackgroundColor = Colors.White, WidthRequest = 100 };

            // Añadir una nueva fila al Grid
            PointsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Añadir las entradas al Grid en la fila correspondiente
            PointsGrid.Children.Add(xEntry);
            Grid.SetRow(xEntry, currentRow);
            Grid.SetColumn(xEntry, 0);

            PointsGrid.Children.Add(fxEntry);
            Grid.SetRow(fxEntry, currentRow);
            Grid.SetColumn(fxEntry, 1);

            // Guardar las entradas en las listas para accederlas más tarde
            xEntries.Add(xEntry);
            fxEntries.Add(fxEntry);

            // Incrementar la fila para el siguiente punto
            currentRow++;
        }

        private void OnAddPointClicked(object sender, EventArgs e)
        {
            // Asegurarse de que hay al menos dos puntos antes de agregar más
            if (xEntries.Count >= 2)
            {
                // Usar el tamaño de la lista para determinar el nuevo índice
                int newIndex = xEntries.Count; // Esto comenzará en 2 para x2, f(x2)
                string xPlaceholder = $"x{newIndex}";
                string fxPlaceholder = $"f(x{newIndex})";
                AddPointToGrid(xPlaceholder, fxPlaceholder);
            }
        }

        private void OnRemovePointClicked(object sender, EventArgs e)
        {
            if (xEntries.Count > 2) // Asegurarse de que siempre haya al menos dos puntos
            {
                int lastRow = xEntries.Count - 1;

                // Eliminar las últimas entradas del Grid
                PointsGrid.Children.Remove(xEntries[lastRow]);
                PointsGrid.Children.Remove(fxEntries[lastRow]);
                xEntries.RemoveAt(lastRow);
                fxEntries.RemoveAt(lastRow);

                // Eliminar la última fila del Grid
                PointsGrid.RowDefinitions.RemoveAt(lastRow + 1); // +1 porque la fila se cuenta desde 0

                // Decrementar la fila actual
                currentRow--;
            }
        }

        private void OnCalculateClicked(object sender, EventArgs e)
        {
            try
            {
                List<double> xValues = new List<double>();
                List<double> fxValues = new List<double>();

                // Leer los datos de los Entrys
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
                        return;
                    }
                }

                double x = 0;
                if (!double.TryParse(XValueEntry.Text, out x))
                {
                    ResultLabel.Text = "Valor de X no válido.";
                    return;
                }

                // Realizamos la interpolación de Lagrange
                double result = LagrangeInterpolation(xValues, fxValues, x);

                ResultLabel.Text = $"Resultado: {result:F4}";
            }
            catch (Exception ex)
            {
                ResultLabel.Text = "Error en los datos.";
            }
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