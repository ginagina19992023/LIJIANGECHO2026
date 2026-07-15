using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter2
{
    /// <summary>
    /// 场景中的一处壮族纹样：球形碰撞盒触发距离 1.2m，玩家靠近激活外发光+淡入淡出+音效；
    /// 射线悬停高亮；扳机点击弹出绘制窗口。完成后标记不再重复弹窗。
    /// 对应「1.2 碰撞与空间判定」「2. 纹样触发交互逻辑」。
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class PatternTrigger : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private int patternIndex;
        [SerializeField] private float triggerRadius = 1.2f;
        [SerializeField] private Transform player;             // 头显/相机
        [SerializeField] private DrawingWindow drawingWindow;

        [Header("表现")]
        [SerializeField] private GameObject glowEffect;        // 外发光特效
        [SerializeField] private GameObject nearPrompt;        // 靠近提示 UI
        [SerializeField] private AudioSource decorSfx;         // 短促壮族装饰音效

        private SphereCollider _col;
        private bool _inRange;
        private bool _hovering;

        private void Awake()
        {
            _col = GetComponent<SphereCollider>();
            _col.isTrigger = true;
            _col.radius = triggerRadius;
        }

        private void Update()
        {
            if (player == null) return;

            bool completed = ProgressTracker.Instance != null && ProgressTracker.Instance.IsDone(patternIndex);
            bool near = Vector3.Distance(player.position, transform.position) <= triggerRadius;

            if (near != _inRange)
            {
                _inRange = near;
                if (glowEffect != null) glowEffect.SetActive(near && !completed);
                if (nearPrompt != null) nearPrompt.SetActive(near && !completed);
                if (near && !completed && decorSfx != null) decorSfx.Play();
            }
        }

        public void SetHover(bool hovering)
        {
            _hovering = hovering;
            // 射线悬停时强化高亮（可在此处切材质/加描边）
        }

        /// <summary>射线对准纹样按下扳机时调用。</summary>
        public void OnClicked()
        {
            if (!_inRange || !_hovering) return;
            if (ProgressTracker.Instance != null && ProgressTracker.Instance.IsDone(patternIndex)) return;
            if (drawingWindow == null) return;

            // 打开弹窗：冻结漫游由 GameFlowManager 状态切换处理
            GameFlowManager.Instance.SetState(GameState.Chapter2Drawing);
            drawingWindow.Open(patternIndex, OnDrawComplete, OnDrawCancel);
        }

        private void OnDrawComplete(int index)
        {
            ProgressTracker.Instance?.MarkDone(index);
            if (glowEffect != null) glowEffect.SetActive(false);
            if (nearPrompt != null) nearPrompt.SetActive(false);

            if (ProgressTracker.Instance != null && ProgressTracker.Instance.AllDone)
                GameFlowManager.Instance.TransitionTo(GameState.Chapter3Countdown); // 三处完成 → 进入打击关
            else
                GameFlowManager.Instance.SetState(GameState.Chapter2Roaming);        // 恢复漫游
        }

        private void OnDrawCancel(int index)
        {
            // 未完成手动关闭：不计进度，恢复漫游，纹样可重复点击
            GameFlowManager.Instance.SetState(GameState.Chapter2Roaming);
        }
    }
}
