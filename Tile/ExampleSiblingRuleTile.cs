using UnityEngine;
using UnityEngine.Tilemaps;

// 创建一个 ScriptableObject 菜单项，可以让用户在 Unity 编辑器中创建该自定义 RuleTile
[CreateAssetMenu(menuName = "Tile/ExampleSiblingRuleTile")]
public class ExampleSiblingRuleTile : RuleTile  // 继承自 Unity 内置的 RuleTile 类
{
    // 定义一个枚举，用于将瓦片分为不同的组，例如 "Poles"（柱子） 和 "Terrain"（地形）
    public enum SibingGroup
    {
        Poles,   // 柱子或其他垂直结构的瓦片
        Terrain, // 地形瓦片
    }

    // 用于存储瓦片的分组信息，每个瓦片会属于一个分组
    public SibingGroup sibingGroup;

    // 重写 RuleTile 的 RuleMatch 方法，来定义相邻瓦片的匹配规则
    public override bool RuleMatch(int neighbor, TileBase other)
    {
        // 如果相邻瓦片是 RuleOverrideTile 类型，则将其转换为对应的实例瓦片进行匹配
        if (other is RuleOverrideTile)
            other = (other as RuleOverrideTile).m_InstanceTile;

        // 根据不同的 neighbor 值，应用不同的规则
        switch (neighbor)
        {
            // 当 neighbor 为 TilingRule.Neighbor.This 时，表示需要匹配与当前瓦片属于同一分组的瓦片
            case TilingRule.Neighbor.This:
                {
                    // 返回 true 如果相邻瓦片是 ExampleSiblingRuleTile 类型，且属于相同的分组
                    return other is ExampleSiblingRuleTile
                        && (other as ExampleSiblingRuleTile).sibingGroup == this.sibingGroup;
                }
            // 当 neighbor 为 TilingRule.Neighbor.NotThis 时，表示需要匹配与当前瓦片不属于同一分组的瓦片
            case TilingRule.Neighbor.NotThis:
                {
                    // 返回 true 如果相邻瓦片不是 ExampleSiblingRuleTile 类型，或属于不同的分组
                    return !(other is ExampleSiblingRuleTile
                        && (other as ExampleSiblingRuleTile).sibingGroup == this.sibingGroup);
                }
        }

        // 如果不匹配上面两种规则，则调用父类的 RuleMatch 方法继续匹配
        return base.RuleMatch(neighbor, other);
    }
}