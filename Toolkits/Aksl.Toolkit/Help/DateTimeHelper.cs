using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace Aksl.Toolkit;

public static class DateTimeExtensions
{
    public static long ConvertToLong(this DateTime dt)
    {
        DateTime epoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan elapsedTime = dt.ToUniversalTime().Subtract(epoch);
        return elapsedTime.Ticks / 10000;
    }

    public static DateTime ConvertToDateTime(this long timestamp)
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return epoch.AddMilliseconds(timestamp).ToUniversalTime();
    }
}

