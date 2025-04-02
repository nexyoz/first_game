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
        // 可以在这里添加死亡效果，如粒子效果
        Debug.Log("玩家被风扇杀死");
        
        // 销毁玩家对象或调用玩家的死亡函数
        // 根据您的游戏设计可以选择以下方式之一：
        
        // 方式1：直接销毁
        // Destroy(player);
        
        // 方式2：调用玩家脚本中的死亡方法
        player.GetComponent<PlayerController>()?.Die();
        
        // 方式3：发送消息
        // player.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
    }
}
