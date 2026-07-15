using UnityEngine;
using LijiangEcho.Chapter3;

namespace LijiangEcho
{
    /// <summary>
    /// 全局音频总控：分段打击音效、背景音乐衰减/暂停、胜负与图鉴音效等。
    /// 对应「七、音频完整需求」。
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        [Header("背景音乐")]
        [SerializeField] private AudioSource music;
        [SerializeField] private float duckedVolume = 0.2f;
        private float _baseMusicVolume = 1f;

        [Header("打击音效")]
        [SerializeField] private AudioSource sfx;
        [SerializeField] private AudioClip perfectClip;  // 完美：重鼓
        [SerializeField] private AudioClip greatClip;    // 良好：轻鼓
        [SerializeField] private AudioClip goodClip;     // good：敲锣
        [SerializeField] private AudioClip missClip;     // Miss：刺耳破音
        [SerializeField] private AudioClip longHoldClip; // 长按持续拖音
        [SerializeField] private AudioSource longHoldSource;

        [Header("界面/结算音效")]
        [SerializeField] private AudioClip victoryClip;  // 民乐高潮合奏
        [SerializeField] private AudioClip defeatClip;   // 低沉闷鼓/破碎声
        [SerializeField] private AudioClip pageFlipClip; // 壮锦布料滑动

        private void Awake()
        {
            if (music != null) _baseMusicVolume = music.volume;
        }

        public void PlayJudgment(Judgment j)
        {
            AudioClip clip = j switch
            {
                Judgment.Perfect => perfectClip,
                Judgment.Great => greatClip,
                Judgment.Good => goodClip,
                _ => missClip
            };
            if (clip != null && sfx != null) sfx.PlayOneShot(clip);
        }

        public void PlayLongHold(bool start)
        {
            if (longHoldSource == null) return;
            if (start)
            {
                longHoldSource.clip = longHoldClip;
                longHoldSource.loop = true;
                longHoldSource.Play();
            }
            else longHoldSource.Stop();
        }

        /// <summary>绘制弹窗期间背景音乐减弱/暂停。</summary>
        public void DuckMusic(bool duck)
        {
            if (music == null) return;
            music.volume = duck ? duckedVolume : _baseMusicVolume;
        }

        public void PlayVictory() { if (victoryClip != null && sfx != null) sfx.PlayOneShot(victoryClip); }
        public void PlayDefeat()  { if (defeatClip != null && sfx != null) sfx.PlayOneShot(defeatClip); }
        public void PlayPageFlip(){ if (pageFlipClip != null && sfx != null) sfx.PlayOneShot(pageFlipClip); }
    }
}
