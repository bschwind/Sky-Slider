using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkySlider.Maps
{
    /// <summary>
    /// Each block on the map will be an instance of this struct.
    /// This struct stores only the block's type and orientation.
    /// All other information about blocks is contained in BlockData.cs
    /// </summary>
    public struct Block
    {
        public byte Type;
    }
}
