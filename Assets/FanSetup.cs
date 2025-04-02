using UnityEngine;

public class FanSetup : MonoBehaviour
{
    [Header("风扇设置")]
    public Sprite fanFrame1;  // 风扇第一帧
    public Sprite fanFrame2;  // 风扇第二帧
    public Sprite fanBase;    // 风扇底座精灵（静态部分）
    
    [Header("物理设置")]
    public bool useCollider = true;   // 是否使用碰撞体
    public bool useTrigger = false;   // 是否使用触发器
    
    private void Awake()
    {
        SetupFan();
    }
    
    private void SetupFan()
    {
        // 确保有SpriteRenderer组件
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // 如果需要，创建底座
        if (fanBase != null)
        {
            GameObject baseObj = new GameObject("FanBase");
            baseObj.transform.SetParent(transform);
            baseObj.transform.localPosition = Vector3.zero;
            
            SpriteRenderer baseRenderer = baseObj.AddComponent<SpriteRenderer>();
            baseRenderer.sprite = fanBase;
            baseRenderer.sortingOrder = -1; // 确保底座显示在叶片后面
        }
        
        // 设置碰撞体
        if (useCollider)
        {
            // 添加碰撞组件
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CircleCollider2D>();
            }
            
            // 如果使用触发器，设置触发器
            collider.isTrigger = useTrigger;
            
            // 添加Rigidbody2D以确保碰撞检测正常工作
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic; // 设置为运动学类型，这样风扇不会受到物理影响
            }
        }
        
        // 添加风扇移动脚本并配置帧动画
        move moveScript = GetComponent<move>();
        if (moveScript == null)
        {
            moveScript = gameObject.AddComponent<move>();
        }
        
        // 设置风扇帧
        if (fanFrame1 != null && fanFrame2 != null)
        {
            moveScript.fanFrames = new Sprite[] { fanFrame1, fanFrame2 };
        }
    }
    
    // 在编辑器中可视化风扇碰撞范围
    private void OnDrawGizmos()
    {
        if (useCollider)
        {
            Gizmos.color = Color.red;
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                Gizmos.DrawWireSphere(transform.position, collider.radius);
            }
            else
            {
                // 默认半径
                Gizmos.DrawWireSphere(transform.position, 0.5f);
            }
        }
    }
} 