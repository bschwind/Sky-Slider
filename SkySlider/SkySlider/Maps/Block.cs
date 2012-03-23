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
        public byte RotationAxis;   //0 = PositiveX
                                    //1 = PositiveZ
                                    //2 = NegativeX
                                    //3 = NegativeZ
                                    //4 = PositiveY
                                    //5 = NegativeY

        public byte Rotation;       //0 = 0 degrees
                                    //1 = 90 degrees counter-clockwise
                                    //2 = 180 degrees counter-clockwise
                                    //3 = 270 degrees counter-clockwise
    }
}
