using UnityEngine;


public abstract class EventListener : MonoBehaviour
{
    private void OnEnable()
    {
        RegisterEvents();
    }

    private void OnDisable()
    {
        UnRegisterEvents();
    }
    protected abstract void RegisterEvents();
    protected abstract void UnRegisterEvents();
}

