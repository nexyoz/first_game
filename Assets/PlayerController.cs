using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    
    [Header("玩家状态")]
    public bool isAlive = true;
    public bool isGrounded = false;
    
    [Header("死亡效果")]
    public GameObject deathEffect;
    public float respawnDelay = 1.5f;
    
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 确保有必要的组件
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 添加碰撞体
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
        
        // 设置玩家标签
        gameObject.tag = "Player";
    }
    
    private void Update()
    {
        if (!isAlive) return;
        
        // 左右移动
        float moveX = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
        
        // 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
            isGrounded = false;
        }
        
        // 翻转角色朝向
        if (moveX > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveX < 0)
        {
            spriteRenderer.flipX = true;
        }
        
        // 更新动画（如果有动画器）
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveX));
            animator.SetBool("IsGrounded", isGrounded);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 检测是否着地
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
    
    // 玩家死亡方法，可以被外部调用
    public void Die()
    {
        if (!isAlive) return;
        
        isAlive = false;
        rb.linearVelocity = Vector2.zero;
        
        // 禁用控制
        rb.bodyType = RigidbodyType2D.Kinematic;
        GetComponent<Collider2D>().enabled = false;
        
        // 播放死亡动画（如果有）
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        // 播放死亡效果（如果有）
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log("玩家死亡");
        
        // 延迟重生或重新加载场景
        Invoke("Respawn", respawnDelay);
    }
    
    private void Respawn()
    {
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
} 