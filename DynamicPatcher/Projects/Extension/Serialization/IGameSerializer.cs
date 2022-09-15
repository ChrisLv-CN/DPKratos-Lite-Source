using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extension.Serialization
{
    interface IGameSerializer
    {
        void Serialize(Stream serializationStream, object graph);
        object Deserialize(Stream serializationStream);

        EventHandler SaveGameHandler { get; }
        EventHandler LoadGameHandler { get; }

    }
}
