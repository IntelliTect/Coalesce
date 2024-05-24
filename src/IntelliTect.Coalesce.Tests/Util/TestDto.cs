using System;

#nullable enable

namespace IntelliTect.Coalesce.Tests.Util
{
    internal class TestDto<T> : IClassDto<T>
        where T : class
    {
        public TestDto() { }

        public TestDto(long? id, Action<T>? mapTo = null)
        {
            Id = id;
            MapTo = mapTo;
        }

        public long? Id { get; set; }

        public T? SourceEntity { get; private set; }

        public Action<TestDto<T>, T>? MapFrom { get; set; }
        public Action<T>? MapTo { get; set; }

        void IResponseDto<T>.MapFrom(T obj, IMappingContext context, IncludeTree? tree)
        {
            SourceEntity = obj;
            MapFrom?.Invoke(this, obj);
        }

        void IParameterDto<T>.MapTo(T obj, IMappingContext context)
        {
            MapTo?.Invoke(obj);
        } 
    }
}
