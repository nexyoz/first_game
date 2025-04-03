using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class player : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("二连跳参数")]
    public bool canDoubleJump = true;
    private bool hasDoubleJumped = false;

    [Header("动画控制")]
    public Sprite[] idleSprites;     // 静止呼吸动画帧
    public Sprite[] runSprites;      // 跑步动画帧
    public Sprite[] jumpSprites;     // 跳跃动画帧
    public Sprite[] doubleJumpSprites; // 二连跳动画帧
    public Sprite[] fallingSprites;  // 下落动画帧
    public float animationSpeed = 0.1f;

    [Header("游戏结束设置")]
    public GameObject gameOverPrefab; // 游戏结束UI预制体
    private GameObject gameOverInstance; // 游戏结束UI实例

    // 组件引用
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentFrame = 0;
    private bool isGrounded;
    private bool isFacingRight = true;
    private float horizontalInput;
    
    // 当前状态
    private enum PlayerState { Idle, Running, Jumping, DoubleJumping, Falling }
    private PlayerState currentState = PlayerState.Idle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 初始化速度为零
        rb.linearVelocity = Vector2.zero;
        
        // 添加初始化调试信息
        Debug.Log("角色初始化 - 位置: " + transform.position);
        
        if (groundCheck == null)
        {
            Debug.LogError("请设置地面检测点 (groundCheck)!");
        }
        else
        {
            Debug.Log("地面检测点位置: " + groundCheck.position + 
                     ", 检测半径: " + groundCheckRadius + 
                     ", 地面层掩码: " + groundLayer.value);
        }
        
        if (rb != null)
        {
            Debug.Log("Rigidbody2D设置 - 质量: " + rb.mass + 
                     ", 重力缩放: " + rb.gravityScale + 
                     ", 约束: " + rb.constraints);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 获取水平输入（放在Update中以获得更好的响应性）
        horizontalInput = Input.GetAxis("Horizontal");
        
        // 跳跃控制
        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("检测到跳跃按键输入");
            Jump();
        }
        
        // 更新动画
        UpdateAnimation();
        
        // 每秒输出一次状态信息（使用Time.frameCount控制频率）
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log("角色状态 - 位置: " + transform.position + 
                     ", 速度: " + rb.linearVelocity + 
                     ", 状态: " + currentState +
                     ", 在地面: " + isGrounded);
        }
    }
    
    // 物理相关处理放在FixedUpdate中
    void FixedUpdate()
    {
        // 地面检测
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 添加调试信息
        if (wasGrounded != isGrounded)
        {
            Debug.Log("地面状态变化 - isGrounded: " + isGrounded + 
                     ", 检测位置: " + groundCheck.position + 
                     ", 半径: " + groundCheckRadius);
        }
        
        // 如果在地面上，重置二连跳状态并强制停止下落
        if (isGrounded)
        {
            // 注释掉下面这行代码，因为它可能导致跳跃力被立即清零
            // rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            
            hasDoubleJumped = false;
            if (currentState == PlayerState.Jumping || currentState == PlayerState.DoubleJumping || currentState == PlayerState.Falling)
            {
                ChangeState(PlayerState.Idle);
            }
        }
        else
        {
            // 如果不在地面上且下落，切换到下落状态
            if (rb.linearVelocity.y < -0.1f && currentState != PlayerState.Falling)
            {
                ChangeState(PlayerState.Falling);
            }
            // 如果不在地面上且上升，保持跳跃或二连跳状态
            else if (rb.linearVelocity.y > 0.1f)
            {
                // 保持当前的跳跃状态
                if (currentState != PlayerState.Jumping && currentState != PlayerState.DoubleJumping)
                {
                    ChangeState(PlayerState.Jumping);
                }
            }
        }
        
        // 移动角色
        MovePlayer(horizontalInput);
    }
    
    void MovePlayer(float horizontalInput)
    {
        // 应用水平移动
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        
        // 如果有水平移动且在地面上，切换到跑步状态
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            if (isGrounded && currentState != PlayerState.Running)
            {
                ChangeState(PlayerState.Running);
            }
            
            // 翻转角色面向方向
            if ((horizontalInput > 0 && !isFacingRight) || (horizontalInput < 0 && isFacingRight))
            {
                Flip();
            }
        }
        else if (isGrounded && currentState != PlayerState.Idle)
        {
            ChangeState(PlayerState.Idle);
        }
    }
    
    void Jump()
    {
        Debug.Log("Jump方法被调用 - isGrounded: " + isGrounded + ", hasDoubleJumped: " + hasDoubleJumped);
        
        if (isGrounded)
        {
            Debug.Log("执行第一段跳跃 - 跳跃力度: " + jumpForce);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            // 添加这行来跟踪应用力后的状态
            Debug.Log("应用跳跃力后 - 速度Y: " + rb.linearVelocity.y);
            
            ChangeState(PlayerState.Jumping);
        }
        else if (canDoubleJump && !hasDoubleJumped)
        {
            // 二连跳
            Debug.Log("执行二连跳 - 跳跃力度: " + (jumpForce * 0.8f));
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
            hasDoubleJumped = true;
            ChangeState(PlayerState.DoubleJumping);
        }
        else
        {
            Debug.Log("无法跳跃 - 不在地面上且已使用二段跳");
        }
    }
    
    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
    
    void ChangeState(PlayerState newState)
    {
        currentState = newState;
        currentFrame = 0;
        animationTimer = 0f;
    }
    
    void UpdateAnimation()
    {
        Sprite[] currentAnimation = null;
        
        // 根据当前状态选择动画帧数组
        switch (currentState)
        {
            case PlayerState.Idle:
                currentAnimation = idleSprites;
                break;
            case PlayerState.Running:
                currentAnimation = runSprites;
                break;
            case PlayerState.Jumping:
                currentAnimation = jumpSprites;
                break;
            case PlayerState.DoubleJumping:
                currentAnimation = doubleJumpSprites != null && doubleJumpSprites.Length > 0 ? 
                                   doubleJumpSprites : jumpSprites; // 如果没有专门的二连跳动画，使用跳跃动画
                break;
            case PlayerState.Falling:
                currentAnimation = fallingSprites != null && fallingSprites.Length > 0 ? 
                                   fallingSprites : jumpSprites; // 如果没有专门的下落动画，使用跳跃动画
                break;
        }
        
        // 如果有可用的动画帧，更新动画
        if (currentAnimation != null && currentAnimation.Length > 0)
        {
            animationTimer += Time.deltaTime;
            
            if (animationTimer >= animationSpeed)
            {
                animationTimer = 0f;
                currentFrame = (currentFrame + 1) % currentAnimation.Length;
                spriteRenderer.sprite = currentAnimation[currentFrame];
            }
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("玩家触发碰撞: " + other.gameObject.name + ", 标签: " + other.tag);
        
        // 检测是否碰到陷阱
        if (other.CompareTag("trap"))
        {
            Debug.Log("检测到trap标签 - 触发死亡");
            Die();
        }
    }
    
    // 添加碰撞检测（防止isTrigger设置问题）
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("玩家发生碰撞: " + collision.gameObject.name + ", 标签: " + collision.gameObject.tag);
        
        // 检测是否碰到陷阱
        if (collision.gameObject.CompareTag("trap"))
        {
            Debug.Log("检测到trap标签 - 触发死亡");
            Die();
        }
    }
    
    public void Die()
    {
        // 角色死亡逻辑
        Debug.Log("角色死亡 - 显示Game Over");
        
        // 禁用控制
        this.enabled = false;
        
        // 禁用刚体物理效果
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        
        // 显示Game Over
        ShowGameOver();
        
        // 重新加载当前场景（延迟2秒）
        Invoke("RestartLevel", 2f);
    }
    
    // 显示Game Over文本
    void ShowGameOver()
    {
        // 检查是否已存在GameOver UI
        GameObject existingUI = GameObject.Find("GameOver");
        if (existingUI != null)
        {
            Debug.Log("发现已存在的GameOver UI，将使用它");
            gameOverInstance = existingUI;
            return;
        }
        
        // 如果没有预制体，动态创建更简单的UI
        Debug.Log("动态创建Game Over UI - 简化版本");
        
        // 创建Game Over文本
        GameObject gameOverObj = new GameObject("GameOver");
        gameOverInstance = gameOverObj;
        
        // 添加Canvas组件
        Canvas canvas = gameOverObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 确保在最上层
        
        // 添加CanvasScaler组件
        CanvasScaler scaler = gameOverObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);
        
        // 添加GraphicRaycaster组件（UI交互需要）
        gameOverObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // 创建背景面板
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(gameOverObj.transform, false);
        
        // 添加面板组件
        UnityEngine.UI.Image panel = panelObj.AddComponent<UnityEngine.UI.Image>();
        panel.color = new Color(0, 0, 0, 0.7f); // 半透明黑色背景
        
        // 设置面板尺寸
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        // 创建文本对象
        GameObject textObj = new GameObject("GameOverText");
        textObj.transform.SetParent(gameOverObj.transform, false);
        
        // 添加文本组件
        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "GAME OVER";
        
        // 尝试使用系统字体
        try {
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        } catch {
            Font[] fonts = Resources.FindObjectsOfTypeAll<Font>();
            if (fonts.Length > 0) {
                text.font = fonts[0];
                Debug.Log("使用找到的字体: " + fonts[0].name);
            }
        }
        
        if (text.font == null) {
            Debug.LogError("无法找到字体资源！Game Over文本可能不可见");
        }
        
        text.fontSize = 50;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.red;
        
        // 设置文本位置为屏幕中央
        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(400, 100);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // 确保在场景切换时不被销毁，但会在RestartLevel中手动销毁
        DontDestroyOnLoad(gameOverObj);
    }
    
    // 清理资源
    void OnDestroy()
    {
        // 销毁Game Over UI以避免内存泄漏
        if (gameOverInstance != null)
        {
            Destroy(gameOverInstance);
        }
    }
    
    void RestartLevel()
    {
        // 在重新加载场景前清理Game Over UI
        if (gameOverInstance != null)
        {
            Destroy(gameOverInstance);
            gameOverInstance = null;
        }
        
        // 查找和销毁所有标记为DontDestroyOnLoad的对象
        GameObject[] persistentObjects = GameObject.FindObjectsOfType<GameObject>(true)
            .Where(g => g.scene.buildIndex == -1).ToArray();
        
        foreach (GameObject obj in persistentObjects)
        {
            // 避免销毁非本脚本创建的对象
            if (obj.name == "GameOver" || obj.name.StartsWith("GameOver"))
            {
                Debug.Log("销毁持久UI对象: " + obj.name);
                Destroy(obj);
            }
        }
        
        // 重置所有箱体到初始位置
        box.ResetAllBoxes();
        
        Debug.Log("重新加载场景");
        
        // 重新加载当前场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
    
    // 在编辑器中绘制地面检测范围（仅用于调试）
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
