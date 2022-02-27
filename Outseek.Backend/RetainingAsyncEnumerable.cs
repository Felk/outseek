using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Outseek.Backend;

internal enum State
{
    Initial,
    Processing,
    Finished
}

/// An <see cref="IAsyncEnumerable{T}"/> that retains the produced elements,
/// allowing for repeated iteration and therefore also multiple readers of which everyone receives all elements.
public class RetainingAsyncEnumerable<T> : IAsyncEnumerable<T>
{
    private State _state = State.Initial;
    private readonly IAsyncEnumerable<T> _asyncEnumerable;
    private readonly List<T> _bufferedResult = new();

    public RetainingAsyncEnumerable(IAsyncEnumerable<T> asyncEnumerable)
    {
        _asyncEnumerable = asyncEnumerable;
    }

    private async IAsyncEnumerable<T> GetInitial()
    {
        _state = State.Processing;
        await foreach (T val in _asyncEnumerable)
        {
            _bufferedResult.Add(val);
            yield return val;
        }

        _state = State.Finished;
    }

    private async IAsyncEnumerable<T> GetWhileProgressing()
    {
        int index = 0;
        while (_state != State.Finished)
        {
            await Task.Delay(10);
            while (index < _bufferedResult.Count)
                yield return _bufferedResult[index++];
        }
    }

    private async IAsyncEnumerable<T> GetFromBuffer()
    {
        await Task.CompletedTask;
        foreach (T val in _bufferedResult)
            yield return val;
    }

    private IAsyncEnumerable<T> Get() => _state switch
    {
        State.Initial => GetInitial(),
        State.Processing => GetWhileProgressing(),
        _ => GetFromBuffer()
    };

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
        Get().GetAsyncEnumerator(cancellationToken);
}
