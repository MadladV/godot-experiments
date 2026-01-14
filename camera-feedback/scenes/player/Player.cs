using Godot;

public partial class Player : CharacterBody3D
{
	[Export] private Camera3D camera;
	[Export] private Node3D head;
	[Export] private CollisionShape3D collider;
	[Export] private Label label;

	public float Speed = 5f;
	public const float JumpVelocity = 4.5f;
	private float lookSensitivity = Mathf.DegToRad(360f / 14400f);

	private CameraBob cameraBob = new();
	private Vector2 inputStrength;
	private Vector2 InputStrength
	{
		get => inputStrength;
		set => inputStrength = value.LimitLength();
	}
	
	public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        cameraBob.Scale = new Vector2(0.0025f, 0.002f);
        cameraBob.Speed = 2f;
    }
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion motion)
		{
			head.Rotation -= head.Rotation with { X = motion.Relative.Y * lookSensitivity};
			head.Rotation = head.Rotation with { X = Mathf.Clamp(head.Rotation.X, -1.5f, 1.5f)};
			Rotation -= Vector3.Up * motion.Relative.X * lookSensitivity;
		}
	}

	public override void _Process(double delta)
	{
		Vector3 velocity = Velocity;
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");
		Vector3 localVelocity = new Vector3(
			Basis.X.Dot(Velocity.Normalized()),
			Basis.Y.Dot(Velocity.Normalized()),
			Basis.Z.Dot(Velocity.Normalized())
		) * Velocity.Length();
		
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}
		
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		if (inputDir.IsZeroApprox() && IsOnFloor())
		{
			velocity -= velocity.LimitLength(3f * Speed * (float)delta);
		}

		Speed = Input.IsActionPressed("sprint") ? 5f : 3f;

		if (!inputDir.IsZeroApprox() && IsOnFloor())
		{
			Vector2 foo = new Vector2(velocity.X, velocity.Z).Normalized();
			float powerMul = -foo.Dot(inputDir.Normalized()) + 2f; // (-1, 1) -> (1, 3)
			powerMul *= 3f;
			
			Vector2 rotated = inputDir.Rotated(-GlobalRotation.Y);
			velocity += new Vector3(rotated.X, 0f, rotated.Y) * Speed * powerMul * (float)delta;
			
			float fallVelocity = velocity.Y;
			velocity *= new Vector3(1f, 0f, 1f);
			velocity = velocity.LimitLength(Speed) with { Y = fallVelocity };
		}
		
		cameraBob.Accum += localVelocity.Length() * (float)delta * cameraBob.Speed;
		cameraBob.Accum %= Mathf.Tau;

		camera.Rotation = camera.Rotation with
		{
			X = (Mathf.Abs(Mathf.Sin(cameraBob.Accum)) - 1f) * cameraBob.Scale.Y * velocity.Length(), 
			Y = Mathf.Cos(cameraBob.Accum) * cameraBob.Scale.X * velocity.Length(),
			Z = localVelocity.X * Mathf.DegToRad(-0.4f)
		};

		label.Text = $"{velocity.Length():0} m/s";
		Velocity = velocity;
		MoveAndSlide();
	}
}
