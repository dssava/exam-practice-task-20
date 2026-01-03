using System;
using System.Collections.Generic;
using System.Linq;
using BakeryTracker.Domain;

namespace BakeryTracker.Production
{
    public sealed class ProductionPlanItem
    {
        public ProductionPlanItem(DoughnutFlavor flavor, int targetQuantity)
        {
            if (targetQuantity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetQuantity), "Кількість має бути додатною.");
            }

            Flavor = flavor;
            TargetQuantity = targetQuantity;
        }

        public DoughnutFlavor Flavor { get; }

        public int TargetQuantity { get; }
    }

    // Поточний стан виконання плану по кожному сорту.
    public sealed class ProductionStatus
    {
        public ProductionStatus(DoughnutFlavor flavor, int targetQuantity, int producedQuantity)
        {
            Flavor = flavor;
            TargetQuantity = targetQuantity;
            ProducedQuantity = producedQuantity;
        }

        public DoughnutFlavor Flavor { get; }

        public int TargetQuantity { get; }

        public int ProducedQuantity { get; }

        public int RemainingQuantity => Math.Max(0, TargetQuantity - ProducedQuantity);
    }

    public interface IDoughnutFactory
    {
        Doughnut Create(DoughnutFlavor flavor);
    }

    // Інкапсулює правила створення пончика та ціноутворення.
    public sealed class DoughnutFactory : IDoughnutFactory
    {
        public Doughnut Create(DoughnutFlavor flavor)
        {
            return new Doughnut(flavor, GetPrice(flavor), DateTime.Now);
        }

        private static decimal GetPrice(DoughnutFlavor flavor)
        {
            return flavor switch
            {
                DoughnutFlavor.Vanilla => 18m,
                DoughnutFlavor.Chocolate => 20m,
                DoughnutFlavor.Strawberry => 19m,
                DoughnutFlavor.Caramel => 21m,
                DoughnutFlavor.Blueberry => 22m,
                _ => 0m
            };
        }
    }

    public sealed class BakeryProductionTracker
    {
        private readonly IDoughnutFactory _factory;
        private readonly int _batchSize;
        private readonly Dictionary<DoughnutFlavor, int> _targets = new();
        private readonly Dictionary<DoughnutFlavor, List<Doughnut>> _produced = new();

        public BakeryProductionTracker(IDoughnutFactory factory, int batchSize)
        {
            if (batchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Розмір партії має бути додатним.");
            }

            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _batchSize = batchSize;
        }

        public bool IsRunning { get; private set; }

        // Перевизначає план та підготовлює лічильники до запуску.
        public void Configure(IEnumerable<ProductionPlanItem> plan)
        {
            if (plan == null)
            {
                throw new ArgumentNullException(nameof(plan));
            }

            _targets.Clear();
            _produced.Clear();

            foreach (var item in plan)
            {
                _targets[item.Flavor] = item.TargetQuantity;
                _produced[item.Flavor] = new List<Doughnut>();
            }

            IsRunning = _targets.Count > 0;
        }

        // Виготовляє партію (до _batchSize) для кожного обраного сорту.
        public IReadOnlyList<ProductionStatus> ProduceBatch()
        {
            if (!IsRunning)
            {
                return GetStatuses();
            }

            foreach (var target in _targets)
            {
                var flavor = target.Key;
                var targetQuantity = target.Value;
                var producedList = _produced[flavor];

                var remaining = targetQuantity - producedList.Count;
                var toMake = Math.Min(_batchSize, remaining);

                for (var i = 0; i < toMake; i++)
                {
                    producedList.Add(_factory.Create(flavor));
                }
            }

            if (_targets.All(t => _produced[t.Key].Count >= t.Value))
            {
                IsRunning = false;
            }

            return GetStatuses();
        }

        // Повертає поточну статистику для таблиці.
        public IReadOnlyList<ProductionStatus> GetStatuses()
        {
            var statuses = new List<ProductionStatus>(_targets.Count);

            foreach (var target in _targets)
            {
                var flavor = target.Key;
                var targetQuantity = target.Value;
                var producedQuantity = _produced.TryGetValue(flavor, out var list) ? list.Count : 0;

                statuses.Add(new ProductionStatus(flavor, targetQuantity, producedQuantity));
            }

            return statuses;
        }
    }
}
