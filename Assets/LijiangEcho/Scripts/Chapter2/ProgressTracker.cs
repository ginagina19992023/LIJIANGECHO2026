using UnityEngine;

namespace LijiangEcho.Chapter2
{
    /// <summary>
    /// 纹样绘制进度（0/3 ~ 3/3），本地缓存，重启不重置。
    /// 对应需求「4.1 进度存储」与角落常驻进度文字。
    /// </summary>
    public class ProgressTracker : MonoBehaviour
    {
        public static ProgressTracker Instance { get; private set; }

        private const string Key = "lje_pattern_progress";
        public int Total = 3;

        public int Completed { get; private set; }

        public delegate void ProgressChanged(int completed, int total);
        public event ProgressChanged OnProgressChanged;

        /// <summary>三处纹样是否已完成的标记，避免重复计入。</summary>
        private bool[] _done;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            _done = new bool[Total];
            Completed = PlayerPrefs.GetInt(Key, 0);
        }

        public bool IsDone(int index) => index >= 0 && index < _done.Length && _done[index];

        /// <summary>标记某处纹样完成，进度 +1 并持久化。</summary>
        public void MarkDone(int index)
        {
            if (index < 0 || index >= _done.Length || _done[index]) return;
            _done[index] = true;
            Completed = Mathf.Min(Completed + 1, Total);
            PlayerPrefs.SetInt(Key, Completed);
            PlayerPrefs.Save();
            OnProgressChanged?.Invoke(Completed, Total);
        }

        public bool AllDone => Completed >= Total;

        /// <summary>进入新一轮打击关卡穿插绘制时可调用（不清持久化，仅重置本轮标记）。</summary>
        public void ResetRuntime()
        {
            for (int i = 0; i < _done.Length; i++) _done[i] = false;
        }
    }
}
