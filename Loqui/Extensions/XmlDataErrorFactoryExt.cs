using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

#nullable enable

namespace Loqui.Extensions
{
    public class XmlDataError : Exception
    {
        public readonly XObject obj;

        public XmlDataError(XObject obj, string message) : base($"{XObjectToMessage(obj)}: {message}")
        {
            this.obj = obj;
        }

        public XmlDataError(XObject obj) : base(XObjectToMessage(obj))
        {
            this.obj = obj;
        }

        public XmlDataError(XObject obj, string message, Exception inner) : base($"{XObjectToMessage(obj)}: {message}", inner)
        {
            this.obj = obj;
        }

        public XmlDataError(XObject obj, Exception inner) : base(XObjectToMessage(obj), inner)
        {
            this.obj = obj;
        }

        public static string XObjectToMessage(XObject obj) => $"{obj.BaseUri} {obj.DescribePath()}";
    }

    public static class XExtensions
    {
        public static string DescribePath(this XObject obj) => obj switch
        {
            XDocument => "/",
            XElement elm => elm.GetAbsoluteXPath(),
            XAttribute attr => attr.DescribePath(),
            XText text => text.DescribePath(),
            _ => throw new NotImplementedException(),
        };

        public static string DescribePath(this XAttribute attr) => $"{attr.Parent?.GetAbsoluteXPath() ?? "unattached"}.{attr.Name}";

        public static string DescribePath(this XText attr) => $"text within {attr.Parent?.GetAbsoluteXPath() ?? "unattached"}";

        // the following from stack exchange

        /// <summary>
        /// Get the absolute XPath to a given XElement
        /// (e.g. "/people/person[6]/name[1]/last[1]").
        /// </summary>
        public static string GetAbsoluteXPath(this XElement element)
        {
            static string relativeXPath(XElement e)
            {
                int index = e.IndexPosition();
                string name = e.Name.LocalName;

                // If the element is the root, no index is required

                return (index == -1) ? "/" + name : string.Format
                (
                    "/{0}[{1}]",
                    name,
                    index.ToString()
                );
            }

            var ancestors = from e in element.Ancestors()
                            select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) +
                   relativeXPath(element);
        }

        /// <summary>
        /// Get the index of the given XElement relative to its
        /// siblings with identical names. If the given element is
        /// the root, -1 is returned.
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        public static int IndexPosition(this XElement element)
        {
            if (element.Parent == null)
            {
                return -1;
            }

            int i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }

                i++;
            }

            throw new InvalidOperationException
                ("element has been removed from its parent.");
        }
    }
}
