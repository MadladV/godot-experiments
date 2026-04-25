using Godot;

public partial class Interface : Control
{
    [Export] public AimAssistSetting aaSlow;
    [Export] public AimAssistSetting aaFollow;
    [Export] public AimAssistSetting aaSnapTo;
    [Export] public AimAssistSetting aaMagnet;

    public override void _Ready()
    {
        Settings.aaSlow = aaSlow;
        Settings.aaFollow = aaFollow;
        Settings.aaSnapTo = aaSnapTo;
        Settings.aaMagnet = aaMagnet;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("toggle_ui"))
        {
            Input.MouseMode = Input.MouseMode switch
            {
                Input.MouseModeEnum.Captured => Input.MouseModeEnum.Visible,
                Input.MouseModeEnum.Visible => Input.MouseModeEnum.Captured,
                _ => Input.MouseMode
            };
        }
    }
}
