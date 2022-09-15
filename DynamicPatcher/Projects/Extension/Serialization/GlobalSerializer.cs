using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.EventSystems;

namespace Extension.Serialization
{
    // slow when amount upto 10k
    class GlobalSerializer : IGameSerializer
    {
        public void Serialize(Stream serializationStream, object graph)
        {
            int id = AddObject(graph);
            var writer = new BinaryWriter(serializationStream);
            writer.Write(id);
        }

        public object Deserialize(Stream serializationStream)
        {
            var reader = new BinaryReader(serializationStream);
            int id = reader.ReadInt32();
            return GetObject(id);
        }

        public EventHandler SaveGameHandler => OnSaveGame;
        public EventHandler LoadGameHandler => OnLoadGame;


        private IFormatter GetFormatter()
        {
            return new EnhancedFormatter();
        }

        private void OnSaveGame(object sender, EventArgs e)
        {
            var args = (SaveGameEventArgs)e;

            if (args.IsStart)
            {
                CreateSaveFile(args.FileName);
            }

            if (args.IsEndInStream)
            {
                WriteAllObjects();
            }

            if (args.IsEnd)
            {
                Finish();
            }
        }

        private void OnLoadGame(object sender, EventArgs e)
        {
            var args = (LoadGameEventArgs)e;

            if (args.IsStart)
            {
                OpenSaveFile(args.FileName);
            }

            if (args.IsStartInStream)
            {
                ReadAllObjects();
            }

            if (args.IsEnd)
            {
                Finish();
            }
        }

        private void ReadAllObjects()
        {
            var stream = GetSaveFile();

            var formatter = GetFormatter();

            object graph = formatter.Deserialize(stream);
            container = (List<object>)graph;
        }

        private void WriteAllObjects()
        {
            var stream = GetSaveFile();

            var formatter = GetFormatter();

            //test obj
            //foreach (var obj in container)
            //{
            //    formatter.Serialize(stream, obj);
            //}
            formatter.Serialize(stream, container);
        }

        private int AddObject(object obj)
        {
            container.Add(obj);
            return container.Count - 1;
        }
        private object GetObject(int id)
        {
            object obj = container[id];

            return obj;
        }

        private string GetSavePath(string name)
        {
            string saveDir;
            if (name.Contains("Saved Games"))
            {
                saveDir = GlobalVars.RootDirectory;
            }
            else
            {
                saveDir = Path.Combine(GlobalVars.RootDirectory, "Saved Games");
            }
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            string fileName = Path.ChangeExtension(name, "gs.data");
            return Path.Combine(saveDir, fileName);
        }

        private void CreateSaveFile(string name)
        {
            container = new List<object>();

            saveFile = File.OpenWrite(GetSavePath(name));
            BinaryWriter writer = new BinaryWriter(saveFile);

            // write header
            writer.Write(GlobalVars.ExtensionVersion);
        }
        private void OpenSaveFile(string name)
        {
            saveFile = File.OpenRead(GetSavePath(name));
            BinaryReader reader = new BinaryReader(saveFile);

            // load header
            string version = reader.ReadString();

            if (version != GlobalVars.ExtensionVersion)
            {
                Logger.LogWarning("current version is {0}, but try to read saved game in version {1}", GlobalVars.ExtensionVersion, version);
            }
        }

        private FileStream GetSaveFile()
        {
            return saveFile;
        }

        private void Finish()
        {
            saveFile?.Dispose();
            container?.Clear();

            saveFile = null;
            container = null;
        }


        private FileStream saveFile;
        private List<object> container;
    }
}
