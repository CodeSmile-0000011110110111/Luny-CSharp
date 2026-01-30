using System.Runtime.CompilerServices;

// can't use nameof() because of a Rider issue
[assembly:InternalsVisibleTo("Luny.Godot")]
[assembly:InternalsVisibleTo("Luny.GodotEditor")]
[assembly:InternalsVisibleTo("Luny.Unity")]
[assembly:InternalsVisibleTo("Luny.UnityEditor")]
[assembly:InternalsVisibleTo("Luny-Test")]
[assembly:InternalsVisibleTo("LunyScript")]
[assembly:InternalsVisibleTo("LunyScript-Test")]
[assembly:InternalsVisibleTo("Luny-ContractTest")]

// reserved namespaces for future C# engine implementations
[assembly:InternalsVisibleTo(nameof(Luny) + ".Cocos")] // no C# support (yet)
[assembly:InternalsVisibleTo(nameof(Luny) + ".CocosEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".CryEngine")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".CryEngineEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".Bevy")] // no C# support (yet)
[assembly:InternalsVisibleTo(nameof(Luny) + ".BevyEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".Evergine")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".EvergineEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".Flax")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".FlaxEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".Open3D")] // no C# support (yet)
[assembly:InternalsVisibleTo(nameof(Luny) + ".Open3DEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".Stride")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".StrideEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".Unigine")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".UnigineEditor")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".Unreal")]
[assembly:InternalsVisibleTo(nameof(Luny) + ".UnrealEditor")]
