// ============================================================================
//  AutoRayInteractable —— 一键自动接线组件（依赖 XRI）
//
//  用法：把本组件加到任意 3D 可交互物体上（绣球 / 关卡图标 / 场景纹样）。
//  它会自动：
//    1）没有 Collider 就补一个 BoxCollider（你再拖动把它罩住美术即可）；
//    2）没有 XR Simple Interactable 就补一个；
//    3）把射线的 Hover / Select 事件，自动连到该物体（或其父物体）上对应的控制脚本方法：
//         - StartMenuController → SetHover / OnConfirm
//         - LevelIcon           → SetHover / OnSelected
//         - PatternTrigger      → SetHover / OnClicked
//  这样就不用再在 Inspector 里手动连 UnityEvent 了。
//
//  ⚠️ 本文件用到 XRI（XR Interaction Toolkit）。若 Console 只在本文件报
//     “UnityEngine.XR.Interaction.Toolkit 找不到”，说明 XRI 没真正装上，
//     告诉我，我把这个文件移除即可，不影响其它脚本。
//
//  注：UI 按钮（图鉴箭头 / 设置）不需要本组件，用 Button 的 On Click 连
//     PatternGallery.Prev / Next 即可。
// ============================================================================
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using LijiangEcho.Chapter1;
using LijiangEcho.Chapter2;

namespace LijiangEcho.Core
{
    [DisallowMultipleComponent]
    public class AutoRayInteractable : MonoBehaviour
    {
        [Tooltip("没有 Collider 时自动补的 BoxCollider 尺寸（本地坐标），加好后可在场景里手动调")]
        [SerializeField] private Vector3 autoColliderSize = new Vector3(0.3f, 0.3f, 0.1f);

        private XRSimpleInteractable _interactable;

        // 缓存目标控制脚本（可能在本物体或父物体上）
        private StartMenuController _start;
        private LevelIcon _icon;
        private PatternTrigger _pattern;

        private void Awake()
        {
            // 1) 保证有 Collider（射线命中靠它）
            if (GetComponent<Collider>() == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.size = autoColliderSize;
            }

            // 2) 保证有 XR Simple Interactable
            _interactable = GetComponent<XRSimpleInteractable>();
            if (_interactable == null)
                _interactable = gameObject.AddComponent<XRSimpleInteractable>();

            // 3) 找到目标控制脚本（本物体或父物体）
            _start = GetComponentInParent<StartMenuController>();
            _icon = GetComponentInParent<LevelIcon>();
            _pattern = GetComponentInParent<PatternTrigger>();
        }

        private void OnEnable()
        {
            if (_interactable == null) return;
            _interactable.hoverEntered.AddListener(HandleHoverEnter);
            _interactable.hoverExited.AddListener(HandleHoverExit);
            _interactable.selectEntered.AddListener(HandleSelect);
        }

        private void OnDisable()
        {
            if (_interactable == null) return;
            _interactable.hoverEntered.RemoveListener(HandleHoverEnter);
            _interactable.hoverExited.RemoveListener(HandleHoverExit);
            _interactable.selectEntered.RemoveListener(HandleSelect);
        }

        private void HandleHoverEnter(HoverEnterEventArgs _) => SetHover(true);
        private void HandleHoverExit(HoverExitEventArgs _) => SetHover(false);

        private void SetHover(bool hovering)
        {
            if (_start != null) _start.SetHover(hovering);
            if (_icon != null) _icon.SetHover(hovering);
            if (_pattern != null) _pattern.SetHover(hovering);
        }

        private void HandleSelect(SelectEnterEventArgs _)
        {
            if (_start != null) _start.OnConfirm();
            if (_icon != null) _icon.OnSelected();
            if (_pattern != null) _pattern.OnClicked();
        }
    }
}
