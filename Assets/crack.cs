using UnityEngine;
using System.Collections;

public class crack : MonoBehaviour
{
    public float shakeTime = 1.0f;     // 晃动持续时间
    public float shakeIntensity = 0.1f; // 晃动强度
    public bool isFalling = false;      // 是否正在下落
    public string playerTag = "Player"; // 玩家标签

    private Vector3 originalPosition;   // 原始位置
    private Rigidbody2D rb;               // 刚体组件

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取刚体组件
        rb = GetComponent<Rigidbody2D>();
        
        // 如果没有刚体组件，则添加一个
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 初始化时关闭重力
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        
        // 初始化isFalling为false，确保重载场景时重置状态
        isFalling = false;
        
        // 保存原始位置
        originalPosition = transform.position;
        
        // 确保位置正确
        transform.position = originalPosition;
        
        // 添加调试信息
        Debug.Log("裂缝平台初始化 - 位置: " + originalPosition);
    }

    // Update is called once per frame
    void Update()
    {
        // 如果正在下落，不需要额外操作
    }
    
    // 2D版本的触发器检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Crack检测到触发器碰撞: " + other.tag);
        // 检查碰撞物体是否为玩家
        if (other.CompareTag(playerTag) && !isFalling)
        {
            // 开始晃动效果
            StartCoroutine(ShakeAndFall());
        }
    }
    
    // 2D版本的碰撞检测
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Crack检测到碰撞: " + collision.gameObject.tag);
        // 检查碰撞物体是否为玩家
        if (collision.gameObject.CompareTag(playerTag) && !isFalling)
        {
            // 开始晃动效果
            StartCoroutine(ShakeAndFall());
        }
    }
    
    // 晃动并下落的协程
    private IEnumerator ShakeAndFall()
    {
        if (isFalling) yield break; // 防止重复触发
        
        Debug.Log("开始晃动效果");
        isFalling = true;
        float elapsed = 0.0f;
        
        while (elapsed < shakeTime)
        {
            // 2D随机偏移 (只在X和Y方向)
            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0
            );
            transform.position = originalPosition + randomOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
        
        Debug.Log("晃动结束，开始下落");
        // 2D物理系统下落设置
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1;
    }
}
