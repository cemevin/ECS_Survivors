using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float Height = 20f;
    public float Smoothing = 10f;

    EntityManager _em;
    Entity _playerEntity;

    void Start()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void LateUpdate()
    {
        if (_playerEntity == Entity.Null)
        {
            var q = _em.CreateEntityQuery(typeof(PlayerTag));
            if (q.IsEmpty)
            {
                q.Dispose();
                return;
            }
            _playerEntity = q.GetSingletonEntity();
            q.Dispose();
        }
        if (!_em.Exists(_playerEntity)) return;

        var transform = _em.GetComponentData<LocalTransform>(_playerEntity);
        Vector3 target = new Vector3(transform.Position.x, Height, transform.Position.z);
        this.transform.position = Vector3.Lerp(this.transform.position, target, Smoothing * Time.deltaTime);
    }
}