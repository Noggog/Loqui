using System.Reactive.Disposables;

namespace Loqui.Internal;

public class ErrorMaskBuilder : IDisposable
{
    public const int OVERALL_INDEX = -2;

    Stack<int>? _depthStack;
    private int? _CurrentIndex;
    public int? CurrentIndex
    {
        get
        {
            if (_CurrentIndex.HasValue)
            {
                return _CurrentIndex.Value;
            }
            if (_depthStack == null) return null;
            if (_depthStack.Count == 0) return null;
            return _depthStack.Peek();
        }
        set => _CurrentIndex = value;
    }
    public List<(int[], Exception)>? Exceptions;
    public List<(int[], string)>? Warnings;
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

    internal int[] GetCurrentStack()
    {
        if (_CurrentIndex.HasValue)
        {
            if (_depthStack == null) throw new NullReferenceException();
            int[] ret = new int[_depthStack.Count + 1];
            _depthStack.CopyTo(ret, 0);
            ret[_depthStack.Count] = _CurrentIndex.Value;
            return ret;
        }
        else
        {
            if (_depthStack == null) throw new NullReferenceException();
            int[] ret = new int[_depthStack.Count];
            _depthStack.CopyTo(ret, 0);
            return ret;
        }
    }

    private int[] GetCurrentStack(int index)
    {
        if (_CurrentIndex.HasValue)
        {
            if (_depthStack == null) throw new NullReferenceException();
            int[] ret = new int[_depthStack.Count + 2];
            _depthStack.CopyTo(ret, 0);
            ret[_depthStack.Count] = _CurrentIndex.Value;
            ret[ret.Length - 1] = index;
            return ret;
        }
        else
        {
            if (_depthStack == null) throw new NullReferenceException();
            int[] ret = new int[_depthStack.Count + 1];
            _depthStack.CopyTo(ret, 0);
            ret[_depthStack.Count] = index;
            return ret;
        }
    }

    public void ReportException(Exception ex)
    {
        if (Exceptions == null)
        {
            Exceptions = new List<(int[], Exception)>();
        }
        Exceptions.Add((
            GetCurrentStack(),
            ex));
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
    public static IDisposable PushIndex(this ErrorMaskBuilder? errorMask, int fieldIndex)
    {
        if (errorMask == null) return Disposable.Empty;
        errorMask.PushIndexInternal(fieldIndex);
        return errorMask;
    }

    public static void ReportExceptionOrThrow(this ErrorMaskBuilder? errorMask, Exception ex)
    {
        if (errorMask == null)
        {
            throw ex;
        }
        errorMask.ReportException(ex);
    }

    public static void ReportWarning(this ErrorMaskBuilder? errorMask, string str)
    {
        if (errorMask == null) return;
        if (errorMask.Warnings == null)
        {
            errorMask.Warnings = new List<(int[], string)>();
        }
        errorMask.Warnings.Add((
            errorMask.GetCurrentStack(),
            str));
    }

    public static void WrapAction(this ErrorMaskBuilder? errorMask, int fieldIndex, Action a)
    {
        if (errorMask == null)
        {
            a();
        }
        else
        {
            try
            {
                errorMask.PushIndex(fieldIndex);
                a();
            }
            catch (Exception ex)
            {
                errorMask.ReportException(ex);
            }
            finally
            {
                errorMask.PopIndex();
            }
        }
    }
}