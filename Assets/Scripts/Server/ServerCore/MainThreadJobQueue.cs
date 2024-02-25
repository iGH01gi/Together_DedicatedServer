using System;
using System.Collections.Generic;

/// <summary>
/// 메인쓰레드에서 처리하도록 일감만 밀어넣고 직접 실행X하기 위한 잡큐
/// </summary>
public class MainThreadJobQueue
{
    public static MainThreadJobQueue Instance { get; } = new MainThreadJobQueue();
    
    Queue<Action> _jobQueue = new Queue<Action>();
    private object _lock = new object();

    /// <summary>
    /// 메인쓰레드에서 처리하도록 일감만 밀어넣고 직접 실행X
    /// </summary>
    /// <param name="job">넣을 일감</param>
    public void Push(Action job)
    {
        lock(_lock)
        {
            _jobQueue.Enqueue(job);
        }
    }
    
    /// <summary>
    /// 무한루프 돌면서 일감 쏙쏙 뽑아서 실행시킴
    /// </summary>
    public void Flush()
    { 
        while (true)
        {
            Action action = Pop();
            if(action==null)
                return;
            
            action.Invoke();
        }
    }
    
    Action Pop()
    {
        lock(_lock)
        {
            if(_jobQueue.Count == 0)
            {
                return null;
            }
            
            return _jobQueue.Dequeue();
        }
    }
    
}