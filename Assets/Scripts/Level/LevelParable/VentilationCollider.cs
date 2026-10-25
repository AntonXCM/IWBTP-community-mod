using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Строит контуры (цепочки точек) для EdgeCollider2D по набору клеток.
///
/// Координаты клеток здесь «Y вверх»: клетка (x, y) занимает квадрат [x, x+1] × [y, y+1]
/// в долях размера тайла.
///
/// Граница — это сторона тайла, за которой нет ни тайла, ни выхода.
/// Каждая такая сторона сдвигается внутрь тайла на свой отступ (верх/низ/лево/право),
/// в углах соседние стороны сходятся в точке пересечения сдвинутых линий.
/// Рядом с выходом контур не замыкается: цепочка обрывается ровно на границе клетки-выхода.
/// </summary>
public static class VentilationOutline
{
    /// <summary>Сдвиг граней внутрь тайла, в единицах мира.</summary>
    public struct Insets
    {
        public float top, bottom, left, right;
    }

    private enum Side { Bottom, Right, Top, Left }

    // Ребро направлено так, что внутренность тайла остаётся слева от направления обхода.
    private struct Edge
    {
        public Vector2Int a, b;
        public Side side;

        public Edge(Vector2Int a, Vector2Int b, Side side)
        {
            this.a = a;
            this.b = b;
            this.side = side;
        }
    }

    /// <param name="solid">Клетки с мешем и коллайдером.</param>
    /// <param name="open">Все проходимые клетки: solid + выходы.</param>
    public static List<Vector2[]> Build(ICollection<Vector2Int> solid, ICollection<Vector2Int> open,
                                        float cellSize, Insets insets)
    {
        var edges = new List<Edge>();
        foreach (Vector2Int c in solid)
        {
            if (!open.Contains(c + Vector2Int.down))
                edges.Add(new Edge(c, c + Vector2Int.right, Side.Bottom));
            if (!open.Contains(c + Vector2Int.right))
                edges.Add(new Edge(c + Vector2Int.right, c + Vector2Int.one, Side.Right));
            if (!open.Contains(c + Vector2Int.up))
                edges.Add(new Edge(c + Vector2Int.one, c + Vector2Int.up, Side.Top));
            if (!open.Contains(c + Vector2Int.left))
                edges.Add(new Edge(c + Vector2Int.up, c, Side.Left));
        }

        var outgoing = new Dictionary<Vector2Int, List<int>>();
        var hasIncoming = new HashSet<Vector2Int>();
        for (int i = 0; i < edges.Count; i++)
        {
            if (!outgoing.TryGetValue(edges[i].a, out List<int> list))
            {
                list = new List<int>(2);
                outgoing[edges[i].a] = list;
            }
            list.Add(i);
            hasIncoming.Add(edges[i].b);
        }

        var used = new bool[edges.Count];
        var chain = new List<int>();
        var result = new List<Vector2[]>();

        // Проход 0: открытые цепочки — начинаются там, где в вершину ничего не входит (у выходов).
        // Проход 1: всё, что осталось, — замкнутые контуры.
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                if (used[i]) continue;
                if (pass == 0 && hasIncoming.Contains(edges[i].a)) continue;

                bool closed = Trace(i, edges, outgoing, used, chain);
                result.Add(ToPoints(edges, chain, closed, cellSize, insets));
            }
        }

        return result;
    }

    // Идёт по рёбрам от start. Возвращает true, если контур замкнулся.
    private static bool Trace(int start, List<Edge> edges, Dictionary<Vector2Int, List<int>> outgoing,
                              bool[] used, List<int> chain)
    {
        chain.Clear();
        int cur = start;
        while (true)
        {
            used[cur] = true;
            chain.Add(cur);

            int next = PickNext(edges, outgoing, cur);
            if (next < 0) return false;            // открытый конец (выход)
            if (used[next]) return next == start;  // вернулись в начало
            cur = next;
        }
    }

    // В вершине может быть два исходящих ребра (тайлы касаются только углами).
    // Всегда выбираем левый поворот: так каждый тайл обходится как отдельный контур.
    private static int PickNext(List<Edge> edges, Dictionary<Vector2Int, List<int>> outgoing, int cur)
    {
        if (!outgoing.TryGetValue(edges[cur].b, out List<int> candidates)) return -1;

        Vector2Int d = edges[cur].b - edges[cur].a;
        int best = -1;
        int bestScore = int.MinValue;

        foreach (int i in candidates)
        {
            Vector2Int n = edges[i].b - edges[i].a;
            int cross = d.x * n.y - d.y * n.x; // > 0 — поворот налево
            int dot = d.x * n.x + d.y * n.y;
            int score = cross > 0 ? 2 : dot > 0 ? 1 : cross < 0 ? 0 : -1;
            if (score > bestScore)
            {
                bestScore = score;
                best = i;
            }
        }

        return best;
    }

    private static Vector2[] ToPoints(List<Edge> edges, List<int> chain, bool closed,
                                      float cellSize, Insets insets)
    {
        int m = chain.Count;
        int vertexCount = closed ? m : m + 1;
        var points = new List<Vector2>(vertexCount + 1);

        for (int i = 0; i < vertexCount; i++)
        {
            // Ребро, входящее в вершину i, и ребро, выходящее из неё.
            int prev = i > 0 ? chain[i - 1] : (closed ? chain[m - 1] : -1);
            int next = i < m ? chain[i] : -1;
            Vector2Int v = i < m ? edges[chain[i]].a : edges[chain[m - 1]].b;

            // Вершина посреди прямой: обе стороны одного типа, точка не нужна.
            if (prev >= 0 && next >= 0 && edges[prev].side == edges[next].side) continue;

            // Горизонтальное ребро сдвигает вершину по Y, вертикальное — по X,
            // поэтому в углу сдвиги просто складываются.
            Vector2 p = new Vector2(v.x, v.y) * cellSize;
            if (prev >= 0) p += InsetOf(edges[prev].side, insets);
            if (next >= 0) p += InsetOf(edges[next].side, insets);
            points.Add(p);
        }

        if (closed) points.Add(points[0]);
        return points.ToArray();
    }

    private static Vector2 InsetOf(Side side, Insets ins)
    {
        switch (side)
        {
            case Side.Bottom: return new Vector2(0f, ins.bottom);
            case Side.Top:    return new Vector2(0f, -ins.top);
            case Side.Left:   return new Vector2(ins.left, 0f);
            default:          return new Vector2(-ins.right, 0f);
        }
    }
}