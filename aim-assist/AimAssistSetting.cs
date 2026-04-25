using Godot;

public partial class AimAssistSetting : VBoxContainer
{
    [Export] private CheckButton toggle;
    [Export] private HSlider slider;
    [Export] private Label valueLabel;
    public bool isEnabled;
    public float strength;

    public override void _Ready()
    {
        toggle.Toggled += (value) =>
        {
            isEnabled = value;
        };
        slider.ValueChanged += (value) =>
        {
            strength = (float)value / 100f;
            valueLabel.Text = $"{value:F0}%";
        };
    }
}
