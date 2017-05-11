using Noggog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Loqui
{
    public class Net
    {
        public interface IHook
        {
            void PullHook();
            void PushHook();
            void DefaultHook();
            void SaveHook();
            void RestoreHook();
            bool IsModified();
        }

        public class Hook<HT, CT> : IHook
        {
            public HT Default;
            public HT SaveItem;
            public Action<HT> HookSetter;
            public Func<HT> HookGetter;
            public Action<CT> ChainSetter;
            public Func<CT> ChainGetter;
            public Func<HT, CT> PullConverter;
            public Func<CT, HT> PushConverter;

            public void PullHook()
            {
                ChainSetter(PullConverter(HookGetter()));
            }

            public void PushHook()
            {
                HookSetter(PushConverter(ChainGetter()));
            }

            public void DefaultHook()
            {
                HookSetter(Default);
            }

            public void SaveHook()
            {
                SaveItem = HookGetter();
            }

            public void RestoreHook()
            {
                HookSetter(SaveItem);
            }

            public bool IsModified()
            {
                return !object.Equals(HookGetter(), PushConverter(ChainGetter()));
            }
        }

        private List<IHook> hooks = new List<IHook>();

        #region Add Hooks
        public void AddHook(IHook hook)
        {
            this.hooks.Add(hook);
        }

        public void AddHook<H, T, C>(
            H hookedItem,
            MemberAccessor<H, T> hookMember,
            C chainItem,
            MemberAccessor<C, T> chainMember,
            T defaultValue)
        {
            hooks.Add(
                new Hook<T, T>()
                {
                    HookGetter = () =>hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) =>chainMember.Setter(chainItem, t),
                    Default = defaultValue,
                    PullConverter = (t) => t,
                    PushConverter = (t) => t
                });
        }

        public void AddHook<H, T, C>(
            H hookedItem,
            Expression<Func<H, T>> hookExpr,
            C chainItem,
            MemberAccessor<C, T> chainMember,
            T defaultItem)
        {
            MemberAccessor<H, T> hookMember = new MemberAccessor<H, T>(hookExpr);
            hooks.Add(
                new Hook<T, T>()
                {
                    HookGetter = () => hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) => chainMember.Setter(chainItem, t),
                    Default = defaultItem,
                    PullConverter = (t) => t,
                    PushConverter = (t) => t
                });
        }

        public void AddHook<H, T, C>(
            H hookedItem,
            MemberAccessor<H, T> hookMember,
            C chainItem,
            Expression<Func<C, T>> chainExpr,
            T defaultItem)
        {
            MemberAccessor<C, T> chainMember = new MemberAccessor<C, T>(chainExpr);
            hooks.Add(
                new Hook<T, T>()
                {
                    HookGetter = () => hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) => chainMember.Setter(chainItem, t),
                    Default = defaultItem,
                    PullConverter = (t) => t,
                    PushConverter = (t) => t
                });
        }

        public void AddHook<H, T, C>(
            H hookedItem,
            Expression<Func<H, T>> hookExpr,
            C chainItem,
            Expression<Func<C, T>> chainExpr,
            T defaultItem)
        {
            MemberAccessor<H, T> hookMember = new MemberAccessor<H, T>(hookExpr);
            MemberAccessor<C, T> chainMember = new MemberAccessor<C, T>(chainExpr);
            hooks.Add(
                new Hook<T, T>()
                {
                    HookGetter = () => hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) => chainMember.Setter(chainItem, t),
                    Default = defaultItem,
                    PullConverter = (t) => t,
                    PushConverter = (t) => t
                });
        }

        public void AddHook<H, HT, C, CT>(
            H hookedItem,
            MemberAccessor<H, HT> hookMember,
            C chainItem,
            MemberAccessor<C, CT> chainMember,
            Func<HT, CT> pullConverter,
            Func<CT, HT> pushConverter,
            HT defaultItem)
        {
            hooks.Add(
                new Hook<HT, CT>()
                {
                    HookGetter = () => hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) => chainMember.Setter(chainItem, t),
                    Default = defaultItem,
                    PullConverter = pullConverter,
                    PushConverter = pushConverter
                });
        }

        public void AddHook<H, HT, C, CT>(
            H hookedItem,
            Expression<Func<H, HT>> hookExpr,
            C chainItem,
            MemberAccessor<C, CT> chainMember,
            Func<HT, CT> pullConverter,
            Func<CT, HT> pushConverter,
            HT defaultItem)
        {
            MemberAccessor<H, HT> hookMember = new MemberAccessor<H, HT>(hookExpr);
            hooks.Add(
                new Hook<HT, CT>()
                {
                    HookGetter = () => hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) => chainMember.Setter(chainItem, t),
                    Default = defaultItem,
                    PullConverter = pullConverter,
                    PushConverter = pushConverter
                });
        }

        public void AddHook<H, HT, C, CT>(
            H hookedItem,
            MemberAccessor<H, HT> hookMember,
            C chainItem,
            Expression<Func<C, CT>> chainExpr,
            Func<HT, CT> pullConverter,
            Func<CT, HT> pushConverter,
            HT defaultItem)
        {
            MemberAccessor<C, CT> chainMember = new MemberAccessor<C, CT>(chainExpr);
            hooks.Add(
                new Hook<HT, CT>()
                {
                    HookGetter = () => hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) => chainMember.Setter(chainItem, t),
                    Default = defaultItem,
                    PullConverter = pullConverter,
                    PushConverter = pushConverter
                });
        }

        public void AddHook<H, HT, C, CT>(
            H hookedItem,
            Expression<Func<H, HT>> hookExpr,
            C chainItem,
            Expression<Func<C, CT>> chainExpr,
            System.Func<HT, CT> pullConverter,
            Func<CT, HT> pushConverter,
            HT defaultItem)
        {
            MemberAccessor<H, HT> hookMember = new MemberAccessor<H, HT>(hookExpr);
            MemberAccessor<C, CT> chainMember = new MemberAccessor<C, CT>(chainExpr);
            hooks.Add(
                new Hook<HT, CT>()
                {
                    HookGetter = () => hookMember.Getter(hookedItem),
                    HookSetter = (t) => hookMember.Setter(hookedItem, t),
                    ChainGetter = () => chainMember.Getter(chainItem),
                    ChainSetter = (t) => chainMember.Setter(chainItem, t),
                    Default = defaultItem,
                    PullConverter = pullConverter,
                    PushConverter = pushConverter
                });
        }
        #endregion

        public void PushHooks()
        {
            foreach (var hook in hooks.ToList())
            {
                hook.PushHook();
            }
        }

        public void PullHooks()
        {
            foreach (var hook in hooks.ToList())
            {
                hook.PullHook();
            }
        }

        public bool IsModified()
        {
            foreach (var hook in hooks)
            {
                if (hook.IsModified()) return true;
            }
            return false;
        }

        public void Default()
        {
            foreach (var hook in hooks)
            {
                hook.DefaultHook();
            }
        }

        public void RestoreHooks()
        {
            foreach (var hook in hooks)
            {
                hook.RestoreHook();
            }
        }

        public void SaveHooks()
        {
            foreach (var hook in hooks)
            {
                hook.SaveHook();
            }
        }
    }
}
