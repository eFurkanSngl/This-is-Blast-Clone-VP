using UnityEngine;

public class CloseBoxClickHandler : MonoBehaviour
{
    private GoalBoxManager _goalBoxManager;

    public void Set(GoalBoxManager goalBoxManager)
    {
        _goalBoxManager = goalBoxManager;
    }

    private void OnMouseDown()
    {
        if (_goalBoxManager != null)
        {
            _goalBoxManager.CloseGoalItemClick(this.gameObject);
        }
    }
}
