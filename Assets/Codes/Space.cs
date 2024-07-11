using System;
using System.Collections.Generic;
using UnityEngine;

public interface ISpaceItem {
    public SpaceContainer spaceContainer { get; set; }
    public ISpaceItem spacePrev { get; set; }
    public ISpaceItem spaceNext { get; set; }
    public int spaceIndex { get; set; } // == -1
    public float spaceX { get; set; }
    public float spaceY { get; set; }
}

public class SpaceContainer {
    public int numRows, numCols;                // size info
    public float cellSize, _1_cellSize;	        // = 1 / cellSize
    public float maxY, maxX;                    // edge position
    //public float maxY1, maxX1;                  // maxXY - float.Epsilon
    public int numItems;                        // for state
    public ISpaceItem[] cells;                  // grid container( numRows * numCols )


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
        //maxY1 = maxY - float.Epsilon;
        //maxX1 = maxX - float.Epsilon;
        if (cells == null) {
            cells = new ISpaceItem[numRows * numCols];
        } else {
            Array.Fill(cells, null);
            Array.Resize(ref cells, numRows * numCols);
        }
    }


    public void Add(ISpaceItem c) {
#if UNITY_EDITOR
        Debug.Assert(c != null);
        Debug.Assert(c.spaceContainer == this);
        Debug.Assert(c.spaceIndex == -1);
        Debug.Assert(c.spacePrev == null);
        Debug.Assert(c.spaceNext == null);
        Debug.Assert(c.spaceX >= 0 && c.spaceX < maxX);
        Debug.Assert(c.spaceY >= 0 && c.spaceY < maxY);
#endif

        // calc rIdx & cIdx
        var idx = PosToIndex(c.spaceX, c.spaceY);
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


    public void Remove(ISpaceItem c) {
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


    public void Update(ISpaceItem c) {
#if UNITY_EDITOR
        Debug.Assert(c != null);
        Debug.Assert(c.spaceContainer == this);
        Debug.Assert(c.spaceIndex > -1);
        Debug.Assert(c.spaceNext != c);
        Debug.Assert(c.spacePrev != c);
        //Debug.Assert(cells[c.spaceIndex] include c);
#endif

        var x = c.spaceX;
        var y = c.spaceY;
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




    public void Foreach(int idx, ref int limit, ISpaceItem except, Action<ISpaceItem> handler) {
        if (limit <= 0) return;
#if UNITY_EDITOR
        Debug.Assert(idx >= 0 && idx < cells.Length);
#endif
        var c = cells[idx];
        while (c != null) {
#if UNITY_EDITOR
            Debug.Assert(cells[c.spaceIndex].spacePrev == null);
            Debug.Assert(c.spaceNext != c);
            Debug.Assert(c.spacePrev != c);
#endif
            var next = c.spaceNext;
            if (c != except) {
                handler(c);
            }
            if (--limit <= 0) return;
            c = next;
        }
    }

    public void Foreach(int rIdx, int cIdx, ref int limit, ISpaceItem except, Action<ISpaceItem> handler) {
        if (rIdx < 0 || rIdx >= numRows) return;
        if (cIdx < 0 || cIdx >= numCols) return;
        Foreach(rIdx * numCols + cIdx, ref limit, except, handler);
    }

    public void Foreach8NeighborCells(int rIdx, int cIdx, ref int limit, ISpaceItem except, Action<ISpaceItem> handler) {
        Foreach(rIdx + 1, cIdx, ref limit, except, handler);
        if (limit <= 0) return;
        Foreach(rIdx - 1, cIdx, ref limit, except, handler);
        if (limit <= 0) return;
        Foreach(rIdx, cIdx + 1, ref limit, except, handler);
        if (limit <= 0) return;
        Foreach(rIdx, cIdx - 1, ref limit, except, handler);
        if (limit <= 0) return;
        Foreach(rIdx + 1, cIdx + 1, ref limit, except, handler);
        if (limit <= 0) return;
        Foreach(rIdx + 1, cIdx - 1, ref limit, except, handler);
        if (limit <= 0) return;
        Foreach(rIdx - 1, cIdx + 1, ref limit, except, handler);
        if (limit <= 0) return;
        Foreach(rIdx - 1, cIdx - 1, ref limit, except, handler);
    }

    public void Foreach9NeighborCells(ISpaceItem c, ref int limit, Action<ISpaceItem> handler) {
#if UNITY_EDITOR
        Debug.Assert(c != null);
#endif
        Foreach(c.spaceIndex, ref limit, c, handler);
        if (limit <= 0) return;
        var rIdx = c.spaceIndex / numCols;
        var cIdx = c.spaceIndex - numCols * rIdx;
        Foreach8NeighborCells(rIdx, cIdx, ref limit, null, handler);
    }

    public void Foreach9NeighborCells(SpaceXYi crIdx, ref int limit, Action<ISpaceItem> handler) {
        Foreach(crIdx.y, crIdx.x, ref limit, null, handler);
        if (limit <= 0) return;
        Foreach8NeighborCells(crIdx.y, crIdx.x, ref limit, null, handler);
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

    // return x: col index   y: row index
    public SpaceXYi PosToCrIdx(float x, float y) {
#if UNITY_EDITOR
        Debug.Assert(x >= 0 && x < maxX);
        Debug.Assert(y >= 0 && y < maxY);
#endif
        return new SpaceXYi { x = (int)(x * _1_cellSize), y = (int)(y * _1_cellSize) };
    }

    // ring diffuse search
    public void ForeachByRange(SpaceRingDiffuseData d, int x, int y, float maxDistance, Func<ISpaceItem, bool> handler) {
        var crIdxBase = PosToCrIdx(x, y);           // calc grid col row index
        float rr = maxDistance * maxDistance;
        var lens = d.lens;
        var idxs = d.idxs;
        for (int i = 1; i < lens.Count; i++) {
            var offsets = lens[i - 1].count;
            var size = lens[i].count - lens[i - 1].count;
            for (int j = 0; j < size; ++j) {
                var tmp = idxs[offsets + j];
                var cIdx = crIdxBase.x + tmp.x;
                if (cIdx < 0 || cIdx >= numCols) continue;
                var rIdx = crIdxBase.y + tmp.y;
                if (rIdx < 0 || rIdx >= numRows) continue;
                var cidx = rIdx * numCols + cIdx;
                var c = cells[cidx];
                while (c != null) {
#if UNITY_EDITOR
                    Debug.Assert(cells[c.spaceIndex].spacePrev == null);
                    Debug.Assert(c.spaceNext != c);
                    Debug.Assert(c.spacePrev != c);
#endif
                    var vx = c.spaceX - x;
                    var vy = c.spaceY - y;
                    if (vx * vx + vy * vy < rr) {
                        var next = c.spaceNext;
                        if (handler(c)) return;
                        c = next;
                    }
                }
            }
            if (lens[i].radius > maxDistance) break;            // limit search range
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
