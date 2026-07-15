using System.Collections.Generic;
using UnityEngine;

namespace LijiangEcho.Chapter2
{
    /// <summary>
    /// 纹样轮廓匹配：把玩家手绘采样点与标准纹样轮廓点做「覆盖率」比对，
    /// 覆盖率 ≥ 阈值(默认 0.8 = 80%) 判定完成。对应「3.2 绘制完成判定」。
    /// 这是一个轻量近似实现（点到轮廓最近距离），可替换为更强的 AI 匹配。
    /// </summary>
    public class PatternMatcher : MonoBehaviour
    {
        [Tooltip("标准纹样轮廓采样点（画布局部坐标）")]
        public List<Vector2> targetOutline = new List<Vector2>();

        [Tooltip("判定为“被覆盖”的距离阈值（画布单位）")]
        public float coverRadius = 12f;

        [Range(0f, 1f)]
        [Tooltip("完成阈值，重合度 ≥ 该值判定完成")]
        public float passThreshold = 0.8f;

        /// <summary>
        /// 计算重合度：标准轮廓上有多少比例的点，被玩家笔迹覆盖到。
        /// </summary>
        public float Evaluate(IReadOnlyList<Vector2> strokePoints)
        {
            if (targetOutline == null || targetOutline.Count == 0) return 0f;
            if (strokePoints == null || strokePoints.Count == 0) return 0f;

            int covered = 0;
            float r2 = coverRadius * coverRadius;
            foreach (var tp in targetOutline)
            {
                for (int i = 0; i < strokePoints.Count; i++)
                {
                    if ((strokePoints[i] - tp).sqrMagnitude <= r2)
                    {
                        covered++;
                        break;
                    }
                }
            }
            return (float)covered / targetOutline.Count;
        }

        public bool IsPass(IReadOnlyList<Vector2> strokePoints) => Evaluate(strokePoints) >= passThreshold;
    }
}
