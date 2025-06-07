using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    [Header("监听事件")]
    public SceneLoadEventSO sceneLoadEvent;
    public VoidEventSO afterSceneLoadedEvent;
    public VoidEventSO loadDataEvent;
    public VoidEventSO backToMenuEvent;

    public PlayerInputControl inputControl;
    private PhysicsCheck physicsCheck;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private CapsuleCollider2D coll;
    private PlayerAnimation playerAnimation;
    private AudioDefinition audioDefinition;
    public Vector2 inputDirection;

    [Header("基本参数")]
    public float speed;
    private float runSpeed;
    private float walkSpeed => speed / 2.5f;
    public float jumpForce;
    public float hurtForce;
    private Vector2 originalSize; // 存储原始碰撞体大小
    private Vector2 originalOffset; // 存储原始碰撞体位移

    [Header("物理材质")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;

    [Header("状态")]
    public bool isHurt;
    public bool isCrouch;
    public bool isDead;
    public bool isAttack; // 攻击状态的判定变量

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        physicsCheck = GetComponent<PhysicsCheck>();
        coll = GetComponent<CapsuleCollider2D>();
        playerAnimation = GetComponent<PlayerAnimation>();
        audioDefinition = GetComponent<AudioDefinition>();
        inputControl = new PlayerInputControl();

        originalOffset = coll.offset;
        originalSize = coll.size;


        // 为跳跃按钮被按下注册函数
        inputControl.Gameplay.Jump.started += Jump;

        #region 强制走路
        runSpeed = speed;
        inputControl.Gameplay.WalkButton.performed += ctx =>
        {
            if (physicsCheck.isGround)
                speed = walkSpeed;
        };

        inputControl.Gameplay.WalkButton.canceled += ctx =>
        {
            if (physicsCheck.isGround)
                speed = runSpeed;
        };
        #endregion

        // 攻击
        inputControl.Gameplay.Attack.started += PlayerAttack;
        inputControl.Enable();
    }

    private void OnEnable()
    {
        // 人物被关闭时，输入系统也关闭
        sceneLoadEvent.LoadRequestEvent += OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
    }

    private void OnDisable()
    {
        // 人物被激活时，输入系统也激活
        inputControl.Disable();
        sceneLoadEvent.LoadRequestEvent -= OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
    }

    // 读取游戏进度
    private void OnLoadDataEvent()
    {
        isDead = false;
    }

    // 周期性函数，每帧执行
    private void Update()
    {
        // 读取控制器输入作为二维向量（大小和方向）
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();

        CheckState();
    }

    // 周期性函数，固定执行
    private void FixedUpdate()
    {
        if (!isHurt && !isAttack)
            Move();
    }

    // 测试
    // private void OnTriggerStay2D(Collider2D other){
    //     Debug.Log(other.name);
    // }

    // 场景加载过程禁用控制
    private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
    {
        inputControl.Gameplay.Disable();
    }

    // 加载结束启用控制
    private void OnAfterSceneLoadedEvent()
    {
        inputControl.Gameplay.Enable();
    }

    public void Move()
    {
        // 执行移动，水平方向的分量：是控制器方向和速度的乘积，竖直方向的分量：是原本的值
        if (!isCrouch)
            rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);

        // 人物反转
        if (inputDirection.x < 0)
            sr.flipX = true;
        else if (inputDirection.x > 0)
            sr.flipX = false;

        isCrouch = inputDirection.y < -0.5f && physicsCheck.isGround;

        if (isCrouch)
        {
            // 修改碰撞体大小和位移
            coll.offset = new Vector2(originalOffset.x, 0.8f);
            coll.size = new Vector2(originalSize.x, 1.6f);

        }
        else
        {
            // 还原碰撞体
            coll.size = originalSize;
            coll.offset = originalOffset;
        }
    }

    private void Jump(InputAction.CallbackContext obj)
    {

        // 如果人物在地面上，执行跳跃
        if (physicsCheck.isGround)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            audioDefinition.PlayAudioClip();
        }
    }

    private void PlayerAttack(InputAction.CallbackContext obj)
    {
        if (!physicsCheck.isGround)
            return;
        playerAnimation.PlayAttack();
        isAttack = true;
    }

    #region UnityEvent
    // 受伤时向受击方向的反方向位移
    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2((transform.position.x - attacker.position.x), 0).normalized;

        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
    }

    public void PlayerDead()
    {
        isDead = true;
        inputControl.Gameplay.Disable();
    }
    #endregion

    private void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGround ? normal : wall;
        // Debug.Log(coll.sharedMaterial);
    }

    // public void Fuckshit()
    // {
    //     Debug.Log("fuck this shit!");
    // }
}
