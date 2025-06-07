using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject
{
    public UnityAction<GameSceneSO, Vector3, bool> LoadRequestEvent; // 参数：场景，玩家新位置坐标，是否要播放渐入渐出效果

    /// <summary>
    /// 场景加载请求
    /// </summary>
    /// <param name="locationToGo">要加载的场景</param>
    /// <param name="positionToGo">Player的目的坐标</param>
    /// <param name="fadeScreen">是否渐入渐出</param>
    public void RaiseLoadRequestEvent(GameSceneSO locationToGo, Vector3 positionToGo, bool fadeScreen)
    {
        // Invoke Event
        LoadRequestEvent?.Invoke(locationToGo, positionToGo, fadeScreen);
    }
}