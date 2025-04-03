using UnityEngine;

public class box : MonoBehaviour
{
    [Header("箱体物理属性")]
    public float mass = 3.0f;                 // 箱体质量
    public float drag = 1.0f;                 // 线性阻力
    public PhysicsMaterial2D physicsMaterial; // 物理材质（可选）

    [Header("箱体状态")]
    public bool isGrounded = false;           // 是否在地面上
    public LayerMask groundLayer;             // 地面检测层
    
    private Rigidbody2D rb;                   // 刚体组件
    private BoxCollider2D boxCollider;        // 碰撞体组件
    private Vector3 originalPosition;         // 初始位置
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 初始化组件
        SetupComponents();
        
        // 保存初始位置，用于场景重置
        originalPosition = transform.position;
        
        Debug.Log("箱体初始化完成 - 位置: " + originalPosition);
    }

    void FixedUpdate()
    {
        // 检测箱体是否在地面上
        CheckGrounded();
        
        // 如果箱体超出一定范围，重置位置
        if (transform.position.y < -20f)
        {
            ResetBox();
        }
    }

    // 初始化组件
    void SetupComponents()
    {
        // 获取或添加刚体组件
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 设置刚体属性
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = 0.05f;
        rb.gravityScale = 1.0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 防止旋转
        
        // 获取或添加碰撞体
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            
            // 自动调整碰撞体大小适应精灵
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                boxCollider.size = spriteRenderer.sprite.bounds.size;
            }
        }
        
        // 应用物理材质
        if (physicsMaterial != null)
        {
            boxCollider.sharedMaterial = physicsMaterial;
        }
        
        // 如果层没有设置，默认使用地面层
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }
    
    // 检测箱体是否在地面上
    void CheckGrounded()
    {
        // 获取碰撞体的边界
        Bounds bounds = boxCollider.bounds;
        
        // 设置检测起点为箱体底部中心略微上方
        float rayLength = 0.1f;
        Vector2 rayStart = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
        
        // 向下发射射线检测地面
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, rayLength, groundLayer);
        
        // 更新地面状态
        isGrounded = hit.collider != null;
    }
    
    // 重置箱体到初始位置
    public void ResetBox()
    {
        transform.position = originalPosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        Debug.Log("箱体已重置到初始位置");
    }
    
    // 在编辑器中可视化地面检测
    void OnDrawGizmosSelected()
    {
        if (boxCollider != null)
        {
            Bounds bounds = boxCollider.bounds;
            Vector2 rayStart = new Vector2(bounds.center.x, bounds.min.y + 0.05f);
            
            // 绘制地面检测线
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rayStart, rayStart + Vector2.down * 0.1f);
        }
    }
    
    // 如果需要在场景重新加载时重置箱体，可以在player.cs的RestartLevel方法中调用此方法
    public static void ResetAllBoxes()
    {
        box[] boxes = GameObject.FindObjectsOfType<box>();
        foreach (box b in boxes)
        {
            b.ResetBox();
        }
    }
}
