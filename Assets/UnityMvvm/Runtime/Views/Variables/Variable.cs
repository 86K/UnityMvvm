

using UnityEngine;

namespace Fusion.Mvvm
{
    [System.Serializable]
    public enum VariableType
    {
        Object,
        GameObject,
        Component,
        Boolean,
        Integer,
        Float,
        String,
        Color,
        Vector2,
        Vector3,
        Vector4
    }

    [System.Serializable]
    public class Variable
    {
        [SerializeField]
        protected string name = "";

        [SerializeField]
        protected Object objectValue;

        [SerializeField]
        protected string dataValue;

        [SerializeField]
        protected VariableType variableType;

        public virtual string Name
        {
            get => name;
            set => name = value;
        }

        public virtual VariableType VariableType => variableType;

        public virtual System.Type ValueType
        {
            get
            {
                switch (variableType)
                {
                    case VariableType.Boolean:
                        return typeof(bool);
                    case VariableType.Float:
                        return typeof(float);
                    case VariableType.Integer:
                        return typeof(int);
                    case VariableType.String:
                        return typeof(string);
                    case VariableType.Color:
                        return typeof(Color);
                    case VariableType.Vector2:
                        return typeof(Vector2);
                    case VariableType.Vector3:
                        return typeof(Vector3);
                    case VariableType.Vector4:
                        return typeof(Vector4);
                    case VariableType.Object:
                        return objectValue == null ? typeof(Object) : objectValue.GetType();
                    case VariableType.GameObject:
                        return objectValue == null ? typeof(GameObject) : objectValue.GetType();
                    case VariableType.Component:
                        return objectValue == null ? typeof(Component) : objectValue.GetType();
                    default:
                        throw new System.NotSupportedException();
                }
            }
        }

        public virtual void SetValue<T>(T value)
        {
            SetValue(value);
        }

        public virtual T GetValue<T>()
        {
            return (T)GetValue();
        }

        public virtual void SetValue(object value)
        {
            switch (variableType)
            {
                case VariableType.Boolean:
                    dataValue = DataConverter.GetString((bool)value);
                    break;
                case VariableType.Float:
                    dataValue = DataConverter.GetString((float)value);
                    break;
                case VariableType.Integer:
                    dataValue = DataConverter.GetString((int)value);
                    break;
                case VariableType.String:
                    dataValue = DataConverter.GetString((string)value);
                    break;
                case VariableType.Color:
                    dataValue = DataConverter.GetString((Color)value);
                    break;
                case VariableType.Vector2:
                    dataValue = DataConverter.GetString((Vector2)value);
                    break;
                case VariableType.Vector3:
                    dataValue = DataConverter.GetString((Vector3)value);
                    break;
                case VariableType.Vector4:
                    dataValue = DataConverter.GetString((Vector4)value);
                    break;
                case VariableType.Object:
                    objectValue = (Object)value;
                    break;
                case VariableType.GameObject:
                    objectValue = (GameObject)value;
                    break;
                case VariableType.Component:
                    objectValue = (Component)value;
                    break;
                default:
                    throw new System.NotSupportedException();
            }
        }
        public virtual object GetValue()
        {
            switch (variableType)
            {
                case VariableType.Boolean:
                    return DataConverter.ToBoolean(dataValue);
                case VariableType.Float:
                    return DataConverter.ToSingle(dataValue);
                case VariableType.Integer:
                    return DataConverter.ToInt32(dataValue);
                case VariableType.String:
                    return DataConverter.ToString(dataValue);
                case VariableType.Color:
                    return DataConverter.ToColor(dataValue);
                case VariableType.Vector2:
                    return DataConverter.ToVector2(dataValue);
                case VariableType.Vector3:
                    return DataConverter.ToVector3(dataValue);
                case VariableType.Vector4:
                    return DataConverter.ToVector4(dataValue);
                case VariableType.Object:
                    return objectValue;
                case VariableType.GameObject:
                    return objectValue;
                case VariableType.Component:
                    return objectValue;
                default:
                    throw new System.NotSupportedException();
            }
        }
    }
}
