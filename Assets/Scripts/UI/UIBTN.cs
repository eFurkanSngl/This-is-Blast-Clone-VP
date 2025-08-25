using UnityEngine;
using UnityEngine.UI;


public abstract class UIBTN : EventListener
{
    [SerializeField] protected  Button _button;

    protected abstract void OnClick();
   
    protected override void RegisterEvents()
    {
        _button.onClick.AddListener(OnClick);
    }

    protected override void UnRegisterEvents()
    {
        _button.onClick.RemoveListener(OnClick);
    }

}