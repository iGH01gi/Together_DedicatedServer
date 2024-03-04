
using System;
using System.Collections.Generic;

public interface IJobQueue
{
    void Push(Action job);
}

public class JobQueue
{
    Queue<Action> _jobQueue = new Queue<Action>();
    private object _lock = new object();
    bool _flush = false; //큐에다 쌓인거를 내가 실행할 것인지 말지를 결정

    public void Push(Action job)
    {
        bool flush = false;
        lock(_lock)
        {
            _jobQueue.Enqueue(job);
            if(_flush == false) //처음 Push한 애가 일 처리 해주는 방식
            {
                flush=_flush=true;
            }
        }

        if (flush)
            Flush(); 
    }
    
    void Flush()
    { //무한루프 돌면서 일감 쏙쏙 뽑아서 실행시킴
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
                _flush = false;
                return null;
            }
            
            return _jobQueue.Dequeue();
        }
    }
}