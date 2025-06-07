using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{

    [Header("控制方式")]
    public bool useTouchControls = true;  // 是否使用触摸屏控制
    public bool useVariableJoystick = true; // 是否使用Variable Joystick（编辑器测试）

    [Header("触摸屏UI")]
    [SerializeField] private Image touchJoystickBackground;  // 虚拟摇杆背景
    [SerializeField] private Image touchJoystickHandle;      // 虚拟摇杆手柄
    [SerializeField] private Button jumpButton;         // 跳跃按钮
    [SerializeField] private Button attackButton;       // 攻击按钮
    [SerializeField] private Button touchWalkButton;         // 走路按钮
    [SerializeField] private float touchJoystickRange = 50f; // 摇杆活动范围

    [Header("Variable Joystick设置")]
    [SerializeField] private VariableJoystick variableJoystick; // Variable Joystick组件


    [Header("监听事件")]
    public SceneLoadEventSO loadEvent;
    public VoidEventSO afterSceneLoadedEvent;

    private PlayerInputControl inputControl;
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

    private bool isTouchWalking = false; // 触摸屏走路状态
    private Vector2 touchStartPosition;  // 触摸起始位置
    private bool isDragging = false;     // 是否正在拖拽摇杆

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

        // 初始化键盘控制
        inputControl.Gameplay.Jump.started += ctx => Jump();
        inputControl.Gameplay.Attack.started += ctx => Attack();

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

        runSpeed = speed;

        // 初始化触摸屏控制
        SetupTouchControls();
    }

    private void SetupTouchControls()
    {
        if (!useTouchControls)
        {
            DisableTouchUI();
            return;
        }

        // 设置按钮事件
        if (jumpButton != null)
        {
            // 添加点击事件
            jumpButton.onClick.AddListener(Jump);

            // 添加按下/释放效果
            EventTrigger jumpTrigger = jumpButton.gameObject.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((data) => { OnJumpButtonPressed(); });
            jumpTrigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((data) => { OnJumpButtonReleased(); });
            jumpTrigger.triggers.Add(pointerUp);
        }

        // 设置攻击按钮
        if (attackButton != null)
        {
            // 添加点击事件
            attackButton.onClick.AddListener(Attack);

            // 添加按下/释放效果
            EventTrigger attackTrigger = attackButton.gameObject.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((data) => { OnAttackButtonPressed(); });
            attackTrigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((data) => { OnAttackButtonReleased(); });
            attackTrigger.triggers.Add(pointerUp);
        }

        if (touchWalkButton != null)
        {
            // 添加持续按下检测
            EventTrigger walkTrigger = touchWalkButton.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
            pointerDownEntry.eventID = EventTriggerType.PointerDown;
            pointerDownEntry.callback.AddListener((data) => { OnTouchWalkButtonDown(); });
            walkTrigger.triggers.Add(pointerDownEntry);

            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
            pointerUpEntry.eventID = EventTriggerType.PointerUp;
            pointerUpEntry.callback.AddListener((data) => { OnTouchWalkButtonUp(); });
            walkTrigger.triggers.Add(pointerUpEntry);

            EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
            pointerExitEntry.eventID = EventTriggerType.PointerExit;
            pointerExitEntry.callback.AddListener((data) => { OnTouchWalkButtonUp(); });
            walkTrigger.triggers.Add(pointerExitEntry);
        }
    }

    private void OnEnable()
    {
        // 始终启用输入系统
        inputControl.Enable();

        if (useTouchControls)
        {
            EnableTouchUI();
        }

        loadEvent.LoadRequestEvent += OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
    }

    private void OnDisable()
    {
        // 始终禁用输入系统
        inputControl.Disable();

        if (useTouchControls)
        {
            DisableTouchUI();
        }

        loadEvent.LoadRequestEvent -= OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
    }

    private void Update()
    {
        CheckInputSources();
        CheckState();
    }

    private void CheckInputSources()
    {
        // 默认使用键盘输入
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();

        // 如果有触摸输入则覆盖
        if (useTouchControls && isDragging)
        {
            // 使用触摸输入（已在OnDrag中设置inputDirection）
        }
        // 如果有Variable Joystick输入则覆盖
        else if (useVariableJoystick && variableJoystick != null)
        {
            inputDirection = new Vector2(variableJoystick.Horizontal, variableJoystick.Vertical);
        }
    }


    #region 按钮控制方法

    // 跳跃功能 - 不需要参数
    public void Jump()
    {

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

    }

    // 攻击功能 - 不需要参数
    public void Attack()
    {



        playerAnimation.PlayAttack();
        isAttack = true;
    }

    // 跳跃按钮按下 - 需要BaseEventData参数
    public void OnJumpButtonPressed()
    {
        // 按钮按下效果
        if (jumpButton != null)
        {
            jumpButton.image.color = new Color(0.8f, 0.8f, 0.8f);
        }

    }

    // 跳跃按钮释放 - 需要BaseEventData参数
    public void OnJumpButtonReleased()
    {
        // 按钮释放效果
        if (jumpButton != null)
        {
            jumpButton.image.color = Color.white;
        }
    }

    // 攻击按钮按下 - 需要BaseEventData参数
    public void OnAttackButtonPressed()
    {
        // 按钮按下效果
        if (attackButton != null)
        {
            attackButton.image.color = new Color(0.8f, 0.8f, 0.8f);
        }

        // 可以直接调用攻击功能
        Attack();
    }

    // 攻击按钮释放 - 需要BaseEventData参数
    public void OnAttackButtonReleased()
    {
        // 按钮释放效果
        if (attackButton != null)
        {
            attackButton.image.color = Color.white;
        }
    }

    #endregion

    private void FixedUpdate()
    {
        if (!isHurt && !isAttack)
            Move();
    }

    private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
    {
        if (!useTouchControls)
        {
            inputControl.Gameplay.Disable();
            // 禁用Variable Joystick
            if (variableJoystick != null)
                variableJoystick.gameObject.SetActive(false);
        }
        else
        {
            DisableTouchUI();
            // 禁用Variable Joystick
            if (variableJoystick != null)
                variableJoystick.gameObject.SetActive(false);
        }
    }

    private void OnAfterSceneLoadedEvent()
    {
        if (!useTouchControls)
        {
            inputControl.Gameplay.Enable();
            // 根据设置决定是否启用Variable Joystick
            if (variableJoystick != null)
                variableJoystick.gameObject.SetActive(false);
        }
        else
        {
            EnableTouchUI();
            // 禁用Variable Joystick（如果使用触摸控制）
            if (variableJoystick != null)
                variableJoystick.gameObject.SetActive(useVariableJoystick);
        }
    }

    public void Move()
    {
        // 执行移动
        if (!isCrouch)
            rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);

        // 人物反转（仅处理水平方向）
        if (inputDirection.x < 0 && !sr.flipX)
        {
            sr.flipX = true;
        }
        else if (inputDirection.x > 0 && sr.flipX)
        {
            sr.flipX = false;
        }

        // 下蹲检测
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

        // 动画控制（合并原有逻辑）

    }



    public void PlayerAttack()
    {
        if (!physicsCheck.isGround)
            return;

        playerAnimation.PlayAttack();
        isAttack = true;
    }

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

        if (!useTouchControls)
        {
            inputControl.Gameplay.Disable();
        }
        else
        {
            DisableTouchUI();
        }
    }

    private void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGround ? normal : wall;
    }

    #region 触摸屏控制实现

    private void EnableTouchUI()
    {
        if (touchJoystickBackground != null)
            touchJoystickBackground.gameObject.SetActive(true);

        if (jumpButton != null)
            jumpButton.gameObject.SetActive(true);

        if (attackButton != null)
            attackButton.gameObject.SetActive(true);

        if (touchWalkButton != null)
            touchWalkButton.gameObject.SetActive(true);
    }

    private void DisableTouchUI()
    {
        if (touchJoystickBackground != null)
            touchJoystickBackground.gameObject.SetActive(false);

        if (jumpButton != null)
            jumpButton.gameObject.SetActive(false);

        if (attackButton != null)
            attackButton.gameObject.SetActive(false);

        if (touchWalkButton != null)
            touchWalkButton.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!useTouchControls || !touchJoystickBackground.gameObject.activeInHierarchy)
            return;

        isDragging = true;
        touchStartPosition = eventData.position;

        if (touchJoystickBackground != null)
        {
            // 将摇杆移动到触摸位置
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                touchJoystickBackground.rectTransform.parent as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 worldPos);

            touchJoystickBackground.transform.position = worldPos;
            touchJoystickHandle.transform.position = worldPos;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || !useTouchControls)
            return;

        Vector2 touchPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            touchJoystickBackground.rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out touchPosition);

        // 修复：将Vector3转换为Vector2以消除歧义
        Vector2 backgroundPosition = (Vector2)(touchJoystickBackground.rectTransform.parent as RectTransform)
            .InverseTransformPoint(touchJoystickBackground.transform.position);

        Vector2 offset = touchPosition - backgroundPosition;

        // 归一化并限制范围
        Vector2 direction = Vector2.ClampMagnitude(offset, touchJoystickRange) / touchJoystickRange;

        // 更新摇杆位置
        if (touchJoystickHandle != null)
        {
            touchJoystickHandle.rectTransform.anchoredPosition = direction * touchJoystickRange;
        }

        // 更新输入方向
        inputDirection = direction;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        // 重置摇杆位置
        if (touchJoystickHandle != null)
        {
            touchJoystickHandle.rectTransform.anchoredPosition = Vector2.zero;
        }

        // 重置输入方向
        inputDirection = Vector2.zero;
    }

    private void OnTouchWalkButtonDown()
    {
        isTouchWalking = true;
        if (physicsCheck.isGround)
            speed = walkSpeed;
    }

    private void OnTouchWalkButtonUp()
    {
        isTouchWalking = false;
        if (physicsCheck.isGround)
            speed = runSpeed;
    }

    #endregion
}