using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class MenuUI : MonoBehaviour
{
    VisualElement _menu;
    Slider _spawnRateSlider;
    SliderInt _spawnMinCountSlider;
    SliderInt _spawnMaxCountSlider;
    SliderInt _enemyMaxCountSlider;
    Label _spawnRateLabel;
    Label _spawnMinCountLabel;
    Label _spawnMaxCountLabel;
    Label _enemyMaxCountLabel;
    bool _menuOpen;

    EntityManager _em;
    EntityQuery _spawnerQuery;

    DropdownField _resolutionDropdown;
    Resolution[] _resolutions;
    void Start()
    {
        _em = World.DefaultGameObjectInjectionWorld.EntityManager;
        _spawnerQuery = _em.CreateEntityQuery(typeof(SpawnerData));

        var root = GetComponent<UIDocument>().rootVisualElement;

        _menu = root.Q<VisualElement>("menu");
        _spawnRateSlider = root.Q<Slider>("spawn-rate");
        _spawnMinCountSlider = root.Q<SliderInt>("spawn-min-count");
        _spawnMaxCountSlider = root.Q<SliderInt>("spawn-max-count");
        _enemyMaxCountSlider = root.Q<SliderInt>("enemy-max-count");
        _spawnRateLabel = root.Q<Label>("spawn-rate-label");
        _spawnMinCountLabel = root.Q<Label>("spawn-min-count-label");
        _spawnMaxCountLabel = root.Q<Label>("spawn-max-count-label");
        _enemyMaxCountLabel = root.Q<Label>("enemy-max-count-label");

        // set values from spawner BEFORE registering callbacks
        if (!_spawnerQuery.IsEmpty)
        {
            var spawner = _spawnerQuery.GetSingleton<SpawnerData>();
            _spawnRateSlider.SetValueWithoutNotify(spawner.BaseInterval);
            _spawnMinCountSlider.SetValueWithoutNotify(spawner.MinSpawnCount);
            _spawnMaxCountSlider.SetValueWithoutNotify(spawner.MaxSpawnCount);
            _enemyMaxCountSlider.SetValueWithoutNotify(spawner.MaxEnemyCount);
            
            _spawnRateLabel.text = $"Spawn Interval: {spawner.BaseInterval:0.00}s";
            _spawnMinCountLabel.text = $"Min Spawn: {spawner.MinSpawnCount}";
            _spawnMaxCountLabel.text = $"Max Spawn: {spawner.MaxSpawnCount}";
            _enemyMaxCountLabel.text = $"Max Enemies: {spawner.MaxEnemyCount}";
        }

        root.Q<Button>("resume-btn").clicked += CloseMenu;

        Toggle _fullscreenToggle;

        _fullscreenToggle = root.Q<Toggle>("fullscreen");
        _fullscreenToggle.SetValueWithoutNotify(Screen.fullScreen);

        _fullscreenToggle.RegisterValueChangedCallback(evt =>
        {
            Screen.fullScreen = evt.newValue;
        });

        _resolutionDropdown = root.Q<DropdownField>("resolution");

        _resolutions = Screen.resolutions;

        var seen = new System.Collections.Generic.HashSet<(int, int)>();
        var filtered = new System.Collections.Generic.List<Resolution>();

        foreach (var r in Screen.resolutions)
        {
            if (seen.Add((r.width, r.height)))
                filtered.Add(r);
        }

        _resolutions = filtered.ToArray();

        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            var r = _resolutions[i];
            options.Add($"{r.width} x {r.height} @ {r.refreshRateRatio.value:0}hz");
            if (r.width == Screen.currentResolution.width && 
                r.height == Screen.currentResolution.height)
                currentIndex = i;
        }

        _resolutionDropdown.choices = options;
        _resolutionDropdown.SetValueWithoutNotify(options[currentIndex]);

        _resolutionDropdown.RegisterValueChangedCallback(evt =>
        {
            int index = _resolutionDropdown.index;
            var r = _resolutions[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, r.refreshRateRatio);
        });

        _spawnRateSlider.RegisterValueChangedCallback(evt =>
        {
            _spawnRateLabel.text = $"Spawn Interval: {evt.newValue:0.00}s";
            UpdateSpawner();
        });

        _spawnMinCountSlider.RegisterValueChangedCallback(evt =>
        {
            _spawnMinCountLabel.text = $"Min Spawn: {evt.newValue}";
            UpdateSpawner();
        });

        _spawnMaxCountSlider.RegisterValueChangedCallback(evt =>
        {
            _spawnMaxCountLabel.text = $"Max Spawn: {evt.newValue}";
            UpdateSpawner();
        });

        _enemyMaxCountSlider.RegisterValueChangedCallback(evt =>
        {
            _enemyMaxCountLabel.text = $"Max Enemies: {evt.newValue}";
            UpdateSpawner();
        });
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_menuOpen) CloseMenu();
            else OpenMenu();
        }
    }

    void UpdateSpawner()
    {
        if (_spawnerQuery.IsEmpty) return;
        var spawner = _spawnerQuery.GetSingleton<SpawnerData>();
        spawner.MinInterval = _spawnRateSlider.value;
        spawner.BaseInterval = _spawnRateSlider.value;
        spawner.MinSpawnCount = _spawnMinCountSlider.value;
        spawner.MaxSpawnCount = _spawnMaxCountSlider.value;
        spawner.MaxEnemyCount = _enemyMaxCountSlider.value;
        _spawnerQuery.SetSingleton(spawner);
    }

    void OpenMenu()
    {
        _menuOpen = true;
        _menu.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
    }

    void CloseMenu()
    {
        _menuOpen = false;
        _menu.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }
}