using System.Collections.Generic;
using UnityEngine;

namespace LijiangEcho.Chapter3
{
    /// <summary>音符类型，对应「手柄多操作识别逻辑」。</summary>
    public enum NoteType
    {
        Single,   // 单击（单侧）
        Double,   // 双击（双侧）
        Long,     // 长按（拖拍，有起止时间）
        Slide     // 符号滑动（跟随纹样滑动，穿插纹样绘制的引子）
    }

    [System.Serializable]
    public struct NoteEntry
    {
        public NoteType type;
        public float time;      // 命中时间点（秒）；Long/Slide 为起始时间
        public float endTime;   // Long/Slide 的结束时间；点击类可留 0
        public string symbol;   // Slide 的纹样名（铜钱纹/蛇纹/巨鸟纹）

        public NoteEntry(NoteType t, float time, float end = 0f, string symbol = "")
        {
            this.type = t; this.time = time; this.endTime = end; this.symbol = symbol;
        }
    }

    /// <summary>
    /// 音乐谱面。可在 Project 里创建为资产手动编辑，
    /// 也可调用 <see cref="BuildDefault"/> 生成需求文档给定的默认谱面。
    /// 对应「音乐判定：游戏关卡音乐音符安排」。
    /// </summary>
    [CreateAssetMenu(fileName = "NoteChart", menuName = "LijiangEcho/Note Chart")]
    public class NoteChart : ScriptableObject
    {
        public List<NoteEntry> notes = new List<NoteEntry>();

        /// <summary>三处穿插纹样绘制触发时间点（在这些 Slide 段之后强制弹绘制窗口）。</summary>
        public float[] interludeTriggerTimes = { 37f, 60f, 103f };

        /// <summary>按需求文档还原默认谱面。</summary>
        public static List<NoteEntry> BuildDefault()
        {
            var n = new List<NoteEntry>
            {
                new NoteEntry(NoteType.Double, 19f),
                new NoteEntry(NoteType.Single, 21f),
                new NoteEntry(NoteType.Single, 23f),
                new NoteEntry(NoteType.Long,   26f, 28f),
                new NoteEntry(NoteType.Long,   29f, 31f),
                new NoteEntry(NoteType.Slide,  32f, 37f, "铜钱纹"),

                new NoteEntry(NoteType.Double, 41f),
                new NoteEntry(NoteType.Double, 42f),
                new NoteEntry(NoteType.Single, 44f),
                new NoteEntry(NoteType.Double, 45f),
                new NoteEntry(NoteType.Single, 46f),
                new NoteEntry(NoteType.Single, 48f),
                new NoteEntry(NoteType.Double, 49f),
                new NoteEntry(NoteType.Double, 50f),
                new NoteEntry(NoteType.Single, 51f),
                new NoteEntry(NoteType.Double, 53f),
                new NoteEntry(NoteType.Single, 54f),
                new NoteEntry(NoteType.Single, 55f),
                new NoteEntry(NoteType.Slide,  57f, 60f, "蛇纹"),

                new NoteEntry(NoteType.Single, 62f),
                new NoteEntry(NoteType.Double, 64f),
                new NoteEntry(NoteType.Long,   66f, 68f),
                new NoteEntry(NoteType.Single, 70f),
                new NoteEntry(NoteType.Long,   72f, 75f),
                new NoteEntry(NoteType.Long,   75f, 78f),
                new NoteEntry(NoteType.Single, 80f),
                new NoteEntry(NoteType.Double, 82f),
                new NoteEntry(NoteType.Single, 84f),
                new NoteEntry(NoteType.Double, 85f),
                new NoteEntry(NoteType.Single, 88f),
                new NoteEntry(NoteType.Double, 89f),
                new NoteEntry(NoteType.Double, 90f),
                new NoteEntry(NoteType.Single, 93f),
                new NoteEntry(NoteType.Single, 95f),
                new NoteEntry(NoteType.Double, 96f),
                new NoteEntry(NoteType.Slide,  97f, 103f, "巨鸟纹"),
            };
            return n;
        }

#if UNITY_EDITOR
        [ContextMenu("填充默认谱面")]
        private void FillDefault() => notes = BuildDefault();
#endif
    }
}
