using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Noggolloquy
{
    public interface INoggolloquyWriterSerializer : INoggolloquyObjectGetter, ICopyable
    {
    }

    public interface INoggolloquyReaderSerializer : INoggolloquyObjectSetter, ICopyInAble
    {
    }

    public interface INoggolloquySerializer : INoggolloquyWriterSerializer, INoggolloquyReaderSerializer
    {
    }
}
