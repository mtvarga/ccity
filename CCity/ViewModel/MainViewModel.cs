using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        private string _inputMayorName;
        private string _inputCityName;
        private Field? _selectedField;
        private bool _isPublicityToggled;

        #endregion

        #region Properties

        public ObservableCollection<FieldItem> Fields { get; private set; }
        public ObservableCollection<ToolItem> Tools { get; private set; }
        public string MayorName { get => _model.MayorName; }
        public string CityName { get => _model.CityName; }
        public int Budget { get => _model.Budget; }
        public int Satisfaction { get => PercentToInt(_model.Satisfaction); }
        public int Population { get => _model.Population; }
        public bool Paused { get; private set; }
        public Speed Speed { get => _model.Speed; }
        public int CommercialTax { get => PercentToInt(_model.Taxes.CommercialTax); }
        public int ResidentialTax { get => PercentToInt(_model.Taxes.ResidentalTax); }
        public int IndustrialTax { get => PercentToInt(_model.Taxes.IndustrialTax); }
        public bool IsFieldSelected { get => _selectedField != null; }
        public string SelectedFieldName { get => IsFieldSelected ? GetFieldName(_selectedField) : ""; }
        //public int SelectedFieldHealth { get; }
        //public string SelectedFieldIsOnFire { get; }
        //public string SelectedFieldIsUpgradeable { get; }
        //public int SelectedFieldUpgradeCost { get; }
        public int SelectedFieldPoliceDepartmentEffect { get => IsFieldSelected ? PercentToInt(_selectedField.PoliceDepartmentEffect) : 0; }
        public int SelectedFieldFireDepartmentEffect { get => IsFieldSelected ? PercentToInt(_selectedField.FireDepartmentEffect) : 0; }
        public int SelectedFieldStadiumEffect { get => IsFieldSelected ? PercentToInt(_selectedField.StadiumEffect) : 0; }
        public int SelectedFieldIndustrialEffect { get => IsFieldSelected ? PercentToInt(_selectedField.IndustrialEffect) : 0; }
        //public int SelectedFieldForestEffect { get => IsFieldSelected ? PercentToInt(_selectedField.ForestEffect) : 0; }
        public int SelectedFieldSatisfaction { get => SelectedFieldIsZone ? PercentToInt(((Zone)(_selectedField!.Placeable!)).Satisfaction()) : 0; }
        public int SelectedFieldPopulation { get => SelectedFieldIsZone ? ((Zone)(_selectedField!.Placeable!)).Current : 0; }
        public bool SelectedFieldIsZone { get => IsFieldSelected && _selectedField!.Placeable is Zone; }
        //public string SelectedFieldCitizenName { get; }
        public int Width { get => _model.Width; }
        public int Height { get => _model.Height; }
        public string OutputCityName { get => _inputCityName == "" ? "Városvezetés a hobbim." : _inputCityName + " büszke polgármestere vagyok."; }
        public string OutputMayorName { get => _inputMayorName == "" ? "Polgármester." : _inputMayorName; }
        public bool CanStart { get => _inputMayorName != "" && _inputCityName != ""; }

        public bool MinimapMinimized
        { 
            get { return _minimapMinimized; }
            set
            {
                if(value != _minimapMinimized) { 
                    _minimapMinimized = value;
                    OnPropertyChanged(nameof(MinimapMinimized));
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
                    OnPropertyChanged(nameof(SelectedTool));
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
                    OnPropertyChanged(nameof(MapPosition));
                }
            }
        }

        public bool IsPublicityToggled
        {
            get { return _isPublicityToggled; }
            set
            {
                if (value != _isPublicityToggled)
                {
                    _isPublicityToggled = value;
                    OnPropertyChanged(nameof(IsPublicityToggled));
                }
            }
        }

        public string InputCityName
        {
            get { return _inputCityName; }
            set
            {
                if (value != _inputCityName)
                {
                    _inputCityName = value;
                    OnPropertyChanged(nameof(InputCityName));
                    OnPropertyChanged(nameof(OutputCityName));
                    OnPropertyChanged(nameof(CanStart));
                }
            }
        }

        public string InputMayorName
        {
            get { return _inputMayorName; }
            set
            {
                if (value != _inputMayorName)
                {
                    _inputMayorName = value;
                    OnPropertyChanged(nameof(InputMayorName));
                    OnPropertyChanged(nameof(OutputMayorName));
                    OnPropertyChanged(nameof(CanStart));
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
        public DelegateCommand RefreshMapCommand { get; private set; }
        
        public DelegateCommand ChangeSpeedCommand { get; } 
        
        //TO DO
        public DelegateCommand SendFiretruckCommand { get; private set; }
        public DelegateCommand UpgradeCommand { get; private set; }
        public DelegateCommand ChangeMinimapSizeCommand { get; private set; }
        public DelegateCommand CloseSelectedFieldWindowCommand { get; private set; }
        public DelegateCommand StartNewGameCommand { get; private set; }
        public DelegateCommand TogglePublicityCommand { get; private set; }

        #endregion

        #region Constructors

        public MainViewModel(MainModel model)
        {
            _model = model;
            _model.FieldsUpdated += Model_FieldUpdated;
            _model.TaxChanged += Model_TaxChanged;
            _model.SatisfactionChanged += Model_SatisfactionChanged;
            _model.BudgetChanged += Model_BudgetChanged;
            _model.PopulationChanged += Model_PopulationChanged;
            _model.SpeedChanged += Model_SpeedChanged;
                
            CreateTable();
            CreateToolbar();

            NewGameCommand = new DelegateCommand(param => OnNewGame());
            PauseGameCommand = new DelegateCommand(param => OnPauseGame());
            ExitGameCommand = new DelegateCommand(param => OnExitGame());
            CloseApplicationCommand = new DelegateCommand(param => OnCloseApplication());
            ChangeResidentialTaxCommand = new DelegateCommand(param => OnChangeResidentialTax(int.Parse(param as string ?? string.Empty)));
            ChangeCommercialTaxCommand = new DelegateCommand(param => OnChangeCommercialTax(int.Parse(param as string ?? string.Empty)));
            ChangeIndustrialTaxCommand = new DelegateCommand(param => OnChangeIndustrialTax(int.Parse(param as string ?? string.Empty)));
            CloseSelectedFieldWindowCommand = new DelegateCommand(OnCloseSelectedFieldWindow);
            RefreshMapCommand = new DelegateCommand(param => OnRefreshMap());
            StartNewGameCommand = new DelegateCommand(param => OnStartNewGame());
            TogglePublicityCommand = new DelegateCommand(param => OnTogglePublicity());
            ChangeSpeedCommand =
                new DelegateCommand(param => OnChangeSpeedCommand(int.Parse(param as string ?? string.Empty)));

            _inputCityName = "";
            _inputMayorName = "";
            _isPublicityToggled = false;
        }

        private void OnTogglePublicity()
        {
            _isPublicityToggled = !_isPublicityToggled;
            OnPropertyChanged(nameof(IsPublicityToggled));
            foreach (FieldItem fieldItem in Fields) RefreshFieldItem(fieldItem, true);
        }

        private void Model_NewGame(object? sender, EventArgs e)
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public methods


        #endregion

        #region Private methods

        private void CreateTable()
        {
            Fields = new();
            for(int i = 0; i < Height; i++) {
                for(int j = 0; j < Width; j++)
                {
                    Fields.Add(new FieldItem
                    {
                        Texture = Texture.None,
                        MinimapColor = Color.FromRgb(0, 255, 0),
                        OverlayColor = Color.FromArgb(0, 0, 0, 0),
                        X = j,
                        Y = i,
                        Number = (i * Width) + j,
                        ClickCommand = new DelegateCommand(param => FieldClicked(Convert.ToInt32(param)))
                    });

                }
            }
            foreach (FieldItem fieldItem in Fields) RefreshFieldItem(fieldItem);
        }

        private void CreateToolbar()
        {
            List<Tool> orderedTools = new()
            {
                Tool.Cursor,
                Tool.ResidentialZone,
                Tool.CommercialZone,
                Tool.IndustrialZone,
                Tool.PoliceDepartment,
                Tool.FireDepartment,
                Tool.Stadium,
                Tool.Road,
                Tool.Bulldozer
            };
            Tools = new();
            foreach (Tool tool in orderedTools)
            {
                Tools.Add(new ToolItem
                {
                    Tool = tool,
                    ClickCommand = new DelegateCommand(param => ToolClicked((Tool)param!))
                });
            }
        }

        private void RefreshFieldItem(FieldItem fieldItem, bool onlyOverlayColor = false)
        {
            fieldItem.OverlayColor = GetOverlayColorFromFieldItem(fieldItem);
            if (onlyOverlayColor) return;
            fieldItem.Texture = GetTextureFromFieldItem(fieldItem);
            fieldItem.MinimapColor = GetMinimapColorFromFieldItem(fieldItem);
        }

        private Color GetOverlayColorFromFieldItem(FieldItem fieldItem)
        {
            Field field = _model.Fields[fieldItem.X, fieldItem.Y];
            if (!field.HasPlaceable) return Color.FromArgb(0, 0, 0, 0);
            switch (field.Placeable)
            {
                case ResidentialZone _: return Color.FromArgb(100, 0, 255, 0);
                case CommercialZone _: return Color.FromArgb(100, 0, 0, 255);
                case IndustrialZone _: return Color.FromArgb(100, 255, 255, 0);
                default: if (field.Placeable.IsPublic && IsPublicityToggled) return Color.FromArgb(50, 22, 32, 255); else return Color.FromArgb(0, 0, 0, 0);
                //default: return Color.FromArgb(0, 0, 0, 0);
            }
        }

        private Color GetMinimapColorFromFieldItem(FieldItem fieldItem)
        {
            return Color.FromRgb(0, 255, 0);
        }

        private Texture GetTextureFromFieldItem(FieldItem fieldItem)
        {
            Field field = _model.Fields[fieldItem.X, fieldItem.Y];
            if (!field.HasPlaceable) return Texture.None;
            switch (field.Placeable)
            {
                case FireDepartment _: return Texture.FireDepartment;
                case PoliceDepartment _: return Texture.PoliceDepartment;
                case Stadium _: return Texture.StadiumBottomLeft;
                case PowerPlant _: return Texture.PowerPlantBottomLeft;
                case Road _:
                    SetNeighboursRoadTexture((Road)(field.Placeable));
                    return GetRoadTextureFromField(field);
                case Filler _: return GetFillerTexture(field);
                default: return Texture.None;
            }
        }

        private Texture GetFillerTexture(Field field)
        {
            Placeable mainPlaceable = (Placeable)((Filler)field.Placeable).Main;
            return Texture.Unhandled;
        }

        private void SetNeighboursRoadTexture(Road road)
        {
            (int[] indicators, List<Road> neighbours) = _model.GetFourRoadNeighbours(road);
            foreach(Road neighbour in neighbours)
            {
                FieldItem fieldItem = GetFieldItemFromField(neighbour.Owner!);
                fieldItem.Texture = GetRoadTextureFromField(neighbour.Owner!);
            }
        }

        private int CalculateIndexFromField(Field field) => field.X * _model.Width + field.Y;

        private bool IsInFields(int index) => 0 <= index && index < Fields.Count;

        private Texture GetRoadTextureFromField(Field field)
        {
            if (field.Placeable is not Road) return Texture.Unhandled;
            (int[] id, List<Road> roads) = _model.GetFourRoadNeighbours((Road)field.Placeable);
            (int, int, int, int) neighbours = (id[0], id[1], id[2], id[3]);
            switch (neighbours)
            {
                //
                case (1, 0, 1, 0): return Texture.RoadVertical;
                case (0, 1, 0, 1): return Texture.RoadHorizontal;
                case (1, 1, 1, 1): return Texture.RoadCross;
                case (0, 0, 0, 0): return Texture.RoadCross;
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
        }

        private void UnselectField()
        {
            _selectedField = null;
            OnPropertyChanged(nameof(IsFieldSelected));
        }

        private void FieldClicked(int index)
        {
            (int x, int y) coord = GetCordinates(index);
            switch (SelectedTool)
            {
                case Tool.Cursor: SelectField(index); break;
                case Tool.ResidentialZone: _model.Place(coord.x, coord.y, new ResidentialZone()); break;
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

        private void ToolClicked(Tool tool)
        {
            SelectedTool = tool;
            if (SelectedTool != Tool.Cursor) UnselectField();
            OnPropertyChanged(nameof(SelectedTool));
        }

        private void SelectField(int index)
        {
            (int x, int y) coord = GetCordinates(index);

            _selectedField = _model.Fields[coord.x, coord.y];
            OnPropertyChanged(nameof(IsFieldSelected));
            OnPropertyChanged(nameof(SelectedFieldName));
            OnPropertyChanged(nameof(SelectedFieldPoliceDepartmentEffect));
            OnPropertyChanged(nameof(SelectedFieldFireDepartmentEffect));
            OnPropertyChanged(nameof(SelectedFieldStadiumEffect));
            OnPropertyChanged(nameof(SelectedFieldIndustrialEffect));
            OnPropertyChanged(nameof(SelectedFieldPopulation));
            OnPropertyChanged(nameof(SelectedFieldSatisfaction));
            OnPropertyChanged(nameof(SelectedFieldIsZone));

            if (_selectedField.Placeable is Road)
            {
                Road road = (Road)_selectedField.Placeable;
            }
        }

        private int PercentToInt(double percent) => (int)Math.Floor(percent * 100);

        private (int, int) GetCordinates(int index)
        {
            int x = index % Width;
            int y = (index-x) / Width;
            return (x, y);
        }

        //TO DO: Utilities or Placeable
        private Placeable GetRoot(Placeable placeable)
        {
            if (placeable is Filler) return (Placeable)(((Filler)placeable).Main);
            else return placeable;
        }

        private string GetFieldName(Field? selectedField)
        {
            if (selectedField is null || selectedField.Placeable is null) return "Üres mező";
            Placeable placeable = GetRoot(selectedField.Placeable!);
            switch (placeable)
            {
                case ResidentialZone _: return "Lakózóna";
                case CommercialZone _: return "Kereskedelmi zóna";
                case IndustrialZone _: return "Ipari zóna";
                case Road road: if(road.IsPublic) return "Közút"; else return "Út";
                case PoliceDepartment _: return "Rendőrség";
                case FireDepartment _: return "Tűzoltóság";
                case Stadium _: return "Stadion";
                default: return "Épület";
            }
        }

        private FieldItem GetFieldItemFromField(Field field)
        {
            return Fields[field.X + field.Y * Width];
        }

        private void Model_GameTicked(object? o, EventArgs e)
        {
            //throw new NotImplementedException();
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

        private void Model_FieldUpdated(object? o, FieldEventArgs e)
        {
            if (e.Fields != null)
            {
                foreach (Field field in e.Fields)
                {
                    int index = field.Y * _model.Width + field.X;
                    RefreshFieldItem(Fields[index]);
                }
            }
            else
            {
                MessageBox.Show("A művelet nem végezhető el.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Model_SpeedChanged(object? o, EventArgs e) => OnPropertyChanged(nameof(Speed));

        #endregion

        #region Events

        public event EventHandler? NewGame;
        public event EventHandler? PauseGame;
        public event EventHandler? ExitGame;
        public event EventHandler? CloseApplication;

        #endregion

        #region Model event methods

        private void OnStartNewGame()
        {
            _model.StartNewGame(_inputCityName, _inputMayorName);
            foreach (FieldItem fieldItem in Fields) RefreshFieldItem(fieldItem);
        }

        private void OnRefreshMap()
        {
            foreach (FieldItem fieldItem in Fields) RefreshFieldItem(fieldItem);
        }

        private void OnNewGame()
        {
            NewGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnPauseGame()
        {
            Paused = !Paused;
            OnPropertyChanged(nameof(Paused));
            
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

        private void OnChangeResidentialTax(int n) => _model.ChangeTax(TaxType.Residental, (n > 0 ? 1 : -1) * 0.01);

        private void OnChangeCommercialTax(int n) => _model.ChangeTax(TaxType.Commercial, (n > 0 ? 1 : -1) * 0.01);

        private void OnChangeIndustrialTax(int n) => _model.ChangeTax(TaxType.Industrial, (n > 0 ? 1 : -1) * 0.01);

        private void OnCloseSelectedFieldWindow(object? obj)
        {
            UnselectField();
            OnPropertyChanged(nameof(IsFieldSelected));
        }

        private void OnChangeSpeedCommand(int n)
        {
            _model.ChangeSpeed(n switch
            {
                < 0 => _model.Speed switch
                {
                    Speed.Normal => Speed.Slow,
                    Model.Speed.Fast => Speed.Normal,
                    _ => _model.Speed
                },
                
                > 0 => _model.Speed switch
                {
                    Speed.Slow => Speed.Normal,
                    Speed.Normal => Speed.Fast,
                    _ => _model.Speed
                },
                
                _ => _model.Speed
            });
        }
        
        #endregion
    }
}
