using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Xml.Serialization;
using CCity.Model;
using Microsoft.Win32;

namespace CCity.ViewModel
{
    public class MainViewModel : ViewModelBase
    {

        #region Fields

        private MainModel _model;
        private bool _minimapMinimized;
        private Tool _selectedTool;
        private int _mapPosition;
        private Field? _selectedField;

        #endregion

        #region Properties

        public ObservableCollection<FieldItem> Fields { get; private set; }
        public string MayorName { get => _model.MayorName; }
        public string CityName { get => _model.CityName; }
        public int Budget { get => _model.Budget; }
        public int Satisfaction { get => _model.Satisfaction; }
        public int Population { get => _model.Population; }
        public bool Paused { get; } //TO DO
        public Speed Speed { get => _model.Speed; }
        public int CommercialTax { get => _model.Taxes.CommercialTax; }
        public int ResidentialTax { get => _model.Taxes.ResidentalTax; }
        public int IndustrialTax { get => _model.Taxes.IndustrialTax; }
        public bool IsFieldSelected { get => _selectedField != null; }
        public string SelectedFieldName { get => IsFieldSelected ? GetFieldName(_selectedField) : ""; }
        //public int SelectedFieldHealth { get; }
        //public string SelectedFieldIsOnFire { get; }
        //public string SelectedFieldIsUpgradeable { get; }
        //public int SelectedFieldUpgradeCost { get; }
        public int SelectedFieldPoliceDeparmentEffect { get => IsFieldSelected ? PercentToInt(_selectedField.PoliceDepartmentEffect) : 0; }
        public int SelectedFieldFiredepartmentEffect { get => IsFieldSelected ? PercentToInt(_selectedField.FireDepartmentEffect) : 0; }
        public int SelectedFieldStadiumEffect { get => IsFieldSelected ? PercentToInt(_selectedField.StadiumEffect) : 0; }
        public int SelectedFieldIndustrialEffect { get => IsFieldSelected ? PercentToInt(_selectedField.IndustrialEffect) : 0; }
        //public int SelectedFieldForestEffect { get => IsFieldSelected ? PercentToInt(_selectedField.ForestEffect) : 0; }
        public int SelectedFieldSatisfaction { get; }
        //public string SelectedFieldCitizenName { get; }

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

        #endregion

        #region Commands

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand PauseGameCommand { get; private set; }
        public DelegateCommand ExitGameCommand { get; private set; }
        public DelegateCommand CloseApplicationCommand { get; private set; }
        public DelegateCommand SelectToolCommand { get; private set; }
        public DelegateCommand ChangeResidentialTaxCommand { get; private set; }
        public DelegateCommand ChangeCommercialTaxCommand { get; private set; }
        public DelegateCommand ChangeIndustrialTaxCommand { get; private set; }
        //TO DO
        public DelegateCommand SendFiretruckCommand { get; private set; }
        public DelegateCommand UpgradeCommand { get; private set; }
        public DelegateCommand ChangeMinimapSizeCommand { get; private set; }
        public DelegateCommand CloseSelectedFieldWindow { get; private set; }

        #endregion

        #region Constructors

        public MainViewModel(MainModel model)
        {
            _model = model;
            _model.TaxChanged += new EventHandler<EventArgs>(Model_TaxChanged);

            NewGameCommand = new DelegateCommand(param => OnNewGame());
            PauseGameCommand = new DelegateCommand(param => OnPauseGame());
            ExitGameCommand = new DelegateCommand(param => OnExitGame());
            CloseApplicationCommand = new DelegateCommand(param => OnCloseApplication());
            SelectToolCommand = new DelegateCommand(param => OnSelectTool((Tool)param!));
            ChangeResidentialTaxCommand = new DelegateCommand(param => OnChangeResidentialTax((int)param!));
            ChangeCommercialTaxCommand = new DelegateCommand(param => OnChangeCommercialTax((int)param!));
            ChangeIndustrialTaxCommand = new DelegateCommand(param => OnChangeIndustrialTax((int)param!));
            CloseSelectedFieldWindow = new DelegateCommand(OnCloseSelectedFieldWindow);

        }

        #endregion

        #region Public methods


        #endregion

        #region Private methods

        private void CreateTable()
        {
            Fields = new();
            for(int i = 0; i < _model.Width; i++) {
                for(int j = 0; j < _model.Height; j++)
                {
                    Fields.Add(new FieldItem
                    {
                        Texture = Texture.None,
                        MinimapColor = Color.Green,
                        OverLayColor = Color.Transparent,
                        X = i,
                        Y = j,
                        Number = i * _model.Width + j,
                        ClickCommand = new DelegateCommand(param => FieldClicked(Convert.ToInt32(param)))
                    });

                }
            }
            foreach (FieldItem fieldItem in Fields) RefreshFieldItem(fieldItem);
        }

        private void RefreshFieldItem(FieldItem fieldItem)
        {
            fieldItem.Texture = GetTextureFromFieldItem(fieldItem);
            fieldItem.MinimapColor = GetMinimapColorFromFieldItem(fieldItem);
        }

        private Color GetMinimapColorFromFieldItem(FieldItem fieldItem)
        {
            return Color.Green;
        }

        private Texture GetTextureFromFieldItem(FieldItem fieldItem)
        {
            Field field = _model.Fields[fieldItem.X, fieldItem.Y];
            SetNeighboursRoadTexture(field);
            if (!field.HasPlaceable) return Texture.None;
            switch (field.Placeable)
            {
                case FireDepartment _: return Texture.FireDepartment;
                case PoliceDepartment _: return Texture.PoliceDepartment;
                case Stadium _: return Texture.StadiumBottomLeft;
                case PowerPlant _: return Texture.PowerPlantBottomLeft;
                case Road _: return GetRoadTextureFromField(field);
                case Filler _: return GetFillerTexture(field);
                default: return Texture.Unhandled;
            }
        }

        private Texture GetFillerTexture(Field field)
        {
            throw new NotImplementedException();
        }

        private void SetNeighboursRoadTexture(Field field)
        {
            int index = CalculateIndexFromField(field);
            List<int> indexes = new() { index - 1, index + 1, index - _model.Width, index + _model.Width };
            foreach(int currentIndex in indexes)
            {
                if(IsInFields(currentIndex))
                {
                    FieldItem currentFieldItem = Fields[currentIndex];
                    currentFieldItem.Texture = GetRoadTextureFromField(_model.Fields[currentFieldItem.X, currentFieldItem.Y]);
                }
            }
        }

        private int CalculateIndexFromField(Field field) => field.X * _model.Width + field.Y;

        private bool IsInFields(int index) => 0 <= index && index <= Fields.Count;

        private Texture GetRoadTextureFromField(Field field)
        {
            //if (!field.Has(typeof(Road))) return Texture.Unhandled;
            //(int t, int r, int b, int l) = GetRoadNeighbours(field);
            (int t, int r, int b, int l) neighbours = (0, 0, 0, 0);
            switch (neighbours)
            {
                //
                case (1, 0, 1, 0): return Texture.RoadVertical;
                case (0, 1, 0, 1): return Texture.RoadHorizontal;
                case (1, 1, 1, 1): return Texture.RoadCross;
                //turns
                case (1, 0, 0, 1): return Texture.RoadTopLeft;
                case (1, 1, 0, 0): return Texture.RoadTopRight;
                case (0, 0, 1, 1): return Texture.RoadBottomLeft;
                case (0, 1, 1, 0): return Texture.RoadBottomRight;
                //nots
                case (0, 1, 1, 1): return Texture.RoadNotTop;
                case (1, 1, 0, 1): return Texture.RoadNotBottom;
                case (1, 1, 1, 0): return Texture.RoadNotLeft;
                case (1, 0, 1, 1): return Texture.RoadNotRight;
                //closes
                case (1, 0, 0, 0): return Texture.RoadTopClose;
                case (0, 0, 1, 0): return Texture.RoadBottomClose;
                case (0, 0, 0, 1): return Texture.RoadLeftClose;
                case (0, 1, 0, 0): return Texture.RoadRightClose;

                default: return Texture.Unhandled;
            }
            throw new NotImplementedException();
        }

        private void UnselectField() => _selectedField = null;

        private void FieldClicked(int index)
        {
            (int x, int y) coord = GetCordinates(index);
            switch (SelectedTool)
            {
                case Tool.Cursor: SelectField(index); break;
                case Tool.ResidentalZone: _model.Place(coord.x, coord.y, new ResidentialZone()); break;
                case Tool.CommercialZone: _model.Place(coord.x, coord.y, new CommercialZone()); break;
                case Tool.IndustrialZone: _model.Place(coord.x, coord.y, new IndustrialZone()); break;
                case Tool.FireDepartment: _model.Place(coord.x, coord.y, new FireDepartment()); break;
                case Tool.PoliceDepartment: _model.Place(coord.x, coord.y, new PoliceDepartment()); break;
                case Tool.Stadium: _model.Place(coord.x, coord.y, new Stadium()); break;
                case Tool.Road: _model.Place(coord.x, coord.y, new Road()); break;
                case Tool.Bulldozer: _model.Demolish(coord.x, coord.y); break;
                default: throw new Exception();
            }
        }

        private void SelectField(int index)
        {
            (int x, int y) coord = GetCordinates(index);
            _selectedField = _model.Fields[coord.x, coord.y];
            OnPropertyChanged(nameof(IsFieldSelected));
            OnPropertyChanged(nameof(SelectedFieldName));
            OnPropertyChanged(nameof(SelectedFieldPoliceDeparmentEffect));
            OnPropertyChanged(nameof(SelectedFieldFiredepartmentEffect));
            OnPropertyChanged(nameof(SelectedFieldStadiumEffect));
            OnPropertyChanged(nameof(SelectedFieldIndustrialEffect));
            OnPropertyChanged(nameof(SelectedFieldSatisfaction));
        }

        private int PercentToInt(double percent) => (int)Math.Floor(percent * 100);

        private (int, int) GetCordinates(int index)
        {
            int x = index % _model.Width;
            int y = index / _model.Width;
            return (x, y);
        }

        private string GetFieldName(Field? selectedField)
        {
            return "...";
        }

        private void Model_GameTicked(object? o, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Model_PopulationChanged(object? o, EventArgs e)
        {
            OnPropertyChanged(nameof(Population));
        }

        private void Model_BudgetChanged(object? o, EventArgs e)
        {
            OnPropertyChanged(nameof(Budget));
        }

        private void Model_SatisfactionChanged(object? o, EventArgs e)
        {
            OnPropertyChanged(nameof(Satisfaction));
        }

        private void Model_TaxChanged(object? o, EventArgs e)
        {
            OnPropertyChanged(nameof(ResidentialTax));
            OnPropertyChanged(nameof(IndustrialTax));
            OnPropertyChanged(nameof(CommercialTax));
        }

        private void Mode_FieldUpdated(object? o, FieldEventArgs e)
        {
            foreach(Field field in e.Fields)
            {
                int index = field.Y * _model.Width + field.X;
                RefreshFieldItem(Fields[index]);
            }
        }

        #endregion

        #region Events

        public EventHandler? NewGame;
        public EventHandler? PauseGame;
        public EventHandler? ExitGame;
        public EventHandler? CloseApplication;

        #endregion

        #region Model event methods

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnPauseGame()
        {
            PauseGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnExitGame()
        {
            ExitGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnCloseApplication()
        {
            CloseApplication?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Delegatecommand methods

        private void OnSelectTool(Tool tool)
        {
            _selectedTool = tool;
        }

        private  void OnChangeResidentialTax(int n)
        {
            //_model.ChangeTax(TaxType.Residental, n);
        }

        private void OnChangeCommercialTax(int n)
        {
            //_model.ChangeTax(TaxType.Commercial, n);
        }

        private void OnChangeIndustrialTax(int n)
        {
            //_model.ChangeTax(TaxType.Industrial, n);
        }

        private void OnCloseSelectedFieldWindow(object? obj)
        {
            UnselectField();
            OnPropertyChanged(nameof(IsFieldSelected));
        }


        #endregion
    }
}
