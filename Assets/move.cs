using UnityEngine;

public class move : MonoBehaviour
{
    [Header("风扇设置")]
    public Sprite[] fanFrames; // 风扇的两个状态图片
    public float animationSpeed = 10f; // 动画切换速度
    public bool isDeadly = true; // 是否致命
    
    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentFrame = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 确保提供了足够的帧
        if (fanFrames == null || fanFrames.Length < 2)
        {
            Debug.LogError("风扇需要至少两帧图片！请在Inspector中设置fanFrames数组。");
        }
        else
        {
            // 初始显示第一帧
            spriteRenderer.sprite = fanFrames[0];
        }
        
        // 确保有碰撞器，并设置为触发器
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // 添加圆形碰撞器
            CircleCollider2D circleCollider = gameObject.AddComponent<CircleCollider2D>();
            circleCollider.radius = 0.4f; // 调整合适的半径
            circleCollider.isTrigger = true; // 设置为触发器
            Debug.Log("已自动添加圆形触发器到风扇");
        }
        else
        {
            // 确保现有的碰撞器是触发器
            collider.isTrigger = true;
            Debug.Log("已将现有碰撞器设置为触发器");
        }
        
        // 设置标签
        if (string.IsNullOrEmpty(gameObject.tag))
        {
            gameObject.tag = "trap";
            Debug.Log("已将风扇标签设为trap");
        }
        
        // 添加Rigidbody2D以确保触发器正常工作
        if (GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            Debug.Log("已添加Kinematic Rigidbody2D到风扇");
        }
        
        Debug.Log("风扇已初始化，isDeadly: " + isDeadly);
    }

    // Update is called once per frame
    void Update()
    {
        if (fanFrames == null || fanFrames.Length < 2)
            return;
            
        // 计时器更新
        animationTimer += Time.deltaTime;
        
        // 当计时器超过帧间隔时切换帧
        if (animationTimer >= 1f / animationSpeed)
        {
            // 切换到下一帧
            currentFrame = (currentFrame + 1) % fanFrames.Length;
            spriteRenderer.sprite = fanFrames[currentFrame];
            
            // 重置计时器
            animationTimer = 0f;
        }
    }
    
    // 当有其他物体与风扇发生碰撞时
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDeadly && collision.gameObject.CompareTag("Player"))
        {
            // 如果碰撞的是玩家，则处理玩家死亡
            KillPlayer(collision.gameObject);
        }
    }
    
    // 也检测触发器碰撞（如果使用触发器）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDeadly && collision.CompareTag("Player"))
        {
            // 如果碰撞的是玩家，则处理玩家死亡
            KillPlayer(collision.gameObject);
        }
    }
    
    // 处理玩家死亡的函数
    private void KillPlayer(GameObject player)
    {
        // 添加日志确认被调用
        Debug.Log("风扇杀死玩家 - 游戏结束");
        
        // 尝试获取player脚本并调用Die方法
        player player_script = player.GetComponent<player>();
        if (player_script != null)
        {
            player_script.Die();
        }
        else
        {
            // 如果没有找到player脚本，尝试PlayerController
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.Die();
            }
            else
            {
                Debug.LogError("无法找到玩家脚本！无法执行死亡逻辑");
                // 直接销毁玩家对象作为后备方案
                Destroy(player);
            }
        }
    }
}
