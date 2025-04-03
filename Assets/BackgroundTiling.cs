using UnityEngine;

public class BackgroundTiling : MonoBehaviour
{
    [Header("Tiling Settings")]
    [SerializeField] private Vector2 tiling = new Vector2(1, 1); // 平铺重复次数
    [SerializeField] private Sprite backgroundSprite; // 背景精灵
    [SerializeField] private Material customMaterial; // 添加自定义材质字段
    
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
            try
            {
                // 更新偏移值，负值表示向下移动
                offset.y -= scrollSpeed * Time.deltaTime;
                
                // 防止偏移值过大，当超过1时进行重置
                if (offset.y < -1f)
                    offset.y += 1f;
                    
                // 应用偏移值到材质
                material.mainTextureOffset = offset;
                
                // 尝试更新不同着色器使用的属性
                if (material.HasProperty("_MainTex_ST"))
                {
                    Vector4 st = material.GetVector("_MainTex_ST");
                    material.SetVector("_MainTex_ST", new Vector4(st.x, st.y, offset.x, offset.y));
                }
                
                if (material.HasProperty("_BaseMap_ST"))
                {
                    Vector4 st = material.GetVector("_BaseMap_ST");
                    material.SetVector("_BaseMap_ST", new Vector4(st.x, st.y, offset.x, offset.y));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("更新材质偏移时出错: " + e.Message);
            }
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
        // 优先使用预设的自定义材质
        if (customMaterial != null)
        {
            material = new Material(customMaterial);
        }
        else
        {
            // 使用最基础的内置着色器
            material = new Material(Shader.Find("Mobile/Unlit (Supports Lightmap)"));
            
            // 如果上面的着色器不可用，尝试更多备选项
            if (material.shader == null)
            {
                material = new Material(Shader.Find("Mobile/Particles/Alpha Blended"));
            }
            
            if (material.shader == null)
            {
                material = new Material(Shader.Find("Unlit/Texture"));
            }
            
            // 最后使用完全内置的透明着色器
            if (material.shader == null)
            {
                material = new Material(Shader.Find("Transparent/Diffuse"));
            }
        }
        
        // 如果提供了精灵，使用它的纹理
        if (backgroundSprite != null)
        {
            material.mainTexture = backgroundSprite.texture;
            
            // 确保纹理被设置为可读写和可重复
            if (material.mainTexture != null)
            {
                material.mainTexture.wrapMode = TextureWrapMode.Repeat;
            }
        }
        
        // 应用到渲染器
        meshRenderer.material = material;
        
        // 立即更新平铺设置
        UpdateTiling();
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
            try
            {
                // 直接设置主纹理的缩放
                material.mainTextureScale = tiling;
                
                // 尝试多种方式设置纹理平铺参数
                if (material.HasProperty("_MainTex_ST"))
                {
                    material.SetVector("_MainTex_ST", new Vector4(tiling.x, tiling.y, offset.x, offset.y));
                }
                
                if (material.HasProperty("_BaseMap_ST")) // URP着色器
                {
                    material.SetVector("_BaseMap_ST", new Vector4(tiling.x, tiling.y, offset.x, offset.y));
                }
                
                // 确保网格有正确的UV坐标
                UpdateMeshUVs();
                
                // 为确保属性更新，强制刷新材质
                meshRenderer.GetPropertyBlock(propertyBlock);
                meshRenderer.SetPropertyBlock(propertyBlock);
            }
            catch (System.Exception e)
            {
                Debug.LogError("更新平铺设置时出错: " + e.Message);
            }
        }
    }

    private void UpdateMeshUVs()
    {
        if (meshFilter == null || meshFilter.mesh == null) return;
        
        Mesh mesh = meshFilter.mesh;
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(tiling.x, 0),
            new Vector2(tiling.x, tiling.y),
            new Vector2(0, tiling.y)
        };
        
        mesh.uv = uvs;
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