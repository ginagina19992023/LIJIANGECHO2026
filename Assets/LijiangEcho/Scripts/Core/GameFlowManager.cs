using System;
using UnityEngine;

namespace LijiangEcho.Core
{
    /// <summary>
    /// 全局流程管理器（单例）。集中管理 GameState 切换，负责场景/界面淡入淡出转场，
    /// 各章节脚本订阅 OnStateChanged 来激活/隐藏自身。
    /// 对应需求「八、全关卡完整流程时序」。
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        public static GameFlowManager Instance { get; private set; }

        [Header("转场")]
        [Tooltip("全屏淡入淡出遮罩（CanvasGroup），用于界面溶解过渡")]
        [SerializeField] private CanvasGroup transitionFade;
        [SerializeField] private float fadeDuration = 0.5f;

        public GameState Current { get; private set; } = GameState.Boot;

        /// <summary>状态变化事件：参数为(上一个状态, 新状态)。</summary>
        public event Action<GameState, GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // 启动后进入开始界面
            SetState(GameState.StartMenu);
        }

        /// <summary>直接切换状态（无转场）。</summary>
        public void SetState(GameState next)
        {
            if (next == Current) return;
            GameState prev = Current;
            Current = next;
            OnStateChanged?.Invoke(prev, next);
        }

        /// <summary>带淡入淡出转场的状态切换。对应「转场：淡入淡出溶解过渡」。</summary>
        public void TransitionTo(GameState next)
        {
            StopAllCoroutines();
            StartCoroutine(TransitionRoutine(next));
        }

        private System.Collections.IEnumerator TransitionRoutine(GameState next)
        {
            yield return Fade(0f, 1f);   // 淡出到黑/白
            SetState(next);
            yield return Fade(1f, 0f);   // 淡入新界面
        }

        private System.Collections.IEnumerator Fade(float from, float to)
        {
            if (transitionFade == null)
                yield break;

            float t = 0f;
            transitionFade.blocksRaycasts = true;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                transitionFade.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            transitionFade.alpha = to;
            transitionFade.blocksRaycasts = to > 0.01f;
        }
    }
}
