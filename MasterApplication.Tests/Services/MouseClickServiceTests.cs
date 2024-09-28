using MasterApplication.Models.Structs;
using MasterApplication.Services.Feature.MouseClicker;

namespace MasterApplication.Tests.Services;

[ConstructorTests(typeof(MouseClickServiceTests))]
public partial class MouseClickServiceTests
{
    [Fact]
    public void GetMousePos_ValidCoordinates_ReturnsCurrentPosition()
    {
        // Arrange
        MouseService sut = new();
        MouseCoordinate expectedPos = new(100, 200);

        // Act
        sut.MoveCursorTo(expectedPos.X, expectedPos.Y);
        MouseCoordinate result = sut.GetMousePos();

        //Assert
        Assert.Equal(expectedPos.X, result.X);
        Assert.Equal(expectedPos.Y, result.Y);
    }

    [Fact]
    public void MoveCursorTo_ValidCoordinates_MovesCursor()
    {
        // Arrange
        MouseService sut = new();
        MouseCoordinate newMouseCoordinate = new(100, 200);

        // Act
        sut.MoveCursorTo(newMouseCoordinate.X, newMouseCoordinate.Y);
        MouseCoordinate currentMouseCoordinates = sut.GetMousePos();

        // Assert that the current mouse position matches the expected position
        Assert.Equal(newMouseCoordinate.X, currentMouseCoordinates.X);
        Assert.Equal(newMouseCoordinate.Y, currentMouseCoordinates.Y);
    }

    [Fact]
    public void MoveCursorTo_NegativeCoordinates_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        MouseService sut = new();

        // Act
        ArgumentOutOfRangeException? exception = null;
        try
        {
            sut.MoveCursorTo(-100, -200);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            exception = ex;
        }

        //Assert
        Assert.NotNull(exception);
        Assert.Contains("Coordinates cannot be negative.", exception.Message);
    }

    [Fact]
    public void ClickLeftMouseButton_Execute_DoesntThrowException()
    {
        // Arrange
        MouseService sut = new();

        // Act
        InvalidOperationException? exception = null;
        try
        {
            sut.MoveCursorTo(0, 0);
            sut.ClickLeftMouseButton();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        //Assert
        Assert.Null(exception);
    }

    [Fact]
    public void StartMouseHook_Execute_ThrowsExceptionIfCalledTwice()
    {
        // Arrange
        MouseService sut = new();

        // Act
        InvalidOperationException? exception = null;
        try
        {
            sut.StartMouseHook();
            sut.StartMouseHook();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("Mouse hook is already active.", exception.Message);
    }

    [Fact]
    public void StopMouseHook_Execute_ThrowsExceptionIfWithoutBeingHooked()
    {
        // Arrange
        MouseService sut = new();

        // Act
        InvalidOperationException? exception = null;
        try
        {
            sut.StopMouseHook();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        //Assert
        Assert.NotNull(exception);
        Assert.Equal("Mouse hook is not currently active.", exception.Message);
    }
}
