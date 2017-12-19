using System;

namespace Loqui.Generation
{
    public struct ObjectNamedKey : IEquatable<ObjectNamedKey>
    {
        public readonly ProtocolKey ProtocolKey;
        public readonly string Name;

        public ObjectNamedKey(
            ProtocolKey protocolKey,
            string name)
        {
            this.ProtocolKey = protocolKey;
            this.Name = name;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ObjectNamedKey)) return false;
            return Equals((ObjectNamedKey)obj);
        }

        public bool Equals(ObjectNamedKey other)
        {
            return this.Name == other.Name
                && this.ProtocolKey.Equals(other.ProtocolKey);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode()
                .CombineHashCode(this.ProtocolKey);
        }

        public override string ToString()
        {
            return $"ObjectNamedKey ({this.ProtocolKey.Namespace}.{this.Name})";
        }

        public static bool TryFactory(string str, ProtocolKey fallbackProtoKey, out ObjectNamedKey objKey)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                objKey = default(ObjectNamedKey);
                return false;
            }
            var split = str.Split('.');
            if (split.Length == 2)
            {
                objKey = new ObjectNamedKey(
                    new ProtocolKey(split[0]),
                    split[1]);
                return true;
            }
            else if (split.Length == 1)
            {
                objKey = new ObjectNamedKey(
                    fallbackProtoKey,
                    split[0]);
                return true;
            }
            objKey = default(ObjectNamedKey);
            return false;
        }

        public static bool TryFactory(string str, out ObjectNamedKey objKey)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                objKey = default(ObjectNamedKey);
                return false;
            }
            var split = str.Split('.');
            if (split.Length == 2)
            {
                objKey = new ObjectNamedKey(
                    new ProtocolKey(split[0]),
                    split[1]);
                return true;
            }
            objKey = default(ObjectNamedKey);
            return false;
        }
    }
}
