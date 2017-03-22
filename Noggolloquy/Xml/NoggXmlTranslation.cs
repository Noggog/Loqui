using Noggog;
using Noggog.Notifying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Noggolloquy.Xml
{
    public class NoggXmlTranslation<T, M> : IXmlTranslation<T>
        where T : IXmlTranslator<M>
    {
        public readonly static NoggXmlTranslation<T, M> Instance = new NoggXmlTranslation<T, M>();
        public string ElementName => throw new NotImplementedException();

        public M CopyIn(XElement root, T item, bool parseMask, NotifyingFireParameters? cmds)
        {
            if (parseMask)
            {
                item.CopyInFromXML(root, out M mask, cmds);
                return mask;
            }
            else
            {
                item.CopyInFromXML(root, cmds);
                return default(M);
            }
        }

        public TryGet<(T Nogg, M Mask)> Parse(XElement root, bool parseMask, Func<T> newFunc)
        {
            var ret = newFunc();
            if (parseMask)
            {
                ret.CopyInFromXML(root, out M errorMask, cmds: null);
                return TryGet<(T, M)>.Success(
                    (ret, errorMask));
            }
            else
            {
                ret.CopyInFromXML(root, cmds: null);
                return TryGet<(T, M)>.Success(
                    (ret, default(M)));
            }
        }

        public TryGet<T> Parse(XElement root)
        {
            throw new NotImplementedException();
        }

        public void Write(XmlWriter writer, string name, T item)
        {
            item.WriteXML(writer, name);
        }

        public void Write(XmlWriter writer, string name, T item, bool doMasks, out M mask)
        {
            if (doMasks)
            {
                item.WriteXML(writer, out mask, name);
            }
            else
            {
                item.WriteXML(writer, name);
                mask = default(M);
            }
        }
    }
}
