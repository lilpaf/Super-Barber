namespace SuperBarber.Services.CutomException
{
    public class ModelStateCustomException : Exception
    {
        public string Key { get; }

        public ModelStateCustomException(string key, string messege) : base(messege)
        {
            Key = key;
        }
    }
}
