namespace Luny.Engine.Bridge
{
	public static class LunyMath
	{
		public static float CopySign(float magnitude, float sign)
		{
			// Reinterprets bits without changing them (No conversion overhead)
			int m = System.BitConverter.SingleToInt32Bits(magnitude);
			int s = System.BitConverter.SingleToInt32Bits(sign);

			// Combine absolute magnitude bits with the sign bit
			// 0x7fffffff = 0111... (all bits but sign)
			// 0x80000000 = 1000... (only the sign bit)
			int result = (m & 0x7fffffff) | (s & unchecked((int)0x80000000));

			return System.BitConverter.Int32BitsToSingle(result);
		}
	}
}
