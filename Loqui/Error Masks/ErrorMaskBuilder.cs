using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Internal
{
    public class ErrorMaskBuilder : IDisposable
    {
        public const int OVERALL_INDEX = -2;

        Stack<int> _depthStack;
        private int? _CurrentIndex;
        public int? CurrentIndex
        {
            get => _CurrentIndex ?? _depthStack?.Peek();
            set => _CurrentIndex = value;
        }
        public List<(int[], Exception)> Exceptions;
        public List<(int[], string)> Warnings;
        public bool Empty => (Exceptions?.Count ?? 0) == 0 && (Warnings?.Count ?? 0) == 0;

        internal IDisposable PushIndexInternal(int fieldIndex)
        {
            if (!_CurrentIndex.HasValue)
            {
                _CurrentIndex = fieldIndex;
                return this;
            }
            if (_depthStack == null)
            {
                _depthStack = new Stack<int>();
            }
            _depthStack.Push(_CurrentIndex.Value);
            _CurrentIndex = fieldIndex;
            return this;
        }

        private int[] GetCurrentStack()
        {
            if (_CurrentIndex.HasValue)
            {
                int[] ret = new int[_depthStack.Count + 1];
                _depthStack.CopyTo(ret, 0);
                ret[_depthStack.Count] = _CurrentIndex.Value;
                return ret;
            }
            else
            {
                int[] ret = new int[_depthStack.Count];
                _depthStack.CopyTo(ret, 0);
                return ret;
            }
        }

        private int[] GetCurrentStack(int index)
        {
            if (_CurrentIndex.HasValue)
            {
                int[] ret = new int[_depthStack.Count + 2];
                _depthStack.CopyTo(ret, 0);
                ret[_depthStack.Count] = _CurrentIndex.Value;
                ret[ret.Length - 1] = index;
                return ret;
            }
            else
            {
                int[] ret = new int[_depthStack.Count + 1];
                _depthStack.CopyTo(ret, 0);
                ret[_depthStack.Count] = index;
                return ret;
            }
        }

        public void ReportException(Exception ex)
        {
            if (this.Exceptions == null)
            {
                this.Exceptions = new List<(int[], Exception)>();
            }
            this.Exceptions.Add((
                GetCurrentStack(),
                ex));
        }

        public void ReportWarning(string str)
        {
            if (this.Warnings == null)
            {
                this.Warnings = new List<(int[], string)>();
            }
            this.Warnings.Add((
                GetCurrentStack(),
                str));
        }

        public void Dispose()
        {
            PopIndex();
        }

        public void PopIndex()
        {
            if (_CurrentIndex.HasValue)
            {
                _CurrentIndex = null;
            }
            else
            {
                _depthStack?.Pop();
            }
        }
    }

    public static class ErrorMaskBuilderExt
    {
        public static IDisposable PushIndex(this ErrorMaskBuilder errorMask, int fieldIndex)
        {
            if (errorMask == null) return Noggog.IDisposableExt.Nothing;
            errorMask.PushIndexInternal(fieldIndex);
            return errorMask;
        }

        public static void ReportExceptionOrThrow(this ErrorMaskBuilder errorMask, Exception ex)
        {
            if (errorMask == null)
            {
                throw ex;
            }
            errorMask.ReportException(ex);
        }
    }
}
