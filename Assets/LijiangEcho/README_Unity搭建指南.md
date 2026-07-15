# 漓江回声 · Unity 工程搭建指南

本工程为「广西壮文化 MR 交互游戏」（需求见 `docs/需求文档.md`）的代码骨架。
以下步骤帮你在 Unity 里从零把场景搭起来。

## 0. 环境

- **Unity 版本**：2022.3 LTS（`ProjectSettings/ProjectVersion.txt` 已写 2022.3.40f1，可用 Unity Hub 装同版本或就近 LTS）。
- **目标设备**：6DoF MR 头显（Meta Quest / PICO 等），透视 Passthrough。
- 首次用 Unity Hub「Open」打开本仓库根目录，Unity 会自动按 `Packages/manifest.json` 拉取：
  Input System、XR Interaction Toolkit、XR Management、OpenXR、TextMeshPro 等。
  弹出「Enable new Input System?」选 **Yes** 重启。

## 1. 开启 MR / XR

1. `Edit > Project Settings > XR Plug-in Management` → 安装并勾选 **OpenXR**（对应你的设备平台页签）。
2. OpenXR 设置里添加交互 Profile（如 Meta Quest Touch / PICO），并启用 **Passthrough / 透视** 功能。
3. 场景里用 `XR Origin (VR)` 预制（XR Interaction Toolkit 自带），相机 Clear Flags 设为 Solid Color 且 alpha=0 以透出实景。

## 2. 场景与全局对象

新建场景 `Main`，放一个空物体 `_Game`，挂：

- `GameFlowManager`（Core）——全局状态机，接一个全屏 `CanvasGroup` 作淡入淡出遮罩。
- `AudioDirector`（Audio）——接背景音乐 AudioSource 和各类音效 Clip。
- `ProgressTracker`（Chapter2）——纹样进度（0/3）。

在每个手柄上挂 `ControllerInput`（Core），把 InputActionReference 绑到 OpenXR 的
Trigger / Grip / Thumbstick（可直接用 XRI 默认的 `XRI Default Input Actions`）。

## 3. 第一章

- **P1 开始界面**：Canvas 里放绣球、装饰、「进入游戏」，挂 `StartMenuController`。
  绣球加 Collider + XRI `Interactable`，Hover 事件接 `SetHover`，Select 事件接 `OnConfirm`。
- **P2 选关**：挂 `LevelSelectController`，两张凤凰图标各挂 `LevelIcon`，
  Icon 的 Select 事件接 `OnSelected`，容器 RectTransform 拖进 `iconContainer`。

## 4. 第二章

- `XR Origin` 上挂 `RoamingLocomotion`，接 `ControllerInput`、head（相机）、CharacterController。
- 每处纹样物体挂 `PatternTrigger`（自带 SphereCollider，触发半径 1.2m），设置 `patternIndex` 0/1/2，
  接 player（相机）、`DrawingWindow`、发光/提示/音效引用。纹样加 XRI Interactable，Select 接 `OnClicked`。
- 全屏绘制弹窗挂 `DrawingWindow` + `PatternMatcher`，把标准纹样轮廓采样点填进 `targetOutline`，
  画布的射线命中点每帧喂给 `FeedPointer(canvasPoint, triggerHeld)`。

## 5. 第三章

- **倒计时**：`CountdownController`，接 TextMeshPro 数字、鼓点 AudioSource。
- **打击主控**：`RhythmConductor`，接 `NoteChart`（可在 Project 右键
  `Create > LijiangEcho > Note Chart` 建资产，或勾选脚本里的 useDefaultChart 用内置默认谱面）、
  背景音乐 AudioSource、`ControllerInput`、`HitRingJudge`、`LifeSystem`、`DrawingWindow`、`ResultPopup`、`AudioDirector`。
- **判定圆环**：`HitRingJudge` 设置 inner/outer 归一化半径；命中圆环 UI 时把归一化半径写入
  `RhythmConductor.lastHitRadius`。
- **穿插绘制**：复用 `DrawingWindow`，`RhythmConductor` 已在 3 个 Slide 段后自动触发。
- **结算**：`ResultPopup`（胜利/失败根节点）→ 胜利后 `BossFireSequence`（4 秒火焰，接火焰遮罩 + 音效）
  → 结束进入 `PatternGallery`（图鉴卡片，填 `cards` 数组，左右箭头按钮接 `Prev`/`Next`）。

## 6. 音符谱面

`NoteChart.BuildDefault()` 已按需求文档还原全部音符时间点（19s 起至 1min43s）。
如需手改：建 NoteChart 资产 → 右键脚本菜单「填充默认谱面」→ 微调。
三处穿插绘制默认触发时间 `interludeTriggerTimes = {37, 60, 103}`（三段符号滑动之后）。

## 7. 状态流转总览

```
Boot → StartMenu → LevelSelect → Chapter2Roaming ⇄ Chapter2Drawing
     → (三纹样完成) → Chapter3Countdown → Chapter3Rhythm ⇄ Chapter3Interlude
     → Victory → BossFire(4s) → Gallery
     （任意失败 → Defeat → 回 Chapter3Countdown 重来）
```

所有界面脚本都订阅 `GameFlowManager.OnStateChanged` 自动显隐，切状态即可驱动整条流程。

---

> 说明：脚本放在默认 `Assembly-CSharp` 下，自动引用已安装的 Input System / XRI 包。
> 美术、音频、预制体资源需按需求文档「六、美术资源 / 七、音频」自行导入并在 Inspector 里接线。
