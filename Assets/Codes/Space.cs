using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpaceItem {
    public SpaceContainer spaceContainer;
    public SpaceItem spacePrev, spaceNext;
    public int spaceIndex = -1;
    public float x, y;
    public float radius;
}

public class SpaceContainer {
    public int numRows, numCols;                // size info
    public float cellSize, _1_cellSize;	        // = 1 / cellSize
    public float maxY, maxX;                    // edge position
    public int numItems;                        // for state
    public SpaceItem[] cells;                  // grid container( numRows * numCols )


    public SpaceContainer(int numRows_, int numCols_, float cellSize_) {
#if UNITY_EDITOR
        Debug.Assert(numRows_ > 0);
        Debug.Assert(numCols_ > 0);
        Debug.Assert(cellSize_ > 0);
#endif
        numRows = numRows_;
        numCols = numCols_;
        cellSize = cellSize_;
        _1_cellSize = 1f / cellSize_;
        maxY = cellSize * numRows;
        maxX = cellSize * numCols;
        if (cells == null) {
            cells = new SpaceItem[numRows * numCols];
        } else {
            Array.Fill(cells, null);
            Array.Resize(ref cells, numRows * numCols);
        }
    }


    public void Add(SpaceItem c) {
#if UNITY_EDITOR
        Debug.Assert(c != null);
        Debug.Assert(c.spaceContainer == this);
        Debug.Assert(c.spaceIndex == -1);
        Debug.Assert(c.spacePrev == null);
        Debug.Assert(c.spaceNext == null);
        Debug.Assert(c.x >= 0 && c.x < maxX);
        Debug.Assert(c.y >= 0 && c.y < maxY);
#endif

        // calc rIdx & cIdx
        var idx = PosToIndex(c.x, c.y);
#if UNITY_EDITOR
        Debug.Assert(cells[idx] == null || cells[idx].spacePrev == null);
#endif

        // link
        if (cells[idx] != null) {
            cells[idx].spacePrev = c;
        }
        c.spaceNext = cells[idx];
        c.spaceIndex = idx;
        cells[idx] = c;
#if UNITY_EDITOR
        Debug.Assert(cells[idx].spacePrev == null);
        Debug.Assert(c.spaceNext != c);
        Debug.Assert(c.spacePrev != c);
#endif

        // stat
        ++numItems;
    }


    public void Remove(SpaceItem c) {
#if UNITY_EDITOR
        Debug.Assert(c != null);
        Debug.Assert(c.spaceContainer == this);
        Debug.Assert(c.spacePrev == null && cells[c.spaceIndex] == c || c.spacePrev.spaceNext == c && cells[c.spaceIndex] != c);
        Debug.Assert(c.spaceNext == null || c.spaceNext.spacePrev == c);
        //Debug.Assert(cells[c.spaceIndex] include c);
#endif

        // unlink
        if (c.spacePrev != null) {  // isn't header
#if UNITY_EDITOR
            Debug.Assert(cells[c.spaceIndex] != c);
#endif
            c.spacePrev.spaceNext = c.spaceNext;
            if (c.spaceNext != null) {
                c.spaceNext.spacePrev = c.spacePrev;
                c.spaceNext = null;
            }
            c.spacePrev = null;
        } else {
#if UNITY_EDITOR
            Debug.Assert(cells[c.spaceIndex] == c);
#endif
            cells[c.spaceIndex] = c.spaceNext;
            if (c.spaceNext != null) {
                c.spaceNext.spacePrev = null;
                c.spaceNext = null;
            }
        }
#if UNITY_EDITOR
        Debug.Assert(cells[c.spaceIndex] != c);
#endif
        c.spaceIndex = -1;
        c.spaceContainer = null;

        // stat
        --numItems;
    }


    public void Update(SpaceItem c) {
#if UNITY_EDITOR
        Debug.Assert(c != null);
        Debug.Assert(c.spaceContainer == this);
        Debug.Assert(c.spaceIndex > -1);
        Debug.Assert(c.spaceNext != c);
        Debug.Assert(c.spacePrev != c);
        //Debug.Assert(cells[c.spaceIndex] include c);
#endif

        var x = c.x;
        var y = c.y;
#if UNITY_EDITOR
        Debug.Assert(x >= 0 && x < maxX);
        Debug.Assert(y >= 0 && y < maxY);
#endif
        int cIdx = (int)(x * _1_cellSize);
        int rIdx = (int)(y * _1_cellSize);
        int idx = rIdx * numCols + cIdx;
#if UNITY_EDITOR
        Debug.Assert(idx <= cells.Length);
#endif

        if (idx == c.spaceIndex) return;  // no change

        // unlink
        if (c.spacePrev != null) {  // isn't header
#if UNITY_EDITOR
            Debug.Assert(cells[c.spaceIndex] != c);
#endif
            c.spacePrev.spaceNext = c.spaceNext;
            if (c.spaceNext != null) {
                c.spaceNext.spacePrev = c.spacePrev;
                //c.spaceNext = {};
            }
            //c.spacePrev = {};
        } else {
#if UNITY_EDITOR
            Debug.Assert(cells[c.spaceIndex] == c);
#endif
            cells[c.spaceIndex] = c.spaceNext;
            if (c.spaceNext != null) {
                c.spaceNext.spacePrev = null;
                //c.spaceNext = {};
            }
        }
        //c.spaceIndex = -1;
#if UNITY_EDITOR
        Debug.Assert(cells[c.spaceIndex] != c);
        Debug.Assert(idx != c.spaceIndex);
#endif

        // link
        if (cells[idx] != null) {
            cells[idx].spacePrev = c;
        }
        c.spacePrev = null;
        c.spaceNext = cells[idx];
        cells[idx] = c;
        c.spaceIndex = idx;
#if UNITY_EDITOR
        Debug.Assert(cells[idx].spacePrev == null);
        Debug.Assert(c.spaceNext != c);
        Debug.Assert(c.spacePrev != c);
#endif
    }

    // return cells index
    public int PosToIndex(float x, float y) {
#if UNITY_EDITOR
        Debug.Assert(x >= 0 && x < maxX);
        Debug.Assert(y >= 0 && y < maxY);
#endif
        int cIdx = (int)(x * _1_cellSize);
        int rIdx = (int)(y * _1_cellSize);
        int idx = rIdx * numCols + cIdx;
#if UNITY_EDITOR
        Debug.Assert(idx <= cells.Length);
#endif
        return idx;
    }



    // 在 9 宫内找出 第1个 相交物 并返回
    public SpaceItem Foreach9FirstHitCheck(float x, float y, float radius) {
        // 5
        int cIdx = (int)(x * _1_cellSize);
        if (cIdx < 0 || cIdx >= numCols) return null;
        int rIdx = (int)(y * _1_cellSize);
        if (rIdx < 0 || rIdx >= numRows) return null;
        int idx = rIdx * numCols + cIdx;
        var c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 6
        ++cIdx;
        if (cIdx >= numCols) return null;
        ++idx;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 3
        ++rIdx;
        if (rIdx >= numRows) return null;
        idx += numCols;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 2
        --idx;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 1
        cIdx -= 2;
        if (cIdx < 0) return null;
        --idx;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 4
        idx -= numCols;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 7
        rIdx -= 2;
        if (rIdx < 0) return null;
        idx -= numCols;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 8
        ++idx;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        // 9
        ++idx;
        c = cells[idx];
        while (c != null) {
            var vx = c.x - x;
            var vy = c.y - y;
            var r = c.radius + radius;
            if (vx * vx + vy * vy < r * r) {
                return c;
            }
            c = c.spaceNext;
        }
        return null;
    }



    // 圆形扩散遍历找出 边距最近的 1 个并返回
    public SpaceItem FindNearestByRange(SpaceRingDiffuseData d, float x, float y, float maxDistance) {
        int cIdxBase = (int)(x * _1_cellSize);
        if (cIdxBase < 0 || cIdxBase >= numCols) return null;
        int rIdxBase = (int)(y * _1_cellSize);
        if (rIdxBase < 0 || rIdxBase >= numRows) return null;
        var searchRange = maxDistance + cellSize;

        SpaceItem rtv = null;
        float minDistance = maxDistance * maxDistance + float.Epsilon;

        var lens = d.lens;
        var idxs = d.idxs;
        for (int i = 1; i < lens.Count; i++) {
            var offsets = lens[i - 1].count;
            var size = lens[i].count - lens[i - 1].count;
            for (int j = 0; j < size; ++j) {
                var tmp = idxs[offsets + j];
                var cIdx = cIdxBase + tmp.x;
                if (cIdx < 0 || cIdx >= numCols) continue;
                var rIdx = rIdxBase + tmp.y;
                if (rIdx < 0 || rIdx >= numRows) continue;
                var cidx = rIdx * numCols + cIdx;

                var c = cells[cidx];
                while (c != null) {
                    var vx = c.x - x;
                    var vy = c.y - y;
                    var dd = vx * vx + vy * vy;
                    var r = maxDistance + c.radius;
                    var v = r * r - dd;

                    if (v > 0) {
                        rtv = c;
                        minDistance = v;
                    }
                    c = c.spaceNext;
                }
            }
            if (lens[i].radius > searchRange) break;
        }
        return rtv;
    }


    // 圆形扩散遍历 找出范围内 ??? 最多 n 个 的结果容器
    public List<DistanceSpaceItem> result_FindNearestN = new();

    // 圆形扩散遍历 找出范围内 边缘最近的 最多 n 个, 返回实际个数。searchRange 决定了要扫多远的格子. maxDistance 限制了结果集最大边距
    public int FindNearestNByRange(SpaceRingDiffuseData d, float x, float y, float maxDistance, int n) {
        int cIdxBase = (int)(x * _1_cellSize);
        if (cIdxBase < 0 || cIdxBase >= numCols) return 0;
        int rIdxBase = (int)(y * _1_cellSize);
        if (rIdxBase < 0 || rIdxBase >= numRows) return 0;
        var searchRange = maxDistance + cellSize;

        var os = result_FindNearestN;
        os.Clear();

        var lens = d.lens;
        var idxs = d.idxs;
        for (int i = 1; i < lens.Count; i++) {
            var offsets = lens[i - 1].count;
            var size = lens[i].count - lens[i - 1].count;
            for (int j = 0; j < size; ++j) {
                var tmp = idxs[offsets + j];
                var cIdx = cIdxBase + tmp.x;
                if (cIdx < 0 || cIdx >= numCols) continue;
                var rIdx = rIdxBase + tmp.y;
                if (rIdx < 0 || rIdx >= numRows) continue;
                var cidx = rIdx * numCols + cIdx;

                var c = cells[cidx];
                while (c != null) {
                    var vx = c.x - x;
                    var vy = c.y - y;
                    var dd = vx * vx + vy * vy;
                    var r = maxDistance + c.radius;
                    var v = r * r - dd;

                    if (v > 0) {
                        if (os.Count < n) {
                            os.Add(new DistanceSpaceItem { distance = v, item = c });
                            if (os.Count == n) {
                                Quick_Sort(0, os.Count - 1);
                            }
                        } else {
                            if (os[0].distance > v) {
                                os[0] = new DistanceSpaceItem { distance = v, item = c };
                                Quick_Sort(0, os.Count - 1);
                            }
                        }
                    }

                    c = c.spaceNext;
                }
            }
            if (lens[i].radius > searchRange) break;
        }
        return os.Count;
    }

    // 排序 result_FindNearestN               .Sort(); 函数会造成 128 byte gc
    private void Quick_Sort(int left, int right) {
        if (left < right) {
            int pivot = Partition(left, right);
            if (pivot > 1) {
                Quick_Sort(left, pivot - 1);
            }
            if (pivot + 1 < right) {
                Quick_Sort(pivot + 1, right);
            }
        }
    }
    private int Partition(int left, int right) {
        var arr = result_FindNearestN;
        var pivot = arr[left];
        while (true) {
            while (arr[left].distance < pivot.distance) {
                left++;
            }
            while (arr[right].distance > pivot.distance) {
                right--;
            }
            if (left < right) {
                if (arr[left].distance == arr[right].distance) return right;
                var temp = arr[left];
                arr[left] = arr[right];
                arr[right] = temp;
            } else return right;
        }
    }

}


public struct SpaceCountRadius {
    public int count;
    public float radius;
};

public struct SpaceXYi {
    public int x, y;
}

public struct DistanceSpaceItem : IComparable<DistanceSpaceItem> {
    public float distance;
    public SpaceItem item;

    public int CompareTo(DistanceSpaceItem o) {
        return this.distance.CompareTo(o.distance);
    }
}

// 填充 圆形扩散的 格子偏移量 数组. 主用于 更高效的范围内找最近
public class SpaceRingDiffuseData {
    public List<SpaceCountRadius> lens = new();
    public List<SpaceXYi> idxs = new();

    public SpaceRingDiffuseData(int crCount, float cellSize) {
        var _1_cellSize = 1f / cellSize;
        var step = cellSize * 0.5f;
        lens.Add(new SpaceCountRadius { count = 0, radius = 0f });
        var lastIdx = new SpaceXYi();
        idxs.Add(lastIdx);
        var n = crCount * 2;
        var idxflags = new int[n * n];
        var center = new SpaceXYi { x = n / 2, y = n / 2 };
        for (float r = step; r < cellSize * crCount; r += step) {
            var c = 2 * Math.PI * r;
            if (c < step) continue;
            var lenBak = idxs.Count;
            var astep = Math.PI * 2 * (step / c) / 10;
            var rd = r * _1_cellSize;
            for (var a = astep; a < Math.PI * 2; a += astep) {
                var idx = new SpaceXYi { x = (int)(rd * Math.Cos(a)), y = (int)(rd * Math.Sin(a)) };
                if (lastIdx.x != idx.x && lastIdx.y != idx.y) {
                    var i = (center.y + idx.y) * crCount + (center.x + idx.x);
                    if (idxflags[i] == 0) {
                        idxflags[i] = 1;
                        idxs.Add(idx);
                        lastIdx = idx;
                    }
                }
            }
            if (idxs.Count > lenBak) {
                lens.Add(new SpaceCountRadius { count = idxs.Count, radius = r });
            }
        }
    }
}


//    // return x: col index   y: row index
//    public SpaceXYi PosToCrIdx(float x, float y) {
//#if UNITY_EDITOR
//        Debug.Assert(x >= 0 && x < maxX);
//        Debug.Assert(y >= 0 && y < maxY);
//#endif
//        return new SpaceXYi { x = (int)(x * _1_cellSize), y = (int)(y * _1_cellSize) };
//    }

//    // 圆形扩散遍历 ( 供参考，用来复制小改比较好 )
//    public void ForeachByRange(SpaceRingDiffuseData d, int x, int y, float maxDistance, Func<SpaceItem, bool> handler) {
//        var crIdxBase = PosToCrIdx(x, y);           // calc grid col row index
//        float rr = maxDistance * maxDistance;
//        var lens = d.lens;
//        var idxs = d.idxs;
//        for (int i = 1; i < lens.Count; i++) {
//            var offsets = lens[i - 1].count;
//            var size = lens[i].count - lens[i - 1].count;
//            for (int j = 0; j < size; ++j) {
//                var tmp = idxs[offsets + j];
//                var cIdx = crIdxBase.x + tmp.x;
//                if (cIdx < 0 || cIdx >= numCols) continue;
//                var rIdx = crIdxBase.y + tmp.y;
//                if (rIdx < 0 || rIdx >= numRows) continue;
//                var cidx = rIdx * numCols + cIdx;
//                var c = cells[cidx];
//                while (c != null) {
//#if UNITY_EDITOR
//                    Debug.Assert(cells[c.spaceIndex].spacePrev == null);
//                    Debug.Assert(c.spaceNext != c);
//                    Debug.Assert(c.spacePrev != c);
//#endif
//                    var vx = c.x - x;
//                    var vy = c.y - y;
//                    if (vx * vx + vy * vy < rr) {
//                        var next = c.spaceNext;
//                        if (handler(c)) return;
//                        c = next;
//                    }
//                }
//            }
//            if (lens[i].radius > maxDistance) break;            // limit search range
//        }
//    }


//    public void Foreach(int idx, ref int limit, SpaceItem except, Action<SpaceItem> handler) {
//        if (limit <= 0) return;
//#if UNITY_EDITOR
//        Debug.Assert(idx >= 0 && idx < cells.Length);
//#endif
//        var c = cells[idx];
//        while (c != null) {
//#if UNITY_EDITOR
//            Debug.Assert(cells[c.spaceIndex].spacePrev == null);
//            Debug.Assert(c.spaceNext != c);
//            Debug.Assert(c.spacePrev != c);
//#endif
//            var next = c.spaceNext;
//            if (c != except) {
//                handler(c);
//            }
//            if (--limit <= 0) return;
//            c = next;
//        }
//    }

//    public void Foreach(int rIdx, int cIdx, ref int limit, SpaceItem except, Action<SpaceItem> handler) {
//        if (rIdx < 0 || rIdx >= numRows) return;
//        if (cIdx < 0 || cIdx >= numCols) return;
//        Foreach(rIdx * numCols + cIdx, ref limit, except, handler);
//    }

//    public void Foreach8NeighborCells(int rIdx, int cIdx, ref int limit, SpaceItem except, Action<SpaceItem> handler) {
//        Foreach(rIdx + 1, cIdx, ref limit, except, handler);
//        if (limit <= 0) return;
//        Foreach(rIdx - 1, cIdx, ref limit, except, handler);
//        if (limit <= 0) return;
//        Foreach(rIdx, cIdx + 1, ref limit, except, handler);
//        if (limit <= 0) return;
//        Foreach(rIdx, cIdx - 1, ref limit, except, handler);
//        if (limit <= 0) return;
//        Foreach(rIdx + 1, cIdx + 1, ref limit, except, handler);
//        if (limit <= 0) return;
//        Foreach(rIdx + 1, cIdx - 1, ref limit, except, handler);
//        if (limit <= 0) return;
//        Foreach(rIdx - 1, cIdx + 1, ref limit, except, handler);
//        if (limit <= 0) return;
//        Foreach(rIdx - 1, cIdx - 1, ref limit, except, handler);
//    }

//    public void Foreach9NeighborCells(SpaceItem c, ref int limit, Action<SpaceItem> handler) {
//#if UNITY_EDITOR
//        Debug.Assert(c != null);
//#endif
//        Foreach(c.spaceIndex, ref limit, c, handler);
//        if (limit <= 0) return;
//        var rIdx = c.spaceIndex / numCols;
//        var cIdx = c.spaceIndex - numCols * rIdx;
//        Foreach8NeighborCells(rIdx, cIdx, ref limit, null, handler);
//    }

//    public void Foreach9NeighborCells(SpaceXYi crIdx, ref int limit, Action<SpaceItem> handler) {
//        Foreach(crIdx.y, crIdx.x, ref limit, null, handler);
//        if (limit <= 0) return;
//        Foreach8NeighborCells(crIdx.y, crIdx.x, ref limit, null, handler);
//    }
