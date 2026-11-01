using UnityEngine;

/// <summary>
/// Абстрактный деспавнер по «экранам».
/// Раз в checkInterval секунд берёт экран объекта и экран камеры и спрашивает у наследника,
/// пора ли деспавнить. Если да — выключает или удаляет объект.
///
/// Минимальный пример наследника:
/// <code>
/// public class MyDespawner : ScreenDespawner
/// {
///     protected override bool ShouldDespawn(Vector2Int objectScreen, Vector2Int cameraScreen)
///         => Mathf.Abs(objectScreen.x - cameraScreen.x) > 1;
/// }
/// </code>
/// </summary>
public abstract class ScreenDespawner : MonoBehaviour
{
    public enum DespawnMode
    {
        /// <summary>SetActive(false) — объект можно позже включить заново (пулинг, повторный спавн).</summary>
        Disable,
        /// <summary>Destroy — объект удаляется навсегда.</summary>
        Destroy
    }

    [Header("Despawn")]
    [SerializeField] private DespawnMode mode = DespawnMode.Disable;

    [Tooltip("Что деспавнить. Пусто — этот же GameObject. Укажите корень префаба, если скрипт висит на дочернем объекте.")]
    [SerializeField] private GameObject target;

    [Header("Check")]
    [Tooltip("Как часто проверять, в секундах. 0 — каждый кадр.")]
    [SerializeField, Min(0f)] private float checkInterval = 0.25f;

    [Tooltip("Не вызывать ShouldDespawn, пока не изменился ни экран объекта, ни экран камеры. " +
             "Отключите, если ваш ShouldDespawn зависит от чего-то кроме этих двух аргументов.")]
    [SerializeField] private bool skipIfScreensUnchanged = true;

    private float nextCheckTime;
    private bool despawned;
    private bool hasLastScreens;
    private Vector2Int lastObjectScreen;
    private Vector2Int lastCameraScreen;

    /// <summary>
    /// Главная функция для переопределения.
    /// </summary>
    /// <param name="objectScreen">Экран объекта (CameraFollow.WorldToScreen).</param>
    /// <param name="cameraScreen">Экран камеры (CameraFollow.CurrentScreen).</param>
    /// <returns>true — объект вне зоны прогрузки и должен быть деспавнен.</returns>
    protected abstract bool ShouldDespawn(Vector2Int objectScreen, Vector2Int cameraScreen);

    /// <summary>Экран объекта. Переопределите, если CameraFollow у вас устроен иначе (например, через Instance).</summary>
    protected virtual Vector2Int GetObjectScreen() => CameraFollow.WorldToScreen(transform.position);

    /// <summary>Экран камеры. Переопределите, если CameraFollow у вас устроен иначе.</summary>
    protected virtual Vector2Int GetCameraScreen() => camera.CurrentScreen;

    /// <summary>
    /// Само действие деспавна. Переопределите, чтобы, например, вернуть объект в пул.
    /// </summary>
    protected virtual void PerformDespawn(GameObject go)
    {
        if (mode == DespawnMode.Destroy)
            Destroy(go);
        else
            go.SetActive(false);
    }

    /// <summary>Сбросить кэш экранов — следующая проверка точно вызовет ShouldDespawn.</summary>
    protected void InvalidateCache() => hasLastScreens = false;
    CameraFollow camera;
    protected virtual void OnEnable()
    {
        camera = FindAnyObjectByType<CameraFollow>();
        despawned = false;
        hasLastScreens = false;
        // Случайное смещение, чтобы проверки сотен объектов не приходились на один кадр.
        nextCheckTime = Time.time + Random.value * checkInterval;
    }

    protected virtual void Update()
    {
        if (despawned || Time.time < nextCheckTime)
            return;

        nextCheckTime = Time.time + checkInterval;
        Check();
    }

    /// <summary>Выполнить проверку немедленно (можно вызывать извне).</summary>
    public void Check()
    {
        Vector2Int objectScreen = GetObjectScreen();
        Vector2Int cameraScreen = GetCameraScreen();

        if (skipIfScreensUnchanged && hasLastScreens &&
            objectScreen == lastObjectScreen && cameraScreen == lastCameraScreen)
            return;

        hasLastScreens = true;
        lastObjectScreen = objectScreen;
        lastCameraScreen = cameraScreen;

        if (ShouldDespawn(objectScreen, cameraScreen))
            Despawn();
    }

    /// <summary>Деспавнить принудительно, не дожидаясь проверки.</summary>
    public void Despawn()
    {
        if (despawned)
            return;

        despawned = true;
        PerformDespawn(target != null ? target : gameObject);
    }
}