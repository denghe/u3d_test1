using System.Collections.Generic;
using UnityEngine;


// 底层对象池( 未来直接玩 shader 填 buf 后可抛弃 )
public struct GO {
    public GameObject g;
    public SpriteRenderer r;
    public Transform t;

    // 测试一下对象池 看看是否省 cpu
    public static Stack<GO> pool;
    public static Material material;
    public static SpaceRingDiffuseData spaceRDD = new(100, 64);

    // 从对象池拿 GO 并返回. 没有就新建
    public static void Pop(ref GO o, Sprite s = null) {
#if UNITY_EDITOR
        Debug.Assert(o.g == null);
#endif
        if (pool.TryPop(out o)) {
            o.g.SetActive(true);
            o.r.sprite = s;
        } else {
            o = New();
        }
    }

    // 将 GO 退回对象池
    public static void Push(ref GO o) {
#if UNITY_EDITOR
        Debug.Assert(o.g != null);
#endif
        o.g.SetActive(false);
        pool.Push(o);
        o.g = null;
        o.r = null;
        o.t = null;
    }

    // 新建 GO 并返回
    public static GO New() {
        var o = new GO();
        o.g = new GameObject();
        o.r = o.g.AddComponent<SpriteRenderer>();
        o.r.material = material;
        o.r.spriteSortPoint = SpriteSortPoint.Pivot;
        o.t = o.g.GetComponent<Transform>();
        return o;
    }

    // 初始化统一材质, 预填充
    public static void Init(Material material, int count) {
#if UNITY_EDITOR
        Debug.Assert(GO.material == null);
#endif
        GO.material = material;
        GO.pool = new Stack<GO>(count);
        for (int i = 0; i < count; i++) {
            var o = New();
            o.g.SetActive(false);
            pool.Push(o);
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
