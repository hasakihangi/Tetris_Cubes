using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class Cube : MonoBehaviour, IPooledObject
{
    public IObjectPool<Cube> pool;
    public SpriteRenderer spriteRenderer;
    public AnimationCurve noiseIntensityCurve;
    
    public void Release()
    {
        pool.Release(this);
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public float noiseSeed;
    
    public void Shake(float range, float scale, float t)
    {
        // 随着t变化位置
        float x = Mathf.PerlinNoise(t*scale+noiseSeed, 0);
        float y = Mathf.PerlinNoise(t*scale+noiseSeed, 1);
        x = 2 * x - 1;
        y = 2 * y - 1;
        x = x * range * noiseIntensityCurve.Evaluate(t);
        y = y * range * noiseIntensityCurve.Evaluate(t);
        transform.localPosition = transform.localPosition + new Vector3(x, y, 0);
    }
    
    // 移动
    public void PosLerp(Vector3 A, Vector3 B, float t)
    {
        // 需要记录初始位置, 在外边记录?
        transform.localPosition = Vector3.Lerp(A, B, t);
    }

    public void ColorLerp(Color A, Color B, float t)
    {
        spriteRenderer.color = Color.Lerp(A, B, t);
    }
    
    public void ScaleLerp(Vector3 A, Vector3 B, float t)
    {
        transform.localScale = Vector3.Lerp(A, B, t);
    }
}
