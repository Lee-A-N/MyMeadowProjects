using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoloPong
{
    public interface ISounds
    {
        void PlayBorderHitSound();

        void PlayPaddleHitSound();

        void PlayGameOverSound();

        void PlayStartSound();
    }
}
