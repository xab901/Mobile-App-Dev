using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using System.IO;

// 单例模式，只有 ISavable 可以调用，其他任何代码不应该调用这个单例模式
[DefaultExecutionOrder(-100)]
public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("事件监听")]
    public VoidEventSO saveDataEvent;
    public VoidEventSO loadDataEvent;
    private List<ISavable> savableList = new List<ISavable>();
    private Data saveData;
    private string jsonFolder;

    /*
    通过datamanager统一管理所有注册的内容，统一删除
    */
    private void Awake()
    {
        // 确保场景内有且只有唯一的一个单例模式可以进行调用
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);
        saveData = new Data();

        jsonFolder = Application.persistentDataPath + "/SAVE DATA/";

        ReadSavedData();
    }

    private void OnEnable()
    {
        saveDataEvent.OnEventRaised += Save;
        loadDataEvent.OnEventRaised += Load;
    }

    private void OnDisable()
    {
        saveDataEvent.OnEventRaised -= Save;
        loadDataEvent.OnEventRaised -= Load;

    }

    private void Update()
    {
        if (Keyboard.current.lKey.wasPressedThisFrame)
            Load();
    }

    public void RegisterSaveData(ISavable savable)
    {
        // 若相同数据没有被保存过，则添加数据
        if (!savableList.Contains(savable))
            savableList.Add(savable);
    }

    public void UnRegisterSaveData(ISavable savable)
    {
        savableList.Remove(savable);
    }

    public void Save()
    {
        foreach (var savable in savableList)
        {
            savable.GetSaveData(saveData);
        }

        var resultPath = jsonFolder + "data.sav";

        var jsonData = JsonConvert.SerializeObject(saveData);

        if (!File.Exists(resultPath))
        {
            Directory.CreateDirectory(jsonFolder);
        }

        File.WriteAllText(resultPath, jsonData);

        //测试
        // foreach (var item in saveData.characterPosDict)
        // {
        //     Debug.Log(item.Key + "    " + item.Value);
        // }
    }

    public void Load()
    {
        foreach (var savable in savableList)
        {
            savable.LoadData(saveData);
        }
    }

    private void ReadSavedData()
    {
        var resultPath = jsonFolder + "data.sav";
        if (File.Exists(resultPath))
        {
            var stringData = File.ReadAllText(resultPath);

            var jsonData = JsonConvert.DeserializeObject<Data>(stringData);

            saveData = jsonData;
        }

    }
}
