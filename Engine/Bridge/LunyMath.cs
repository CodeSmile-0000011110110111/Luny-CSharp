using System;

namespace Luny.Engine.Bridge
{
	public static class LunyMath
	{
		public static Single CopySign(Single magnitude, Single sign)
		{
			// Reinterprets bits without changing them (No conversion overhead)
			var m = BitConverter.SingleToInt32Bits(magnitude);
			var s = BitConverter.SingleToInt32Bits(sign);

			// Combine absolute magnitude bits with the sign bit
			// 0x7fffffff = 0111... (all bits but sign)
			// 0x80000000 = 1000... (only the sign bit)
			var result = m & 0x7fffffff | s & unchecked((Int32)0x80000000);

			return BitConverter.Int32BitsToSingle(result);
		}
	}
}
