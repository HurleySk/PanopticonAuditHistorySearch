using System;
using System.ServiceModel;
using System.Threading;
using Microsoft.Xrm.Sdk;

namespace PanopticonAuditHistorySearch.Services
{
    public class ThrottleGuard
    {
        private const int MaxAttempts = 6;
        private readonly Action<string> _log;

        public ThrottleGuard(Action<string> log)
        {
            _log = log ?? (m => { });
        }

        public T Execute<T>(Func<T> call, CancellationToken token)
        {
            var attempt = 0;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    return call();
                }
                catch (FaultException<OrganizationServiceFault> fault)
                {
                    attempt++;
                    var wait = RetryDelay(fault, attempt);
                    if (!wait.HasValue || attempt >= MaxAttempts) throw;
                    _log(string.Format("Service protection limit hit; retrying in {0:N0}s (attempt {1}/{2}).",
                        wait.Value.TotalSeconds, attempt, MaxAttempts));
                    token.WaitHandle.WaitOne(wait.Value);
                }
            }
        }

        private static TimeSpan? RetryDelay(FaultException<OrganizationServiceFault> fault, int attempt)
        {
            var detail = fault.Detail;
            if (detail == null) return null;

            var throttled = detail.ErrorCode == -2147015902  // number of requests
                         || detail.ErrorCode == -2147015903  // execution time
                         || detail.ErrorCode == -2147015898  // concurrent requests
                         || detail.ErrorCode == -2147015896; // combined execution time

            if (!throttled) return null;

            object retryAfter;
            if (detail.ErrorDetails != null &&
                detail.ErrorDetails.TryGetValue("Retry-After", out retryAfter) &&
                retryAfter is TimeSpan)
            {
                return (TimeSpan)retryAfter;
            }

            return TimeSpan.FromSeconds(Math.Min(120, Math.Pow(2, attempt) * 2));
        }
    }
}
