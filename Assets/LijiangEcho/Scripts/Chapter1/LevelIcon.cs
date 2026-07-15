using UnityEngine;

namespace LijiangEcho.Chapter1
{
    /// <summary>
    /// 单个壮锦凤凰关卡图标：命中高亮（放大 10% + 提亮），并把 Select 事件转给选关控制器。
    /// </summary>
    public class LevelIcon : MonoBehaviour
    {
        [SerializeField] private LevelSelectController controller;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float lerp = 8f;

        private Vector3 _baseScale;
        private bool _hovering;

        private void Awake() => _baseScale = transform.localScale;

        public void SetHover(bool hovering) => _hovering = hovering;

        /// <summary>由 XR 射线 Select（确认键）事件调用。</summary>
        public void OnSelected()
        {
            if (controller != null) controller.SelectLevel(this);
        }

        private void Update()
        {
            Vector3 target = _hovering ? _baseScale * hoverScale : _baseScale;
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * lerp);
        }
    }
}
