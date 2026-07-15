namespace LijiangEcho.Core
{
    /// <summary>
    /// 全局游戏流程状态。对应需求：第一章开始/选关 → 第二章漫游绘制 → 第三章音乐打击 → 结算图鉴。
    /// </summary>
    public enum GameState
    {
        Boot,            // 启动
        StartMenu,       // 第一章 P1：开始游戏界面（绣球）
        LevelSelect,     // 第一章 P2：关卡选择（摇杆滑动）
        Chapter2Roaming, // 第二章：空间漫游寻纹样
        Chapter2Drawing, // 第二章：纹样绘制弹窗
        Chapter3Countdown, // 第三章：3-2-1 倒计时
        Chapter3Rhythm,  // 第三章：音乐节奏打击
        Chapter3Interlude, // 第三章：穿插纹样绘制
        Victory,         // 通关胜利弹窗
        Defeat,          // 失败弹窗
        BossFire,        // 反派火焰结局动画（4s）
        Gallery          // 纹样图鉴卡片浏览
    }
}
