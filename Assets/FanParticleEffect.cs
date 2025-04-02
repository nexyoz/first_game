using UnityEngine;

public class FanParticleEffect : MonoBehaviour
{
    [Header("粒子设置")]
    public Color particleColor = new Color(0.7f, 0.7f, 0.7f, 0.3f);
    public float particleSize = 0.1f;
    public float emissionRate = 10f;
    public float particleSpeed = 3f;
    public float particleLifetime = 1f;
    public float emissionRadius = 0.5f;
    
    private ParticleSystem fanParticles;
    
    private void Start()
    {
        CreateParticleSystem();
    }
    
    private void CreateParticleSystem()
    {
        // 创建粒子系统游戏对象
        GameObject particleObj = new GameObject("FanParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        // 添加粒子系统组件
        fanParticles = particleObj.AddComponent<ParticleSystem>();
        
        // 配置主模块
        var main = fanParticles.main;
        main.startColor = particleColor;
        main.startSize = particleSize;
        main.startSpeed = particleSpeed;
        main.startLifetime = particleLifetime;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // 配置形状模块
        var shape = fanParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = emissionRadius;
        shape.radiusThickness = 0.0f; // 在边缘发射
        
        // 配置发射模块
        var emission = fanParticles.emission;
        emission.rateOverTime = emissionRate;
        
        // 配置颜色随时间变化
        var colorOverLifetime = fanParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        // 粒子颜色会随着生命周期而变淡
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        // 启动粒子系统
        fanParticles.Play();
    }
    
    // 允许在运行时调整粒子系统
    public void UpdateParticleSystem()
    {
        if (fanParticles == null) return;
        
        var main = fanParticles.main;
        main.startColor = particleColor;
        main.startSize = particleSize;
        main.startSpeed = particleSpeed;
        main.startLifetime = particleLifetime;
        
        var shape = fanParticles.shape;
        shape.radius = emissionRadius;
        
        var emission = fanParticles.emission;
        emission.rateOverTime = emissionRate;
    }
    
    // 在编辑器中更改属性时更新粒子系统
    private void OnValidate()
    {
        if (Application.isPlaying && fanParticles != null)
        {
            UpdateParticleSystem();
        }
    }
} 