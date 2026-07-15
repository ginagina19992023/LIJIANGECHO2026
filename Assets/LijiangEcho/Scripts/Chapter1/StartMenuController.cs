using System.Collections;
using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter1
{
    /// <summary>
    /// P1 开始游戏界面：中心壮族绣球 +「进入游戏」文字。
    /// 射线命中绣球 → 放大高亮；按确认键 → 绣球缩放淡出、装饰消散 → 转场进入选关。
    /// 对应需求「第一章 · P1」。
    /// </summary>
    public class StartMenuController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject root;              // 整个开始界面根节点
        [SerializeField] private Transform embroideryBall;     // 绣球
        [SerializeField] private CanvasGroup decorations;      // 国风装饰图层（用于逐层消散）

        [Header("高亮反馈")]
        [SerializeField] private float hoverScale = 1.1f;      // 命中放大 10%
        [SerializeField] private float scaleLerp = 8f;

        private Vector3 _baseScale;
        private bool _hovering;
        private bool _entering;

        private void Awake()
        {
            if (embroideryBall != null) _baseScale = embroideryBall.localScale;
        }

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
            if (root != null) root.SetActive(next == GameState.StartMenu);
        }

        /// <summary>由 XR 射线的 Hover Enter/Exit 事件调用。</summary>
        public void SetHover(bool hovering) => _hovering = hovering;

        /// <summary>由 XR 射线的 Select（按下确认键）事件调用。</summary>
        public void OnConfirm()
        {
            if (_entering || !_hovering) return;   // 未命中不响应
            _entering = true;
            StartCoroutine(EnterRoutine());
        }

        private void Update()
        {
            if (embroideryBall == null) return;
            Vector3 target = _hovering ? _baseScale * hoverScale : _baseScale;
            embroideryBall.localScale = Vector3.Lerp(embroideryBall.localScale, target, Time.deltaTime * scaleLerp);
        }

        private IEnumerator EnterRoutine()
        {
            // 绣球缓慢缩放淡出 + 装饰逐层消散
            float t = 0f, dur = 1f;
            Vector3 start = embroideryBall.localScale;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                if (embroideryBall != null)
                    embroideryBall.localScale = Vector3.Lerp(start, Vector3.zero, k);
                if (decorations != null)
                    decorations.alpha = 1f - k;
                yield return null;
            }
            // 转场跳转到关卡选择
            GameFlowManager.Instance.TransitionTo(GameState.LevelSelect);
        }
    }
}
