using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;
using UnitsNet;
using Wpf_GenericUnitConverter.Annotations;

namespace Wpf_GenericUnitConverter
{
    public sealed class MainWindowVm : IMainWindowVm
    {
        private readonly ObservableCollection<UnitPresenter> _units;
        private decimal _fromValue;

        [CanBeNull] private UnitPresenter _selectedFromUnit;

        private QuantityType _selectedQuantity;

        [CanBeNull] private UnitPresenter _selectedToUnit;

        private decimal _toValue;

        public MainWindowVm()
        {
            Quantities = ToReadOnly(Enum.GetValues(typeof(QuantityType)).Cast<QuantityType>().Skip(1));

            _units = new ObservableCollection<UnitPresenter>();
            Units = new ReadOnlyObservableCollection<UnitPresenter>(_units);
            BindingOperations.EnableCollectionSynchronization(_units, this); // Cross-thread safety

            FromValue = 1;
            SwapCommand = new DelegateCommand(Swap);

            OnSelectedQuantity(QuantityType.Length);
        }

        public ICommand SwapCommand { get; }

        public ReadOnlyObservableCollection<QuantityType> Quantities { get; }
        public ReadOnlyObservableCollection<UnitPresenter> Units { get; }

        public QuantityType SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                if (value == _selectedQuantity) return;
                _selectedQuantity = value;
                OnPropertyChanged();
                OnSelectedQuantity(value);
            }
        }

        public UnitPresenter SelectedFromUnit
        {
            get => _selectedFromUnit;
            set
            {
                if (Equals(value, _selectedFromUnit)) return;
                _selectedFromUnit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FromHeader));
                UpdateResult();
            }
        }

        public UnitPresenter SelectedToUnit
        {
            get => _selectedToUnit;
            set
            {
                if (Equals(value, _selectedToUnit)) return;
                _selectedToUnit = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ToHeader));
                UpdateResult();
            }
        }

        public string FromHeader => $"Value [{SelectedFromUnit.Abbreviation}]";

        public string ToHeader => $"Result [{SelectedToUnit.Abbreviation}]";

        public decimal FromValue
        {
            get => _fromValue;
            set
            {
                if (value == _fromValue) return;
                _fromValue = value;
                OnPropertyChanged();
                UpdateResult();
            }
        }

        public decimal ToValue
        {
            get => _toValue;
            private set
            {
                if (value == _toValue) return;
                _toValue = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Swap()
        {
            UnitPresenter oldToUnit = SelectedToUnit;
            decimal oldToValue = ToValue;

            // Setting these will change ToValue
            SelectedToUnit = SelectedFromUnit;
            SelectedFromUnit = oldToUnit;

            FromValue = oldToValue;
        }

        private void UpdateResult()
        {
            if (SelectedFromUnit == null || SelectedToUnit == null) return;

            ToValue = Convert.ToDecimal(UnitConverter.ConvertByName(FromValue,
                SelectedQuantity.ToString(),
                SelectedFromUnit.UnitEnumValue.ToString(),
                SelectedToUnit.UnitEnumValue.ToString()));
        }

        private void OnSelectedQuantity(QuantityType quantity)
        {
            _units.Clear();

            // Ex: Find unit enum type UnitsNet.Units.LengthUnit from quantity enum value QuantityType.Length
            Type unitEnumType = Assembly.GetAssembly(typeof(Length)).ExportedTypes.First(t => t.FullName == $"UnitsNet.Units.{quantity}Unit");
            IEnumerable<object> unitValues = Enum.GetValues(unitEnumType).Cast<object>().Skip(1);
            foreach (object unitValue in unitValues) _units.Add(new UnitPresenter(unitValue));

            SelectedQuantity = quantity;
            SelectedFromUnit = Units.FirstOrDefault();
            SelectedToUnit = Units.Skip(1).FirstOrDefault() ?? Units.FirstOrDefault();
        }

        private static ReadOnlyObservableCollection<T> ToReadOnly<T>(IEnumerable<T> items)
        {
            return new ReadOnlyObservableCollection<T>(new ObservableCollection<T>(items));
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}