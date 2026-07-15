using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter3
{
    /// <summary>
    /// 关卡前置 3-2-1 倒计时：大号数字 + 双层金色铜鼓圆环 + 地主剪影底层。
    /// 每秒切换并播鼓点音效，结束无缝进入打击主界面。倒计时期间锁定漫游，仅设置按钮可交互。
    /// 对应「第三章 · 一、关卡前置倒计时」。
    /// </summary>
    public class CountdownController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMPro.TextMeshProUGUI numberText;  // 需要 TextMeshPro
        [SerializeField] private AudioSource tickSfx;               // 每秒单声轻铜鼓
        [SerializeField] private float perSecond = 1f;
        [SerializeField] private UnityEvent onFinished;             // 结束回调（启动打击）

        private void OnEnable()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.OnStateChanged += HandleState;
        }

        private void OnDisable()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.OnStateChanged -= HandleState;
        }

        private void HandleState(GameState prev, GameState next)
        {
            bool active = next == GameState.Chapter3Countdown;
            if (root != null) root.SetActive(active);
            if (active) StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            for (int i = 3; i >= 1; i--)
            {
                if (numberText != null) numberText.text = i.ToString();
                if (tickSfx != null) tickSfx.Play();
                yield return new WaitForSeconds(perSecond);
            }
            if (root != null) root.SetActive(false);
            onFinished?.Invoke();
            // 无缝切入打击主界面
            GameFlowManager.Instance.SetState(GameState.Chapter3Rhythm);
        }
    }
}
