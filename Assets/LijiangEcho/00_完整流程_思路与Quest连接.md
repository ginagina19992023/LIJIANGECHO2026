# 漓江回声 · 完整流程：设计思路 + 打开项目 + 连 Quest 3

本文回答三件事：①整体思路是怎么设计的；②Unity 里怎么打开这个项目；③MR 从零跑通、连上 Quest 3 的完整流程。

---

# 一、整体设计思路（先理解，再动手）

## 1. 三层分离架构

整个游戏拆成三层，各管各的、互不耦合，这样美术、程序、MR 可以并行推进：

```
┌─────────────────────────────────────────────┐
│  美术层  你的 *-layers.unity / png / mp4       │  ← 只管"长什么样"
├─────────────────────────────────────────────┤
│  逻辑层  我写的 C# 脚本(状态机/判定/绘制/谱面)  │  ← 只管"怎么玩"
├─────────────────────────────────────────────┤
│  MR 层   OpenXR 透视 + XR 手柄射线             │  ← 只管"虚实融合 + 输入"
└─────────────────────────────────────────────┘
```

- **美术层**：你已有的分层场景（开始界面、选关、关卡、过场）。美术只做视觉，不写逻辑。
- **逻辑层**：脚本挂在美术物体上，靠事件被调用。换美术不用改脚本。
- **MR 层**：透视相机 + 手柄射线是全局底座，切场景不动它。

## 2. 一条状态机串起全流程（核心思路）

`GameFlowManager` 是总指挥，用一个 `GameState` 枚举表示当前在哪个界面：

```
Boot → StartMenu → LevelSelect → Chapter2Roaming ⇄ Chapter2Drawing
     →(三纹样完成)→ Chapter3Countdown → Chapter3Rhythm ⇄ Chapter3Interlude
     → Victory → BossFire(4s) → Gallery
     （任意失败 → Defeat → 回 Countdown 重来）
```

- 每个界面脚本都**订阅 `OnStateChanged`**，状态一变就自动显示/隐藏自己。
- 想推进流程，只要调 `GameFlowManager.Instance.SetState(...)` 或带转场的 `TransitionTo(...)`。
- 好处：整条流程就是这一张图，任何一处 bug 都能定位到是哪个状态没切对。

## 3. 事件驱动交互（MR 手柄怎么连到逻辑）

MR 里没有鼠标，靠**手柄射线**。思路是：
- 射线命中一个物体 → 触发它的 `Hover Entered` / `Select Entered` 事件；
- 这些事件在 Inspector 里连到我脚本的方法（`SetHover` / `OnConfirm` / `OnClicked`…）。
- 扳机的**单击 / 双击 / 长按**由 `ControllerInput` 自己计时判断，不依赖美术。

## 4. 数据驱动谱面

第三章打击的所有音符（19s~1min43s）写在 `NoteChart` 里，是**纯数据**。
改节奏只改数据、不改代码。`RhythmConductor` 读数据推进判定、在 3 个点插入绘制。

## 5. 复用

第二章的"纹样绘制窗口"`DrawingWindow`，第三章打击中途的穿插绘制**直接复用同一个组件**，
只是完成后回调不同（第二章计进度、第三章解锁下一段节拍）。少写一半代码。

---

# 二、Unity 怎么打开这个项目

## 1. 装 Unity（一次性）

1. 装 **Unity Hub**（官网下载）。
2. Hub 里 `Installs > Install Editor` 选 **2022.3 LTS**（和 `ProjectSettings/ProjectVersion.txt` 对上）。
3. 勾选安装模块：**Android Build Support**（含 **OpenJDK** 和 **Android SDK & NDK Tools**）——连 Quest 必须要这个。

## 2. 打开项目（注意选对目录！）

- GitHub Desktop 里先确认在 `claude/zhuang-culture-mr-game-lgme9n` 分支并已 **Pull** 到最新。
- Unity Hub → `Open` → 选 **仓库根目录 `D:\LIJIANGECHO2026`**（就是有 `Assets` 文件夹那层）。
- ❌ 不要去开 `LIJIANGECHOFILE` 或 `LIJIANGECHOFILEE`，那不是工程。
- 首次打开 Unity 会拉包（OpenXR/AR Foundation/XRI…）+ 编译，等几分钟。
  弹「Enable new Input System / Restart」选 **Yes**。

## 3. 把美术接进工程（关键一步，否则 Unity 看不到你的美术）

你的美术现在在 `LIJIANGECHOFILE/LIJIANGECHOFILEE/ARTS/`，**在 Assets 外面，Unity 不认**。
把它挪进 Assets：

- 用 Windows 资源管理器，把整个 `ARTS` 文件夹（连同里面的 `.meta` 文件一起）
  移动到 `D:\LIJIANGECHO2026\Assets\LijiangEcho\ARTS`。
- 回到 Unity，Project 窗口里就能看到这些 `*-layers.unity` 场景、png、mp4 了。
- （`.meta` 一定要跟着一起挪，否则引用会丢。）

> 移动完可以 commit 一次，把美术正式纳入工程 Assets 目录。

---

# 三、MR 完整流程 + 连 Quest 3

## 阶段 A：开 MR 能力（项目设置，做一次）

1. `File > Build Settings > Android > Switch Platform`。
2. `Edit > Project Settings > XR Plug-in Management`：Android 页勾 **OpenXR**。
3. `OpenXR` 子页：
   - Interaction Profiles 加 **Oculus Touch Controller Profile**；
   - 勾 **Meta Quest Support** + **Meta Quest: Passthrough**。
4. `Player Settings`：Scripting Backend = **IL2CPP**，Target Architectures = **ARM64**，Color Space = **Linear**。

## 阶段 B：搭 MR 场景（做一次）

（详细见 `MR搭建_Quest3.md`）要点：
- `XR Origin (AR)` + `AR Session`；相机加 `AR Camera Manager` + `AR Camera Background`，
  **Clear Flags=Solid Color、背景 Alpha=0**（透出真实画面）；
- 两手柄加 `XR Ray Interactor` + `Line Renderer`；
- 界面 Canvas 全设 **World Space**，悬浮在玩家前方 1.5~2m。

## 阶段 C：连接 Quest 3（两种方式）

### 方式①：真机 Build And Run —— 验证透视，最真实【推荐】

**准备（一次性）：**
1. 手机装 **Meta Horizon** app，登录和头显同一账号。
2. app 里：设备 → 你的 Quest 3 → **开发者模式 → 打开**
   （需要先在 Meta 官网注册一个"开发者组织"，免费，填个名字即可）。
3. USB-C 数据线把 Quest 3 连到电脑。
4. 戴上头显，弹出「允许 USB 调试」→ **允许**（勾"始终允许这台电脑"）。

**每次运行：**
5. Unity → `File > Build And Run`（第一次会让你选 apk 保存位置，随便建个 `Builds` 文件夹）。
6. Unity 自动打包 → 装到头显 → 自动启动。
7. 戴上头显，如果没自动开：资源库 →「未知来源 / Unknown Sources」里找到你的 app。
8. 这时应该能看到**真实房间画面 + 悬浮的壮族 UI**。

> 透视（passthrough）只有在**真机**上才真正显示，编辑器里看不到透视背景，这是正常的。

### 方式②：Quest Link 编辑器实时预览 —— 调逻辑/UI 位置最快

1. 电脑装 **Meta Quest（PC 桌面版）** 应用并登录。
2. USB 连接（或同网 Air Link），头显里进入 **Quest Link**。
3. Unity 里直接点 **Play**，画面串流到头显，手柄能动、能点 UI。
4. 用来调交互、UI 摆放、流程跑通很方便；**但透视背景可能不显示**（Link 下 passthrough 受限），
   透视效果仍以方式①真机为准。
5. 若想在编辑器里连交互都懒得连头显，可加 **XR Device Simulator**（XRI 自带）用键鼠模拟手柄。

## 阶段 D：一步步接起来（建议顺序）

1. 阶段 A/B 配好，真机 Build 一次 → 看到「房间 + 一个测试方块」就说明 MR 底座通了。
2. 接**第一章开始界面**：把 `start-screen-layers` 摆成 World Space Canvas，挂 `StartMenuController`，
   绣球加 Collider + XR Simple Interactable，Hover/Select 事件连脚本 → 能"射线点绣球进游戏"。
3. 同法接选关 → 第二章漫游+绘制 → 第三章打击。每接一章 Build 一次验证。
4. 谱面用 `NoteChart` 默认数据即可，音频按 `docs/需求文档.md` 第七节接。

---

## 一句话总结

**打开 `D:\LIJIANGECHO2026`（不是 LIJIANGECHOFILEE）→ 把 ARTS 挪进 Assets → 开 OpenXR 透视 →
搭 XR Origin(AR) 透视相机 → 手柄射线事件接脚本 → Build And Run 到 Quest 3 看效果。**
逻辑脚本全就绪，你主要做的是"在 Unity 里把美术和脚本、射线事件连起来"。
