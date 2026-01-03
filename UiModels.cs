using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BakeryTracker.Domain;

namespace BakeryTracker.Ui
{
    // Рядок таблиці, що відображає план та факт виробництва сорту.
    public sealed class ProductionRow : INotifyPropertyChanged
    {
        private bool _isSelected;
        private int _targetQuantity;
        private int _producedQuantity;

        public ProductionRow(DoughnutFlavor flavor)
        {
            Flavor = flavor;
        }

        public DoughnutFlavor Flavor { get; }

        public string FlavorDisplay => DoughnutFlavorNames.ToDisplayName(Flavor);

        public bool IsSelected
        {
            get => _isSelected;
            set => SetField(ref _isSelected, value);
        }

        public int TargetQuantity
        {
            get => _targetQuantity;
            set
            {
                var sanitized = Math.Max(0, value);
                if (SetField(ref _targetQuantity, sanitized))
                {
                    OnPropertyChanged(nameof(RemainingQuantity));
                }
            }
        }

        public int ProducedQuantity
        {
            get => _producedQuantity;
            set
            {
                var sanitized = Math.Max(0, value);
                if (SetField(ref _producedQuantity, sanitized))
                {
                    OnPropertyChanged(nameof(RemainingQuantity));
                }
            }
        }

        public int RemainingQuantity => Math.Max(0, TargetQuantity - ProducedQuantity);

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Локалізовані назви сортів для відображення у UI.
    public static class DoughnutFlavorNames
    {
        private static readonly IReadOnlyDictionary<DoughnutFlavor, string> Map =
            new Dictionary<DoughnutFlavor, string>
            {
                { DoughnutFlavor.Vanilla, "Ванільний" },
                { DoughnutFlavor.Chocolate, "Шоколадний" },
                { DoughnutFlavor.Strawberry, "Полуничний" },
                { DoughnutFlavor.Caramel, "Карамельний" },
                { DoughnutFlavor.Blueberry, "Чорничний" }
            };

        public static IReadOnlyList<DoughnutFlavor> AllFlavors { get; } =
            Enum.GetValues<DoughnutFlavor>();

        public static string ToDisplayName(DoughnutFlavor flavor)
        {
            return Map.TryGetValue(flavor, out var name) ? name : flavor.ToString();
        }
    }
}
