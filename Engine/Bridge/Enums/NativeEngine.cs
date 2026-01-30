namespace Luny.Engine.Bridge.Enums
{
	public enum NativeEngine
	{
		None = 0, // Reserved for simulated runs (unit testing)
		Custom = 0xff, // use for custom engine adapters

		// alphabetically sorted, numbered by order of initial implementation likelihood (no guarantees)
		CryEngine = 7,
		Evergine = 9,
		Flax = 5,
		Godot = 1,
		Unigine = 8,
		Unity = 2,
		Unreal = 6,
		Stride = 4,
		TheEngine = 3, // coming soon ... (placeholder name)
	}
}
