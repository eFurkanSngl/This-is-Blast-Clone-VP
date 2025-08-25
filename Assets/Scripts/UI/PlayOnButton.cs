using UnityEngine;
using Zenject;

public class PlayOnButton : UIBTN
{
    [Inject] private LauncherManager _launcherManager;
    protected override void OnClick()
    {
        ActivePlayOn();
    }

    private void ActivePlayOn()
    {
        this._button.interactable = false;
        Debug.Log("Active on playOn Button");
        _launcherManager.PlayOnShot();
    }

}
