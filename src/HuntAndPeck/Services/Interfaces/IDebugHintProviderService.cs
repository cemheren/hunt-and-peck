using System;
using System.Collections;
using System.Collections.Generic;
using HuntAndPeck.Models;

namespace HuntAndPeck.Services.Interfaces
{
    public interface IDebugHintProviderService
    {
        IEnumerable<Hint> EnumDebugHints(IntPtr hWnd);
    }
}
