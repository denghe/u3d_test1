using System.Collections.Generic;
using UnityEngine;


// 底层对象池( 未来直接玩 shader 填 buf 后可抛弃 )
public struct GO {
    public GameObject g;
    public SpriteRenderer r;
    public Transform t;
    public bool actived;

    public void Enable() {
        if (!actived) {
            actived = true;
            g.SetActive(true);
        }
    }

    public void Disable() {
        if (actived) {
            actived = false;
            g.SetActive(false);
        }
    }

    // 测试一下对象池 看看是否省 cpu
    public static Stack<GO> pool;
    public static Material material;

    // 从对象池拿 GO 并返回. 没有就新建
    public static void Pop(ref GO o, string sortingLayerName = "Default") {
#if UNITY_EDITOR
        Debug.Assert(o.g == null);
#endif
        if (!pool.TryPop(out o)) {
            o = New();
        }
        o.r.sortingLayerName = sortingLayerName;
    }

    // 将 GO 退回对象池
    public static void Push(ref GO o) {
#if UNITY_EDITOR
        Debug.Assert(o.g != null);
#endif
        o.Disable();
        pool.Push(o);
        o.g = null;
        o.r = null;
        o.t = null;
        o.actived = false;
    }

    // 新建 GO 并返回( 顺便设置统一的材质球 排序 pivot )
    public static GO New() {
        GO o = new();
        o.g = new GameObject();
        o.r = o.g.AddComponent<SpriteRenderer>();
        o.r.material = material;
        o.r.spriteSortPoint = SpriteSortPoint.Pivot;
        o.t = o.g.GetComponent<Transform>();
        o.g.SetActive(false);
        return o;
    }

    // 预填充
    public static void Init(Material material, int count) {
#if UNITY_EDITOR
        Debug.Assert(GO.material == null);
#endif
        GO.material = material;
        GO.pool = new(count);
        for (int i = 0; i < count; i++) {
            pool.Push(New());
        }
    }

    // 释放池资源
    public static void Destroy() {
        foreach (var o in pool) {
            GameObject.Destroy(o.g);
        }
        pool.Clear();
    }
}
