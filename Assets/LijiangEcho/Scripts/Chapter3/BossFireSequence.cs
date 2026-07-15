using System.Collections;
using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter3
{
    /// <summary>
    /// 反派结局动画：紫色背景 + 地主剪影，底部火焰逐步上吞。强制 4 秒、锁定所有操作、无跳过，
    /// 火焰音量由小到大，结束后自动跳转纹样图鉴。对应「过场动画模块 · 反派结局动画」。
    /// </summary>
    public class BossFireSequence : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform fireMask;   // 火焰遮罩（自下而上放大）
        [SerializeField] private AudioSource fireSfx;      // 柴火燃烧音效
        [SerializeField] private float duration = 4f;      // 强制时长

        public void Play()
        {
            if (root != null) root.SetActive(true);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            float t = 0f;
            if (fireSfx != null) { fireSfx.volume = 0f; fireSfx.Play(); }

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                // 火焰自下而上吞噬
                if (fireMask != null)
                    fireMask.localScale = new Vector3(1f, Mathf.Lerp(0f, 1.2f, k), 1f);
                // 火焰音量从小到大
                if (fireSfx != null) fireSfx.volume = k;
                yield return null;   // 全程锁定，不接受任何输入
            }

            if (root != null) root.SetActive(false);
            // 自动跳转纹样科普图鉴
            GameFlowManager.Instance.SetState(GameState.Gallery);
        }
    }
}
