

using System;
using System.Runtime.CompilerServices;

using Extension.Components;




namespace Extension.Script
{
    public static class ScriptComponentHelpers
    {
    


        //public static void CreateScriptComponent(this Component component, string scriptName, int id, string description, params object[] parameters)
        //{
        //    var script = ScriptManager.GetScript(scriptName);
        //    var scriptComponent = ScriptManager.CreateScriptableTo(component, script, parameters);
        //    scriptComponent.ID = id;
        //    scriptComponent.Name = description;
        //}
        public static void CreateScriptComponent(this Component component, string scriptName, int id, string description)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1>(this Component component, string scriptName, int id, string description, T1 p1)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8, p9);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Component component, string scriptName, int id, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15)
        {
            var script = ScriptManager.GetScript(scriptName);
            var scriptComponent = ScriptManager.CreateScriptableTo(component, script, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
            //scriptComponent.ID = id;
            scriptComponent.Name = description;
        }

    
        //public static void CreateScriptComponent(this Component component, string scriptName, string description, params object[] parameters)
        //{
        //    component.CreateScriptComponent(scriptName, Component.NO_ID, description, parameters);
        //}
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent(this Component component, string scriptName, string description)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1>(this Component component, string scriptName, string description, T1 p1)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2>(this Component component, string scriptName, string description, T1 p1, T2 p2)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8, p9);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CreateScriptComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Component component, string scriptName, string description, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15)
        {
            component.CreateScriptComponent(scriptName, Component.NO_ID, description, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        }

        //public static void CreateScriptComponent<T>(this Component component, int id, string description, params object[] parameters) where T : ScriptComponent
        //{
        //    component.CreateScriptComponent(typeof(T).Name, id, description, parameters);
        //}

        //public static void CreateScriptComponent<T>(this Component component, string description, params object[] parameters) where T : ScriptComponent
        //{
        //    component.CreateScriptComponent<T>(Component.NO_ID, description, parameters);
        //}
    }

}

