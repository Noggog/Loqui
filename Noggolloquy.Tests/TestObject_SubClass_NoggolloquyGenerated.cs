/*
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * Autogenerated by Noggolloquy.  Do not manually change.
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Noggolloquy;
using Noggog;
using Noggog.Notifying;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Noggog.Xml;
using Noggolloquy.Xml;

namespace Noggolloquy.Tests
{
    #region Class
    public partial class TestObject_SubClass : TestObject, ITestObject_SubClass, INoggolloquyObjectSetter, IEquatable<TestObject_SubClass>
    {
        INoggolloquyRegistration INoggolloquyObject.Registration => TestObject_SubClass_Registration.Instance;
        public static TestObject_SubClass_Registration Registration => TestObject_SubClass_Registration.Instance;

        public TestObject_SubClass()
        {
            CustomCtor();
        }
        partial void CustomCtor();
        #region NewField
        protected readonly INotifyingItem<Boolean> _NewField = new NotifyingItem<Boolean>(
            default(Boolean),
            markAsSet: false
        );
        public INotifyingItem<Boolean> NewField_Property => _NewField;
        public Boolean NewField { get { return _NewField.Value; } set { _NewField.Value = value; } }
        INotifyingItem<Boolean> ITestObject_SubClass.NewField_Property => this.NewField_Property;
        INotifyingItemGetter<Boolean> ITestObject_SubClassGetter.NewField_Property => this.NewField_Property;
        #endregion


        #region Noggolloquy Getter Interface

        public override object GetNthObject(ushort index) => TestObject_SubClassCommon.GetNthObject(index, this);

        public override bool GetNthObjectHasBeenSet(ushort index) => TestObject_SubClassCommon.GetNthObjectHasBeenSet(index, this);

        public override void SetNthObject(ushort index, object obj, NotifyingFireParameters? cmds) => TestObject_SubClassCommon.SetNthObject(index, this, obj, cmds);

        public override void UnsetNthObject(ushort index, NotifyingUnsetParameters? cmds) => TestObject_SubClassCommon.UnsetNthObject(index, this, cmds);

        #endregion

        #region Noggolloquy Interface
        public override void SetNthObjectHasBeenSet(ushort index, bool on)
        {
            TestObject_SubClassCommon.SetNthObjectHasBeenSet(index, on, this);
        }

        public void CopyFieldsFrom(ITestObject_SubClassGetter rhs, ITestObject_SubClassGetter def = null, NotifyingFireParameters? cmds = null)
        {
            TestObject_SubClassCommon.CopyFieldsFrom(this, rhs, def, null, cmds);
        }

        public void CopyFieldsFrom(ITestObject_SubClassGetter rhs, out TestObject_SubClass_ErrorMask errorMask, ITestObject_SubClassGetter def = null, NotifyingFireParameters? cmds = null)
        {
            var retErrorMask = new TestObject_SubClass_ErrorMask();
            errorMask = retErrorMask;
            TestObject_SubClassCommon.CopyFieldsFrom(this, rhs, def, retErrorMask, cmds);
        }

        #endregion

        #region To String
        public override string ToString()
        {
            return this.PrintPretty();
        }
        #endregion

        #region Equals and Hash
        public override bool Equals(object obj)
        {
            TestObject_SubClass rhs = obj as TestObject_SubClass;
            if (rhs == null) return false;
            return Equals(obj);
        }

        public bool Equals(TestObject_SubClass rhs)
        {
            if (!object.Equals(this.NewField, rhs.NewField)) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return 
            HashHelper.GetHashCode(NewField)
            ;
        }

        #endregion

        #region Set To
        public void SetTo(TestObject_SubClass rhs, ITestObject_SubClass def = null, NotifyingFireParameters? cmds = null)
        {
            SetTo_Internal(rhs, def, null, cmds);
        }

        public void SetTo(TestObject_SubClass rhs, ITestObject_SubClass def, out TestObject_SubClass_ErrorMask errorMask, NotifyingFireParameters? cmds = null)
        {
            var retErrorMask = new TestObject_SubClass_ErrorMask();
            errorMask = retErrorMask;
            SetTo_Internal(rhs, def, retErrorMask, cmds);
        }

        private void SetTo_Internal(TestObject_SubClass rhs, ITestObject_SubClass def, TestObject_SubClass_ErrorMask errorMask, NotifyingFireParameters? cmds)
        {
            try
            {
                if (rhs.NewField_Property.HasBeenSet)
                {
                    this.NewField_Property.Set(
                        rhs.NewField,
                        cmds);
                }
                else
                {
                    if (def == null)
                    {
                        this.NewField_Property.Unset(cmds.ToUnsetParams());
                    }
                    else
                    {
                        this.NewField_Property.Set(
                            def.NewField,
                            cmds);
                    }
                }

            }
            catch (Exception ex)
            {
                if (errorMask != null)
                {
                    errorMask.SetNthException(46, ex);
                }
            }
        }
        #endregion
        #region XML Translation
        public static TestObject_SubClass CreateFromXML(XElement root)
        {
            var ret = new TestObject_SubClass();
            NoggXmlTranslation<TestObject_SubClass, TestObject_SubClass_ErrorMask>.Instance.CopyIn(
                root: root,
                item: ret,
                skipReadonly: false,
                doMasks: false,
                mask: out TestObject_SubClass_ErrorMask errorMask,
                cmds: null);
            return ret;
        }

        public static TestObject_SubClass CreateFromXML(XElement root, out TestObject_SubClass_ErrorMask errorMask)
        {
            var ret = new TestObject_SubClass();
            NoggXmlTranslation<TestObject_SubClass, TestObject_SubClass_ErrorMask>.Instance.CopyIn(
                root: root,
                item: ret,
                skipReadonly: false,
                doMasks: true,
                mask: out errorMask,
                cmds: null);
            return ret;
        }

        public override void CopyInFromXML(XElement root, NotifyingFireParameters? cmds = null)
        {
            NoggXmlTranslation<TestObject_SubClass, TestObject_SubClass_ErrorMask>.Instance.CopyIn(
                root: root,
                item: this,
                skipReadonly: true,
                doMasks: false,
                mask: out TestObject_SubClass_ErrorMask errorMask,
                cmds: cmds);
        }

        public virtual void CopyInFromXML(XElement root, out TestObject_SubClass_ErrorMask errorMask, NotifyingFireParameters? cmds = null)
        {
            NoggXmlTranslation<TestObject_SubClass, TestObject_SubClass_ErrorMask>.Instance.CopyIn(
                root: root,
                item: this,
                skipReadonly: true,
                doMasks: true,
                mask: out errorMask,
                cmds: cmds);
        }

        public override void CopyInFromXML(XElement root, out TestObject_ErrorMask errorMask, NotifyingFireParameters? cmds = null)
        {
            CopyInFromXML(root, out TestObject_SubClass_ErrorMask errMask, cmds: cmds);
            errorMask = errMask;
        }

        #endregion
        #region Mask
        #endregion
        void ICopyInAble.CopyFieldsFrom(object rhs, object def, NotifyingFireParameters? cmds)
        {
            this.CopyFieldsFrom_Generic(rhs, def, cmds);
        }

        protected override void CopyFieldsFrom_Generic(object rhs, object def, NotifyingFireParameters? cmds)
        {
            base.CopyFieldsFrom_Generic(rhs, def, cmds);
            if (rhs is TestObject_SubClass rhsCast)
            {
                this.CopyFieldsFrom(rhsCast, def as TestObject_SubClass, cmds);
            }
        }

        public TestObject_SubClass Copy(ITestObject_SubClassGetter def = null)
        {
            return Copy(this, def: def);
        }

        public static TestObject_SubClass Copy(ITestObject_SubClassGetter item, ITestObject_SubClassGetter def = null)
        {
            var ret = new TestObject_SubClass();
            ret.CopyFieldsFrom(item, def);
            return ret;
        }

        public override void Clear(NotifyingUnsetParameters? cmds = null)
        {
            base.Clear(cmds);
            this.NewField_Property.Unset(cmds.ToUnsetParams());
        }

        public static TestObject_SubClass Create(IEnumerable<KeyValuePair<ushort, object>> fields)
        {
            var ret = new TestObject_SubClass();
            INoggolloquyObjectExt.CopyFieldsIn(ret, fields, def: null, skipReadonly: false, cmds: null);
            return ret;
        }

        public static void CopyIn(IEnumerable<KeyValuePair<ushort, object>> fields, TestObject_SubClass obj)
        {
            INoggolloquyObjectExt.CopyFieldsIn(obj, fields, def: null, skipReadonly: false, cmds: null);
        }

    }
    #endregion

    #region Interface
    public interface ITestObject_SubClass : ITestObject_SubClassGetter, ITestObject, INoggolloquyClass<ITestObject_SubClass, ITestObject_SubClassGetter>, INoggolloquyClass<TestObject_SubClass, ITestObject_SubClassGetter>
    {
        new Boolean NewField { get; set; }
        new INotifyingItem<Boolean> NewField_Property { get; }

    }

    public interface ITestObject_SubClassGetter : ITestObjectGetter
    {
        #region NewField
        Boolean NewField { get; }
        INotifyingItemGetter<Boolean> NewField_Property { get; }

        #endregion


        #region XML Translation
        #endregion
        #region Mask
        #endregion
    }

    #endregion

    #region Registration
    public class TestObject_SubClass_Registration : INoggolloquyRegistration
    {
        public static readonly TestObject_SubClass_Registration Instance = new TestObject_SubClass_Registration();

        public static ProtocolDefinition ProtocolDefinition => ProtocolDefinition_NoggolloquyTests.Definition;

        public static readonly ObjectKey ObjectKey = new ObjectKey(
            protocolKey: ProtocolDefinition_NoggolloquyTests.ProtocolKey,
            msgID: 4,
            version: 0);

        public const string GUID = "3c0cceee-3747-449d-ae3e-617f5c366ef7";

        public const ushort FieldCount = 1;

        public static readonly Type MaskType = typeof(TestObject_SubClass_Mask<>);

        public static readonly Type ErrorMaskType = typeof(TestObject_SubClass_ErrorMask);

        public static readonly Type ClassType = typeof(TestObject_SubClass);

        public const string FullName = "Noggolloquy.Tests.TestObject_SubClass";

        public const string Name = "TestObject_SubClass";

        public const byte GenericCount = 0;

        public static readonly Type GenericRegistrationType = null;

        public static ushort? GetNameIndex(StringCaseAgnostic str)
        {
            switch (str.Upper)
            {
                case "NEWFIELD":
                    return 0;
                default:
                    throw new ArgumentException($"Queried unknown field: {str}");
            }
        }

        public static bool GetNthIsEnumerable(ushort index)
        {
            switch (index)
            {
                case 46:
                    return false;
                default:
                    return TestObject_Registration.GetNthIsEnumerable(index);
            }
        }

        public static bool GetNthIsNoggolloquy(ushort index)
        {
            switch (index)
            {
                case 46:
                    return false;
                default:
                    return TestObject_Registration.GetNthIsNoggolloquy(index);
            }
        }

        public static bool GetNthIsSingleton(ushort index)
        {
            switch (index)
            {
                default:
                    return TestObject_Registration.GetNthIsSingleton(index);
            }
        }

        public static string GetNthName(ushort index)
        {
            switch (index)
            {
                case 46:
                    return "NewField";
                default:
                    return TestObject_Registration.GetNthName(index);
            }
        }

        public static bool IsNthDerivative(ushort index)
        {
            switch (index)
            {
                case 46:
                    return false;
                default:
                    return TestObject_Registration.IsNthDerivative(index);
            }
        }

        public static bool IsReadOnly(ushort index)
        {
            switch (index)
            {
                case 46:
                    return false;
                default:
                    return TestObject_Registration.IsReadOnly(index);
            }
        }

        public static Type GetNthType(ushort index)
        {
            switch (index)
            {
                case 46:
                    return typeof(Boolean);
                default:
                    return TestObject_Registration.GetNthType(index);
            }
        }

        #region Interface
        ProtocolDefinition INoggolloquyRegistration.ProtocolDefinition => ProtocolDefinition;
        ObjectKey INoggolloquyRegistration.ObjectKey => ObjectKey;
        string INoggolloquyRegistration.GUID => GUID;
        int INoggolloquyRegistration.FieldCount => FieldCount;
        Type INoggolloquyRegistration.MaskType => MaskType;
        Type INoggolloquyRegistration.ErrorMaskType => ErrorMaskType;
        Type INoggolloquyRegistration.ClassType => ClassType;
        string INoggolloquyRegistration.FullName => FullName;
        string INoggolloquyRegistration.Name => Name;
        byte INoggolloquyRegistration.GenericCount => GenericCount;
        Type INoggolloquyRegistration.GenericRegistrationType => GenericRegistrationType;
        ushort? INoggolloquyRegistration.GetNameIndex(StringCaseAgnostic name) => GetNameIndex(name);
        bool INoggolloquyRegistration.GetNthIsEnumerable(ushort index) => GetNthIsEnumerable(index);
        bool INoggolloquyRegistration.GetNthIsNoggolloquy(ushort index) => GetNthIsNoggolloquy(index);
        bool INoggolloquyRegistration.GetNthIsSingleton(ushort index) => GetNthIsSingleton(index);
        string INoggolloquyRegistration.GetNthName(ushort index) => GetNthName(index);
        bool INoggolloquyRegistration.IsNthDerivative(ushort index) => IsNthDerivative(index);
        bool INoggolloquyRegistration.IsReadOnly(ushort index) => IsReadOnly(index);
        Type INoggolloquyRegistration.GetNthType(ushort index) => GetNthType(index);
        #endregion
    }
    #endregion
    #region Extensions
    public static class TestObject_SubClassCommon
    {
        #region Copy Fields From
        public static void CopyFieldsFrom(ITestObject_SubClass item, ITestObject_SubClassGetter rhs, ITestObject_SubClassGetter def, TestObject_SubClass_ErrorMask errorMask, NotifyingFireParameters? cmds)
        {
            TestObjectCommon.CopyFieldsFrom(item, rhs, def, errorMask, cmds);
            try
            {
                if (rhs.NewField_Property.HasBeenSet)
                {
                    item.NewField_Property.Set(
                        rhs.NewField,
                        cmds);
                }
                else
                {
                    if (def == null)
                    {
                        item.NewField_Property.Unset(cmds.ToUnsetParams());
                    }
                    else
                    {
                        item.NewField_Property.Set(
                            def.NewField,
                            cmds);
                    }
                }

            }
            catch (Exception ex)
            {
                if (errorMask != null)
                {
                    errorMask.SetNthException(46, ex);
                }
            }
        }

        #endregion

        public static void SetNthObjectHasBeenSet(ushort index, bool on, ITestObject_SubClass obj, NotifyingFireParameters? cmds = null)
        {
            switch (index)
            {
                case 46:
                    obj.NewField_Property.HasBeenSet = on;
                    break;
                default:
                    TestObjectCommon.SetNthObjectHasBeenSet(index, on, obj);
                    break;
            }
        }

        public static void UnsetNthObject(ushort index, ITestObject_SubClass obj, NotifyingUnsetParameters? cmds = null)
        {
            switch (index)
            {
                case 46:
                    obj.NewField_Property.Unset(cmds);
                    break;
                default:
                    TestObjectCommon.UnsetNthObject(index, obj);
                    break;
            }
        }

        public static bool GetNthObjectHasBeenSet(ushort index, ITestObject_SubClass obj)
        {
            switch (index)
            {
                case 46:
                    return obj.NewField_Property.HasBeenSet;
                default:
                    return TestObjectCommon.GetNthObjectHasBeenSet(index, obj);
            }
        }

        public static object GetNthObject(ushort index, ITestObject_SubClassGetter obj)
        {
            switch (index)
            {
                case 46:
                    return obj.NewField;
                default:
                    return TestObjectCommon.GetNthObject(index, obj);
            }
        }

        public static void SetNthObject(ushort index, ITestObject_SubClass nog, object obj, NotifyingFireParameters? cmds = null)
        {
            switch (index)
            {
                case 46:
                    nog.NewField_Property.Set(
                        ((Boolean)obj),
                        cmds);
                    break;
                default:
                    TestObjectCommon.SetNthObject(index, nog, obj);
                    break;
            }
        }

    }
    public static class TestObject_SubClassExt
    {
        public static TestObject_SubClass Copy_ToNoggolloquy(this ITestObject_SubClassGetter item)
        {
            return TestObject_SubClass.Copy(item, def: null);
        }

    }
    #endregion

    #region Modules
    #region XML Translation
    #endregion

    #region Mask
    public class TestObject_SubClass_Mask<T>  : TestObject_Mask<T>
    {
        public T NewField;
    }

    public class TestObject_SubClass_ErrorMask : TestObject_ErrorMask
    {
        public Exception NewField;

        public override void SetNthException(ushort index, Exception ex)
        {
            switch (index)
            {
                case 46:
                    this.NewField = ex;
                    break;
                default:
                    base.SetNthException(index, ex);
                    break;
            }
        }

        public override void SetNthMask(ushort index, object obj)
        {
            switch (index)
            {
                case 46:
                    this.NewField = (Exception)obj;
                    break;
                default:
                    base.SetNthMask(index, obj);
                    break;
            }
        }
    }
    #endregion

    #endregion

    #region Noggolloquy Interfaces
    #endregion

}