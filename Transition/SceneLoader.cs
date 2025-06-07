using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, ISavable
{
    public Transform playerTrans;
    public Vector3 firstPosition;
    public Vector3 menuPosition;

    [Header("事件监听")]
    public SceneLoadEventSO loadEventSO;
    public VoidEventSO newGameEvent;
    public VoidEventSO backToMenuEvent;

    [Header("广播")]
    public VoidEventSO afterSceneLoadedEvent;
    public FadeEventSO fadeEvent;
    public SceneLoadEventSO unloadedSceneEvent;

    [Header("场景")]
    public GameSceneSO firstLoadScene;
    public GameSceneSO menuScene;
    [SerializeField] private GameSceneSO currentLoadedScene;
    private GameSceneSO locToLoad;
    private Vector3 posToGo;
    private bool fScreen;
    private bool isLoading;
    public float fadeDuration;

    private void Awake()
    {
        // Addressables.LoadSceneAsync(firstLoadScene.sceneReference, UnityEngine.SceneManagement.LoadSceneMode.Additive);
        // currentLoadedScene = firstLoadScene;
        // currentLoadedScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
    }

    // TODO:做完MainMenu之后更改
    private void Start()
    {
        // NewGame();

        // 加载菜单
        loadEventSO.RaiseLoadRequestEvent(menuScene, menuPosition, true);
    }

    private void OnEnable()
    {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGameEvent.OnEventRaised += NewGame;
        backToMenuEvent.OnEventRaised += BackToMenu;

        ISavable savable = this;
        savable.RegisterSaveData();
    }

    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEvent.OnEventRaised -= NewGame;
        backToMenuEvent.OnEventRaised -= BackToMenu;

        ISavable savable = this;
        savable.UnRegisterSaveData();
    }

    private void BackToMenu()
    {
        locToLoad = menuScene;
        loadEventSO.RaiseLoadRequestEvent(locToLoad, menuPosition, true);
    }

    private void NewGame()
    {
        locToLoad = firstLoadScene;
        // OnLoadRequestEvent(locToLoad, firstPosition, true);
        loadEventSO.RaiseLoadRequestEvent(locToLoad, firstPosition, true);
    }

    /// <summary>
    /// 场景加载事件请求
    /// </summary>
    /// <param name="locationToLoad"></param>
    /// <param name="positionToGo"></param>
    /// <param name="fadeScreen"></param>
    private void OnLoadRequestEvent(GameSceneSO locationToLoad, Vector3 positionToGo, bool fadeScreen)
    {
        if (isLoading)
            return;

        isLoading = true;
        locToLoad = locationToLoad;
        posToGo = positionToGo;
        fScreen = fadeScreen;

        // Debug.Log(locToLoad.sceneReference.SubObjectName);
        if (currentLoadedScene != null)
        {
            StartCoroutine(UnloadPreviousScene());
        }
        else
        {
            LoadNewScene();
        }
    }

    private IEnumerator UnloadPreviousScene()
    {
        if (fScreen)
        {
            //变黑
            fadeEvent.FadeIn(fadeDuration);
        }

        yield return new WaitForSeconds(fadeDuration);

        // 广播事件显示血条调整
        unloadedSceneEvent.RaiseLoadRequestEvent(locToLoad, posToGo, true);

        yield return currentLoadedScene.sceneReference.UnLoadScene(); // 无论当前场景是什么，都卸载当前场景

        // 关闭人物
        playerTrans.gameObject.SetActive(false);

        // 加载新场景
        LoadNewScene();
    }

    private void LoadNewScene()
    {
        var loadingOperation = locToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
        loadingOperation.Completed += OnLoadCompleted;
    }

    /// <summary>
    /// 场景加载完成后
    /// </summary>
    /// <param name="handle"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
    {
        currentLoadedScene = locToLoad;

        playerTrans.position = posToGo;

        // 移动人物坐标后，启动人物
        playerTrans.gameObject.SetActive(true);

        if (fScreen)
        {
            //变透明
            fadeEvent.FadeOut(fadeDuration);
        }

        isLoading = false;

        if (currentLoadedScene.sceneType == SceneType.Location)
            // 场景加载完成后事件
            afterSceneLoadedEvent.RaiseEvent();
    }

    public DataDefinition GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    public void GetSaveData(Data data)
    {
        data.SaveGameScene(currentLoadedScene);
        // Debug.Log("Game Scene Saved.");
    }

    public void LoadData(Data data)
    {
        var playerID = playerTrans.GetComponent<DataDefinition>().ID;
        if (data.characterPosDict.ContainsKey(playerID))
        {
            posToGo = data.characterPosDict[playerID].ToVector3();
            locToLoad = data.GetSavedScene();

            OnLoadRequestEvent(locToLoad, posToGo, true);
        }
    }
}
