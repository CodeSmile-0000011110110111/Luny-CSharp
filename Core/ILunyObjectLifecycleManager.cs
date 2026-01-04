using Luny.Proxies;
using System;

namespace Luny.Core
{
    public interface ILunyObjectLifecycleManager
    {
        void EnqueueReady(ILunyObject lunyObject);
        void EnqueueDestroy(ILunyObject lunyObject);
        void ProcessPendingReady();
        void ProcessPendingDestroy();
        void OnObjectEnabled(ILunyObject lunyObject);
        void Dispose();
    }
}
