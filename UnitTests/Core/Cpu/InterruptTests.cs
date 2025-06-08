using Xunit;
using NEStor.Core.Cpu;

namespace NEStor.Core.Cpu
{
    public class InterruptTests
    {
        private class MockCycleProvider : ICycleProviding
        {
            public int Cycle { get; set; }
        }

        [Fact]
        public void Start_SetsEnabledAndCycleAndDelay()
        {
            var cycleProvider = new MockCycleProvider { Cycle = 10 };
            var interrupt = new Interrupt(cycleProvider);

            interrupt.Start(5);

            Assert.True(interrupt.Enabled);
            Assert.Equal(10, interrupt.Cycle);
            Assert.Equal(5, interrupt.Delay);
        }

        [Fact]
        public void IsActive_ReturnsTrue_WhenEnabledAndCyclesPassedGreaterOrEqualDelay()
        {
            var cycleProvider = new MockCycleProvider { Cycle = 15 };
            var interrupt = new Interrupt(cycleProvider);
            interrupt.Start(3); // sets Cycle to 15, Delay to 3

            // Simulate cycles passing
            cycleProvider.Cycle = 19; // CyclesPassed = 4

            Assert.True(interrupt.IsActive);
        }

        [Fact]
        public void IsReady_ReturnsTrue_WhenEnabledAndCyclesPassedEqualsDelay()
        {
            var cycleProvider = new MockCycleProvider { Cycle = 20 };
            var interrupt = new Interrupt(cycleProvider);
            interrupt.Start(4); // sets Cycle to 20, Delay to 4

            cycleProvider.Cycle = 24; // CyclesPassed = 4

            Assert.True(interrupt.IsReady);
        }

        [Fact]
        public void Acknowledge_DisablesInterrupt()
        {
            var cycleProvider = new MockCycleProvider { Cycle = 5 };
            var interrupt = new Interrupt(cycleProvider);
            interrupt.Start(2);

            interrupt.Acknowledge();

            Assert.False(interrupt.Enabled);
        }

        [Fact]
        public void IsActiveAndIsReady_ReturnFalse_WhenNotEnabled()
        {
            var cycleProvider = new MockCycleProvider { Cycle = 100 };
            var interrupt = new Interrupt(cycleProvider);

            Assert.False(interrupt.IsActive);
            Assert.False(interrupt.IsReady);
        }
    }
}