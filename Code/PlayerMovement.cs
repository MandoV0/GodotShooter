using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
    public double LookSensitivity = 0.05f;
    public float Speed = 8.0f;
    public float Acceleration = 8.0f;
    public float Deacceleration = 8.0f;
    public float AirAcceleration = 1f;
    public float AirDeacceleration = 1f;
    public float JumpForce = 15.0f;
    public const float GRAVITY = 13f;

    public float BaseFov = 90.0f;
    public float MaxFov = 103.0f;

    [Export] public Node3D CameraTransform;
    [Export] public Camera3D PlayerCamera;

    private Vector3 _velocity = Vector3.Zero;
    private float _velocityY = 0.0f;

    private Vector2 _mouseInput = Vector2.Zero;
    private Vector3 _cameraRotation = Vector3.Zero; 

    public override void _Ready()
    {
        Engine.MaxFps = 144;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        PlayerCamera.Fov = BaseFov;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent) 
        {
            _mouseInput = mouseEvent.Relative;
        }
    }

    public float GetAcceleration() => IsOnFloor() ? Acceleration : AirAcceleration;
    public float GetDeacceleration() => IsOnFloor() ? Deacceleration : AirDeacceleration;
    
    public Vector2 GetMoveInput() => Input.GetVector("left", "right", "up", "down");

    public override void _Process(double delta)
    {
        Look();
        VelocityFov();
    }

    private void VelocityFov()
    {
        float speedFraction = Mathf.Clamp(_velocity.Length() / Speed, 0, 1);
        PlayerCamera.Fov = Mathf.Lerp(BaseFov, MaxFov, speedFraction);
    }

    private void Move(double delta)
    {
        Vector2 moveInput = GetMoveInput();
        Vector3 moveVector = Transform.Basis * new Vector3(moveInput.X, 0, moveInput.Y);        // Tranform.basis so we face the look direction
        if (moveInput != Vector2.Zero)
        {
            _velocity = _velocity.Lerp(moveVector * Speed, (float)(GetAcceleration() * delta));
        }
        else
        {
            _velocity = _velocity.Lerp(moveVector * Speed, (float)(GetDeacceleration() * delta));
        }
    }

    public void Look()
    {
        RotateY(-ConvertToRadians( (float)(_mouseInput.X * LookSensitivity) ));
        CameraTransform.RotateX(-ConvertToRadians( (float)(_mouseInput.Y * LookSensitivity) ));
        CameraTransform.Rotation = new Vector3(Mathf.Clamp(CameraTransform.Rotation.X, ConvertToRadians(-90), ConvertToRadians(90)), CameraTransform.Rotation.Y, CameraTransform.Rotation.Z);
        // _Input Function only gets called once the Mouse gets moved, so the mouse input doesnt get resetted to zero. Have to reset manually
        _mouseInput = Vector2.Zero;
    }

    public float ConvertToRadians(float angle)
    {
        return 3.14f / 180.0f * angle;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsOnFloor())
        { 
            _velocityY -= GRAVITY * (float)delta;
        }
        else {
            _velocityY = 0;
        }
        
        HandleJump();
        _velocity.Y = _velocityY;
        Move(delta);
        Velocity = _velocity;
        MoveAndSlide();
    }
    
    private void HandleJump()
    {
        if (IsOnFloor() && Input.IsActionJustPressed("jump"))
        {
            _velocityY = JumpForce;
        }
    }
}