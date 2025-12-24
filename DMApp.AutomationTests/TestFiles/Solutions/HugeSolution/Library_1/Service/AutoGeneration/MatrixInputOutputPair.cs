namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.AutoGeneration
{
    using Skyline.DataMiner.Net.ResourceManager.Objects;

    public struct InputOutputPair
    {
        public FunctionResource input;
        public FunctionResource output;

        public InputOutputPair(FunctionResource input, FunctionResource output)
        {
            this.input = input;
            this.output = output;
        }
    }
}
