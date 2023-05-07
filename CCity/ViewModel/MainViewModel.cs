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
using System.Windows.Input;
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
        private ToolItem _selectedToolItem;
        private int _mapPosition;
        private string _inputMayorName;
        private string _inputCityName;
        private Field? _selectedField;
        private bool _isPublicityToggled;
        private bool _isElectricityToggled;

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
        public int SelectedFieldForestEffect { get => IsFieldSelected ? PercentToInt(_selectedField.ForestEffect) : 0; }
        public int SelectedFieldSatisfaction { get => SelectedFieldIsZone ? PercentToInt(_model.ZoneSatisfaction((Zone)_selectedField!.Placeable!)) : 0; }
        public int SelectedFieldPopulation { get => SelectedFieldIsZone ? ((Zone)_selectedField!.Placeable!).Count : 0; }
        public bool SelectedFieldIsZone { get => IsFieldSelected && _selectedField!.Placeable is Zone; }
        public int SelectedFieldCurrentElectricity { get => IsFieldSelected && _selectedField!.HasPlaceable ? _selectedField.Placeable!.CurrentSpreadValue[SpreadType.Electricity] : 0; }
        public int SelectedFieldNeededElectricity { get => IsFieldSelected && _selectedField!.HasPlaceable ? _selectedField.Placeable!.MaxSpreadValue[SpreadType.Electricity]() : 0; }
        public bool SelectedFieldIsIncinerated { get => IsFieldSelected && _selectedField!.HasPlaceable && _selectedField!.Placeable is IFlammable && ((IFlammable)_selectedField!.Placeable!).Burning; }
        //public string SelectedFieldCitizenName { get; }
        public int Width { get => _model.Width; }
        public int Height { get => _model.Height; }
        public string OutputCityName { get => _inputCityName == "" ? "Városvezetés a hobbim." : _inputCityName + " büszke polgármestere vagyok."; }
        public string OutputMayorName { get => _inputMayorName == "" ? "Polgármester" : _inputMayorName; }
        public bool CanStart { get => _inputMayorName != "" && _inputCityName != ""; }

        public Tool SelectedTool => _selectedToolItem.Tool;

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

        public bool IsElectricityToggled
        {
            get { return _isElectricityToggled; }
            set
            {
                if (value != _isElectricityToggled)
                {
                    _isElectricityToggled = value;
                    OnPropertyChanged(nameof(IsElectricityToggled));
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
        public DelegateCommand SendFiretruckToSelectedFieldCommand { get; private set; }
        public DelegateCommand UpgradeCommand { get; private set; }
        public DelegateCommand ChangeMinimapSizeCommand { get; private set; }
        public DelegateCommand CloseSelectedFieldWindowCommand { get; private set; }
        public DelegateCommand StartNewGameCommand { get; private set; }
        public DelegateCommand TogglePublicityCommand { get; private set; }
        public DelegateCommand ToggleElectricityCommand { get; private set; }

        #endregion

        #region Constructors

        public MainViewModel(MainModel model)
        {
            _model = model;
            _model.FieldsUpdated += Model_FieldUpdated;
            _model.ErrorOccured += Model_ErrorOccured;
            _model.TaxChanged += Model_TaxChanged;
            _model.SatisfactionChanged += Model_SatisfactionChanged;
            _model.BudgetChanged += Model_BudgetChanged;
            _model.PopulationChanged += Model_PopulationChanged;
            _model.SpeedChanged += Model_SpeedChanged;
            _model.FireTruckMoved += Model_FireTruckMoved;
                
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
            ToggleElectricityCommand = new DelegateCommand(param => OnToggleElecticity());
            ChangeMinimapSizeCommand = new DelegateCommand(param => OnChangeMinimapSize());
            ChangeSpeedCommand = new DelegateCommand(param => OnChangeSpeedCommand(int.Parse(param as string ?? string.Empty)));
            SendFiretruckToSelectedFieldCommand = new DelegateCommand(param => OnSendFiretruckToSelectedFieldCommand());

            InputCityName = "";
            InputMayorName = "";
            IsPublicityToggled = false;
            IsElectricityToggled = false;
        }

        private void OnTogglePublicity()
        {
            IsElectricityToggled = false;
            IsPublicityToggled = !IsPublicityToggled;
            foreach (FieldItem fieldItem in Fields) RefreshFieldItem(fieldItem, true);
        }
        private void OnToggleElecticity()
        {
            IsPublicityToggled = false;
            IsElectricityToggled = !IsElectricityToggled;
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
                Tool.PowerPlant,
                Tool.Pole,
                Tool.Road,
                Tool.Forest,
                Tool.Bulldozer,
                Tool.FlintAndSteel
            };
            Tools = new();
            int number = 0;
            foreach (Tool tool in orderedTools)
            {
                Tools.Add(new ToolItem
                {
                    Tool = tool,
                    Number = number++,
                    ClickCommand = new DelegateCommand(param => ToolClicked((int)param!))
                });
            }
            _selectedToolItem = Tools[0];
            _selectedToolItem.IsSelected = true;
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
            byte opacity = 50;
            if(field.Placeable is Zone zone)
            {
                if (!zone.Empty) opacity = 37;
                switch (zone)
                {
                    case ResidentialZone _: return Color.FromArgb(opacity, 0, 255, 0);
                    case CommercialZone _: return Color.FromArgb(opacity, 0, 0, 255);
                    case IndustrialZone _: return Color.FromArgb(opacity, 255, 255, 0);
                    default: return Color.FromArgb(0, 0, 0, 0);
                }
            }
            
            if (field.Placeable!.IsPublic && IsPublicityToggled) return Color.FromArgb(50, 22, 32, 255);
            if (field.Placeable!.IsElectrified && IsElectricityToggled) return Color.FromArgb(50, 255, 255, 32);
            else return Color.FromArgb(0, 0, 0, 0);
        }

        private Color GetMinimapColorFromFieldItem(FieldItem fieldItem)
        {
            Field field = _model.Fields[fieldItem.X, fieldItem.Y];
            if (!field.HasPlaceable) return Color.FromRgb(0, 255, 0);
            return field.Placeable! switch
            {
                Road => Color.FromRgb(0, 0, 0),
                _ => Color.FromRgb(0, 255, 0)
            };
        }

        private Texture GetTextureFromFieldItem(FieldItem fieldItem)
        {
            Field field = _model.Fields[fieldItem.X, fieldItem.Y];
            if (!field.HasPlaceable) return Texture.None;
            if (field.Placeable is IFlammable && ((IFlammable)field.Placeable).Burning) return Texture.Fire;
            return GetTextureFromPlaceable(field.ActualPlaceable!);
        }

        private Texture GetTextureFromPlaceable(Placeable placeable)
        {
            return placeable switch
            {
                FireDepartment => Texture.FireDepartment,
                PoliceDepartment => Texture.PoliceDepartment,
                Stadium => Texture.Stadium,
                PowerPlant => Texture.PowerPlant,
                Pole => Texture.Pole,
                Road road => ReturnAndHandleRoadTexture(road),
                Zone zone => GetZoneTexture(zone),
                Forest forest => GetForestTexture(forest),
                Filler filler => GetFillerTexture(filler),
                _ => Texture.Unhandled
            };
        }

        private Texture GetForestTexture(Forest forest)
        {
            Texture texture = Texture.Forest;
            int shifter = (forest.Age * 2) / forest.MaxAge;
            texture += shifter;
            return texture;
        }

        private Texture GetZoneTexture(Zone zone)
        {
            if (zone.Empty) return Texture.None;
            String textureString = zone switch
            {
                ResidentialZone => nameof(ResidentialZone),
                CommercialZone => nameof(CommercialZone),
                _ => nameof(IndustrialZone),
            };
            textureString += ((IUpgradeable)zone).Level.ToString();
            textureString += "Half";
            Texture texture;
            if (!Enum.TryParse(textureString, out texture))
            {
                return Texture.Unhandled;
            }
            if (!zone.BelowHalfPopulation) ++texture;
            return texture;
        }

        private Texture GetFillerTexture(Filler filler)
        {
            Field mainField = ((Placeable)filler.Main).Owner!;
            Field fillerField = ((Placeable)filler).Owner!;
            (int x, int y) = (fillerField.X - mainField.X, mainField.Y - fillerField.Y);
            string enumString = $"{GetTextureFromPlaceable((Placeable)filler.Main).ToString()}_{x}_{y}";
            if (Enum.TryParse(enumString, out Texture texture)) return texture;
            else return Texture.Unhandled;
        }

        private void SetNeighboursRoadTexture(Road road)
        {
            (int[] indicators, List<Road> neighbours) = _model.GetFourRoadNeighbours(road);
            foreach(Road neighbour in neighbours)
            {
                FieldItem fieldItem = GetFieldItemFromField(neighbour.Owner!);
                fieldItem.Texture = GetRoadTexture(neighbour);
            }
        }

        private Texture ReturnAndHandleRoadTexture(Road road)
        {
            SetNeighboursRoadTexture(road);
            return GetRoadTexture(road);
        }

        private int CalculateIndexFromField(Field field) => field.X * _model.Width + field.Y;

        private bool IsInFields(int index) => 0 <= index && index < Fields.Count;

        private Texture GetRoadTexture(Road road)
        {
            (int[] id, _) = _model.GetFourRoadNeighbours(road);
            (int t, int r, int b, int l) = (id[0], id[1], id[2], id[3]);
            string enumString = $"Road_{t}_{r}_{b}_{l}";
            if (Enum.TryParse(enumString, out Texture texture)) return texture;
            else return Texture.Unhandled;
        }

        private void UnselectField()
        {
            _selectedField = null;
            OnPropertyChanged(nameof(IsFieldSelected));
        }

        private void FieldClicked(int index)
        {
            //Coord will be removed (so x and y remains without coord.) in the viewmodel branch, I won't modify it in this branch
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
                case Tool.PowerPlant: _model.Place(coord.x, coord.y, new PowerPlant()); break;
                case Tool.Pole: _model.Place(coord.x, coord.y, new Pole()); break;
                case Tool.Road: _model.Place(coord.x, coord.y, new Road()); break;
                case Tool.Forest: _model.Place(coord.x, coord.y, new Forest()); break;
                case Tool.Bulldozer: _model.Demolish(coord.x, coord.y); break;
                case Tool.FlintAndSteel: _model.IgniteBuilding(coord.x, coord.y); break;
                default: throw new Exception();
            }
        }

        private void ToolClicked(int index)
        {
            _selectedToolItem.IsSelected = false;
            _selectedToolItem = Tools[index];
            _selectedToolItem.IsSelected = true;
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
            OnPropertyChanged(nameof(SelectedFieldForestEffect));
            OnPropertyChanged(nameof(SelectedFieldIsIncinerated));

            OnPropertyChanged(nameof(SelectedFieldCurrentElectricity));
            OnPropertyChanged(nameof(SelectedFieldNeededElectricity));
        }

        private int PercentToInt(double percent) => (int)Math.Floor(percent * 100);

        private (int, int) GetCordinates(int index)
        {
            int x = index % Width;
            int y = (index-x) / Width;
            return (x, y);
        }

        private string GetFieldName(Field? selectedField)
        {
            if (selectedField is null || selectedField.Placeable is null) return "Üres mező";
            switch (selectedField.Placeable!)
            {
                case ResidentialZone _: return "Lakózóna";
                case CommercialZone _: return "Kereskedelmi zóna";
                case IndustrialZone _: return "Ipari zóna";
                case Road road: if(road.IsPublic) return "Közút"; else return "Út";
                case PoliceDepartment _: return "Rendőrség";
                case PowerPlant _: return "Erőmű";
                case FireDepartment _: return "Tűzoltóság";
                case Stadium _: return "Stadion";
                case Forest _: return "Erdő";
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
            foreach (Field field in e.Fields)
            {
                int index = field.Y * _model.Width + field.X;
                RefreshFieldItem(Fields[index]);
            }
        }

        private void Model_FireTruckMoved(object? sender, FieldEventArgs e)
        {
            foreach (Field field in e.Fields)
            {
                int index = field.Y * _model.Width + field.X;
                RefreshFieldItem(Fields[index]);
            }
            foreach(Field field in _model.FireTruckLocations())
            {
                int index = field.Y * _model.Width + field.X;
                Fields[index].Texture = Texture.Firetruck;
            }
        }

        private void Model_ErrorOccured(object? sender, ErrorEventArgs e)
        {
            string errorMessage="";
            switch(e.ErrorCode)
            {
                case "PLACE-OUTOFFIELDBOUNDRIES": errorMessage = "Nem építhetsz a pályán kívülre.";  break;
                case "PLACE-ALREADYUSEDFIELD": errorMessage = "Csak üres mezőre építhetsz."; break;
                case "DEMOLISH - OUTOFFIELDBOUNDS": errorMessage = "Nem rombolhatsz a pályán kívülről."; break;
                case "DEMOLISH-NOTEMPTYFIELD": errorMessage = "Nem lehet üres mezőről rombolni."; break;
                case "DEMOLISH-MAINROAD": errorMessage = "A főútat nem lehet lerombolni."; break;
                case "DEMOLISH - FIELDHASCIZIZEN": errorMessage = "Csak az üres zónát lehet visszaminősíteni."; break;
                case "DEMOLISH-FIELDPUBLICITY": errorMessage = "Az út rombolásával legalább egy épület elérhetetlenné válna.";break;
            }
            MessageBox.Show("A művelet nem végezhető el: \n"+errorMessage, "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void OnChangeMinimapSize()
        {
            MinimapMinimized = !MinimapMinimized;
        }

        private void OnSendFiretruckToSelectedFieldCommand()
        {
            if (_selectedField != null)
            {
                _model.DeployFireTruck(_selectedField.X, _selectedField.Y);
            }
        }

        public void ExitToMainMenu()
        {
            CreateTable();
            CreateToolbar();
            InputCityName = "";
            InputMayorName = "";
            IsPublicityToggled = false;
        }

        #endregion
    }
}
