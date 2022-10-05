using ArtStreamHelper.Core.Services;
using System;

namespace ArtStreamHelper.WinUI.Services;

public class PlatformServices : IPlatformServices
{
    public void BeginInvokeOnMainThread(Action action)
    {
        App.Window.DispatcherQueue.TryEnqueue(() => action());
    }
}