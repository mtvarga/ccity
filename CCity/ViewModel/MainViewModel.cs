using CCity.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using CCity.ViewModel.Items;
using CCity.ViewModel.Enums;
using System.Reflection.Metadata;

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
        private bool _isLogbookToggled;
        private bool _paused;

        #endregion

        #region Properties

        #region Collections

            public ObservableCollection<FieldItem> Fields { get; private set; }
            public ObservableCollection<ToolItem> Tools { get; private set; }
            public ObservableCollection<TransactionItem> Logbook { get; private set; }

        #endregion

        #region StartWindow

        public string OutputCityName => _inputCityName == "" ? "Városvezetés a hobbim." : _inputCityName + " büszke polgármestere vagyok.";
        public string OutputMayorName => _inputMayorName == "" ? "Polgármester" : _inputMayorName;
        public bool CanStart => _inputMayorName != "" && _inputCityName != "";

        #endregion

        #region Model game data

        public string MayorName => _model.MayorName;
        public string CityName => _model.CityName;
        public int Width => _model.Width;
        public int Height => _model.Height;
        public DateTime Date => _model.Date;
        public int Budget => _model.Budget;
        public int Satisfaction => PercentToInt(_model.Satisfaction);
        public int Population => _model.Population;
        public Speed Speed => _model.Speed;
        public int CommercialTax => PercentToInt(_model.Taxes.CommercialTax);
        public int ResidentialTax => PercentToInt(_model.Taxes.ResidentialTax);
        public int IndustrialTax => PercentToInt(_model.Taxes.IndustrialTax);

        #endregion

        #region SelectedField related

        public bool IsFieldSelected => _selectedField != null;
        public string SelectedFieldName => IsFieldSelected ? GetPlaceableName(_selectedField!.Placeable) : "";
        public bool SelectedFieldIsUpgradeable => IsFieldSelected && _selectedField!.Placeable is IUpgradeable && ((IUpgradeable)(_selectedField!.Placeable)!).CanUpgrade;
        public int SelectedFieldUpgradeCost => SelectedFieldIsUpgradeable ? ((IUpgradeable)(_selectedField!.Placeable)!).NextUpgradeCost : 0;
        public int SelectedFieldPoliceDepartmentEffect => IsFieldSelected ? PercentToInt(_selectedField!.PoliceDepartmentEffect) : 0;
        public int SelectedFieldFireDepartmentEffect => IsFieldSelected ? PercentToInt(_selectedField!.FireDepartmentEffect) : 0;
        public int SelectedFieldStadiumEffect => IsFieldSelected ? PercentToInt(_selectedField!.StadiumEffect) : 0;
        public int SelectedFieldIndustrialEffect => IsFieldSelected ? PercentToInt(_selectedField!.IndustrialEffect) : 0;
        public int SelectedFieldForestEffect => IsFieldSelected ? PercentToInt(_selectedField!.ForestEffect) : 0;
        public int SelectedFieldSatisfaction => SelectedFieldIsZone ? PercentToInt(_model.ZoneSatisfaction((Zone)_selectedField!.Placeable!)) : 0;
        public int SelectedFieldPopulation => SelectedFieldIsZone ? ((Zone)_selectedField!.Placeable!).Count : 0;
        public int SelectedFieldCapacity => SelectedFieldIsZone ? ((Zone)_selectedField!.Placeable!).Capacity : 0;
        public bool SelectedFieldIsZone => IsFieldSelected && _selectedField!.Placeable is Zone;
        public int SelectedFieldCurrentElectricity => IsFieldSelected && _selectedField!.HasPlaceable ? _selectedField.Placeable!.CurrentSpreadValue[SpreadType.Electricity] : 0;
        public int SelectedFieldNeededElectricity => IsFieldSelected && _selectedField!.HasPlaceable ? _selectedField.Placeable!.MaxSpreadValue[SpreadType.Electricity]() : 0;
        public bool SelectedFieldShowsElectricity => SelectedFieldNeededElectricity > 0 || IsFieldSelected && _selectedField!.Placeable is PowerPlant;
        public bool SelectedFieldIsIncinerated => SelectedFieldIsFlammable && ((IFlammable)_selectedField!.Placeable!).Burning;
        public bool SelectedFieldIsFlammable => IsFieldSelected && _selectedField!.HasPlaceable && _selectedField!.Placeable is IFlammable;
        public int SelectedFieldHealth => SelectedFieldIsFlammable ? PercentToInt(((IFlammable)_selectedField!.Placeable!).Health / (double)IFlammable.FlammableMaxHealth) : 0;
        public bool SelectedFieldIsForest => IsFieldSelected && _selectedField!.Placeable is Forest;
        public int SelectedFieldForestAge => SelectedFieldIsForest ? ((Forest)(_selectedField!.Placeable)!).Age : 0;
        public bool SelectedFieldIsResidentialZone => IsFieldSelected && _selectedField!.Placeable is ResidentialZone;
        public int SelectedFieldResidentialZoneDesireToMoveIn => SelectedFieldIsResidentialZone ? PercentToInt(((ResidentialZone)(_selectedField!.Placeable)!).DesireToMoveIn) : 0;

        #endregion

        #region SelectedTool related

        public Tool SelectedTool => _selectedToolItem.Tool;
        public bool SelectedToolHasInfo => GetToolPlaceable(SelectedTool) != null;
        public string SelectedToolPlaceableName => SelectedToolHasInfo ? GetPlaceableName(GetToolPlaceable(SelectedTool)) : "";
        public int SelectedToolPlaceableWidth => SelectedToolHasInfo ? GetPlaceableInfo(GetToolPlaceable(SelectedTool)!).Width : 0;
        public int SelectedToolPlaceableHeight => SelectedToolHasInfo ? GetPlaceableInfo(GetToolPlaceable(SelectedTool)!).Height : 0;
        public int SelectedToolPlaceablePlacementCost => SelectedToolHasInfo ? GetPlaceableInfo(GetToolPlaceable(SelectedTool)!).PlacementCost : 0;
        public int SelectedToolPlaceableMaintenanceCost => SelectedToolHasInfo ? GetPlaceableInfo(GetToolPlaceable(SelectedTool)!).MaintenanceCost : 0;

        #endregion

        #region OnPropertyChanged

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

        public bool Paused
        {
            get { return _paused; }
            set
            {
                if(value != _paused)
                {
                    _paused = value;
                    OnPropertyChanged(nameof(Paused));
                }
            }
        }

        public bool MinimapMinimized
        { 
            get { return _minimapMinimized; }
            set
            {
                if(value != _minimapMinimized)
                { 
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

        public bool IsLogbookToggled
        {
            get { return _isLogbookToggled; }
            set
            {
                if (value != _isLogbookToggled)
                {
                    _isLogbookToggled = value;
                    OnPropertyChanged(nameof(IsLogbookToggled));
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

        #endregion

        #endregion

        #region Events

        public event EventHandler? NewGame;
        public event EventHandler? PauseGame;
        public event EventHandler? ExitGame;
        public event EventHandler? CloseApplication;

        #endregion

        #region Commands

        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand PauseGameCommand { get; private set; }
        public DelegateCommand ExitGameCommand { get; private set; }
        public DelegateCommand CloseApplicationCommand { get; private set; }
        public DelegateCommand ChangeResidentialTaxCommand { get; private set; }
        public DelegateCommand ChangeCommercialTaxCommand { get; private set; }
        public DelegateCommand ChangeIndustrialTaxCommand { get; private set; }
        public DelegateCommand ToggleLogbookCommand { get; private set; }
        public DelegateCommand ChangeSpeedCommand { get; } 
        public DelegateCommand SendFiretruckToSelectedFieldCommand { get; private set; }
        public DelegateCommand UpgradeSelectedFieldCommand { get; private set; }
        public DelegateCommand ChangeMinimapSizeCommand { get; private set; }
        public DelegateCommand CloseSelectedFieldWindowCommand { get; private set; }
        public DelegateCommand StartNewGameCommand { get; private set; }
        public DelegateCommand TogglePublicityCommand { get; private set; }
        public DelegateCommand ToggleElectricityCommand { get; private set; }

        #endregion

        #region Constructors

        // Fields is declared in CreateTable(), Tools and _selectedToolItem are declared in CreateToolbar();
        public MainViewModel(MainModel model)
        {
            _model = model;
            _model.FieldsUpdated += Model_FieldUpdated;
            _model.GameTicked += Model_GameTicked;
            _model.ErrorOccured += Model_ErrorOccured;
            _model.TaxChanged += Model_TaxChanged;
            _model.SatisfactionChanged += Model_SatisfactionChanged;
            _model.BudgetChanged += Model_BudgetChanged;
            _model.PopulationChanged += Model_PopulationChanged;
            _model.SpeedChanged += Model_SpeedChanged;
            _model.FireTruckMoved += Model_FireTruckMoved;
            _model.DateChanged += Model_DateChanged;
                
            CreateTable();
            CreateToolbar();
            Logbook = new();

            NewGameCommand = new DelegateCommand(_ => OnNewGame());
            PauseGameCommand = new DelegateCommand(_ => OnPauseGame());
            ExitGameCommand = new DelegateCommand(_ => OnExitGame());
            CloseApplicationCommand = new DelegateCommand(_ => OnCloseApplication());
            ChangeResidentialTaxCommand = new DelegateCommand(param => OnChangeResidentialTax(int.Parse(param as string ?? string.Empty)));
            ChangeCommercialTaxCommand = new DelegateCommand(param => OnChangeCommercialTax(int.Parse(param as string ?? string.Empty)));
            ChangeIndustrialTaxCommand = new DelegateCommand(param => OnChangeIndustrialTax(int.Parse(param as string ?? string.Empty)));
            CloseSelectedFieldWindowCommand = new DelegateCommand(OnCloseSelectedFieldWindow);
            StartNewGameCommand = new DelegateCommand(_ => OnStartNewGame());
            TogglePublicityCommand = new DelegateCommand(_ => OnTogglePublicity());
            ToggleElectricityCommand = new DelegateCommand(_ => OnToggleElecticity());
            ChangeMinimapSizeCommand = new DelegateCommand(_ => OnChangeMinimapSize());
            ChangeSpeedCommand = new DelegateCommand(param => OnChangeSpeedCommand(int.Parse(param as string ?? string.Empty)));
            UpgradeSelectedFieldCommand = new DelegateCommand(_ => OnUpgradeCommand());
            SendFiretruckToSelectedFieldCommand = new DelegateCommand(_ => OnSendFiretruckToSelectedFieldCommand());
            ToggleLogbookCommand = new DelegateCommand(_ => OnToggleLogbookCommand());

            _inputCityName = "";
            _inputMayorName = "";
            IsPublicityToggled = false;
            IsElectricityToggled = false;
        }

        #endregion

        #region Private methods

        #region Collection declaration

        private void CreateTable()
        {
            Fields = new();
            for(int i = 0; i < Height; i++) {
                for(int j = 0; j < Width; j++)
                {
                    Fields.Add(new FieldItem
                    {
                        Texture = Texture.None,
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
                #if DEBUG
                Tool.FlintAndSteel,
                #endif
                Tool.Bulldozer
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

        #endregion

        #region FieldItem refresh related

        private void RefreshFieldItem(FieldItem fieldItem, bool onlyOverlayColor = false)
        {
            fieldItem.OverlayColor = GetOverlayColorFromFieldItem(fieldItem);
            if (onlyOverlayColor) return;
            fieldItem.Texture = GetTextureFromFieldItem(fieldItem);
            fieldItem.AdditionalTexture = GetAdditionalTextureFromFieldItem(fieldItem, fieldItem.AdditionalTexture);
        }

        private Color GetOverlayColorFromFieldItem(FieldItem fieldItem)
        {
            Field field = _model.Fields[fieldItem.X, fieldItem.Y];
            if (!field.HasPlaceable) return Color.FromArgb(0, 0, 0, 0);
            if(field.Placeable is Zone zone && zone.Empty) return zone switch
            {
                ResidentialZone _ => Color.FromArgb(50, 0, 255, 0),
                CommercialZone _ => Color.FromArgb(50, 0, 0, 255),
                IndustrialZone _ => Color.FromArgb(50, 255, 255, 0),
                _ => Color.FromArgb(0, 0, 0, 0),
            };
            if (field.Placeable!.IsPublic && field.Placeable is not Forest && field.Placeable is not Pole && IsPublicityToggled) return Color.FromArgb(50, 22, 32, 255);
            if (field.Placeable!.IsElectrified && IsElectricityToggled) return Color.FromArgb(50, 255, 255, 32);
            else return Color.FromArgb(0, 0, 0, 0);
        }

        private Texture GetTextureFromFieldItem(FieldItem fieldItem)
        {
            Field field = _model.Fields[fieldItem.X, fieldItem.Y];
            return (!field.HasPlaceable) ? Texture.None : GetTextureFromPlaceable(field.ActualPlaceable!);
        }
        
        private Texture GetAdditionalTextureFromFieldItem(FieldItem fieldItem, Texture oldValue) => _model.Fields[fieldItem.X, fieldItem.Y] switch
        {
            { Placeable: Road } => oldValue,
            { Placeable: IFlammable { Burning: true } } => Texture.Fire,
            _ => Texture.None
        };

        private Texture GetTextureFromPlaceable(Placeable placeable) => placeable switch
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

        private static Texture GetForestTexture(Forest forest) => Texture.Forest + ((forest.Age * 2) / forest.MaxAge);

        private Texture GetZoneTexture(Zone zone)
        {
            if (zone.Empty) return Texture.None;
            if (!Enum.TryParse(zone switch
            {
                ResidentialZone => nameof(ResidentialZone),
                CommercialZone => nameof(CommercialZone),
                _ => nameof(IndustrialZone),
            } + ((IUpgradeable)zone).Level.ToString() + "Half", out Texture texture)) return Texture.Unhandled;
            return zone.BelowHalfPopulation ? texture : texture + 1;
        }

        private Texture GetFillerTexture(Filler filler)
        {
            (Field mainField, Field fillerField) = (((Placeable)filler.Main).Owner!, filler.Owner!);
            (int x, int y) = (fillerField.X - mainField.X, mainField.Y - fillerField.Y);
            return Enum.TryParse($"{GetTextureFromPlaceable((Placeable)filler.Main)}_{x}_{y}", out Texture texture) ? texture : Texture.Unhandled;
        }

        private void SetNeighboursRoadTexture(Road road)
        {
            (_, List<Road> neighbours) = _model.GetFourRoadNeighbours(road);
            foreach(Road neighbour in neighbours)
                Fields[GetIndexFromField(neighbour.Owner!)].Texture = GetRoadTexture(neighbour);
        }

        private Texture ReturnAndHandleRoadTexture(Road road)
        {
            SetNeighboursRoadTexture(road);
            return GetRoadTexture(road);
        }

        private Texture GetRoadTexture(Road road)
        {
            ((byte t, byte r, byte b, byte l), _) = _model.GetFourRoadNeighbours(road);
            return Enum.TryParse($"Road_{t}_{r}_{b}_{l}", out Texture texture) ? texture : Texture.Unhandled;
        }

        #endregion

        #region Item clicked

        private void SelectField(int index)
        {
            (int x, int y) = GetCordinates(index);
            _selectedField = _model.Fields[x, y];
            InvokeSelectedFieldRelatedPropertyChanges();
        }

        private void FieldClicked(int index)
        {
            (int x, int y) = GetCordinates(index);
            Placeable? placeable = GetToolPlaceable(SelectedTool);
            if(placeable == null)
            {
                switch (SelectedTool)
                {
                    case Tool.Cursor: SelectField(index); break;
                    case Tool.Bulldozer: _model.Demolish(x, y); break;
                    case Tool.FlintAndSteel: _model.IgniteBuilding(x, y); break;
                    default: throw new Exception();
                }
            } else
            {
                _model.Place(x, y, placeable);
            }
        }

        private void ToolClicked(int index)
        {
            _selectedToolItem.IsSelected = false;
            _selectedToolItem = Tools[index];
            _selectedToolItem.IsSelected = true;
            if (SelectedTool != Tool.Cursor) UnselectField();
            OnPropertyChanged(nameof(SelectedTool));
            OnPropertyChanged(nameof(SelectedToolHasInfo));
            OnPropertyChanged(nameof(SelectedToolPlaceableName));
            OnPropertyChanged(nameof(SelectedToolPlaceableWidth));
            OnPropertyChanged(nameof(SelectedToolPlaceableHeight));
            OnPropertyChanged(nameof(SelectedToolPlaceablePlacementCost));
            OnPropertyChanged(nameof(SelectedToolPlaceableMaintenanceCost));
        }

        #endregion

        #region Transaction log related

        private string GetPlaceableTransactionName(PlaceableTransactionType placeableTransactionType) => placeableTransactionType switch
        {
            PlaceableTransactionType.Placement => "lehelyezés",
            PlaceableTransactionType.Maintenance => "fenntartás",
            PlaceableTransactionType.Upgrade => "fejlesztés",
            PlaceableTransactionType.Takeback => "visszatérítés",
            _ => throw new ArgumentException()
        };

        private Placeable GetPlaceableFromTaxType(TaxType taxType) => taxType switch
        {
            TaxType.Residental => new ResidentialZone(),
            TaxType.Commercial => new CommercialZone(),
            TaxType.Industrial => new IndustrialZone(),
            _ => throw new ArgumentException()
        };

        private TransactionItem GetTransactionItemFromITransaction(ITransaction transaction) => new TransactionItem 
        { 
            Amount = transaction.Amount,
            Add = transaction.Add,
            TransactionName = transaction switch {
                PlaceableTransaction placeableTransaction => $"{GetPlaceableName(placeableTransaction.Placeable)} {GetPlaceableTransactionName(placeableTransaction.TransactionType)}",
                TaxTransaction taxTransaction => $"{GetPlaceableName(GetPlaceableFromTaxType(taxTransaction.TaxType))} adó beszedés",
                _ => throw new ArgumentException()
            }
        };

        private void RefreshTransactionLog()
        {
            Logbook.Clear();
            foreach(ITransaction transatcion in _model.Logbook)
                Logbook.Add(GetTransactionItemFromITransaction(transatcion));
            OnPropertyChanged(nameof(Logbook));
        }

        #endregion

        #region Helpers

        private void InvokeSelectedFieldRelatedPropertyChanges()
        {
            OnPropertyChanged(nameof(IsFieldSelected));
            OnPropertyChanged(nameof(SelectedFieldName));
            OnPropertyChanged(nameof(SelectedFieldPoliceDepartmentEffect));
            OnPropertyChanged(nameof(SelectedFieldFireDepartmentEffect));
            OnPropertyChanged(nameof(SelectedFieldStadiumEffect));
            OnPropertyChanged(nameof(SelectedFieldIndustrialEffect));
            OnPropertyChanged(nameof(SelectedFieldPopulation));
            OnPropertyChanged(nameof(SelectedFieldCapacity));
            OnPropertyChanged(nameof(SelectedFieldSatisfaction));
            OnPropertyChanged(nameof(SelectedFieldIsZone));
            OnPropertyChanged(nameof(SelectedFieldForestEffect));
            OnPropertyChanged(nameof(SelectedFieldIsIncinerated));
            OnPropertyChanged(nameof(SelectedFieldIsFlammable));
            OnPropertyChanged(nameof(SelectedFieldHealth));
            OnPropertyChanged(nameof(SelectedFieldIsUpgradeable));
            OnPropertyChanged(nameof(SelectedFieldUpgradeCost));
            OnPropertyChanged(nameof(SelectedFieldCurrentElectricity));
            OnPropertyChanged(nameof(SelectedFieldNeededElectricity));
            OnPropertyChanged(nameof(SelectedFieldShowsElectricity));
            OnPropertyChanged(nameof(SelectedFieldIsForest));
            OnPropertyChanged(nameof(SelectedFieldForestAge));
            OnPropertyChanged(nameof(SelectedFieldIsResidentialZone));
            OnPropertyChanged(nameof(SelectedFieldResidentialZoneDesireToMoveIn));
        }

        private static int PercentToInt(double percent) => (int)Math.Floor(percent * 100);

        private (int, int) GetCordinates(int index) => (index % Width, (index - (index % Width)) / Width);

        private int GetIndexFromField(Field field) => field.X + field.Y * Width;

        private static (int Width, int Height, int PlacementCost, int MaintenanceCost) GetPlaceableInfo(Placeable placeable)
        {
            (int width, int height) = (1, 1);
            if (placeable is IMultifield multifield) (width, height) = (multifield.Width, multifield.Height);
            return (width, height, placeable.PlacementCost, placeable.MaintenanceCost);
        }

        private static Placeable? GetToolPlaceable(Tool tool) => tool switch
        {
            Tool.ResidentialZone => new ResidentialZone(),
            Tool.CommercialZone => new CommercialZone(),
            Tool.IndustrialZone => new IndustrialZone(),
            Tool.FireDepartment => new FireDepartment(),
            Tool.PoliceDepartment => new PoliceDepartment(),
            Tool.Stadium => new Stadium(),
            Tool.PowerPlant => new PowerPlant(),
            Tool.Pole => new Pole(),
            Tool.Road => new Road(),
            Tool.Forest => new Forest(),
            _ => null
        };

        private static string GetPlaceableName(Placeable? placeable) => placeable == null ? "Üres mező" : placeable switch
        {
            ResidentialZone _ => "Lakózóna",
            CommercialZone _ => "Kereskedelmi zóna",
            IndustrialZone _ => "Ipari zóna",
            Road road => road.IsPublic ? "Közút" : "Út",
            PoliceDepartment _ => "Rendőrség",
            PowerPlant _ => "Erőmű",
            FireDepartment _ => "Tűzoltóság",
            Stadium _ => "Stadion",
            Forest _ => "Erdő",
            Pole _ => "Távvezeték",
            _ => "Épület"
        };

        private void UnselectField()
        {
            _selectedField = null;
            OnPropertyChanged(nameof(IsFieldSelected));
        }

        public void ExitToMainMenu()
        {
            CreateTable();
            CreateToolbar();
            InputCityName = "";
            InputMayorName = "";
            IsPublicityToggled = false;
            IsElectricityToggled = false;
        }

        #endregion

        #endregion

        #region Model event methods

        private void Model_GameTicked(object? o, EventArgs e) => InvokeSelectedFieldRelatedPropertyChanges();

        private void Model_PopulationChanged(object? o, EventArgs e) => OnPropertyChanged(nameof(Population));

        private void Model_BudgetChanged(object? o, EventArgs e)
        {
            RefreshTransactionLog();
            OnPropertyChanged(nameof(Budget));
        }

        private void Model_SatisfactionChanged(object? o, EventArgs e) => OnPropertyChanged(nameof(Satisfaction));

        private void Model_TaxChanged(object? o, EventArgs e)
        {
            OnPropertyChanged(nameof(ResidentialTax));
            OnPropertyChanged(nameof(IndustrialTax));
            OnPropertyChanged(nameof(CommercialTax));
        }

        private void Model_FieldUpdated(object? o, FieldEventArgs e)
        {
            foreach (Field field in e.Fields)
                RefreshFieldItem(Fields[GetIndexFromField(field)]);

            InvokeSelectedFieldRelatedPropertyChanges();
        }

        private void Model_FireTruckMoved(object? sender, FieldEventArgs e)
        {
            foreach (Field field in e.Fields)
                Fields[GetIndexFromField(field)].AdditionalTexture = Texture.None;

            foreach (Field field in _model.FireTruckLocations())
                Fields[GetIndexFromField(field)].AdditionalTexture = Texture.Firetruck;
        }

        private void Model_DateChanged(object? sender, EventArgs e) => OnPropertyChanged(nameof(Date));

        private void Model_ErrorOccured(object? sender, ErrorEventArgs e)
        {
            MessageBox.Show("A művelet nem végezhető el: \n" + e.ErrorType switch
            {
                GameErrorType.PlaceOutOfFieldBoundries => "Nem építhetsz a pályán kívülre.",
                GameErrorType.PlaceAlreadyUsedField => "Csak pályán belüli üres mezőre a építhetsz.",
                GameErrorType.DemolishOutOfFieldBoundries => "Nem rombolhatsz a pályán kívülről.",
                GameErrorType.DemolishEmptyField => "Nem lehet üres mezőről rombolni.",
                GameErrorType.DemolishMainRoad => "A főútat nem lehet lerombolni.",
                GameErrorType.DemolishFieldHasCitizen => "Csak az üres zónát lehet visszaminősíteni.",
                GameErrorType.DemolishFieldPublicity => "Az út rombolásával legalább egy épület elérhetetlenné válna.",
                GameErrorType.DemolishFieldOnFire => "Nem lehet égő épületet törölni.",
                GameErrorType.DeployFireTruckNoFire => "Csak égő épülethez küldhetsz tűzoltóautót.",
                GameErrorType.DeployFireTruckOutOfFieldBounds => "Pályán kívüli mezőre nem küldhetsz tűzoltóautót.",
                GameErrorType.DeployFireTruckBadBuilding => "Az épület nem éghető.",
                GameErrorType.DeployFireTruckNoneAvaiable => "Nincs elérhető tűzoltóautó vagy nincs lehelyezve áram alatt levő tűzoltóság.",
                GameErrorType.DeployFireTruckAlreadyAssigned => "Erre a helyre már ki lett rendelve tűzoltóautó.",
                GameErrorType.Unhandled => "Valami hiba történt.",
                _ => "Kezeletlen hiba."
            }, "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Model_SpeedChanged(object? o, EventArgs e) => OnPropertyChanged(nameof(Speed));

        private void Model_NewGame(object? sender, EventArgs e) => NewGame?.Invoke(this, EventArgs.Empty);

        #endregion

        #region DelegateCommand methods

        private void OnStartNewGame()
        {
            _model.StartNewGame(_inputCityName, _inputMayorName);
            foreach (FieldItem fieldItem in Fields)
                RefreshFieldItem(fieldItem);
        }

        private void OnNewGame() => NewGame?.Invoke(this, EventArgs.Empty);

        private void OnPauseGame()
        {
            Paused = !Paused;
            PauseGame?.Invoke(this, EventArgs.Empty);
        }

        private void OnExitGame() => ExitGame?.Invoke(this, EventArgs.Empty);

        private void OnCloseApplication() => CloseApplication?.Invoke(this, EventArgs.Empty);

        private void OnTogglePublicity()
        {
            IsElectricityToggled = false;
            IsPublicityToggled = !IsPublicityToggled;
            foreach (FieldItem fieldItem in Fields)
                RefreshFieldItem(fieldItem, true);
        }

        private void OnToggleElecticity()
        {
            IsPublicityToggled = false;
            IsElectricityToggled = !IsElectricityToggled;
            foreach (FieldItem fieldItem in Fields)
                RefreshFieldItem(fieldItem, true);
        }

        private void OnChangeResidentialTax(int n) => _model.ChangeTax(TaxType.Residental, (n > 0 ? 1 : -1) * 0.01);

        private void OnChangeCommercialTax(int n) => _model.ChangeTax(TaxType.Commercial, (n > 0 ? 1 : -1) * 0.01);

        private void OnChangeIndustrialTax(int n) => _model.ChangeTax(TaxType.Industrial, (n > 0 ? 1 : -1) * 0.01);

        private void OnCloseSelectedFieldWindow(object? obj)
        {
            UnselectField();
            OnPropertyChanged(nameof(IsFieldSelected));
        }

        private void OnChangeSpeedCommand(int n) => _model.ChangeSpeed(n switch
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

        private void OnUpgradeCommand()
        {
            if (_selectedField != null)
            {
                _model.Upgrade(_selectedField.X, _selectedField.Y);
            }
        }

        private void OnChangeMinimapSize() => MinimapMinimized = !MinimapMinimized;

        private void OnSendFiretruckToSelectedFieldCommand()
        {
            if (_selectedField != null)
            {
                _model.DeployFireTruck(_selectedField.X, _selectedField.Y);
            }
        }

        private void OnToggleLogbookCommand() => IsLogbookToggled = !IsLogbookToggled;
        
        #endregion
    }
}
