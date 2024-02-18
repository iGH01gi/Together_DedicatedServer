using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Util
{
    
    //Fetches a component. Adds the requested component if it doesnt exist.
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    //FindChild util specified for GameObject
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T :  UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }
    
    public static List<String> EnumToString<T> ()
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList().Select(s => s.ToString()).ToList();
    }
    
    public static object GetValueClassField<T>(T classToBind, string fieldName)
    {
        return classToBind.GetType().GetField(fieldName).GetValue(classToBind);
    }

    public static E GetEnumByIndex<E>(E e, int index)
    {
        return (E)(Enum.GetValues(e.GetType())).GetValue(index);
    }

    public static int GetIndexOfEnum<E>(E e)
    {
        return Array.IndexOf(Enum.GetValues(e.GetType()), e);
    }
    
    public static void PrintLog(string log)
    {
        Debug.Log(log);
        Console.WriteLine(log);
    }
}