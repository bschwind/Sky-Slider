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
        private byte rotationAxis;   //0 = PositiveX
                                    //1 = PositiveZ
                                    //2 = NegativeX
                                    //3 = NegativeZ
                                    //4 = PositiveY
                                    //5 = NegativeY

        private byte rotation;       //0 = 0 degrees
                                    //1 = 90 degrees counter-clockwise
                                    //2 = 180 degrees counter-clockwise
                                    //3 = 270 degrees counter-clockwise

        public byte RotationAxis
        {
            get
            {
                return rotationAxis;
            }
            set
            {
                if (value < 0 || value > 5)
                {
                    throw new Exception("Rotation axis must be between 0 and 5 inclusive");
                }

                rotationAxis = value;
            }
        }

        public byte Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                if (value < 0 || value > 3)
                {
                    throw new Exception("Rotation must be between 0 and 3 inclusive");
                }

                rotation = value;
            }
        }
    }
}
