using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter2
{
    /// <summary>
    /// 空间漫游移动：支持①物理行走（6DoF 头显真实位移，自动跟随，无需代码）；
    /// ②手柄摇杆平滑位移（头部控制朝向）。弹窗开启时屏蔽移动。
    /// 对应「1.1 定位追踪 / 1.2 弹窗屏蔽移动」。
    /// </summary>
    public class RoamingLocomotion : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private ControllerInput input;
        [SerializeField] private Transform head;              // 头显相机，用于确定前进朝向
        [SerializeField] private CharacterController body;    // XR Rig 上的 CharacterController（可选）

        [Header("参数")]
        [SerializeField] private float moveSpeed = 1.5f;      // m/s
        [SerializeField] private float deadZone = 0.15f;
        [SerializeField] private bool joystickMoveEnabled = true; // 移动模式切换

        private bool CanMove
        {
            get
            {
                var s = GameFlowManager.Instance != null ? GameFlowManager.Instance.Current : GameState.Boot;
                return s == GameState.Chapter2Roaming;  // 仅漫游态可移动，弹窗态自动屏蔽
            }
        }

        public void ToggleJoystickMove() => joystickMoveEnabled = !joystickMoveEnabled;

        private void Awake()
        {
            // Prefab 友好化：head 留空则自动用 Camera.main（XR 相机需 tag = MainCamera）
            if (head == null && Camera.main != null) head = Camera.main.transform;
        }

        private void Update()
        {
            if (!joystickMoveEnabled || !CanMove || input == null || head == null) return;

            Vector2 stick = input.Thumbstick;
            if (stick.magnitude < deadZone) return;

            // 以头部朝向为前方，在水平面平滑位移
            Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
            Vector3 move = (forward * stick.y + right * stick.x) * moveSpeed * Time.deltaTime;

            if (body != null) body.Move(move);
            else transform.position += move;
        }
    }
}
