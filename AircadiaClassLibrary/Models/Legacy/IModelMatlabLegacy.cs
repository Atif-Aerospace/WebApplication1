using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Models.Legacy
{
	public interface IModelMatlabLegacy
	{
		[Serialize(Type = SerializationType.Path)]
		string DllPath { get; }
		[Serialize]
		string MethodName { get; }
	}
}