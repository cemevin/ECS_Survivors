using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    Label _enemyLabel;
    Label _healthLabel;
    Label _shockwaveLabel;
    Label _fpsLabel;

    EntityManager _em;
    EntityQuery _enemyQuery;
    Entity _playerEntity;

    float[] _frameTimes = new float[60];
    int _frameIndex;
    float _fpsTimer;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _enemyLabel = root.Q<Label>("enemy-count");
        _healthLabel = root.Q<Label>("health");
        _shockwaveLabel = root.Q<Label>("shockwave");
        _fpsLabel = root.Q<Label>("fps");
        _fpsTimer = 0;

        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _enemyQuery = _em.CreateEntityQuery(typeof(EnemyTag));

    }

    void Update()
    {
        // lazy init player entity
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
        
        int numEnemies = _enemyQuery.CalculateEntityCount();
        _enemyLabel.text = $"Enemies: {numEnemies}";

        var health = _em.GetComponentData<Health>(_playerEntity);
        _healthLabel.text = $"HP: {health.Current:0} / {health.Max:0}";

        var weapon = _em.GetComponentData<WeaponData>(_playerEntity);
        float cooldown = weapon.CurrentShockwaveCooldown - Time.time;
        _shockwaveLabel.text = cooldown > 0
            ? $"Shockwave: {cooldown:0.0}s" 
            : "Shockwave: Ready";

        _frameTimes[_frameIndex % 60] = Time.deltaTime;
        _frameIndex++;

        _fpsTimer += Time.deltaTime;
        if (_fpsTimer >= 0.5f)
        {
            _fpsTimer = 0f;
            float avg = 0f;
            int count = Mathf.Min(_frameIndex, 60);
            for (int i = 0; i < count; i++)
                avg += _frameTimes[i];
            avg /= count;
            _fpsLabel.text = $"FPS: {(int)(1f / avg)}";
        }
    }
}