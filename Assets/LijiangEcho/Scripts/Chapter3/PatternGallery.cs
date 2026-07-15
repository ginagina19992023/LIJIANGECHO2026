using System;
using UnityEngine;
using LijiangEcho.Core;

namespace LijiangEcho.Chapter3
{
    [Serializable]
    public class PatternCard
    {
        public string title;                 // 纹样名（铜鼓纹/花卉纹/地主恶霸叙事纹…）
        [TextArea(3, 8)] public string description;  // 科普介绍
        public Sprite silhouette;            // 黑白剪纸剪影
    }

    /// <summary>
    /// 纹样图鉴卡片浏览：左右箭头切换科普卡片，可无限循环翻页；左上角设置按钮返回重玩/漫游。
    /// 对应「五、收尾：纹样图鉴卡片浏览系统」。
    /// </summary>
    public class PatternGallery : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameObject root;
        [SerializeField] private UnityEngine.UI.Image silhouetteImage;
        [SerializeField] private UnityEngine.UI.Text titleText;   // uGUI 内置文本（离线可用）
        [SerializeField] private UnityEngine.UI.Text descText;
        [SerializeField] private AudioDirector audioDirector;

        [Header("卡片数据")]
        [SerializeField] private PatternCard[] cards;

        private int _index;

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
            bool active = next == GameState.Gallery;
            if (root != null) root.SetActive(active);
            if (active) Refresh();
        }

        /// <summary>由右箭头按钮调用。</summary>
        public void Next()
        {
            if (cards == null || cards.Length == 0) return;
            _index = (_index + 1) % cards.Length;    // 无限循环
            audioDirector?.PlayPageFlip();
            Refresh();
        }

        /// <summary>由左箭头按钮调用。</summary>
        public void Prev()
        {
            if (cards == null || cards.Length == 0) return;
            _index = (_index - 1 + cards.Length) % cards.Length;
            audioDirector?.PlayPageFlip();
            Refresh();
        }

        private void Refresh()
        {
            if (cards == null || cards.Length == 0) return;
            var c = cards[_index];
            if (titleText != null) titleText.text = c.title;
            if (descText != null) descText.text = c.description;
            if (silhouetteImage != null) silhouetteImage.sprite = c.silhouette;
        }

        /// <summary>设置按钮：返回前期漫游场景重玩。</summary>
        public void BackToRoaming()
        {
            GameFlowManager.Instance.TransitionTo(GameState.Chapter2Roaming);
        }

        /// <summary>设置按钮：重玩打击关卡。</summary>
        public void ReplayRhythm()
        {
            GameFlowManager.Instance.TransitionTo(GameState.Chapter3Countdown);
        }
    }
}
