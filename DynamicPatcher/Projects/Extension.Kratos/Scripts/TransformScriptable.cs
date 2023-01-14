using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;
using System.Runtime.InteropServices.ComTypes;

namespace Extension.Script
{

    [Serializable]
    public abstract class TransformScriptable : TechnoScriptable
    {

        public TransformScriptable(TechnoExt owner) : base(owner)
        {
        }

        public override void Awake()
        {
            // 初始化失败，移除脚本
            if (!OnAwake())
            {
                GameObject.RemoveComponent(this);
                return;
            }
            // 注册变身事件
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        /// <summary>
        /// 脚本初始化事件，返回false移除脚本
        /// </summary>
        /// <returns></returns>
        public abstract bool OnAwake();

        public override void LoadFromStream(IStream stream)
        {
            base.LoadFromStream(stream);
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public override void OnUnInit()
        {
            EventSystem.Techno.RemoveTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public abstract void OnTransform(object sender, EventArgs args);
    }
}