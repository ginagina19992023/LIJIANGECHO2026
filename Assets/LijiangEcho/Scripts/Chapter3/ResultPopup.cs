using System;
using System.Collections;
using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter3
{
    /// <summary>
    /// 胜利/失败弹窗。胜利：金色八角壮锦底 +「胜利」→ 播放高潮音乐 → 进入火焰结局动画；
    /// 失败：深紫八角壮锦底 +「失败」，停留 2 秒自动重置回倒计时。
    /// 对应「四、2 胜利弹窗」「3. 失败机制」。
    /// </summary>
    public class ResultPopup : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject victoryRoot;
        [SerializeField] private GameObject defeatRoot;
        [SerializeField] private BossFireSequence bossFire;
        [SerializeField] private AudioDirector audioDirector;
        [SerializeField] private float defeatHold = 2f;
        [SerializeField] private float victoryHold = 2f;

        public void ShowVictory()
        {
            if (victoryRoot != null) victoryRoot.SetActive(true);
            audioDirector?.PlayVictory();
            StartCoroutine(VictoryRoutine());
        }

        private IEnumerator VictoryRoutine()
        {
            yield return new WaitForSeconds(victoryHold);
            if (victoryRoot != null) victoryRoot.SetActive(false);
            // 自动切入 4 秒火焰结局动画
            GameFlowManager.Instance.SetState(GameState.BossFire);
            if (bossFire != null) bossFire.Play();
        }

        public void ShowDefeat(Action onReset)
        {
            if (defeatRoot != null) defeatRoot.SetActive(true);
            audioDirector?.PlayDefeat();
            StartCoroutine(DefeatRoutine(onReset));
        }

        private IEnumerator DefeatRoutine(Action onReset)
        {
            yield return new WaitForSeconds(defeatHold);
            if (defeatRoot != null) defeatRoot.SetActive(false);
            onReset?.Invoke();   // 回到倒计时重挑战
        }
    }
}
