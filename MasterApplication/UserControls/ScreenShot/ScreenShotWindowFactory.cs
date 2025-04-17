using CommunityToolkit.Mvvm.Messaging;
using MasterApplication.Services.Feature.MouseClicker;

namespace MasterApplication.UserControls.ScreenShot;

public class ScreenShotWindowFactory : IScreenShotWindowFactory
{
    private readonly IKeyboardService _keyboardService;
    private readonly IMessenger _messenger;

    public ScreenShotWindowFactory(IKeyboardService keyboardService, IMessenger messenger)
    {
        _keyboardService = keyboardService;
        _messenger = messenger;
    }

    /// <summary>
    /// Creates a <see cref="ScreenShotWindow"/>.
    /// </summary>
    /// <returns>The <see cref="ScreenShotWindow"/> created.</returns>
    public ScreenShotWindow Create()
    {
        return new ScreenShotWindow(_keyboardService, _messenger);
    }
}
