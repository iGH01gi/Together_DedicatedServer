
using System;

struct JobTimerElem : IComparable<JobTimerElem>
{
    public int execTick; //실행 시간
    public Action action;

    public int CompareTo(JobTimerElem other)
    {
        return other.execTick - execTick;
    }
}

public class JobTimer
{
    PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
    object _lock = new object();
    
    public static JobTimer Instance { get; } = new JobTimer();
    
    public void Push(Action action, int tickAfter = 0)
    {
        JobTimerElem job;
        job.execTick = System.Environment.TickCount + tickAfter;
        job.action = action;

        lock (_lock)
        {
            _pq.Push(job);
        }
    } 
    
    /// <summary>
    /// Flush를 호출하면, 현재 시간을 체크해서 실행할 시간이 된 애들은 전부 실행시켜줌
    /// </summary>
    public void Flush()
    {
        while (true)
        {
            int now = System.Environment.TickCount;
            JobTimerElem job;
            lock (_lock)
            {
                if (_pq.Count == 0)
                    break;

                job = _pq.Peek();
                if (job.execTick > now)
                    break;

                _pq.Pop();
            }

            job.action.Invoke();
        }
    }
}