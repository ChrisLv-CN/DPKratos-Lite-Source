using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    class EnhancedFormatter : IRemotingFormatter, IFormatter
    {
        public EnhancedFormatter()
        {
            _binaryFormatter = new BinaryFormatter(GetSurrogateSelector(), new StreamingContext(StreamingContextStates.All));
        }


        static ISurrogateSelector GetSurrogateSelector()
        {
            var surrogates = new ISurrogateSelector[]
            {
                new CoroutineSurrogateSelector(),
                new DelegateSurrogateSelector(),
            };

            for (int i = 1; i < surrogates.Length; i++)
            {
                surrogates[i - 1].ChainSelector(surrogates[i]);
            }

            return surrogates[0];
        }

        public ISurrogateSelector SurrogateSelector
        {
            get => _binaryFormatter.SurrogateSelector;
            set => _binaryFormatter.SurrogateSelector = value;
        }
        public SerializationBinder Binder
        {
            get => _binaryFormatter.Binder;
            set => _binaryFormatter.Binder = value;
        }
        public StreamingContext Context
        {
            get => _binaryFormatter.Context;
            set => _binaryFormatter.Context = value;
        }

        public object Deserialize(Stream serializationStream) => _binaryFormatter.Deserialize(serializationStream);
        public void Serialize(Stream serializationStream, object graph) => _binaryFormatter.Serialize(serializationStream, graph);
        public object Deserialize(Stream serializationStream, HeaderHandler handler) => _binaryFormatter.Deserialize(serializationStream, handler);
        public void Serialize(Stream serializationStream, object graph, Header[] headers) => _binaryFormatter.Serialize(serializationStream, graph, headers);

        private BinaryFormatter _binaryFormatter;

    }
}
