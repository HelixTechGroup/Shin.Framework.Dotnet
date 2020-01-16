namespace Shin.Framework.Extensions
{
    public static class UintExtensions
    {
        #region Methods
        public static ushort Low(this uint dword)
        {
            return (ushort)dword;
        }

        public static uint WithLow(this uint dword, ushort low16)
        {
            return dword & 4294901760U | low16;
        }

        public static ushort High(this uint dword)
        {
            return (ushort)(dword >> 16);
        }

        public static uint WithHigh(this uint dword, ushort high16)
        {
            return (uint)high16 << 16 | dword.LowAsUInt();
        }

        public static uint LowAsUInt(this uint dword)
        {
            return dword & ushort.MaxValue;
        }

        public static uint HighAsUInt(this uint dword)
        {
            return dword >> 16 & ushort.MaxValue;
        }
        #endregion
    }
}