using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace PlaywrightLibrary.Extensions;

public static class LocatorExtensions
{
    extension(ILocator locator)
    {
        /// <summary>
        /// Converts a locator that may match multiple elements into one locator per match.
        /// </summary>
        public async Task<IEnumerable<ILocator>> EnumerateAsync()
        {
            var count = await locator.CountAsync();
            return Enumerable.Range(0, count).Select(locator.Nth);
        }

        /// <summary>
        /// Walks up the DOM to determine whether a computed CSS property contains a value on the
        /// element or any ancestor. Necessary because some properties (text-decoration) are painted
        /// by an ancestor and do not appear on the child's computed style.
        /// </summary>
        /// <param name="property">camelCase JS property name, e.g. "textDecorationLine" not "text-decoration-line".</param>
        public async Task<bool> HasComputedStyleAsync(string property, string value)
        {
            return await locator.EvaluateAsync<bool>(@$"el => {{
              let node = el;
              while (node) {{
                  const style = window.getComputedStyle(node).{property};
                  if (style.includes('{value}'))
                      return true;
                  node = node.parentElement;
              }}
              return false;
            }}");
        }

        public async Task<bool> IsPressedAsync()
            => await locator.GetAttributeAsync("aria-pressed") is { } attr && bool.Parse(attr);

        public async Task<bool> HasClassAsync(string className)
            => await locator.GetAttributeAsync("class") is { } attr && attr.Contains(className);

        /// <summary>
        /// Polls a predicate until it returns true or the timeout elapses.
        /// </summary>
        /// <param name="maxTimeout">Defaults to 30 seconds.</param>
        /// <param name="pollInterval">Defaults to 250ms.</param>
        public async Task WaitForAsync(
            Func<ILocator, Task<bool>> predicate,
            TimeSpan? maxTimeout = null,
            TimeSpan? pollInterval = null,
            [CallerArgumentExpression(nameof(predicate))] string? predicateExpression = null)
        {
            var timeout = maxTimeout ?? TimeSpan.FromSeconds(30);
            var timeoutAt = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < timeoutAt)
            {
                if (await predicate(locator))
                    return;

                await Task.Delay(pollInterval ?? TimeSpan.FromMilliseconds(250));
            }

            throw new TimeoutException(
                $"Timed out after {timeout.TotalSeconds} seconds waiting for '{predicateExpression}'.");
        }

        public async Task WaitUntilDetachedAsync()
            => await locator.WaitForAsync(new() { State = WaitForSelectorState.Detached });

        public async Task WaitForVisibleThenHiddenAsync(float? maxTimeoutInMilliseconds = null)
        {
            await locator.WaitForVisibleAsync(maxTimeoutInMilliseconds);
            await locator.WaitForHiddenAsync(maxTimeoutInMilliseconds);
        }

        public async Task WaitForVisibleAsync(float? maxTimeoutInMilliseconds = null)
            => await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = maxTimeoutInMilliseconds ?? 30000
            });

        public async Task WaitForHiddenAsync(float? maxTimeoutInMilliseconds = null)
            => await locator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = maxTimeoutInMilliseconds ?? 30000
            });

        public Task WaitForAllDetachedAsync(TimeSpan? maxTimeout = null, TimeSpan? pollInterval = null)
            => locator.WaitForAsync(async l => await l.CountAsync() == 0, maxTimeout, pollInterval);

        public Task WaitForNonEmptyTextAsync(TimeSpan? maxTimeout = null, TimeSpan? pollInterval = null)
            => locator.WaitForAsync(async l => !string.IsNullOrWhiteSpace(await l.InnerTextAsync()), maxTimeout, pollInterval);
    }
}
