using Godot;

public partial class Enemy : Node3D
{
	public Vector3 Velocity = Vector3.Zero;
	private Vector3 lastPosition = Vector3.Zero;

	private bool isPaused = true;
	private float movementSpeed = 2.5f;
	
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("ui_accept"))
		{
			isPaused = !isPaused;
		}
		
		if (!isPaused)
		{
			GlobalPosition += Vector3.Right * movementSpeed * (float)delta;
		}
		if (Mathf.Abs(GlobalPosition.X) > 5f)
		{
			movementSpeed = -movementSpeed;
		}
		
		Velocity = (GlobalPosition - lastPosition) / (float)delta;
		lastPosition = GlobalPosition;
	}
}
