using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using CCity.Model;

namespace CCity.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Fields

        private MainModel _model;
        private Tool _currentTool;

        private bool _minimapMinimized;
        private Tool _selectedTool;
        private int _mapPosition;
        private bool _fieldSelected;

        #endregion

        #region Properties

        public ObservableCollection<FieldItem> Fields { get; private set; }
        public string MayorName { get; }
        public string CityName { get; }
        public int Budget { get; }
        public int Satisfaction { get; }
        public int Population { get; }
        public bool Paused { get; }
        public Speed Speed { get; }
        public int CommercialTax { get; }
        public int ResidentialTax { get; }
        public int IndustrialTax { get; }
        public string SelectedFieldName { get; }
        public string SelectedFieldHealth { get; }
        public string SelectedFieldIsOnFire { get; }
        public string SelectedFieldIsUpgradeable { get; }
        public int SelectedFieldUpgradeCost { get; }
        public string SelectedFieldStadiumEffect { get; }
        public string SelectedFieldForestEffect { get; }
        public string SelectedFieldFiredepartmentEffect { get; }
        public string SelectedFieldPoliceEffect { get; }
        public string SelectedFieldSatisfaction { get; }
        public string SelectedFieldCitizenName { get; }

        public bool MinimapMinimized
        { 
            get { return _minimapMinimized; }
            set
            {
                if(value != _minimapMinimized) { 
                    _minimapMinimized = value;
                    OnPropertyChanged(nameof(_minimapMinimized));
                }
            }
        }

        public Tool SelectedTool
        {
            get { return _selectedTool; }
            set
            {
                if (value != _selectedTool)
                {
                    _selectedTool = value;
                    OnPropertyChanged(nameof(_selectedTool));
                }
            }
        }

        public int MapPosition
        {
            get { return _mapPosition; }
            set
            {
                if (value != _mapPosition)
                {
                    _mapPosition = value;
                    OnPropertyChanged(nameof(_mapPosition));
                }
            }
        }

        public bool FieldSelected
        {
            get { return _fieldSelected; }
            set
            {
                if (value != _fieldSelected)
                {
                    _fieldSelected = value;
                    OnPropertyChanged(nameof(_fieldSelected));
                }
            }
        }


        #endregion

        #region Commands

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand SelectToolCommand { get; private set; }
        public DelegateCommand ChangeResidentialTaxCommand { get; private set; }
        public DelegateCommand ChangeCommercialTaxCommand { get; private set; }
        public DelegateCommand ChangeIndustrialTaxCommand { get; private set; }
        public DelegateCommand SendFiretruckCommand { get; private set; }
        public DelegateCommand UpgradeCommand { get; private set; }
        public DelegateCommand ChangeMinimapSizeCommand { get; private set; }
        public DelegateCommand ExitGameCommand { get; private set; }

        #endregion

        #region Constructors

        public MainViewModel(MainModel model)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public methods


        #endregion

        #region Private methods

        private void FieldClicked(int index)
        {
            throw new NotImplementedException();
        }

        private void Model_GameTicked(object o, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Model_PopulationChanged(object o, FieldEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Model_BudgetChanged(object o, FieldEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Model_SatisfactionChanged(object o, FieldEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Model_TaxChanged(object o, FieldEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Events

        public EventHandler<EventArgs> NewGame;
        public EventHandler<EventArgs> ExitGame;
        public EventHandler<EventArgs> InspectField;

        #endregion

    }
}
