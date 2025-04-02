using UnityEngine;
using System.Collections;

public class crack : MonoBehaviour
{
    public float shakeTime = 1.0f;     // 晃动持续时间
    public float shakeIntensity = 0.1f; // 晃动强度
    public bool isFalling = false;      // 是否正在下落
    public string playerTag = "Player"; // 玩家标签

    private Vector3 originalPosition;   // 原始位置
    private Rigidbody rb;               // 刚体组件

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 获取刚体组件
        rb = GetComponent<Rigidbody>();
        
        // 如果没有刚体组件，则添加一个
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 初始化时关闭重力，直到需要下落
        rb.useGravity = false;
        rb.isKinematic = true;
        
        // 保存原始位置
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 如果正在下落，不需要额外操作
    }
    
    // 碰撞检测
    private void OnTriggerEnter(Collider other)
    {
        // 检查碰撞物体是否为玩家
        if (other.CompareTag(playerTag) && !isFalling)
        {
            // 开始晃动效果
            StartCoroutine(ShakeAndFall());
        }
    }
    
    // 碰撞检测（如果使用的是碰撞器而非触发器）
    private void OnCollisionEnter(Collision collision)
    {
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
        isFalling = true;
        float elapsed = 0.0f;
        
        // 晃动阶段
        while (elapsed < shakeTime)
        {
            // 随机位置偏移
            Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
            transform.position = originalPosition + randomOffset;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 恢复原始位置
        transform.position = originalPosition;
        
        // 启用重力并允许物理作用
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
