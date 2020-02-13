using HuntAndPeck.Extensions;
using HuntAndPeck.Models;
using HuntAndPeck.NativeMethods;
using HuntAndPeck.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HuntAndPeck.Services
{
    public class ForegroundAppCachingService : IHintProviderService
    {
        private readonly IHintProviderService hintProviderService;
        Thread _workerThread;

        ConcurrentDictionary<IntPtr, (DateTime date, List<Hint> hints)> processHintCache = new ConcurrentDictionary<IntPtr, (DateTime, List<Hint>)>();

        public ForegroundAppCachingService(IHintProviderService hintProviderService)
        {
            this.hintProviderService = hintProviderService;
        }

        public void Start()
        {
            _workerThread = new Thread(Run);
            _workerThread.Start();
        }

        private void Run()
        {
            var windows = this.GetOpenWindows();

            foreach (var window in windows)
            {
                Task.Run(() =>
                    {
                        if (window.Key != IntPtr.Zero)
                        {
                            UpdateCache(window.Key);
                        }
                    });
            }

            Thread.Sleep(4000);
        }

        private List<Hint> UpdateCache(IntPtr hWin)
        {
            (DateTime Now, List<Hint>) values;
            try
            {
                values = (DateTime.Now, this.hintProviderService.EnumHints(hWin).ToList());

            }
            catch (Exception)
            {
                values = (DateTime.Now, new List<Hint>());
            }

            processHintCache.AddOrUpdate(hWin, values, (_, __) => values);
            return values.Item2;
        }

        public IEnumerable<Hint> EnumHints(IntPtr hWin)
        {
            if (processHintCache.TryGetValue(hWin, out var val))
            {
                if (val.date < DateTime.Now.Subtract(TimeSpan.FromSeconds(10)))
                {
                    return UpdateCache(hWin);
                }

                return val.hints;
            }

            return UpdateCache(hWin);
        }
        public void Invalidate(IntPtr hWin)
        {
            this.processHintCache.TryRemove(hWin, out var _);
            UpdateCache(hWin);
        }

        private IDictionary<IntPtr, string> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;

                int length = GetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder builder = new StringBuilder(length);
                GetWindowText(hWnd, builder, length + 1);

                windows[hWnd] = builder.ToString();
                return true;

            }, 0);

            return windows;
        }


        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetShellWindow();

    }
}
