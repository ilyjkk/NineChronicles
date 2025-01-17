using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using Bencodex.Types;
using Nekoyume.Model.State;
using UnityEditor.IMGUI.Controls;

namespace StateViewer.Editor
{
    [Serializable]
    public sealed class StateTreeViewItem : TreeViewItem
    {
        public class Model : IState
        {
            public int Id { get; }
            public string Key { get; }
            public string DisplayKey { get; }
            public string Type { get; }
            public string Value { get; private set; }
            public bool Editable { get; }
            public Model Parent { get; private set; }
            public List<Model> Children { get; } = new();

            public Model(int id, string key, string displayKey, string type, string value,
                bool editable = true)
            {
                Id = id;
                Key = key;
                DisplayKey = displayKey;
                Type = type;
                Value = value;
                Editable = editable;
            }

            public void SetValue(string value)
            {
                Value = value;
            }

            public void AddChild(Model child)
            {
                if (child is null)
                {
                    return;
                }

                child.Parent?.RemoveChild(child);
                Children.Add(child);
                child.Parent = this;
            }

            public void RemoveChild(Model child)
            {
                if (child is null)
                {
                    return;
                }

                if (Children.Contains(child))
                {
                    Children.Remove(child);
                    child.Parent = null;
                }
            }

            private static IValue Convert(string value, bool bom = true)
            {
                var sanitized = value.Replace("[", "").Replace("]", "");
                var converter = new Bencodex.Json.BencodexJsonConverter();
                var serializerOptions = new JsonSerializerOptions();
                var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(
                    bom ? $"\"\\uFEFF{sanitized}\"" : $"\"{sanitized}\""));
                return converter.Read(ref reader, typeof(Binary), serializerOptions);
            }

            public IValue Serialize()
            {
                switch (Enum.Parse<ValueKind>(Type))
                {
                    case ValueKind.Null:
                    case ValueKind.Boolean:
                        return Value.Serialize();
                    case ValueKind.Binary:
                    case ValueKind.Integer:
                        return Convert(Value, false);
                    case ValueKind.Text:
                        return Convert(Value);
                    case ValueKind.List:
                        return new List(Children.Select(child =>
                            child.Serialize()));
                    case ValueKind.Dictionary:
                    {
                        return new Dictionary(Children.Aggregate(
                            ImmutableDictionary<IKey, IValue>.Empty,
                            (current, child) => current.SetItem(
                                (IKey)Convert(child.Key),
                                child.Serialize())));
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Model ViewModel { get; }

        public StateTreeViewItem(Model viewModel) : base(viewModel.Id)
        {
            displayName = viewModel.Key;
            ViewModel = viewModel;
        }
    }
}
