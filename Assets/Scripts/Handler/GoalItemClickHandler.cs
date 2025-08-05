using UnityEngine;

public class GoalItemClickHandler : MonoBehaviour
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
            _goalBoxManager.OnGoalItemClicked(this.gameObject);
        }
    }
}
