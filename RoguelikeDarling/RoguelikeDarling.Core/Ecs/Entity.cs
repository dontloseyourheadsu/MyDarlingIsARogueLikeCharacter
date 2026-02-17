using System;
using System.Collections.Generic;

namespace RoguelikeDarling.Core.Ecs
{
    public sealed class Entity
    {
        private readonly Dictionary<Type, object> components;

        public Entity(int id)
        {
            Id = id;
            components = new Dictionary<Type, object>();
        }

        public int Id { get; }

        public Entity AddComponent<TComponent>(TComponent component) where TComponent : class
        {
            components[typeof(TComponent)] = component;
            return this;
        }

        public bool TryGetComponent<TComponent>(out TComponent component) where TComponent : class
        {
            if (components.TryGetValue(typeof(TComponent), out object value) && value is TComponent castValue)
            {
                component = castValue;
                return true;
            }

            component = null;
            return false;
        }

        public TComponent GetRequiredComponent<TComponent>() where TComponent : class
        {
            if (!TryGetComponent<TComponent>(out TComponent component) || component == null)
            {
                throw new InvalidOperationException($"Entity {Id} does not have component {typeof(TComponent).Name}.");
            }

            return component;
        }
    }
}
