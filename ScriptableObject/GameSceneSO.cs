using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Game Scene/GameSceneSO")]
public class GameSceneSO : ScriptableObject
{
    public SceneType sceneType; // 使用枚举区分主菜单界面和场景界面

    public AssetReference sceneReference; // 使用Addressable打包好的游戏资源可以使用AssestReference调用
}