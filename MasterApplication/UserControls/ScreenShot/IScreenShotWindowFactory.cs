namespace MasterApplication.UserControls.ScreenShot;

public interface IScreenShotWindowFactory
{
    /// <summary>
    /// Creates a <see cref="ScreenShotWindow"/>.
    /// </summary>
    /// <returns>The <see cref="ScreenShotWindow"/> created.</returns>
    ScreenShotWindow Create();
}
