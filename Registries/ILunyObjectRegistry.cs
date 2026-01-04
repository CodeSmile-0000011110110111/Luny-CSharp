using Luny.Proxies;
using System;
using System.Collections.Generic;

namespace Luny.Registries
{
    public interface ILunyObjectRegistry
    {
        Int32 Count { get; }
        IEnumerable<ILunyObject> AllObjects { get; }
        void Register(ILunyObject lunyObject);
        Boolean Unregister(ILunyObject lunyObject);
        Boolean Unregister(LunyID lunyID);
        ILunyObject GetByLunyID(LunyID lunyID);
        ILunyObject GetByNativeID(NativeID nativeID);
        void Dispose();
    }
}
