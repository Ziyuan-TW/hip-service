namespace In.ProjectEKA.HipService.DataFlow
{
    using HipLibrary.Patient;
    using Task = System.Threading.Tasks.Task;

    public class DataFlowMessageHandler
    {
        private readonly ICollect collect;
        private readonly DataFlowClient dataFlowClient;
        private readonly DataEntryFactory dataEntryFactory;

        public DataFlowMessageHandler(
            ICollect collect,
            DataFlowClient dataFlowClient,
            DataEntryFactory dataEntryFactory)
        {
            this.collect = collect;
            this.dataFlowClient = dataFlowClient;
            this.dataEntryFactory = dataEntryFactory;
        }

        public async Task HandleDataFlowMessage(HipLibrary.Patient.Model.DataRequest dataRequest)
        {
            var sentKeyMaterial = dataRequest.KeyMaterial;
            var data = await collect.CollectData(dataRequest).ConfigureAwait(false);
            var encryptedEntries = data.FlatMap(entries =>
                dataEntryFactory.Process(entries, sentKeyMaterial, dataRequest.TransactionId));
            encryptedEntries.MatchSome(async entries =>
                await dataFlowClient.SendDataToHiu(dataRequest,
                    entries.Entries,
                    entries.KeyMaterial).ConfigureAwait(false));
        }
    }
}