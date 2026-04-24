using Godot;

public partial class Enemy : Node3D
{
	public Vector3 Velocity = Vector3.Zero;
	private Vector3 lastPosition = Vector3.Zero;

	private bool isPaused = true;
	
	public override void _Process(double delta)
	{
		if (!isPaused)
		{
			GlobalPosition = new Vector3(5f * Mathf.Sin(Time.GetTicksMsec() / 2000f), 1f, -20f);
		}

		if (Input.IsActionJustPressed("ui_accept"))
		{
			isPaused = !isPaused;
		}

		Velocity = (GlobalPosition - lastPosition) / (float)delta;
		lastPosition = GlobalPosition;
		
		DebugDraw3D.DrawArrow(GlobalPosition - Vector3.Up, GlobalPosition + Velocity - Vector3.Up, Colors.Blue, 0.25f);
	}
}
