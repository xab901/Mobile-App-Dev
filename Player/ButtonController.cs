using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Color pressedColor = new Color(0.8f, 0.8f, 0.8f);

    private PlayerController playerController;

    private void Awake()
    {
        if (jumpButton == null || attackButton == null || playerController == null)
        {
            Debug.LogError("ButtonController: 缺少必要引用!");
            return;
        }

        SetupJumpButton();
        SetupAttackButton();
    }

    private void SetupJumpButton()
    {
        // 添加点击事件
        jumpButton.onClick.AddListener(() => playerController.Jump());

        // 添加按下/释放效果
        var trigger = jumpButton.GetComponent<EventTrigger>();
        if (trigger == null) trigger = jumpButton.gameObject.AddComponent<EventTrigger>();

        // 按下效果
        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => {
            jumpButton.image.color = pressedColor;
        });
        trigger.triggers.Add(pointerDown);

        // 释放效果
        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => {
            jumpButton.image.color = Color.white;
        });
        trigger.triggers.Add(pointerUp);
    }

    private void SetupAttackButton()
    {
        // 添加点击事件
        attackButton.onClick.AddListener(() => playerController.Attack());

        // 添加按下/释放效果
        var trigger = attackButton.GetComponent<EventTrigger>();
        if (trigger == null) trigger = attackButton.gameObject.AddComponent<EventTrigger>();

        // 按下效果
        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => {
            attackButton.image.color = pressedColor;
            playerController.OnAttackButtonPressed();
        });
        trigger.triggers.Add(pointerDown);

        // 释放效果
        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => {
            attackButton.image.color = Color.white;
            playerController.OnAttackButtonReleased();
        });
        trigger.triggers.Add(pointerUp);
    }
}
