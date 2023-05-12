using CCity.Model;
using CCity.View;
using CCity.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CCity
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private MainModel _model;
        private MainViewModel _viewModel;
        private MainWindow _mainWindow;
        private UserControl _startupWindow;
        private UserControl _gameWindow;
        private DispatcherTimer _timer;

        #endregion

        #region Constructor

        App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        #endregion

        #region Application event handler

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _model = new();
            _model.GameOver += Model_GameOver;
            _model.NewGame += Model_NewGame;

            _viewModel = new(_model);
            _viewModel.PauseGame += ViewModel_PauseGame;
            _viewModel.ExitGame += ViewModel_ExitGame;
            _viewModel.CloseApplication += ViewModel_CloseApplication;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += Time_Ticked;

            _mainWindow = new();
            _mainWindow.Closing += new System.ComponentModel.CancelEventHandler(View_Closing);


            _startupWindow = new StartupWindow();
            _gameWindow = new GameWindow();


            _mainWindow.DataContext = _viewModel;

            _mainWindow.NavigateTo(_startupWindow);
            _mainWindow.Show();
        }

        private void Time_Ticked(object? value, EventArgs eventArgs)
        {
            _model.TimerTick();
        }

        #endregion

        #region View event handlers

        //TODO
        private void View_Closing(object? sender, CancelEventArgs e)
        {
            bool restartTimer = _timer.IsEnabled;
            _timer.Stop();
            if (MessageBox.Show("Biztos be akarod zárni a játékot?", "C City", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel=true;

                if(restartTimer)
                {
                    _timer.Start();
                }
            }

        }

        #endregion

        #region ViewModel event handlers

        private void ViewModel_ExitGame(object? sender, EventArgs e)
        {
            bool restartTimer = _timer.IsEnabled;
            _timer.Stop();
            if (MessageBox.Show("Biztos új játékot akarsz kezdeni? A jelenlegi játékmentet elfog veszni.", "C City", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _mainWindow.NavigateTo(_startupWindow);
                _viewModel.ExitToMainMenu();
            }
            else
            {
                if (restartTimer)
                {
                    _timer.Start();
                }
            }

        }

        private void ViewModel_PauseGame(object? sender, EventArgs e)
        {
            if (_viewModel.Paused)
                _timer.Stop();
            else
                _timer.Start();
        }

        private void ViewModel_CloseApplication(object? sender, EventArgs e)
        {
            _mainWindow.Close();
        }

        #endregion

        #region Model event handlers

        private void Model_NewGame(object? sender, EventArgs e)
        {
            _mainWindow.NavigateTo(_gameWindow);
            _timer.Start();
        }

        private void Model_GameOver(object? sender, EventArgs e)
        {
            _timer.Stop();
            if (MessageBox.Show("Vége a játéknak, túl sok elégedetlen polgár volt!", "C City",    MessageBoxButton.OK, MessageBoxImage.Asterisk) == MessageBoxResult.OK)
            {
                _mainWindow.NavigateTo(_startupWindow);
                _viewModel.ExitToMainMenu();
            }
            else
            {
                throw new Exception("Illegal game over state!");
            }
         
        }

        #endregion

    }
}
