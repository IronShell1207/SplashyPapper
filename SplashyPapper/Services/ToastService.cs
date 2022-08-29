using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplashyPapper.Services;

public sealed class ToastService
{
    public static event Action<ToastService> ToastRequested;

    public string Message
    {
        get;
        set;
    }

    public TimeSpan Duration
    {
        get;
        set;
    }

    public ToastService(string message, TimeSpan duration)
    {
        Message = message;
        Duration = duration;
    }

    public void RequestToast()
    {
        ToastRequested?.Invoke(this);
    }
}