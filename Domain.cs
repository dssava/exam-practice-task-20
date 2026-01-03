using System;

namespace BakeryTracker.Domain
{
    public enum DoughnutFlavor
    {
        Vanilla,
        Chocolate,
        Strawberry,
        Caramel,
        Blueberry
    }

    // Сутність «Пончик» з незмінними властивостями.
    public sealed class Doughnut
    {
        public Doughnut(DoughnutFlavor flavor, decimal price, DateTime producedAt)
        {
            Flavor = flavor;
            Price = price;
            ProducedAt = producedAt;
        }

        public DoughnutFlavor Flavor { get; }

        public decimal Price { get; }

        // Час виготовлення задається під час створення і не змінюється.
        public DateTime ProducedAt { get; }
    }
}
