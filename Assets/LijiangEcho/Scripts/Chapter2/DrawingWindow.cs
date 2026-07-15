using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter2
{
    /// <summary>
    /// 纹样绘制弹窗：全屏半透明置顶，手柄扳机按住拖动射线在画布上绘制金色线条，
    /// 撤回/清空，重合度 ≥80% 自动完成并淡出关闭；侧键手动关闭（不计进度）。
    /// 第二章漫游绘制与第三章穿插绘制均复用本组件（对应「3. 纹样绘制窗口核心逻辑」/「三、穿插机制」）。
    /// </summary>
    public class DrawingWindow : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private CanvasGroup windowGroup;      // 弹窗淡入淡出
        [SerializeField] private LineRenderer strokePrefabHint; // 可选：笔迹渲染（也可用 UI 方案）
        [SerializeField] private PatternMatcher matcher;
        [SerializeField] private ControllerInput input;

        [Header("参数")]
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float sampleMinDistance = 4f;  // 采样最小间距

        // 当前笔迹采样点（画布局部坐标）
        private readonly List<Vector2> _points = new List<Vector2>();
        private readonly List<List<Vector2>> _strokes = new List<List<Vector2>>(); // 支持撤回（按笔）
        private bool _drawing;
        private bool _open;
        private int _patternIndex;

        // 完成/取消回调
        private System.Action<int> _onComplete;
        private System.Action<int> _onCancel;

        private void OnEnable()
        {
            if (input != null) input.OnSideButton += HandleSideButton;
        }

        private void OnDisable()
        {
            if (input != null) input.OnSideButton -= HandleSideButton;
        }

        /// <summary>
        /// 打开绘制弹窗。
        /// </summary>
        /// <param name="patternIndex">纹样索引</param>
        /// <param name="onComplete">达标完成回调</param>
        /// <param name="onCancel">手动关闭/未完成回调（穿插模式可视为失败）</param>
        public void Open(int patternIndex, System.Action<int> onComplete, System.Action<int> onCancel)
        {
            _patternIndex = patternIndex;
            _onComplete = onComplete;
            _onCancel = onCancel;
            _points.Clear();
            _strokes.Clear();
            _open = true;
            gameObject.SetActive(true);
            StartCoroutine(Fade(0f, 1f));
        }

        /// <summary>由画布上的射线 Hit 事件驱动：传入画布局部坐标与是否按下扳机。</summary>
        public void FeedPointer(Vector2 canvasPoint, bool triggerHeld)
        {
            if (!_open) return;

            if (triggerHeld)
            {
                if (!_drawing)
                {
                    _drawing = true;
                    _strokes.Add(new List<Vector2>());
                }
                var cur = _strokes[_strokes.Count - 1];
                if (cur.Count == 0 || (cur[cur.Count - 1] - canvasPoint).magnitude >= sampleMinDistance)
                {
                    cur.Add(canvasPoint);
                    _points.Add(canvasPoint);
                    TryEvaluate();
                }
            }
            else
            {
                _drawing = false;
            }
        }

        private void TryEvaluate()
        {
            if (matcher != null && matcher.IsPass(_points))
                Complete();
        }

        /// <summary>撤回上一笔。</summary>
        public void Undo()
        {
            if (_strokes.Count == 0) return;
            var last = _strokes[_strokes.Count - 1];
            foreach (var p in last) _points.Remove(p);
            _strokes.RemoveAt(_strokes.Count - 1);
        }

        /// <summary>清空画布。</summary>
        public void Clear()
        {
            _points.Clear();
            _strokes.Clear();
        }

        private void Complete()
        {
            if (!_open) return;
            _open = false;
            var cb = _onComplete;
            int idx = _patternIndex;
            StartCoroutine(CloseThen(() => cb?.Invoke(idx)));
        }

        private void HandleSideButton()
        {
            if (!_open) return;
            _open = false;
            var cb = _onCancel;
            int idx = _patternIndex;
            StartCoroutine(CloseThen(() => cb?.Invoke(idx)));
        }

        private IEnumerator CloseThen(System.Action after)
        {
            yield return Fade(1f, 0f);
            gameObject.SetActive(false);
            after?.Invoke();
        }

        private IEnumerator Fade(float from, float to)
        {
            if (windowGroup == null) yield break;
            float t = 0f;
            windowGroup.blocksRaycasts = true;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                windowGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            windowGroup.alpha = to;
        }
    }
}
