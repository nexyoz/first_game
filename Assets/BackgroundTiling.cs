using UnityEngine;

public class BackgroundTiling : MonoBehaviour
{
    [Header("Tiling Settings")]
    [SerializeField] private Vector2 tiling = new Vector2(1, 1); // 平铺重复次数
    [SerializeField] private Sprite backgroundSprite; // 背景精灵
    
    [Header("Scrolling Settings")]
    [SerializeField] private float scrollSpeed = 0.1f; // 滚动速度
    [SerializeField] private bool scrollEnabled = true; // 是否启用滚动

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Material material;
    private MaterialPropertyBlock propertyBlock;
    private Vector2 offset = Vector2.zero; // 纹理偏移值

    void Awake()
    {
        // 确保有必要的组件
        EnsureComponents();
        
        // 创建网格和材质
        CreateMesh();
        SetupMaterial();
        
        // 调整大小并设置平铺
        AdjustSize();
        UpdateTiling();
    }
    
    void Update()
    {
        if (scrollEnabled && material != null)
        {
            // 更新偏移值，负值表示向下移动
            offset.y -= scrollSpeed * Time.deltaTime;
            
            // 防止偏移值过大，当超过1时进行重置
            if (offset.y < -1f)
                offset.y += 1f;
                
            // 应用偏移值到材质
            material.mainTextureOffset = offset;
        }
    }

    private void EnsureComponents()
    {
        // 获取或添加必要的组件
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
            
        propertyBlock = new MaterialPropertyBlock();
    }

    private void CreateMesh()
    {
        // 创建一个简单的四边形网格
        Mesh mesh = new Mesh();
        
        // 四个顶点（按逆时针顺序）
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0)
        };
        
        // 三角形索引
        int[] triangles = new int[6]
        {
            0, 2, 1, // 第一个三角形
            0, 3, 2  // 第二个三角形
        };
        
        // UV坐标（对应于顶点）
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        
        // 设置网格数据
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        
        // 应用到MeshFilter
        meshFilter.mesh = mesh;
    }

    private void SetupMaterial()
    {
        // 创建一个使用Unlit/Texture着色器的材质
        material = new Material(Shader.Find("Unlit/Texture"));
        
        // 如果提供了精灵，使用它的纹理
        if (backgroundSprite != null)
        {
            material.mainTexture = backgroundSprite.texture;
        }
        
        // 设置纹理环绕模式
        material.mainTexture.wrapMode = TextureWrapMode.Repeat;
        
        // 应用到渲染器
        meshRenderer.material = material;
    }

    // 调整大小以适配相机视图
    private void AdjustSize()
    {
        if (Camera.main == null) return;

        // 计算相机视野的世界尺寸
        float worldHeight = Camera.main.orthographicSize * 2f;
        float worldWidth = worldHeight * Camera.main.aspect;

        // 设置物体的缩放以匹配相机视野
        transform.localScale = new Vector3(worldWidth, worldHeight, 1);
    }

    // 更新平铺设置
    private void UpdateTiling()
    {
        if (material != null)
        {
            // 获取当前属性
            meshRenderer.GetPropertyBlock(propertyBlock);
            
            // 设置主纹理的缩放
            material.mainTextureScale = tiling;
            
            // 应用属性块
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
    
    // 设置滚动速度
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
    
    // 启用/禁用滚动
    public void SetScrollEnabled(bool enabled)
    {
        scrollEnabled = enabled;
    }

    // 在编辑器中修改时更新
    void OnValidate()
    {
        if (material != null)
        {
            UpdateTiling();
        }
    }
}