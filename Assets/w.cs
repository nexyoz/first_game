using UnityEngine;

public class w : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5f;      // 移动速度
    public float jumpForce = 10f;     // 跳跃力度
    public Transform groundCheck;      // 地面检测点
    public float groundCheckRadius = 0.1f;  // 地面检测半径
    public LayerMask groundLayer;      // 地面图层

    private Rigidbody2D rb;           // 刚体组件
    private bool isGrounded;          // 是否着地
    private float moveInput;          // 水平输入值
    private bool facingRight = true;  // 是否面向右侧
    private Animator animator;        // 动画组件（如果有）

    // 组件初始化
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // 如果没有手动设置地面检测点，则创建一个
        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = check.transform;
        }
    }

    // 每帧更新输入和动画
    void Update()
    {
        // 获取水平输入（左右箭头或AD键）
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // 跳跃输入（空格键或W键）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }
        
        // 更新动画参数（如果有）
        UpdateAnimations();
    }
    
    // 处理物理相关的更新
    void FixedUpdate()
    {
        // 检查是否着地
        CheckGrounded();
        
        // 移动角色
        Move();
    }
    
    // 移动功能
    void Move()
    {
        // 设置水平速度
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // 根据移动方向翻转角色
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
    }
    
    // 跳跃功能
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
        // 播放跳跃音效
        // AudioManager.Instance.PlaySound("jump");
    }
    
    // 检查是否着地
    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    // 翻转角色朝向
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    // 更新动画状态
    void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalSpeed", rb.linearVelocity.y);
        }
    }
    
    // 绘制地面检测范围（仅在编辑器中可见）
    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
