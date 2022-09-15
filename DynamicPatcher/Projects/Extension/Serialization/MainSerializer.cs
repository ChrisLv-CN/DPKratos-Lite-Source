using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;
using Extension.Utilities;

namespace Extension.Serialization
{
    public class MainSerializer
    {
        public static void Serialize(Stream serializationStream, object graph)
        {
            if (GlobalVars.Serialization.SerializationCheck)
            {
                SerializationHelper.Check(graph);
            }
            Serializer.Serialize(serializationStream, graph);
        }

        public static object Deserialize(Stream serializationStream)
        {
            return Serializer.Deserialize(serializationStream);
        }

        public static T Deserialize<T>(Stream serializationStream)
        {
            return (T)Deserialize(serializationStream);
        }

        static MainSerializer()
        {
            EventSystem.SaveGame.AddPermanentHandler(EventSystem.SaveGame.SaveGameEvent, Serializer.SaveGameHandler);

            EventSystem.SaveGame.AddPermanentHandler(EventSystem.SaveGame.LoadGameEvent, Serializer.LoadGameHandler);
        }

        private static IGameSerializer Serializer = new GlobalSerializer();

    }
}
