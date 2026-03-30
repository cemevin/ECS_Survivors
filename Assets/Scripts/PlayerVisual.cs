using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    public Animator Animator;
    public SpriteRenderer Renderer;

    EntityManager _em;
    Entity _playerEntity;

    static readonly int IsRunning = Animator.StringToHash("IsRunning");

    void Start()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        var q = _em.CreateEntityQuery(typeof(PlayerTag));
        if (!q.IsEmpty) _playerEntity = q.GetSingletonEntity();
        q.Dispose();
    }

    void LateUpdate()
    {
        if (_playerEntity == Entity.Null) return;
        if (!_em.Exists(_playerEntity)) return;

        var transform = _em.GetComponentData<LocalTransform>(_playerEntity);
        var input = _em.GetComponentData<PlayerInput>(_playerEntity);
        var aimDir = _em.GetComponentData<PlayerAimDirection>(_playerEntity);

        this.transform.position = new Vector3(
            transform.Position.x,
            transform.Position.y,
            transform.Position.z);

        Renderer.flipX = input.FireHeld ? aimDir.Value.x < 0 : input.MoveStick.x < 0; 

        bool isRunning = math.lengthsq(input.MoveStick) > 0.01f;
        Animator.SetBool(IsRunning, isRunning);
    }
}