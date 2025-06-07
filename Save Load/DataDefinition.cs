using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataDefinition : MonoBehaviour
{
    // 数据描述定义，用于生成GUID
    public PersistentType persistentType;
    public string ID;

    private void OnValidate()
    {
        // 一旦编辑器中有任何的数据变化，则生成GUID
        if (persistentType == PersistentType.ReadWrite)
        {
            if (ID == string.Empty)
                ID = System.Guid.NewGuid().ToString();
        }
        else
        {
            ID = string.Empty;
        }
    }
}
