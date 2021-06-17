using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Infra.MongoDB
{
    public class ImmutableTypeClassMapConventionProtectedSupport : ConventionBase, IClassMapConvention
    {
        public void Apply(BsonClassMap classMap)
        {
            var typeInfo = classMap.ClassType.GetTypeInfo();

            if (typeInfo.GetConstructor(Type.EmptyTypes) != null)
            {
                return;
            }

            var propertyBindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var properties = typeInfo.GetProperties(propertyBindingFlags).Where(x => x.CanWrite);

            var constructorBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            var zeroParameterCtor = typeInfo.GetConstructors(constructorBindingFlags).FirstOrDefault(x => x.GetParameters().Length == 0);

            if (typeInfo.IsAbstract is false && zeroParameterCtor != null)
            {
                (classMap.CreatorMaps as List<BsonCreatorMap>)?.Clear();
                classMap.MapConstructor(zeroParameterCtor);
            }

            foreach (var property in properties)
            {
                if (property.DeclaringType != classMap.ClassType)
                {
                    continue;
                }

                var memberMap = classMap.MapMember(property);
                if (classMap.IsAnonymous)
                {
                    var defaultValue = memberMap.DefaultValue;
                    memberMap.SetDefaultValue(defaultValue);
                }
            }
        }
    }
}
