# 搭建总流程 + Prefab 结构

看不清"每一步在哪"时，对照本文。

---

## 一、总流程：每一步在哪

```
阶段 A · 一次性准备（整个工程只做一次）
 1. 打开工程、装包（XRI 等）
 2. 搭 MR 底座：XR Origin + 透视相机 + 手柄射线
    └ 把 XR 相机的 Tag 设为 MainCamera（脚本自动找相机靠这个）

阶段 B · 每个界面重复做（开始→选关→漫游→打击→图鉴）
 3. 把美术摆进场景（World Space Canvas / 3D 物体）
 4. 给可交互物体挂脚本、填引用
 5. ★接射线事件★  ← “射线接线”这步在这里：让点击真的触发脚本
 6. 测试这个界面（Play 或 Build）

阶段 C
 7. Build 到 Quest
```

**“接射线”= 阶段 B 第 5 步**，是每做一个界面都要做的小步骤，不是全程只做一次。
详见 `射线接线_图文步骤.md`。

---

## 二、Prefab 结构（做成预制，别人拖进去就能用）

把"美术 + Collider + Interactable + 脚本 + 接好的事件"打包成 Prefab，
从 Hierarchy 拖到 Project 窗口即生成。接好的事件会随 Prefab 一起保存。

| Prefab | 里面放什么 | 怎么用 |
| --- | --- | --- |
| **XROrigin** | 透视相机（Tag=MainCamera）+ 两手柄 XR Ray Interactor | 每个场景放 1 个 |
| **GameSystem** | GameFlowManager + AudioDirector + ProgressTracker + DrawingWindow | 每个场景放 1 个 |
| **绣球** | 美术 + Collider + XR Simple Interactable + StartMenuController，事件→`OnConfirm`/`SetHover` | 开始界面拖入 |
| **关卡图标** | 美术 + Collider + Interactable + LevelIcon，事件→`OnSelected`/`SetHover` | 选关界面拖入 |
| **纹样** | 美术 + SphereCollider + Interactable + PatternTrigger，事件→`OnClicked`/`SetHover` | 漫游场景拖 3 个（patternIndex 设 0/1/2） |
| **图鉴卡** | Canvas + Button 箭头 + PatternGallery，Button.OnClick→`Prev`/`Next` | 图鉴界面拖入 |

### 为什么现在能做到"拖进去就用"

我把脚本改成了 **Prefab 友好**：以下引用**留空会自动补全**，不用手动连场景物体——

| 脚本 | 留空自动找 |
| --- | --- |
| PatternTrigger | `player`→`Camera.main`；`drawingWindow`→场景里的 DrawingWindow |
| RoamingLocomotion | `head`→`Camera.main` |
| RhythmConductor | `drawingWindow`→场景里的 DrawingWindow |
| LevelIcon | `controller`→场景里的 LevelSelectController |

加上所有流程切换都走 `GameFlowManager.Instance`（单例，不需要连场景物体），
所以这些界面 Prefab **只要场景里有 XROrigin + GameSystem 两个 Prefab，就能直接工作**。

### 铁律（Prefab 能复用的前提）

UnityEvent 里连的方法**必须在 Prefab 内部**（同一个 Prefab 上的脚本）才会被保存。
- ✅ 绣球 Prefab 的事件连它自己身上的 `StartMenuController` → 存得住、可复用。
- ❌ 连到场景里外部某个物体 → 换场景就 Missing。
（我脚本用单例驱动流程，正是为了绕开这条限制。）

### 仍需手动指定的少数引用

- 各脚本的 **`input`（ControllerInput）**：因为有左右两个手柄，自动找会分不清哪只手，
  所以摇杆/打击相关的 `input` 字段还是手动拖一下（拖左手柄上的 ControllerInput 即可）。

---

## 三、给"别人直接用"的最简步骤

1. 场景里拖入 **XROrigin** + **GameSystem** 两个 Prefab；
2. 拖入需要的界面 Prefab（绣球 / 图标 / 纹样…）；
3. 把手柄的 ControllerInput 拖给需要摇杆的脚本（选关/漫游/打击）；
4. Play 或 Build。
