using System;
using System.Collections;
using System.Collections.Generic;
using HuntAndPeck.Models;

namespace HuntAndPeck.Services.Interfaces
{
    /// <summary>
    /// Provides hints for the entire desktop or a given window handle
    /// </summary>
    public interface IHintProviderService
    {
        /// <summary>
        /// Enumerate the available hints for the current foreground window
        /// </summary>
        /// <returns>The hint session containing the available hints or null if there is no foreground window</returns>
        IEnumerable<Hint> EnumHints(IntPtr hWin);

        void Invalidate(IntPtr hWin);
    }
}
