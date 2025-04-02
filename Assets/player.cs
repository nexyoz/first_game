using UnityEngine;

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
        
        if (groundCheck == null)
        {
            Debug.LogError("请设置地面检测点 (groundCheck)!");
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
            Jump();
        }
        
        // 更新动画
        UpdateAnimation();
    }
    
    // 物理相关处理放在FixedUpdate中
    void FixedUpdate()
    {
        // 地面检测
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        // 如果在地面上，重置二连跳状态并强制停止下落
        if (isGrounded)
        {
            // 强制停止垂直速度
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            
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
        if (isGrounded)
        {
            // 第一段跳跃
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            ChangeState(PlayerState.Jumping);
        }
        else if (canDoubleJump && !hasDoubleJumped)
        {
            // 二连跳
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
            hasDoubleJumped = true;
            ChangeState(PlayerState.DoubleJumping);
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
        // 检测是否碰到陷阱
        if (other.CompareTag("trap"))
        {
            Die();
        }
    }
    
    void Die()
    {
        // 角色死亡逻辑
        Debug.Log("角色死亡");
        
        // 禁用控制
        this.enabled = false;
        
        // 可以在这里添加死亡动画或其他效果
        
        // 重新加载当前场景（延迟1秒）
        Invoke("RestartLevel", 1f);
    }
    
    void RestartLevel()
    {
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
