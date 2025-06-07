using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
    [Header("广播")]
    public VoidEventSO saveDataEvent;

    [Header("变量参数")]
    public SpriteRenderer spriteRenderer;
    public GameObject lightObj;
    public Sprite doneSprite;
    public Sprite unDoneSprite;
    public bool isDone;

    private void OnEnable()
    {
        spriteRenderer.sprite = isDone ? doneSprite : unDoneSprite;
        lightObj.SetActive(isDone);
    }

    public void TriggerAction()
    {
        if (!isDone)
        {
            isDone = true;
            spriteRenderer.sprite = doneSprite; //更换图片
            lightObj.SetActive(true);
            //TODO:保存数据

            this.gameObject.tag = "Untagged"; // 更改标识为Untagged, 防止第二次互动
            saveDataEvent.RaiseEvent();

        }
    }

}
