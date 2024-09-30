using MasterApplication.Services.Feature.MouseClicker;

namespace MasterApplication.Tests.Services;

[ConstructorTests(typeof(KeyboardServiceTests))]
public partial class KeyboardServiceTests
{
    /*[Fact]
    public void GetKeyByCode_ValidKey_GetsTheNameOfTheKey()
    {
        // Arrange
        KeyboardService sut = new();
        Key escapeKey = Key.Escape;

        // Act
        string result = sut.GetKeyByCode((int)escapeKey);

        // Assert
        Assert.Equal(escapeKey.ToString(), result);
    }

    [Fact]
    public void GetKeyByCode_InvalidKey_GetsTheSameKey()
    {
        // Arrange
        KeyboardService sut = new();
        Key escapeKey = Key.Escape;

        // Act
        string result = sut.GetKeyByCode((int)escapeKey);

        // Assert
        Assert.Equal(escapeKey.ToString(), result);
    }*/

    [Fact]
    public void StartKeyboardHook_Execute_ThrowsExceptionIfCalledTwice()
    {
        // Arrange
        KeyboardService sut = new();

        // Act
        InvalidOperationException? exception = null;
        try
        {
            sut.StartKeyboardHook();
            sut.StartKeyboardHook();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("Keyboard hook is already active.", exception.Message);
    }

    [Fact]
    public void StopKeyboardHook_Execute_ThrowsExceptionIfWithoutBeingHooked()
    {
        // Arrange
        KeyboardService sut = new();

        // Act
        InvalidOperationException? exception = null;
        try
        {
            sut.StopKeyboardHook();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("Keyboard hook is not currently active.", exception.Message);
    }
}
