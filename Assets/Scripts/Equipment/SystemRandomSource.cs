using System;

namespace IdleGame.Equipment
{
    public class SystemRandomSource : IRandomSource
    {
        private readonly Random _random = new Random();

        public double NextDouble() => _random.NextDouble();
    }
}
