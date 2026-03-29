using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputBridge : MonoBehaviour
{
    PlayerActionMap _actions;
    EntityManager _em;
    Entity _playerEntity;

    void Awake()
    {
        _actions = new PlayerActionMap();
        _actions.Enable();
    }

    void Start()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = _em.CreateEntityQuery(typeof(PlayerTag));
        _playerEntity = query.GetSingletonEntity();
    }

    void Update()
    {
        if (_playerEntity == Entity.Null) return;

        Vector2 move = _actions.Player.Move.ReadValue<Vector2>();

        float3 aimDir = new float3(0, 0, 1); // fallback
        Vector2 gamepadFireDir = _actions.Player.FireDir.ReadValue<Vector2>();

        if (gamepadFireDir.x != 0 || gamepadFireDir.y != 0)
        {
            aimDir = new float3(gamepadFireDir.x, 0, gamepadFireDir.y);
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                float3 worldPoint = (float3)ray.GetPoint(distance);
                var transform = _em.GetComponentData<LocalTransform>(_playerEntity);
                float3 playerPos = transform.Position;
                aimDir = math.normalizesafe(worldPoint - playerPos);
            }
        }

        _em.SetComponentData(_playerEntity, new PlayerInput
        {
            MoveDirection = new float3(move.x, 0, move.y),
            FirePressed = _actions.Player.Fire.IsPressed(),
            ShockwavePressed = _actions.Player.Shockwave.WasPressedThisFrame(),
        });

        _em.SetComponentData(_playerEntity, new AimDirection { Value = aimDir });
    }

    void OnDestroy()
    {
        _actions.Disable();
    }
}