using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISavable
{
    DataDefinition GetDataID(); // 声明有返回值的函数方法，返回值类型为数据定义
    void RegisterSaveData() => DataManager.instance.RegisterSaveData(this);
    void UnRegisterSaveData() => DataManager.instance.UnRegisterSaveData(this); // 角色死亡时，注销数据

    void GetSaveData(Data data); // 获取需要保存的数据（需要通过 Data Manager)
    void LoadData(Data data); // 加载已经保存的数据
}
