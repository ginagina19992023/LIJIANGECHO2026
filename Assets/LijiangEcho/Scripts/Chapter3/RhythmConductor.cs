using System.Collections.Generic;
using UnityEngine;
using LijiangEcho.Core;
using LijiangEcho.Chapter2;

namespace LijiangEcho.Chapter3
{
    /// <summary>
    /// 音乐打击关卡主控制器：以背景音乐播放进度为节拍时钟，按谱面推进音符判定，
    /// 处理单击/双击/长按输入，在 3 个穿插点强制弹出绘制窗口（冻结音符、暂停音乐），
    /// 统计 Miss 与完成度并裁定胜利/失败。对应第三章「二/三/四」。
    /// </summary>
    public class RhythmConductor : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private NoteChart chart;
        [SerializeField] private bool useDefaultChart = true;
        [SerializeField] private AudioSource music;
        [SerializeField] private ControllerInput input;
        [SerializeField] private HitRingJudge ring;
        [SerializeField] private LifeSystem life;
        [SerializeField] private DrawingWindow drawingWindow;   // 复用第二章绘制弹窗
        [SerializeField] private ResultPopup result;
        [SerializeField] private AudioDirector audioDirector;

        [Header("判定")]
        [SerializeField] private float timingWindow = 0.3f;     // ±0.3s

        private List<NoteEntry> _notes;
        private int _nextInterlude;                              // 下一个穿插点索引
        private int _interludeDone;                              // 已完成穿插绘制次数
        private bool _frozen;                                    // 穿插时冻结
        private bool _running;
        private int _hitCount;                                  // 已判定的点击类音符数
        private int _totalScorable;                             // 需判定的音符总数

        // 供打击表现层读取当前命中半径（由射线命中圆环 UI 时写入）
        [HideInInspector] public float lastHitRadius = 0.5f;

        public float SongTime => music != null ? music.time : 0f;

        private void OnEnable()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.OnStateChanged += HandleState;
            if (input != null)
            {
                input.OnSingleTap += () => OnTap(NoteType.Single);
                input.OnDoubleTap += () => OnTap(NoteType.Double);
                input.OnLongPressStart += OnLongStart;
                input.OnLongPressEnd += OnLongEnd;
            }
            if (life != null) life.OnFail += Fail;
        }

        private void OnDisable()
        {
            if (GameFlowManager.Instance != null)
                GameFlowManager.Instance.OnStateChanged -= HandleState;
        }

        private void HandleState(GameState prev, GameState next)
        {
            if (next == GameState.Chapter3Rhythm && prev == GameState.Chapter3Countdown)
                StartLevel();
        }

        public void StartLevel()
        {
            // Prefab 友好化：绘制窗口留空则自动在场景里找（含未激活）
            if (drawingWindow == null) drawingWindow = FindObjectOfType<DrawingWindow>(true);

            _notes = (useDefaultChart || chart == null) ? NoteChart.BuildDefault() : new List<NoteEntry>(chart.notes);
            _nextInterlude = 0;
            _interludeDone = 0;
            _hitCount = 0;
            _totalScorable = CountScorable(_notes);
            _frozen = false;
            _running = true;
            life?.ResetLife();
            if (music != null) { music.time = 0f; music.Play(); }
        }

        private int CountScorable(List<NoteEntry> notes)
        {
            int c = 0;
            foreach (var n in notes) if (n.type != NoteType.Slide) c++;
            return c;
        }

        private void Update()
        {
            if (!_running || _frozen) return;

            // 到达穿插触发点 → 强制弹绘制窗口
            if (chart != null && _nextInterlude < chart.interludeTriggerTimes.Length &&
                SongTime >= chart.interludeTriggerTimes[_nextInterlude])
            {
                TriggerInterlude();
                return;
            }
            // 使用默认谱面时也提供三个穿插点
            else if (chart == null && _nextInterlude < 3)
            {
                float[] def = { 37f, 60f, 103f };
                if (SongTime >= def[_nextInterlude]) { TriggerInterlude(); return; }
            }

            // 音乐播放结束且未失败 → 判定通关
            if (music != null && !music.isPlaying && SongTime > 1f)
                CheckVictory();
        }

        // ---- 打击输入 ----
        private NoteEntry? FindActiveNote(NoteType expected)
        {
            if (_notes == null) return null;
            float t = SongTime;
            NoteEntry? best = null;
            float bestErr = timingWindow;
            foreach (var n in _notes)
            {
                if (n.type == NoteType.Slide) continue;
                float err = Mathf.Abs(n.time - t);
                if (err <= bestErr) { bestErr = err; best = n; }
            }
            return best;
        }

        private void OnTap(NoteType tapType)
        {
            if (!_running || _frozen) return;
            var note = FindActiveNote(tapType);
            Judgment j;
            if (note.HasValue)
            {
                float err = note.Value.time - SongTime;
                // 操作类型需匹配（单击对单音、双击对重拍）
                bool typeOk = note.Value.type == tapType || note.Value.type == NoteType.Long;
                j = typeOk ? ring.Judge(err, lastHitRadius, timingWindow) : Judgment.Miss;
                _hitCount++;
            }
            else
            {
                j = Judgment.Miss;   // 空窗点击算 Miss
            }
            ApplyJudgment(j);
        }

        private void OnLongStart()
        {
            if (!_running || _frozen) return;
            var note = FindActiveNote(NoteType.Long);
            if (note.HasValue && note.Value.type == NoteType.Long)
            {
                // 长按开始填充；结束判定在 OnLongEnd
                audioDirector?.PlayLongHold(true);
            }
        }

        private void OnLongEnd()
        {
            audioDirector?.PlayLongHold(false);
            var note = FindActiveNote(NoteType.Long);
            if (note.HasValue && note.Value.type == NoteType.Long)
            {
                ApplyJudgment(ring.JudgeByRadius(lastHitRadius));
                _hitCount++;
            }
        }

        private void ApplyJudgment(Judgment j)
        {
            audioDirector?.PlayJudgment(j);
            life?.ReportJudgment(j);
        }

        // ---- 穿插绘制 ----
        private void TriggerInterlude()
        {
            _frozen = true;
            if (music != null) music.Pause();
            audioDirector?.DuckMusic(true);
            GameFlowManager.Instance.SetState(GameState.Chapter3Interlude);
            drawingWindow.Open(_interludeDone, OnInterludeComplete, OnInterludeFail);
        }

        private void OnInterludeComplete(int idx)
        {
            _interludeDone++;
            _nextInterlude++;
            _frozen = false;
            audioDirector?.DuckMusic(false);
            GameFlowManager.Instance.SetState(GameState.Chapter3Rhythm);
            if (music != null) music.UnPause();
        }

        private void OnInterludeFail(int idx)
        {
            // 超时/匹配不足 → 直接失败
            Fail();
        }

        // ---- 结算 ----
        private void CheckVictory()
        {
            _running = false;
            bool allInterlude = _interludeDone >= 3;
            bool notOverMiss = life == null || life.TotalMiss < 10;
            if (allInterlude && notOverMiss)
                Win();
            else
                Fail();
        }

        private void Win()
        {
            _running = false;
            if (music != null) music.Stop();
            GameFlowManager.Instance.SetState(GameState.Victory);
            result?.ShowVictory();
        }

        private void Fail()
        {
            if (!_running && GameFlowManager.Instance.Current == GameState.Defeat) return;
            _running = false;
            _frozen = false;
            if (music != null) music.Stop();
            GameFlowManager.Instance.SetState(GameState.Defeat);
            result?.ShowDefeat(RestartLevel);
        }

        /// <summary>失败后自动重置回倒计时。</summary>
        public void RestartLevel()
        {
            GameFlowManager.Instance.SetState(GameState.Chapter3Countdown);
        }
    }
}
