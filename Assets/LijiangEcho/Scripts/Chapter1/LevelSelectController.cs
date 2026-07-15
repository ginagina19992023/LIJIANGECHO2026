using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter1
{
    /// <summary>
    /// P2 关卡选择：左右悬浮壮锦凤凰关卡图标。
    /// 摇杆向左滑 → 图标整体左移（可循环、带缓动拖尾）；向右滑 → 复位。
    /// 射线选中图标 + 确认 → 图标原地旋转 360° → 转场加载第二章。
    /// 对应需求「第一章 · P2」。
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject root;
        [SerializeField] private ControllerInput input;
        [SerializeField] private RectTransform iconContainer;   // 所有关卡图标的父节点
        [SerializeField] private List<LevelIcon> icons = new List<LevelIcon>();

        [Header("摇杆滑动")]
        [SerializeField] private float slideSpeed = 400f;       // 每秒像素/单位位移
        [SerializeField] private float slideSmooth = 6f;        // 缓动系数
        [SerializeField] private float deadZone = 0.2f;

        [Header("进入动画")]
        [SerializeField] private float spinDuration = 0.6f;     // 旋转一周时长

        private float _targetX;
        private bool _busy;

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
            bool active = next == GameState.LevelSelect;
            if (root != null) root.SetActive(active);
        }

        private void Update()
        {
            if (_busy || GameFlowManager.Instance == null ||
                GameFlowManager.Instance.Current != GameState.LevelSelect) return;

            // 摇杆水平滑动：向左滑动图标整体左移，向右复位
            float x = input != null ? input.Thumbstick.x : 0f;
            if (Mathf.Abs(x) > deadZone)
                _targetX -= x * slideSpeed * Time.deltaTime;   // 左滑(x<0) → 内容右移复位；右滑 → 左移

            if (iconContainer != null)
            {
                Vector2 pos = iconContainer.anchoredPosition;
                pos.x = Mathf.Lerp(pos.x, _targetX, Time.deltaTime * slideSmooth);  // 位移拖尾
                iconContainer.anchoredPosition = pos;
            }
        }

        /// <summary>由某个关卡图标的射线 Select 事件调用。</summary>
        public void SelectLevel(LevelIcon icon)
        {
            if (_busy || icon == null) return;
            _busy = true;
            StartCoroutine(SpinAndEnter(icon));
        }

        private IEnumerator SpinAndEnter(LevelIcon icon)
        {
            // 匀速旋转完整一周
            float t = 0f;
            Quaternion start = icon.transform.localRotation;
            while (t < spinDuration)
            {
                t += Time.deltaTime;
                float deg = Mathf.Lerp(0f, 360f, t / spinDuration);
                icon.transform.localRotation = start * Quaternion.Euler(0f, 0f, deg);
                yield return null;
            }
            icon.transform.localRotation = start;
            // 加载对应第二章关卡
            GameFlowManager.Instance.TransitionTo(GameState.Chapter2Roaming);
        }
    }
}
