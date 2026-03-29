using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    Label _enemyLabel;
    Label _healthLabel;
    Label _shockwaveLabel;

    EntityManager _em;
    EntityQuery _enemyQuery;
    Entity _playerEntity;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        _enemyLabel = root.Q<Label>("enemy-count");
        _healthLabel = root.Q<Label>("health");
        _shockwaveLabel = root.Q<Label>("shockwave");

        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _enemyQuery = _em.CreateEntityQuery(typeof(EnemyTag));
        var playerQuery = _em.CreateEntityQuery(typeof(PlayerTag));
        _playerEntity = playerQuery.GetSingletonEntity();
    }

    void Update()
    {
        int numEnemies = _enemyQuery.CalculateEntityCount();
        _enemyLabel.text = $"Enemies: {numEnemies}";

        var health = _em.GetComponentData<Health>(_playerEntity);
        _healthLabel.text = $"HP: {health.Current:0} / {health.Max:0}";

        var weapon = _em.GetComponentData<WeaponData>(_playerEntity);
        float cooldown = weapon.CurrentShockwaveCooldown - Time.time;
        _shockwaveLabel.text = cooldown > 0
            ? $"Shockwave: {cooldown:0.0}s"
            : "Shockwave: Ready";
    }
}