using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class ElapsedCpuTimer
{
    private long maxTime;
    private long oldTime;
    
    public ElapsedCpuTimer()
    {
        //DisplayTimerProperties();
        
        oldTime = Stopwatch.GetTimestamp();
    }

    public ElapsedCpuTimer copy()
    {
        var newTimer = new ElapsedCpuTimer();
        newTimer.oldTime = this.oldTime;
        newTimer.maxTime = this.maxTime;
        return newTimer;
    }
    
    
    
    public TimeSpan Elapsed
    {
        get
        {
            if (Stopwatch.IsHighResolution)
                return TimeSpan.FromTicks(this.ElapsedTicks / (Stopwatch.Frequency / 10000000L));
            return TimeSpan.FromTicks(this.ElapsedTicks);
        }
    }

    public long ElapsedMilliseconds
    {
        get
        {
            if (Stopwatch.IsHighResolution)
                return this.ElapsedTicks / (Stopwatch.Frequency / 1000L);
            return checked ((long) this.Elapsed.TotalMilliseconds);
        }
    }

    public long ElapsedNanoseconds
    {
        get
        {
            return ElapsedMilliseconds * 1000000L;
        }
    }
    
    public long ElapsedTicks
    {
        get
        {
            return Stopwatch.GetTimestamp() - this.oldTime;
        }
    }

    public override string ToString() {
        return ElapsedMilliseconds + " ms elapsed";
    }

    public void setMaxTimeMilliseconds(long time) {
        maxTime = time * 1000000L;
    }

    public long remainingTimeMilliseconds()
    {
        var maxMilliseconds = (long) (maxTime / 1000000.0); 
        return maxMilliseconds - ElapsedMilliseconds;
    }

    public bool exceededMaxTime()
    {
        return ElapsedNanoseconds > maxTime;
    }
    
    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?redirectedfrom=MSDN&view=netframework-4.7.2
    /// </summary>
    private static void DisplayTimerProperties()
    {
        // Display the timer frequency and resolution.
        if (Stopwatch.IsHighResolution)
        {
            Debug.Log("Operations timed using the system's high-resolution performance counter.");
        }
        else 
        {
            Debug.Log("Operations timed using the DateTime class.");
        }

        long frequency = Stopwatch.Frequency;
        Debug.LogFormat("  Timer frequency in ticks per second = {0}", frequency);
        long nanosecPerTick = (1000L*1000L*1000L) / frequency;
        Debug.LogFormat("  Timer is accurate within {0} nanoseconds", nanosecPerTick);
    }
}