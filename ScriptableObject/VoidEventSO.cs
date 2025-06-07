using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/VoidEventSO")]
public class VoidEventSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    // 进行广播
    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}
