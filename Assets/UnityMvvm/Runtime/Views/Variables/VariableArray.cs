

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Fusion.Mvvm
{
    [System.Serializable]
    public class VariableArray
    {
        [SerializeField]
        private List<Variable> variables;

        public ReadOnlyCollection<Variable> Variables => variables.AsReadOnly();

        public Variable this[int index] => variables[index];

        public object Get(string name)
        {
            if (variables == null || variables.Count <= 0)
                return null;
            var variable = variables.Find(v => v.Name.Equals(name));
            if (variable == null)
                return null;
            return variable.GetValue();
        }

        public T Get<T>(string name)
        {
            if (variables == null || variables.Count <= 0)
                return default(T);
            var variable = variables.Find(v => v.Name.Equals(name));
            if (variable == null)
                return default(T);
            return variable.GetValue<T>();
        }

        public static implicit operator List<Variable>(VariableArray array)
        {
            return array.variables;
        }

        public static implicit operator VariableArray(List<Variable> variables)
        {
            return new VariableArray() { variables = variables };
        }
    }
}
