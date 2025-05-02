using System.Drawing;

namespace MasterApplication.Models.Messages;

public class ScreenShotMessage
{
    public Rectangle TemplateBounds { get; set; }

    public AutoClickerTemplate AutoClickerTemplate { get; set; }

    public bool IsSearchingBoundsScreenshot { get; set; }

    public ScreenShotMessage()
    {
        TemplateBounds = new Rectangle();
        AutoClickerTemplate = new AutoClickerTemplate();
        IsSearchingBoundsScreenshot = false;
    }
}
