using System;

namespace ArtStreamHelper.Core.Services;

public interface IPlatformServices
{
    void BeginInvokeOnMainThread(Action action);
}