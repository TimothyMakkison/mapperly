using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Riok.Mapperly.Abstractions;
using Riok.Mapperly.Abstractions.ReferenceHandling;
using Riok.Mapperly.Abstractions.ReferenceHandling.Internal;

namespace Riok.Mapperly.Descriptors;

public class WellKnownTypes
{
    private readonly Compilation _compilation;

    private INamedTypeSymbol? _referenceHandlerAttribute;
    private INamedTypeSymbol? _objectFactoryAttribute;
    private INamedTypeSymbol? _mapperConstructorAttribute;

    private INamedTypeSymbol? _obsoleteAttribute;

    private INamedTypeSymbol? _iReferenceHandler;
    private INamedTypeSymbol? _preserveReferenceHandler;

    private INamedTypeSymbol? _iDictionaryT;
    private INamedTypeSymbol? _iReadOnlyDictionaryT;
    private INamedTypeSymbol? _iEnumerableT;
    private INamedTypeSymbol? _enumerable;
    private INamedTypeSymbol? _iCollection;
    private INamedTypeSymbol? _iCollectionT;
    private INamedTypeSymbol? _iReadOnlyCollectionT;
    private INamedTypeSymbol? _iListT;
    private INamedTypeSymbol? _listT;
    private INamedTypeSymbol? _stackT;
    private INamedTypeSymbol? _queueT;
    private INamedTypeSymbol? _iReadOnlyListT;
    private INamedTypeSymbol? _keyValuePairT;
    private INamedTypeSymbol? _dictionaryT;
    private INamedTypeSymbol? _enum;

    private INamedTypeSymbol? _iQueryableT;

    private INamedTypeSymbol? _dateOnly;
    private INamedTypeSymbol? _timeOnly;

    internal WellKnownTypes(Compilation compilation)
    {
        _compilation = compilation;
    }

    public INamedTypeSymbol ReferenceHandlerAttribute => _referenceHandlerAttribute ??= GetTypeSymbol(typeof(ReferenceHandlerAttribute));
    public INamedTypeSymbol ObjectFactoryAttribute => _objectFactoryAttribute ??= GetTypeSymbol(typeof(ObjectFactoryAttribute));
    public INamedTypeSymbol MapperConstructorAttribute => _mapperConstructorAttribute ??= GetTypeSymbol(typeof(MapperConstructorAttribute));
    public INamedTypeSymbol ObsoleteAttribute => _obsoleteAttribute ??= GetTypeSymbol(typeof(ObsoleteAttribute));
    public INamedTypeSymbol IReferenceHandler => _iReferenceHandler ??= GetTypeSymbol(typeof(IReferenceHandler));
    public INamedTypeSymbol PreserveReferenceHandler => _preserveReferenceHandler ??= GetTypeSymbol(typeof(PreserveReferenceHandler));
    public INamedTypeSymbol IDictionaryT => _iDictionaryT ??= GetTypeSymbol(typeof(IDictionary<,>));
    public INamedTypeSymbol IReadOnlyDictionaryT => _iReadOnlyDictionaryT ??= GetTypeSymbol(typeof(IReadOnlyDictionary<,>));
    public INamedTypeSymbol IEnumerableT => _iEnumerableT ??= GetTypeSymbol(typeof(IEnumerable<>));
    public INamedTypeSymbol Enumerable => _enumerable ??= GetTypeSymbol(typeof(Enumerable));
    public INamedTypeSymbol ICollection => _iCollection ??= GetTypeSymbol(typeof(System.Collections.ICollection));
    public INamedTypeSymbol ICollectionT => _iCollectionT ??= GetTypeSymbol(typeof(ICollection<>));
    public INamedTypeSymbol IReadOnlyCollectionT => _iReadOnlyCollectionT ??= GetTypeSymbol(typeof(IReadOnlyCollection<>));
    public INamedTypeSymbol IListT => _iListT ??= GetTypeSymbol(typeof(IList<>));
    public INamedTypeSymbol ListT => _listT ??= GetTypeSymbol(typeof(List<>));
    public INamedTypeSymbol StackT => _stackT ??= GetTypeSymbol(typeof(Stack<>));
    public INamedTypeSymbol QueueT => _queueT ??= GetTypeSymbol(typeof(Queue<>));
    public INamedTypeSymbol IReadOnlyListT => _iReadOnlyListT ??= GetTypeSymbol(typeof(IReadOnlyList<>));
    public INamedTypeSymbol KeyValuePairT => _keyValuePairT ??= GetTypeSymbol(typeof(KeyValuePair<,>));
    public INamedTypeSymbol DictionaryT => _dictionaryT ??= GetTypeSymbol(typeof(Dictionary<,>));
    public INamedTypeSymbol Enum => _enum ??= GetTypeSymbol(typeof(Enum));
    public INamedTypeSymbol IQueryableT => _iQueryableT ??= GetTypeSymbol(typeof(IQueryable<>));

    // use string type name as they are not available in netstandard2.0
    public INamedTypeSymbol? DateOnly => _dateOnly ??= GetTypeSymbol("System.DateOnly");

    public INamedTypeSymbol? TimeOnly => _timeOnly ??= GetTypeSymbol("System.TimeOnly");

    private INamedTypeSymbol GetTypeSymbol(Type type)
        => _compilation.GetTypeByMetadataName(type.FullName ?? throw new InvalidOperationException("Could not get name of type " + type))
            ?? throw new InvalidOperationException("Could not get type " + type.FullName);

    private INamedTypeSymbol? GetTypeSymbol(string typeFullName)
        => _compilation.GetTypeByMetadataName(typeFullName);
}
