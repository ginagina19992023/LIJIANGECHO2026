using UnityEngine;

namespace LijiangEcho.Chapter3
{
    /// <summary>打击判定等级。</summary>
    public enum Judgment { Perfect, Great, Good, Miss }

    /// <summary>
    /// 同心双层金色打击圆环的分层判定（从外至内）：
    ///   环外 → Miss；外环线 → 良好(Great)；环内(两环之间) → 完美(Perfect)；
    ///   内环线 → Good；内环以内(中心) → Miss。
    /// 传入命中点到圆心的归一化半径 r(0=圆心,1=外环外沿)，返回判定等级。
    /// 对应「二、1. 判定圆环分层规则」。
    /// </summary>
    public class HitRingJudge : MonoBehaviour
    {
        [Header("归一化半径分界 (0=圆心, 1=最外)")]
        [Range(0f, 1f)] public float innerRadius = 0.35f;   // 内环半径
        [Range(0f, 1f)] public float outerRadius = 0.75f;   // 外环半径
        [Tooltip("环线的容差带宽度")]
        [Range(0f, 0.3f)] public float lineTolerance = 0.08f;

        /// <summary>根据命中半径判定等级。</summary>
        public Judgment JudgeByRadius(float r)
        {
            if (r > outerRadius + lineTolerance) return Judgment.Miss;                 // 环外
            if (Mathf.Abs(r - outerRadius) <= lineTolerance) return Judgment.Great;    // 外环线 → 良好
            if (Mathf.Abs(r - innerRadius) <= lineTolerance) return Judgment.Good;     // 内环线 → good
            if (r > innerRadius && r < outerRadius) return Judgment.Perfect;           // 环内 → 完美
            return Judgment.Miss;                                                      // 中心以内 → miss
        }

        /// <summary>
        /// 结合节拍时间容差判定：|命中时刻 - 音符时刻| ≤ 0.3s 视为有效窗口，
        /// 再按半径分层；超窗口直接 Miss。对应「判定容错 ±0.3s」。
        /// </summary>
        public Judgment Judge(float hitTimeError, float radius, float timingWindow = 0.3f)
        {
            if (Mathf.Abs(hitTimeError) > timingWindow) return Judgment.Miss;
            return JudgeByRadius(radius);
        }
    }
}
