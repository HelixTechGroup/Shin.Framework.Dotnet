using System;
using System.Collections.Generic;
using System.Text;

namespace Shin.Framework.Extensions
{
    public static class IntExtensions
    {
        public static uint ToUint(this int value)
        {
            return Convert.ToUInt32(value);
        }
    }
}
