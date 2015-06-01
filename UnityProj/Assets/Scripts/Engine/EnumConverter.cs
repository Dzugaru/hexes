using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Engine
{
    static class EnumConverter<TEnum> where TEnum : struct, IConvertible
    {
        public static readonly Func<TEnum, uint> ConvertToUInt = GenerateConverterToUInt();
        public static readonly Func<uint, TEnum> ConvertFromUInt = GenerateConverterFromUInt();

        static Func<uint, TEnum> GenerateConverterFromUInt()
        {
            var parameter = Expression.Parameter(typeof(uint), "a");
            var dynamicMethod = Expression.Lambda<Func<uint, TEnum>>(
                Expression.Convert(parameter, typeof(TEnum)),
                parameter);
            return dynamicMethod.Compile();
        }

        static Func<TEnum, uint> GenerateConverterToUInt()
        {
            var parameter = Expression.Parameter(typeof(TEnum), "a");
            var dynamicMethod = Expression.Lambda<Func<TEnum, uint>>(
                Expression.Convert(parameter, typeof(uint)),
                parameter);
            return dynamicMethod.Compile();
        }
    }
}
