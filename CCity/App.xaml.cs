using CCity.Model;
using CCity.View;
using CCity.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CCity
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private MainModel _model;
        //private MainViewModel _viewModel;
        private MainWindow _mainWindow;
        private UserControl _startupWindow;
        private UserControl _gameWindow;

        App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            //_model = new();
            //_viewModel = new(_model);
            _mainWindow = new();
            _startupWindow = new StartupWindow();
            _gameWindow = new GameWindow();

            //_mainWindow.DataContext = _viewModel;

            _mainWindow.NavigateTo(_startupWindow);
            //_mainWindow.NavigateTo(_gameWindow); //for testing purposes
            _mainWindow.Show();

        }
    }
}
