using System.ComponentModel.DataAnnotations;
using Godot;
using Godot.Collections;

public partial class CameraController : Camera3D
{
	private float lookSensitivity = Mathf.DegToRad(0.05f);
	private float aimAssistSensitivityMul = 1f;
	private float aimAssistRange = 50f;
	private Camera3D camera; // For ease of decoupling aim assist from the camera controller

	private bool captureCursor = false;
	
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured; 
		camera = this;

		aaSnapTimer = new Timer();
		AddChild(aaSnapTimer);
		aaSnapTimer.OneShot = true;
		aaSnapTimer.WaitTime = 0.2f;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion eventMouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			RotateCamera(eventMouseMotion.Relative);
		}
	}
	
	public override void _Process(double delta)
	{
		if (IsLookingAtEnemy(out Enemy enemy))
		{
			if (Settings.aaSlow.isEnabled)
			{
				AimAssist_Slowdown(true, Settings.aaSlow.strength);
			}

			if (Settings.aaFollow.isEnabled)
			{
				Vector2 follow = AimAssist_Follow(enemy, delta, Settings.aaFollow.strength);
				camera.RotateX(follow.X);
				camera.RotateY(follow.Y);
			}

			if (Input.IsActionJustPressed("attack1") && aaSnapTimer.IsStopped())
			{
				aaSnapTimer.Start();
			}
			if (!aaSnapTimer.IsStopped() && Settings.aaSnapTo.isEnabled)
			{
				AimAssist_SnapToTarget(enemy, delta);	
			}

			if (Settings.aaMagnet.isEnabled)
			{
				Vector3 magnet = AimAssist_BulletMagnet(enemy, Settings.aaMagnet.strength);
				DebugDraw3D.DrawLine(camera.GlobalPosition - Vector3.Up * 0.1f, camera.GlobalPosition + magnet * aimAssistRange, Colors.Cyan);
			}
		}
		else
		{
			AimAssist_Slowdown(false);
		}
	}

	private void RotateCamera(Vector2 motion)
	{
		motion *= lookSensitivity * aimAssistSensitivityMul;
		Rotation -= (new Quaternion(Vector3.Up, motion.X) * new Quaternion(Vector3.Right, motion.Y)).GetEuler();
		Rotation = Rotation with { X = Mathf.Clamp(Rotation.X, -1.5f, 1.5f) };
	}
	
	private bool LineTrace(Vector3 from, Vector3 to, out Dictionary result)
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas = true;
		query.CollideWithBodies = false;
		query.HitBackFaces = false;
		query.Exclude = [GetCameraRid()];

		result = spaceState.IntersectRay(query);

		return result.Count > 0;
	}
	
	private bool IsLookingAtEnemy(out Enemy target)
	{
		if (LineTrace(GlobalPosition, GlobalPosition - Basis.Z * aimAssistRange, out var hitResult))
		{
			hitResult.TryGetValue("collider", out Variant hitCollider);
			target = ((AimAssistArea)hitCollider).Parent;
			return true;
		}

		target = null;
		return false;
	}
	
	/// <summary>
	/// Reduces the sensitivity while aiming at an enemy.
	/// </summary>
	/// /// <param name="enabled">Should the effect be active</param>
	/// <param name="strength">Strength of the effect. 0 - no effect, 1 - no control for the player</param>
	private void AimAssist_Slowdown(bool enabled, [Range(0f,1f)] float strength = 0.5f)
	{
		aimAssistSensitivityMul = enabled ? 1f - strength : 1f;
	}
	
	
	/// <summary>
	/// Makes the camera follow the movement of the target.
	/// </summary>
	/// <param name="enemy">Enemy to follow</param>
	/// <param name="delta">deltaTime parameter</param>
	/// <param name="strength">Strength of the effect. 0 - no effect. 1 - target movement is matched perfectly</param>
	private Vector2 AimAssist_Follow(Enemy enemy, double delta, [Range(0f,1f)] float strength = 0.5f)
	{
		float distance = camera.GlobalPosition.DistanceTo(enemy.GlobalPosition);
		float verticalVelocity = enemy.Velocity.Y;
		
		Vector3 horizontalVelocity = new(enemy.Velocity.X, 0f, enemy.Velocity.Z);
		float horizontalDot = camera.GlobalBasis.X.Dot(horizontalVelocity.Normalized());
		float lateralVelocity = horizontalVelocity.Length() * horizontalDot;

		float horizontalFollow = Mathf.Atan2(lateralVelocity, distance);
		float verticalFollow = Mathf.Atan2(verticalVelocity, distance);
		
		return new Vector2(verticalFollow, horizontalFollow) * strength * (float)delta * -1f;
	}

	private Timer aaSnapTimer;
	/// <summary>
	/// Makes the camera snap towards the closest target
	/// </summary>
	/// <param name="enemy">Enemy to snap to</param>
	/// <param name="delta">deltaTime parameter</param>
	private void AimAssist_SnapToTarget(Enemy enemy, double delta, float weight = 0.1f)
	{
		// TODO: Replace Slerp with something that will operate more consistently regardless of deltaTime
		Quaternion rotation = new(camera.GlobalBasis);
		camera.LookAt(enemy.GlobalPosition, Vector3.Up);
		Quaternion targetRotation = new(camera.GlobalBasis);
		camera.Rotation = rotation.Slerp(targetRotation, 0.1f).GetEuler();
	}

	/// <summary>
	/// Returns a vector directed towards the enemy
	/// </summary>
	/// <param name="enemy">Enemy to follow</param>
	/// <param name="strength">Strength of the effect. 0 - no effect. 1 - exact direction towards the enemy</param>
	private Vector3 AimAssist_BulletMagnet(Enemy enemy, float strength = 0.5f)
	{
		Vector3 enemyDirection = (camera.GlobalPosition - enemy.GlobalPosition).Normalized();
		Vector3 forward = camera.GlobalBasis.Z;
		Vector3 cross = forward.Cross(enemyDirection);
		float dot = forward.Dot(enemyDirection);
	
		return -camera.GlobalBasis.Z.Rotated(cross.Normalized(), Mathf.Acos(dot) * strength);
	}
}

