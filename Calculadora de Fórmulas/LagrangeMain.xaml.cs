using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Calculadora_de_Fórmulas
{
    public partial class LagrangeMain : ContentPage
    {
        private List<Entry> xEntries = new List<Entry>();
        private List<Entry> fxEntries = new List<Entry>();

        public LagrangeMain()
        {
            InitializeComponent();

            // Agregar los primeros valores (x0, f(x0)) y (x1, f(x1)) en las listas
            xEntries.Add(x0Entry);
            fxEntries.Add(fx0Entry);
            xEntries.Add(x1Entry);
            fxEntries.Add(fx1Entry);
        }

        private void OnAddPointClicked(object sender, EventArgs e)
        {
            int row = xEntries.Count + 1;

            Entry xEntry = new Entry { Placeholder = $"x{row}", BackgroundColor = Colors.White };
            Entry fxEntry = new Entry { Placeholder = $"f(x{row})", BackgroundColor = Colors.White };

            xEntries.Add(xEntry);
            fxEntries.Add(fxEntry);

            PointsGrid.Add(xEntry, 0, row);
            PointsGrid.Add(fxEntry, 1, row);
        }

        private void OnCalculateClicked(object sender, EventArgs e)
        {
            try
            {
                List<double> xValues = new List<double>();
                List<double> fxValues = new List<double>();

                for (int i = 0; i < xEntries.Count; i++)
                {
                    xValues.Add(double.Parse(xEntries[i].Text));
                    fxValues.Add(double.Parse(fxEntries[i].Text));
                }

                double x = double.Parse(x0Entry.Text); // El valor de X a interpolar
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
