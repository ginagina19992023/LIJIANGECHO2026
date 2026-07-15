using UnityEngine;

namespace LijiangEcho.Chapter3
{
    /// <summary>
    /// 生命/失败判定：连续 3 次 Miss 触发失败；累计 10 个 Miss 为超量（通关判定用）。
    /// 对应「3. 生命值&失败机制」与「四、通关判定 ②」。
    /// </summary>
    public class LifeSystem : MonoBehaviour
    {
        [SerializeField] private int consecutiveMissLimit = 3;   // 连续 Miss 失败线
        [SerializeField] private int totalMissLimit = 10;        // 累计 Miss 超量线

        public int ConsecutiveMiss { get; private set; }
        public int TotalMiss { get; private set; }

        public System.Action OnFail;

        public void ReportJudgment(Judgment j)
        {
            if (j == Judgment.Miss)
            {
                ConsecutiveMiss++;
                TotalMiss++;
                if (ConsecutiveMiss >= consecutiveMissLimit || TotalMiss >= totalMissLimit)
                    OnFail?.Invoke();
            }
            else
            {
                ConsecutiveMiss = 0;   // 命中重置连续 Miss
            }
        }

        public void ResetLife()
        {
            ConsecutiveMiss = 0;
            TotalMiss = 0;
        }
    }
}
