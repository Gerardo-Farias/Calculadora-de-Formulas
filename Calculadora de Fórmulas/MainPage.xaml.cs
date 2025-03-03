using System;
using Microsoft.Maui.Controls;

namespace Calculadora_de_Fórmulas
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLagrangeButtonClicked(object sender, EventArgs e)
        {
            // Navega a la página de interpolación de Lagrange
            await Navigation.PushAsync(new LagrangeMain());
        }
    }
}