using Android.Content;
using AndroidX.Work;
using ProcraStop.Maui11.Platforms.Android.Services;

namespace ProcraStop.Maui11.Platforms.Android.Workers;

public sealed class ReminderReconciliationWorker : Worker
{
    public const string UniqueWorkName = "procrastop-reminder-reconciliation";
    public ReminderReconciliationWorker(Context context, WorkerParameters parameters) : base(context, parameters) { }
    public override Result DoWork()
    {
        try
        {
            ReminderRuntime.GetRequired<AndroidTaskReminderService>().ReconcileAsync(true).GetAwaiter().GetResult();
            return Result.InvokeSuccess();
        }
        catch { return Result.InvokeRetry(); }
    }
}
