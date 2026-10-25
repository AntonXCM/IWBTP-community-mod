using UnityEngine;

public abstract class ObjectVisual : MonoBehaviour
{
    protected new CameraFollow camera;
    protected Vector2Int myScreen;
    protected virtual void Start()
    {
        camera = FindAnyObjectByType<CameraFollow>();
        myScreen = CameraFollow.WorldToScreen(transform.position);
    }
    public void Update()
    {
        if (camera.CurrentScreen == myScreen) Render();
    }
    protected abstract void Render();
}
