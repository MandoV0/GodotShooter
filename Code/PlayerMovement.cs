using Godot;
using System;

public partial class PlayerMovement : CharacterBody3D
{
    public double LookSensitivity = 0.05f;
    // Player Movement Speed
    [ExportCategory("Movement - Settings")]
    [Export] public float Speed = 12.0f;
    [Export] public float Acceleration = 20.0f;
    [Export] public float Deacceleration = 10.0f;
    [Export] public float AirAcceleration = 10.0f;
    [Export] public float AirDeacceleration = 10.0f;
    [Export] public float AirControl = 0.5f;
    [Export] public float JumpForce = 15.0f;
    [Export] public int MaxJumps = 2;
    
    [Export] public float GRAVITY = 20f;
    
    [ExportCategory("Camera - Settings")]
    [Export] public float BaseFov = 90.0f;
    [Export] public float MaxFov = 103.0f;

    [Export] public Node3D CameraTransform;
    [Export] public Camera3D PlayerCamera;
    
    [ExportCategory("Dash - Settings")]
    [Export] public float DashSpeed = 20.0f;
    [Export] public float DashCooldown = 1.0f;
    [Export] public float DashDuration = 0.2f;
    [Export] public bool _isDashing = false;
    [Export] public float _dashTime = 0.0f;
    [Export] public float _lastDashTime = 0.0f;

    [ExportCategory("Ground Check - Settings")] 
    [Export] public RayCast3D GroundCheckRaycaster;
    [Export] public float MaxDistanceToGround = -0.1f;
    
    private int _jumpsLeft;

    private Vector3 _velocity = Vector3.Zero;
    private float _velocityY = 0.0f;

    private Vector2 _mouseInput = Vector2.Zero;
    private Vector3 _cameraRotation = Vector3.Zero;

    private bool _wasGroundedLastFrame;

    public override void _Ready()
    {
        Engine.MaxFps = 144;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
        PlayerCamera.Fov = BaseFov;
        _jumpsLeft = MaxJumps;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseEvent) 
        {
            _mouseInput = mouseEvent.Relative;
        }
    }

    public float GetAcceleration() => IsGrounded() ? Acceleration : AirAcceleration;
    public float GetDeacceleration() => IsGrounded() ? Deacceleration : AirDeacceleration;
    
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
        Vector2 moveInput = GetMoveInput().Normalized();
        Vector3 moveVector = GetMoveVector();
        
        if (IsGrounded() && !_wasGroundedLastFrame)
        {
            _velocityY = 0;
            _jumpsLeft = MaxJumps;
        }
        
        if (!IsGrounded()) _velocityY -= GRAVITY * (float)delta;

        if (moveInput != Vector2.Zero)
        {
            _velocity = _velocity.Lerp(moveVector * Speed, (float)(GetAcceleration() * delta));
        }
        else
        {
            _velocity = _velocity.Lerp(moveVector * Speed, (float)(GetDeacceleration() * delta));
        }
    }

    private Vector3 GetMoveVector()
    {
        Vector2 moveInput = GetMoveInput();
        return Transform.Basis * new Vector3(moveInput.X, 0, moveInput.Y);
    }

    public void OnDash()
    {
        if (_isDashing || Time.GetTicksMsec() - _lastDashTime < DashCooldown * 1000)
        {
            return; 
        }

        Vector3 moveDir = GetMoveVector();
        
        if (moveDir == Vector3.Zero) return;
        _isDashing = true;
        _dashTime = DashDuration;
        _lastDashTime = Time.GetTicksMsec();
        _velocity = moveDir * DashSpeed;
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
        if (Input.IsActionJustPressed("dash"))
        {
            OnDash();
        }
        
        if (_isDashing)
        {
            _dashTime -= (float)delta;
            if (_dashTime <= 0)
            {
                _isDashing = false;
            }
        }
        
        HandleJump();
        if (!_isDashing)
        {
            Move(delta);
        }
        _velocity.Y = _velocityY;
        Velocity = _velocity;
        MoveAndSlide();

        _wasGroundedLastFrame = IsGrounded();
    }

    public bool IsGrounded()
    {
        return GroundCheckRaycaster.IsColliding();
    }
    
    private void HandleJump()
    {
        if (Input.IsActionJustPressed("jump") && (_jumpsLeft > 0 || IsGrounded()))
        {
            _jumpsLeft--;
            _velocityY = JumpForce;
        }
    }
}