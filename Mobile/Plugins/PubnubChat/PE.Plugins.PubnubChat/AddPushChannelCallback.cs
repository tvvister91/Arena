using PubnubApi;

namespace PE.Plugins.PubnubChat
{
    public class AddPushChannelCallback : PNCallback<PNPushAddChannelResult>
    {
        public override void OnResponse(PNPushAddChannelResult result, PNStatus status)
        {
            System.Diagnostics.Debug.WriteLine($"*** {GetType().Name}.{nameof(OnResponse)} - Push registration result: {status.StatusCode}, Error: {status.Error}, Result: {result}");
        }
    }
}
