using System.Windows.Controls;

using MasterApplication.Models.Structs;
using MasterApplication.Services.Feature.MouseClicker;

namespace MasterApplication.UserControls;

/// <summary>
/// Interaction logic for ConfirmDialog.xaml
/// </summary>
public partial class KeybindDialog : UserControl
{
    private readonly IKeyboardService _keyboardService;

    public Keybind KeybindKey { get; set; } = new();

    /// <summary>
    /// Creates and instance of a <see cref="KeybindDialog"/>.
    /// </summary>
    /// <param name="keyboardService"><see cref="IKeyboardService"/> to intercept keyboard presses.</param>
    public KeybindDialog(IKeyboardService keyboardService)
    {
        InitializeComponent();
        ContentTextblock.Text = "Press any key on the keyboard.";
        _keyboardService = keyboardService ?? throw new ArgumentNullException(nameof(keyboardService));

        _keyboardService.KeyPressed -= KeyboardServiceOnKeyPressed;
        _keyboardService.KeyPressed += KeyboardServiceOnKeyPressed;

        _keyboardService.StartKeyboardHook();
    }

    /// <summary>
    /// Key pressed intercepted from the keyboard.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="vkCode">Code of the key pressed.</param>
    private void KeyboardServiceOnKeyPressed(object? sender, int vkCode)
    {
        string keyName = _keyboardService.GetKeyByCode(vkCode).ToUpper();
        KeyBindTextBlock.Text = keyName;

        Keybind key = new() 
        { 
            KeyCode = vkCode, 
            KeyName = keyName 
        };

        KeybindKey = key;
    }

    /// <summary>
    /// Event to intercept when the dialog closes.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UserControl_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (!IsVisible)
            _keyboardService.StopKeyboardHook();
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    ~KeybindDialog()
    {
        _keyboardService?.StopKeyboardHook();
    }
}
