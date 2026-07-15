using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LijiangEcho.Core
{
    /// <summary>
    /// 手柄输入抽象层：识别扳机「单击 / 双击 / 长按」三类操作，暴露摇杆二维输入与侧键。
    /// 对应需求「手柄多操作识别逻辑（单击/双击/长按）」与摇杆滑动选关/漫游移动。
    /// 用 InputActionReference 绑定，方便在 Inspector 里接 OpenXR / XRI 的输入动作。
    /// </summary>
    public class ControllerInput : MonoBehaviour
    {
        [Header("输入动作绑定（Input System）")]
        [Tooltip("扳机键（Button）")]
        public InputActionReference trigger;
        [Tooltip("侧键 / Grip（Button），用于手动关闭绘制弹窗")]
        public InputActionReference sideButton;
        [Tooltip("摇杆（Vector2）")]
        public InputActionReference thumbstick;

        [Header("识别参数")]
        [Tooltip("双击最大间隔（秒）")]
        public float doubleTapWindow = 0.3f;
        [Tooltip("长按判定阈值（秒），超过即视为长按")]
        public float longPressThreshold = 0.35f;

        // 事件
        public event Action OnSingleTap;   // 单击（延迟到双击窗口结束确认）
        public event Action OnDoubleTap;   // 双击
        public event Action OnLongPressStart;
        public event Action OnLongPressEnd;
        public event Action OnSideButton;

        private float _lastTapTime = -10f;
        private int _pendingTaps = 0;
        private bool _triggerHeld = false;
        private float _triggerDownTime = 0f;
        private bool _longPressFired = false;

        public Vector2 Thumbstick => thumbstick != null ? thumbstick.action.ReadValue<Vector2>() : Vector2.zero;

        private void OnEnable()
        {
            trigger?.action.Enable();
            sideButton?.action.Enable();
            thumbstick?.action.Enable();

            if (trigger != null)
            {
                trigger.action.started += OnTriggerDown;
                trigger.action.canceled += OnTriggerUp;
            }
            if (sideButton != null)
                sideButton.action.performed += _ => OnSideButton?.Invoke();
        }

        private void OnDisable()
        {
            if (trigger != null)
            {
                trigger.action.started -= OnTriggerDown;
                trigger.action.canceled -= OnTriggerUp;
            }
        }

        private void OnTriggerDown(InputAction.CallbackContext ctx)
        {
            _triggerHeld = true;
            _triggerDownTime = Time.time;
            _longPressFired = false;
        }

        private void OnTriggerUp(InputAction.CallbackContext ctx)
        {
            _triggerHeld = false;
            float held = Time.time - _triggerDownTime;

            if (_longPressFired)
            {
                // 长按结束
                OnLongPressEnd?.Invoke();
                _longPressFired = false;
                return;
            }

            if (held < longPressThreshold)
            {
                // 计入点击，交给 Update 里的双击窗口判定
                _pendingTaps++;
                _lastTapTime = Time.time;
            }
        }

        private void Update()
        {
            // 长按检测
            if (_triggerHeld && !_longPressFired && Time.time - _triggerDownTime >= longPressThreshold)
            {
                _longPressFired = true;
                OnLongPressStart?.Invoke();
            }

            // 单击 / 双击窗口结算
            if (_pendingTaps > 0 && Time.time - _lastTapTime >= doubleTapWindow && !_triggerHeld)
            {
                if (_pendingTaps >= 2) OnDoubleTap?.Invoke();
                else OnSingleTap?.Invoke();
                _pendingTaps = 0;
            }
        }
    }
}
