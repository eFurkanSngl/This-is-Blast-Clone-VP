using UnityEngine;

public class GoalItemClickHandler : MonoBehaviour
{
    private GoalBoxManager _goalBoxManager;
    private bool _isCloseBox;

    public void Set(GoalBoxManager goalBoxManager,bool isCloseBox)
    {
        _goalBoxManager = goalBoxManager;
        _isCloseBox = isCloseBox;
    }

    private void OnMouseDown()
    {
        if (_goalBoxManager == null) return;

        if (_isCloseBox)
        {
            _goalBoxManager.CloseGoalItemClick(this.gameObject);
        }
        else
        {
            _goalBoxManager.OnGoalItemClicked(this.gameObject);
        }
    }

    public void SetCloseBox(bool isClose)
    {
        _isCloseBox = isClose;
    }
}
