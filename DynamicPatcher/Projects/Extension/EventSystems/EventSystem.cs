using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace Extension.EventSystems
{
    public abstract class EventBase
    {
        public abstract string Name { get; }
        public abstract string Description { get; }
    }

    public abstract partial class EventSystem
    {
        

        protected EventSystem()
        {
            Register();
        }

        ~EventSystem()
        {
            Unregister();
        }

        /// <summary>
        /// add handler that exists permanently during game process
        /// </summary>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void AddPermanentHandler(EventBase e, EventHandler handler)
        {
            AddHandler(_permanentHandlers, e, handler);
        }
        /// <summary>
        /// remove handler that exists permanently during game process
        /// </summary>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void RemovePermanentHandler(EventBase e, EventHandler handler)
        {
            RemoveHandler(_permanentHandlers, e, handler);
        }

        /// <summary>
        /// add handler that only exists in single game scenario
        /// </summary>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void AddTemporaryHandler(EventBase e, EventHandler handler)
        {
            AddHandler(_temporaryHandlers, e, handler);
        }
        /// <summary>
        /// remove handler that only exists in single game scenario
        /// </summary>
        /// <param name="e"></param>
        /// <param name="handler"></param>
        public virtual void RemoveTemporaryHandler(EventBase e, EventHandler handler)
        {
            RemoveHandler(_temporaryHandlers, e, handler);
        }

        public virtual void Broadcast(EventBase e, EventArgs args)
        {
            Broadcast(_permanentHandlers, e, args);
            Broadcast(_temporaryHandlers, e, args);
        }



        private void AddHandler(Dictionary<EventBase, EventHandler> handlers, EventBase e, EventHandler handler)
        {
            if (!handlers.ContainsKey(e))
            {
                handlers[e] = handler;
            }
            else
            {
                handlers[e] += handler;
            }

        }
        private void RemoveHandler(Dictionary<EventBase, EventHandler> handlers, EventBase e, EventHandler handler)
        {
            if (handlers.ContainsKey(e))
            {
                handlers[e] -= handler;
            }
        }
        private void Broadcast(Dictionary<EventBase, EventHandler> handlers, EventBase e, EventArgs args)
        {
            if (handlers.TryGetValue(e, out var handler))
            {
                foreach (EventHandler h in handler.GetInvocationList().Cast<EventHandler>())
                {
                    try
                    {
                        h(this, args);
                    }
                    catch (Exception exception)
                    {
                        Logger.PrintException(exception);
                    }
                }
            }
        }

        private void ClearTemporaryHandler(object sender, EventArgs e)
        {
            ClearTemporaryHandler();
        }
        private void ClearTemporaryHandler()
        {
            _temporaryHandlers.Clear();
        }

        private Dictionary<EventBase, EventHandler> _permanentHandlers = new();
        private Dictionary<EventBase, EventHandler> _temporaryHandlers = new();
    }
    
}
