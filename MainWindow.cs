using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Threading;
using BakeryTracker.Production;
using BakeryTracker.Ui;

namespace BakeryTracker
{
    public sealed class MainWindow : Window
    {
        private const int BatchSize = 50;
        private static readonly TimeSpan BatchInterval = TimeSpan.FromSeconds(10);

        private readonly BakeryProductionTracker _tracker;
        private readonly ObservableCollection<ProductionRow> _rows;
        private readonly DataGrid _grid;
        private readonly Button _startButton;
        private readonly TextBlock _statusText;
        private readonly DispatcherTimer _timer;

        public MainWindow()
        {
            Title = "Пекарня: контроль виробництва пончиків";
            Width = 900;
            Height = 520;
            MinWidth = 720;
            MinHeight = 420;

            _tracker = new BakeryProductionTracker(new DoughnutFactory(), BatchSize);
            _rows = new ObservableCollection<ProductionRow>(CreateRows());

            _grid = CreateGrid();
            _grid.ItemsSource = _rows;

            _startButton = new Button
            {
                Content = "Старт",
                Width = 120,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _startButton.Click += StartButton_Click;

            _statusText = new TextBlock
            {
                Text = "Готово до старту.",
                VerticalAlignment = VerticalAlignment.Center
            };

            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Margin = new Thickness(12, 12, 12, 6)
            };
            header.Children.Add(_startButton);
            header.Children.Add(_statusText);

            var root = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*")
            };
            root.Children.Add(header);
            Grid.SetRow(_grid, 1);
            root.Children.Add(_grid);

            Content = root;

            // Таймер керує виготовленням партій кожні 10 секунд.
            _timer = new DispatcherTimer { Interval = BatchInterval };
            _timer.Tick += Timer_Tick;
        }

        // Створює рядки для всіх доступних сортів.
        private static List<ProductionRow> CreateRows()
        {
            var rows = new List<ProductionRow>();

            foreach (var flavor in DoughnutFlavorNames.AllFlavors)
            {
                rows.Add(new ProductionRow(flavor));
            }

            return rows;
        }

        private static DataGrid CreateGrid()
        {
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                GridLinesVisibility = DataGridGridLinesVisibility.All,
                SelectionMode = DataGridSelectionMode.Single,
                Margin = new Thickness(12, 6, 12, 12)
            };

            grid.Columns.Add(new DataGridCheckBoxColumn
            {
                Header = "Виробляти",
                Binding = new Binding(nameof(ProductionRow.IsSelected), BindingMode.TwoWay),
                Width = new DataGridLength(90)
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Сорт",
                Binding = new Binding(nameof(ProductionRow.FlavorDisplay)),
                IsReadOnly = true,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Потрібно, шт",
                Binding = new Binding(nameof(ProductionRow.TargetQuantity), BindingMode.TwoWay),
                Width = new DataGridLength(130)
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Виготовлено, шт",
                Binding = new Binding(nameof(ProductionRow.ProducedQuantity)),
                IsReadOnly = true,
                Width = new DataGridLength(150)
            });
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Залишилось, шт",
                Binding = new Binding(nameof(ProductionRow.RemainingQuantity)),
                IsReadOnly = true,
                Width = new DataGridLength(150)
            });

            return grid;
        }

        private void StartButton_Click(object? sender, EventArgs e)
        {
            var plan = BuildPlan();
            if (plan.Count == 0)
            {
                SetRunningState(false, "Оберіть сорт і задайте кількість для виробництва.");
                return;
            }

            ResetProducedCounts();
            _tracker.Configure(plan);
            UpdateRows(_tracker.GetStatuses());

            SetRunningState(true);
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var statuses = _tracker.ProduceBatch();
            UpdateRows(statuses);

            if (!_tracker.IsRunning)
            {
                _timer.Stop();
                SetRunningState(false, "План виробництва виконано.");
            }
        }

        // Формує план з вибраних користувачем рядків.
        private List<ProductionPlanItem> BuildPlan()
        {
            var plan = new List<ProductionPlanItem>();

            foreach (var row in _rows.Where(r => r.IsSelected))
            {
                if (row.TargetQuantity <= 0)
                {
                    continue;
                }

                plan.Add(new ProductionPlanItem(row.Flavor, row.TargetQuantity));
            }

            return plan;
        }

        // Очищає лічильники перед новим запуском.
        private void ResetProducedCounts()
        {
            foreach (var row in _rows)
            {
                row.ProducedQuantity = 0;
            }
        }

        // Синхронізує таблицю зі станом виробництва.
        private void UpdateRows(IReadOnlyList<ProductionStatus> statuses)
        {
            foreach (var status in statuses)
            {
                var row = _rows.FirstOrDefault(r => r.Flavor == status.Flavor);
                if (row == null)
                {
                    continue;
                }

                row.TargetQuantity = status.TargetQuantity;
                row.ProducedQuantity = status.ProducedQuantity;
            }
        }

        // Блокує редагування під час виробництва та оновлює статус.
        private void SetRunningState(bool isRunning, string? statusOverride = null)
        {
            _grid.IsEnabled = !isRunning;
            _startButton.IsEnabled = !isRunning;
            _statusText.Text = statusOverride ?? (isRunning
                ? "Виробництво запущено. Оновлення кожні 10 секунд."
                : "Готово до старту.");
        }
    }
}
