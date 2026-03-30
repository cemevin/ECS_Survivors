using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class InputGatheringSystem : SystemBase,
    PlayerActionMap.IGameplayActions
{
    PlayerActionMap _inputActions;
    
    // raw values stored from callbacks
    Vector2 _moveInput = Vector2.zero;
    Vector2 _aimInput = Vector2.zero;
    bool _fireHeld = false;
    bool _shockwavePressed = false;

    EntityQuery _playerInputQuery;

    
    public void OnMove(InputAction.CallbackContext ctx) => 
        _moveInput = ctx.ReadValue<Vector2>();
    
    public void OnFire(InputAction.CallbackContext ctx) => 
        _fireHeld = ctx.ReadValue<float>() > 0.5f;
    
    public void OnShockwave(InputAction.CallbackContext ctx)
    {
        if (ctx.started) _shockwavePressed = true;
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        _aimInput = ctx.ReadValue<Vector2>();
    }

    protected override void OnCreate()
    {
        _inputActions = new PlayerActionMap();
        _inputActions.Gameplay.SetCallbacks(this);

        _playerInputQuery = GetEntityQuery(typeof(PlayerInput));

        RequireForUpdate<PlayerTag>();
    }

    protected override void OnStartRunning() => _inputActions.Enable();
    protected override void OnStopRunning() => _inputActions.Disable();

    // callbacks — fire on input thread, just store values

    protected override void OnUpdate() 
    {
        // push to singleton each frame
        if (_playerInputQuery.CalculateEntityCount() == 0)
            EntityManager.CreateEntity(typeof(PlayerInput));

        float2 mouseValue = Mouse.current.position.ReadValue();
        bool isUsingMouse = math.lengthsq(mouseValue) > 0.001f;
        if (isUsingMouse)
        {
            // use mouse
            _aimInput = mouseValue;
        }

        _playerInputQuery.SetSingleton(new PlayerInput
        {
            MoveStick = _moveInput,
            AimStick = _aimInput,
            AimWithMouse = isUsingMouse,
            FireHeld = _fireHeld,
            ShockwavePressed = _shockwavePressed
        });

        // reset one-shot inputs
        _shockwavePressed = false;
    }
}