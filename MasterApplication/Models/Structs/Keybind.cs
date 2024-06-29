namespace MasterApplication.Models.Structs;

public struct Keybind
{
    public string KeyName { get; set; }
    public int KeyCode { get; set; }

    public Keybind(string keyName, int keyCode)
    {
        KeyName = keyName;
        KeyCode = keyCode;
    }
}
