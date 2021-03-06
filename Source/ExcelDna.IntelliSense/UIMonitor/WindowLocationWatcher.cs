﻿using System;
using System.Threading;

namespace ExcelDna.IntelliSense
{
    public class WindowLocationWatcher : IDisposable
    {
        IntPtr _hWnd;
        SynchronizationContext _syncContextAuto;
        WinEventHook _windowLocationChangeHook;

        public event EventHandler LocationChanged;

        // NOTE: An earlier attempt was to monitor LOCATIONCHANGE only between EVENT_SYSTEM_MOVESIZESTART and EVENT_SYSTEM_MOVESIZEEND
        //       This nearly worked, and meant we were watching many fewer events ...
        //       ...but we missed some of the resizing events for the window, leaving our tooltip stranded.
        //       So until we can find a workaround for that (perhaps a timer would work fine for this), we watch all the LOCATIONCHANGE events.
        public WindowLocationWatcher(IntPtr hWnd, SynchronizationContext syncContextAuto)
        {
            _hWnd = hWnd;
            _syncContextAuto = syncContextAuto;
            _windowLocationChangeHook = new WinEventHook(WinEventHook.WinEvent.EVENT_OBJECT_LOCATIONCHANGE, WinEventHook.WinEvent.EVENT_OBJECT_LOCATIONCHANGE, _syncContextAuto, _hWnd);
            _windowLocationChangeHook.WinEventReceived += _windowLocationChangeHook_WinEventReceived;
        }

        void _windowLocationChangeHook_WinEventReceived(object sender, WinEventHook.WinEventArgs winEventArgs)
        {
#if DEBUG
            Logger.WinEvents.Verbose($"{winEventArgs.EventType} - Window {winEventArgs.WindowHandle:X} ({Win32Helper.GetClassName(winEventArgs.WindowHandle)} - Object/Child {winEventArgs.ObjectId} / {winEventArgs.ChildId} - Thread {winEventArgs.EventThreadId} at {winEventArgs.EventTimeMs}");
#endif
            LocationChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (_windowLocationChangeHook != null)
            {
                _windowLocationChangeHook.Dispose();
                _windowLocationChangeHook = null;
            }
        }
    }
}
