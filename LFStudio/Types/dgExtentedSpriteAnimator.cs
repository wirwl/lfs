using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LFStudio.Types
{
    public class dgExtentedSpriteAnimator
    {
        public int NFrame { get; set; }
        public int State { get; set; }
        public int Wait { get; set; }
        public int Next { get; set; }
        public int Hit_a { get; set; }
        public int Hit_j { get; set; }
        public int Hit_d { get; set; }
        public string Comment { get; set; }
        public dgExtentedSpriteAnimator()
        {

        }
    }
}