using Qwiq.Mapper.Attributes;

namespace Qwiq.Mapper.Mocks
{
    [WorkItemType("Fizz")]
    [WorkItemType("Baz")]
    [WorkItemType("Buzz")]
    public class MockModelMultipleTypes
    {
        [FieldDefinition("ID")]
        public int Id { get; internal set; }

        [FieldDefinition("IntField")]
        public int IntField { get; internal set; }
    }
}

