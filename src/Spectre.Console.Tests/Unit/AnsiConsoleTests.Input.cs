namespace Spectre.Console.Tests.Unit;

public partial class AnsiConsoleTests
{
    public sealed class Input
    {
        [Fact]
        public void Should_Use_Supplied_Input()
        {
            // Given
            var input = new TestConsoleInput();

            // When
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(new StringWriter()),
                In = input,
            });

            // Then
            console.Input.ShouldBeSameAs(input);
        }

        [Fact]
        public void Should_Use_Default_Input_When_None_Supplied()
        {
            // Given, When
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(new StringWriter()),
            });

            // Then
            console.Input.ShouldNotBeNull();
        }

        [Fact]
        public void Should_Read_Keys_From_Supplied_Input()
        {
            // Given
            var input = new TestConsoleInput();
            input.PushKey(ConsoleKey.Enter);
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Out = new AnsiConsoleOutput(new StringWriter()),
                In = input,
            });

            // When
            var key = console.Input.ReadKey(true);

            // Then
            key.ShouldNotBeNull();
            key.Value.Key.ShouldBe(ConsoleKey.Enter);
        }
    }
}
