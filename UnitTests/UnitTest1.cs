using Xunit;

namespace MyProject.Tests
{
    public class CalculatorTests
    {
        [Fact] // This attribute indicates a test method
        public void Add_TwoNumbers_ReturnsCorrectSum()
        {
            // Arrange
            var calculator = new Calculator(); // Your class under test
            var a = 2;
            var b = 3;

            // Act
            var result = calculator.Add(a, b);

            // Assert
            Assert.Equal(5, result);
        }
    }

    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
