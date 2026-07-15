# Quest 3 MR（透视）搭建步骤

目标：让「漓江回声」在 Meta Quest 3 上跑成 MR —— 真实办公室画面做背景，
壮族国风 UI 悬浮在空间里，手柄射线可交互。本工程走
**Unity OpenXR + Meta OpenXR（AR Foundation 透视）** 路线，与已有的 XRI / Input System 脚本无缝配合。

> 另一条路线是装 Meta XR All-in-One SDK 用 OVR 那套（Building Blocks 拖一个 Passthrough 块最快），
> 但那套输入是 OVR Interaction，和本工程的 `ControllerInput` 需要额外桥接。本文用 OpenXR 路线，脚本可直接用。

---

## 1. 平台切到 Android（Quest 是安卓设备）

`File > Build Settings > Android > Switch Platform`。
`Player Settings`：
- Minimum API Level：Android 10 (API 29) 或以上；
- Scripting Backend：**IL2CPP**；Target Architectures：**ARM64**；
- Color Space：**Linear**。

## 2. 装包（本工程 manifest 已声明，打开会自动拉取）

- `com.unity.xr.openxr`、`com.unity.xr.arfoundation`、`com.unity.xr.meta-openxr`、
  `com.unity.xr.interaction.toolkit`、`com.unity.inputsystem`。
- 若没自动装，`Window > Package Manager` 里手动确认这几个已安装。

## 3. 开 OpenXR + Meta 透视功能

`Edit > Project Settings > XR Plug-in Management`：
1. Android 页签勾选 **OpenXR**；
2. 进 `OpenXR` 子页：
   - Interaction Profiles 添加 **Oculus Touch Controller Profile**；
   - 勾选功能：**Meta Quest Support**、**Meta Quest: Passthrough**（由 meta-openxr 提供）；
3. `Project Settings > XR Plug-in Management > Meta OpenXR` 里确认 Passthrough 已启用。

## 4. 场景：XR Origin + AR 透视

新建场景 `Main`，删掉默认 Main Camera，改用：
1. `GameObject > XR > XR Origin (AR)` —— 它带 AR Camera；
2. 加 `GameObject > XR > AR Session`；
3. 选中 XR Origin 下的 **Main Camera**：
   - 加组件 **AR Camera Manager**、**AR Camera Background**（AR Foundation 会把透视画面铺到背景）；
   - Camera 的 **Clear Flags = Solid Color**，Background 颜色 **Alpha = 0**（关键！这样虚拟物后面透出实景）；
4. 两个手柄：XR Origin 下建 `LeftHand / RightHand Controller`，各加
   **XR Controller (Action-based)** + **XR Ray Interactor** + **Line Renderer**（可视化射线）。

> 这样「透视做底 + UI 悬浮 + 手柄射线」这三件 MR 的事就齐了。

## 5. 把射线接到游戏脚本

我写的脚本靠 XR 射线的 Hover/Select 事件驱动。对每个可交互物体（绣球、关卡图标、纹样、按钮、圆环、箭头）：
1. 加 **Collider** + XRI 的 **XR Simple Interactable**；
2. 在它的 Interactable 事件里连脚本方法：
   - `Hover Entered` → `SetHover(true)`；`Hover Exited` → `SetHover(false)`；
   - `Select Entered` → 对应确认方法（`OnConfirm` / `OnSelected` / `OnClicked` / `Prev` / `Next`…）。
3. 手柄的扳机/摇杆绑定：`ControllerInput` 的三个 InputActionReference 指到 XRI 默认
   `XRI Default Input Actions` 里的 Trigger / Grip / Thumbstick（单击/双击/长按由脚本自己算）。

## 6. UI 要「悬浮在空间里」

所有界面 Canvas 设为 **Render Mode = World Space**，摆到玩家前方约 1.5~2m、加 **Tracked Device Graphic Raycaster**，
这样 UI 就是浮在真实办公室里的 2D 卡通图层（符合需求「悬浮在空中不贴合实景平面」）。
World Space Canvas 缩放很小（如 0.001），字号才正常。

## 7. 手柄/手部永远在最上层

需求要「手部实景永远渲染在 UI 图层最上层」。做法：
- 手柄模型放单独 Layer，用一个 **Overlay 相机**最后渲染，或把手柄材质 Render Queue 调高；
- 简易做法：手柄射线端点/手柄模型的 Sorting 放最后，保证不被 World Space UI 遮住。

## 8. 真机调试

- Quest 3 开开发者模式，USB 连电脑，`Build And Run` 直接部署 apk；
- 或用 **Meta Quest Link + Meta XR Simulator / Play Mode over Link** 在编辑器里点 Play 直接在头显里预览（省去反复打包）。

---

## 各章 MR 要点回顾（配合 docs/需求文档.md）

- **第一/二/三章共用**：第 4 步的透视相机 + 手柄射线是全程底层，切场景不动它。
- **第二章漫游**：`RoamingLocomotion` 用摇杆平滑移动；物理行走靠 Quest 自身 6DoF，无需代码。
- **第三章打击**：圆环、进度条、判定都是 World Space UI；命中圆环时把归一化半径写入
  `RhythmConductor.lastHitRadius`，判定交给 `HitRingJudge`。
- **绘制窗口 / 图鉴**：全屏 World Space 弹窗，射线当画笔/翻页，脚本已就绪。
