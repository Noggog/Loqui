﻿using Noggog;
using Loqui.Xml;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Loqui.Xml
{
    public interface IXmlTranslation<T>
    {
        string ElementName { get; }
        void Write(XmlWriter writer, string name, T item, bool doMasks, out object maskObj);
        TryGet<T> Parse(XElement root, bool doMasks, out object maskObj);
    }
}