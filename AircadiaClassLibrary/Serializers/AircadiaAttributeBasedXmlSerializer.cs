using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Studies;
using Aircadia.ObjectModel.Workflows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Aircadia.Numerics;
using Aircadia.Numerics.Solvers;
using System.Globalization;
using System.Collections;
using System.Runtime.Serialization;

namespace Aircadia.Services.Serializers
{
	public class AircadiaAttributeBasedXmlSerializer
	{
		static AircadiaProject Project => AircadiaProject.Instance;

		private Dictionary<string, object> processedObjects = new Dictionary<string, object>();

		private const string NullSerializedString = "None";
		private const char CharacterSeparator = '\'';

		//// Save

		public void SaveProjectXML(string fileName)
		{
			string p = Path.Combine(Project.ProjectPath, fileName + ".explorer");

			var xmlDocument = new XDocument();

			var projectElement = new XElement("Project");

			projectElement.Add(new XAttribute("Version", "1.1"));

			projectElement.Add(SerializeEnumerable("DataRepository", Project.DataStore));
			projectElement.Add(SerializeEnumerable("DataRepositoryAux", Project.AuxiliaryDataStore));

			projectElement.Add(SerializeEnumerable("ModelRepository", Project.ModelStore));
			projectElement.Add(SerializeEnumerable("ModelRepositoryAux", Project.AuxiliaryModelStore));

			projectElement.Add(SerializeEnumerable("WorkflowRepository", Project.WorkflowStore));
			projectElement.Add(SerializeEnumerable("WorkflowRepositoryAux", Project.AuxiliaryWorkflowStore));

			projectElement.Add(SerializeEnumerable("StudyRepository", Project.StudyStore));

			xmlDocument.Add(projectElement);

			xmlDocument.Save(p);
		}

		private XElement SerializeEnumerable<T>(string name, IEnumerable<T> enumerable)
		{
			var elements = new XElement(name);
			foreach (T element in enumerable)
			{
				XElement xElement = SerializeObject(element);
				elements.Add(xElement);
			}

			return elements;
		}

		private XElement SerializeObject(object serialized)
		{
			if (serialized == null)
			{
				return null;
			}

			// Get the type of the object to be serialized
			Type type = serialized.GetType();

			// Get the properties to be serialize
			var propertiesDict = GetSerializeProperties(type);

			// Create the XML element
			var objectElement = new XElement(type.Name);

			// Add Name andd description (if present). This order 
			// is for readability purposes
			SerializePropertyIfPresentAndRemove("Name");
			SerializePropertyIfPresentAndRemove("Description");

			foreach (var key in propertiesDict.Keys)
			{
				SerializeProperty(propertiesDict[key]);
			}

			// Ads the namespace element to be able to recover 
			// the type when deserializing
			//element.Add(new XAttribute("Namespace", type.Namespace));

			// If the object has the property named it can be referenced 
			// by other objects, e.g. model referencing data. 
			// So this object is only serialized once
			if (type.GetProperty("Name")?.GetValue(serialized) is string name && !String.IsNullOrWhiteSpace(name))
			{
				processedObjects[$"{type.Name}.{name}"] = serialized;
			}


			return objectElement;


			void SerializePropertyIfPresentAndRemove(string serializedName)
			{
				if (propertiesDict.ContainsKey(serializedName))
				{
					SerializeProperty(propertiesDict[serializedName]);
					propertiesDict.Remove(serializedName);
				}
			}

			void SerializeProperty(PropertyInfo property)
			{
				// Get serialization options related to the property
				var serializationOptions = Attribute.GetCustomAttribute(property, typeof(SerializeAttribute)) as SerializeAttribute;
				if (serializationOptions == null)
				{
					serializationOptions = new SerializeAttribute
					{
						Type = SerializationType.Value
					};
				}
				if (String.IsNullOrWhiteSpace(serializationOptions.SerializedName))
				{
					serializationOptions.SerializedName = property.Name;
				}

				// Get the property value
				object propertyValue = property.GetValue(serialized) ?? serializationOptions.Default;

				// Serialize tyhe property
				SerializeItem(propertyValue, objectElement, serializationOptions, property);
			}
		}

		private void SerializeItem(object itemToSerialize, XElement parentNode, SerializeAttribute serializationOptions, PropertyInfo propertyInfo = null)
		{
			if (parentNode == null)
			{
				throw new ArgumentNullException(nameof(parentNode));
			}

			if (serializationOptions == null)
			{
				throw new ArgumentNullException(nameof(serializationOptions));
			}

			if (itemToSerialize == null)
			{
				Console.WriteLine($"The property {propertyInfo?.Name}, serialized name {serializationOptions.SerializedName} is null");
				//throw new SerializationException();
				AddAttribute(parentNode, serializationOptions.SerializedName, NullSerializedString);
				return;
			}


			if (itemToSerialize is double d)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, d.ToString(CultureInfo.InvariantCulture));
			}
			else if (itemToSerialize is decimal de)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, de.ToString(CultureInfo.InvariantCulture));
			}
			else if (itemToSerialize is long l)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, l.ToString(CultureInfo.InvariantCulture));
			}
			else if (itemToSerialize is bool b)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, b.ToString(CultureInfo.InvariantCulture));
			}
			else if (itemToSerialize is int i)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, i.ToString(CultureInfo.InvariantCulture));
			}
			else if (itemToSerialize is IEnumerable<double> da)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, DoubleVectorData.ValueToString(da.ToArray()));
			}
			else if (itemToSerialize is IEnumerable<decimal> dea)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, dea.Aggregate(String.Empty, (t, dec) => t += dec + ", ", t => t.TrimEnd(',', ' ')));
			}
			else if (itemToSerialize is IEnumerable<int> ia)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, IntegerVectorData.ValueToString(ia.ToArray()));
			}
			else if (itemToSerialize is double[,] dm)
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, DoubleMatrixData.ValueToString(dm));
			}
			else if (itemToSerialize is IEnumerable<string> ls)
			{
				SerializeLines(parentNode, serializationOptions.SerializedName, ls);
			}
			else if (itemToSerialize is string s)
			{
				if (serializationOptions.Type == SerializationType.Lines)
				{
					SerializeLines(parentNode, serializationOptions.SerializedName, s);
				}
				else if (serializationOptions.Type == SerializationType.Path)
				{
					AddAttribute(parentNode, serializationOptions.SerializedName, SerializePath(s));
				}
				else
				{
					AddAttribute(parentNode, serializationOptions.SerializedName, s);
				}
			}
			else if (itemToSerialize is IEnumerable<char> ca)
			{
				string serializedCharArray = ca.Aggregate(String.Empty, (t, c) => t += c + CharacterSeparator, t => t.TrimEnd(CharacterSeparator));
				AddAttribute(parentNode, serializationOptions.SerializedName, serializedCharArray);
			}
			else if (itemToSerialize.GetType().BaseType == typeof(Enum))
			{
				AddAttribute(parentNode, serializationOptions.SerializedName, itemToSerialize.ToString());
			}
			else if (itemToSerialize is IDictionary dictionary)
			{
				if (!(serializationOptions is SerializeDictionaryAttribute dictionaryOptions))
				{
					throw new SerializationException($"The property {serializationOptions.SerializedName} is marked as a collection of items to be referenced, but the attribute information is missing");
				}

				// Element that contains the dictionary entries
				var entriesElement = new XElement(serializationOptions.SerializedName);

				// Name of the entry elements
				string childrenName = dictionaryOptions.SerializedChildrenName;

				// Add entries
				if (serializationOptions.Type == SerializationType.Value)
				{
					foreach (DictionaryEntry kvp in dictionary)
					{
						var kvpElement = new XElement(childrenName);
						Type kvpType = kvp.GetType();
						SerializeItem(kvp.Key, kvpElement, new SerializeAttribute("Key"), kvpType.GetProperty("Key"));
						SerializeItem(kvp.Value, kvpElement, new SerializeAttribute("Value"), kvpType.GetProperty("Value"));
						entriesElement.Add(kvpElement);
					}
				}

				parentNode.Add(entriesElement);
			}
			else if (itemToSerialize is IEnumerable enumerable)
			{
				// Element that contains the enumerable entries
				var entriesElement = new XElement(serializationOptions.SerializedName);

				// Add entries
				if (serializationOptions.Type == SerializationType.Reference)
				{
					if (!(serializationOptions is SerializeEnumerableAttribute enumerableOptions))
					{
						throw new SerializationException($"The property {serializationOptions.SerializedName} is marked as a collection of items to be referenced, but the attribute information is missing");
					}

					if (propertyInfo == null)
					{
						throw new SerializationException($"The property {serializationOptions.SerializedName} is marked as a collection of items to be referenced, but the property information is missing");
					}

					PropertyInfo nameProperty = propertyInfo.PropertyType.GenericTypeArguments.First().GetProperty("Name");
					foreach (object item in enumerable)
					{
						if (nameProperty.GetValue(item) is string reference && !String.IsNullOrWhiteSpace(reference))
						{
							var referenceAttribute = new XAttribute("Name", $"{item.GetType().Name}.{reference}");
							AddElement(entriesElement, enumerableOptions.SerializedChildrenName, referenceAttribute);
						}
						else
						{
							throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be referenced, but {enumerableOptions.SerializedChildrenName} does not have a name and therefore it cannot be referenced");
						}
					}
				}
				else if (serializationOptions.Type == SerializationType.Value)
				{
					foreach (object item in enumerable)
					{
						SerializeItem(item, entriesElement, new SerializeAttribute(""));
					}
				}

				parentNode.Add(entriesElement);
			}
			
			else // object
			{
				if (serializationOptions.Type == SerializationType.Value)
				{
					// if serializing an enumerable the items don't have a name, so they are not wrapped in the property name
					XElement serializedObject = SerializeObject(itemToSerialize);
					if (String.IsNullOrWhiteSpace(serializationOptions.SerializedName))
					{
						parentNode.Add(serializedObject);
					}
					else
					{
						AddElement(parentNode, serializationOptions.SerializedName, serializedObject);
					}
				}
				if (serializationOptions.Type == SerializationType.Reference)
				{
					if (propertyInfo == null)
					{
						throw new SerializationException($"{serializationOptions.SerializedName} is not a property, therefore it cannot be serialized as a reference");
					}

					PropertyInfo nameProperty = propertyInfo.PropertyType.GetProperty("Name");
					if (nameProperty.GetValue(itemToSerialize) is string reference && !String.IsNullOrWhiteSpace(reference))
					{
						AddAttribute(parentNode, serializationOptions.SerializedName, $"{itemToSerialize.GetType().Name}.{reference}");
					}
					else
					{
						throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be referenced, but {serializationOptions.SerializedName} does not have a name and therefore it cannot be referenced");
					}
				}
			}
		}

		private static void AddAttribute(XElement parentNode, string name, object content) => parentNode.Add(new XAttribute(name, content));

		private static void AddElement(XElement parentNode, string name, object content) => parentNode.Add(new XElement(name, content));

		private static IDictionary<string, PropertyInfo> GetSerializeProperties(Type type)
		{
			var dict = new Dictionary<string, PropertyInfo>();
			foreach (PropertyInfo property in type.GetRuntimeProperties())
			{
				if (Attribute.GetCustomAttribute(property, typeof(SerializeAttribute)) is SerializeAttribute attribute)
				{
					string key = String.IsNullOrWhiteSpace(attribute.SerializedName) ? property.Name : attribute.SerializedName;
					dict.Add(key, property);
				}
			}
			return dict;
		}

		//// Open

		public void OpenProjectXML(string path, Action<string, string> initialize)
		{
			if (!File.Exists(path))
			{
				throw new ArgumentException("File \"" + path + "\" does not exists");
			}

			string name = Path.GetFileNameWithoutExtension(path);
			initialize(name.EndsWith("new") ? name.Substring(0, name.Length - 3) : name, Path.GetDirectoryName(path));

			XElement projectElement = XDocument.Load(path).Element("Project");
			if (projectElement == null)
			{
				throw new ArgumentException("The .xml \"Project\" element is null. Please make sure the .xml file contains an Aircadia project definition");
			}

			string version = GetString(projectElement, "Version");
			if (version != "1.1")
			{
				throw new SerializationVersionException("Wrong project version, older serializer will be used instead");
			}


			var list = DeserializeElementIntoList<Data>(projectElement.Element("DataRepositoryAux"));
			foreach (var item in list)
				Project.Add(item);

			list = DeserializeElementIntoList<Data>(projectElement.Element("DataRepository"));
			foreach (var item in list)
				Project.Add(item);

			var list2 = DeserializeElementIntoList<Model>(projectElement.Element("ModelRepositoryAux"));
			foreach (var item in list2)
				Project.Add(item);

			list2 = DeserializeElementIntoList<Model>(projectElement.Element("ModelRepository"));
			foreach (var item in list2)
				Project.Add(item);

			XElement workflowElements = new XElement("Workflows");
			
			workflowElements.Add(projectElement.Element("WorkflowRepositoryAux").Elements().ToArray() ?? new XElement[0]);
			workflowElements.Add(projectElement.Element("WorkflowRepository"   ).Elements().ToArray() ?? new XElement[0]);

			var list3 = DeserializeElementIntoList<Workflow>(workflowElements);
			foreach (var item in list3)
				Project.Add(item);

			//var workflowDict = workflowElements.ToDictionary(w => GetValue(w, "Name"));

			//// Clean-up
			////var hash = new HashSet<WorkflowComponent>(Project.WorkflowStoreAux.Values);
			////foreach (var component in Project.WorkflowStore)
			////{
			////	foreach (var subcomp in component.ScheduledComponents)
			////	{
			////		if (hash.Contains(subcomp))
			////			hash.Remove(subcomp);
			////	}
			////}
			////var old = new List<Workflow>(Project.WorkflowStoreAux.Values);
			////Project.WorkflowStoreAux.Clear();
			////foreach (var item in old)
			////{
			////	if (!hash.Contains(item))
			////		Project.WorkflowStoreAux.Add(item.Name, item);
			////}

			var list4 = DeserializeElementIntoList<Study>(projectElement.Element("StudyRepository"));
			foreach (var item in list4)
				Project.Add(item);
		}

		private List<T> DeserializeElementIntoList<T>(XElement element) where T : class
		{
			List<T> list = new List<T>();
			foreach (XElement e in element.Elements())
			{
				T deserialized = DeserializeObject(e) as T;
				list.Add(deserialized);
			}
			return list;
		}

		private object DeserializeObject(XElement e)
		{
			// The object to deserialize is empty
			if (e == null)
			{
				return null;
			}

			// Names of the type to be deserialized
			string name = GetString(e, "Name") ?? String.Empty;

			// Namespace to find the type to be deserialized
			//string namespaceName = GetValue(e, "Namespace"); 
			//if (String.IsNullOrWhiteSpace(namespaceName))
			//{
			//	throw new SerializationException($"Element '{name}' is missing the namespace attribute.\n\r{e}");
			//}

			// Get the type to be deserialized
			//string typeName = $"{namespaceName}.{e.Name}";
			//var type = Type.GetType(typeName);
			var type = AssemblyLoader.GetType(e.Name.ToString());
			if (type == null)
			{
				throw new SerializationException($"Type of element '{name}' could not be found.\n\r{e}");
			}

			// Get a suitable constructor
			var propertiesDict = GetSerializeProperties(type).ToDictionary(kvp => kvp.Value.Name, kvp => (kvp.Key, kvp.Value));
			ConstructorInfo constructor = GetDeserializeConstructor(type);
			if (constructor == null)
			{
				throw new SerializationException($"No constructor markded with 'DeserializeConstructor or default parameterless constructor was found for element '{name}'.\n\r{e}");
			}

			// Get the constructor parameters
			var parameters = new List<object>();
			foreach (ParameterInfo parameterInfo in constructor.GetParameters())
			{
				string propertyKey = Capitalize(parameterInfo.Name);
				if (propertiesDict.ContainsKey(propertyKey))
				{
					(string serializedName, PropertyInfo property) = propertiesDict[propertyKey];
					var serializationOptions = Attribute.GetCustomAttribute(property, typeof(SerializeAttribute)) as SerializeAttribute;
					if (serializationOptions == null)
					{
						serializationOptions = new SerializeAttribute
						{
							Type = SerializationType.Value
						};
					}
					if (String.IsNullOrWhiteSpace(serializationOptions.SerializedName))
					{
						serializationOptions.SerializedName = property.Name;
					}

					object parameterValue = DeserializeItem(e, serializationOptions, new NameTypePair(parameterInfo));

					if (property.PropertyType != parameterInfo.ParameterType)
					{
						// Convert?
						Console.WriteLine($"Property and Parameter type mismatch for parameter {parameterInfo.Name} while constructing '{type}'");
					}
					parameters.Add(parameterValue);
					propertiesDict.Remove(propertyKey);
				}
				else if (parameterInfo.IsOptional)
				{
					parameters.Add(parameterInfo.DefaultValue);
				}
				else 
				{
					throw new KeyNotFoundException($"The property '{propertyKey}' could not be deserialized from {e}");
				}
			}

			// Create the object
			object deserialized;
			try
			{
				deserialized = constructor.Invoke(parameters.ToArray());
			}
			catch (Exception ex)
			{
				throw new SerializationException($"Failed to construct object '{name}'.\n\r{e}", ex);
			}

			// Add the properties that are not assigned in the constructor
			foreach (var val in propertiesDict.Values)
			{
				(string serializedName, PropertyInfo propertyInfo) = val;

				if (Attribute.GetCustomAttribute(propertyInfo, typeof(SerializeAttribute)) is SerializeAttribute attribute && attribute.ConstructorOnly)
				{
					continue;
				}

				if (propertyInfo.CanWrite)
				{
					object parameterValue = DeserializeItem(e, new SerializeAttribute(serializedName), new NameTypePair(propertyInfo));
					propertyInfo.SetValue(deserialized, parameterValue);
				}
			}

			// Store in a dictionary to enable retrieval through reference
			if (!String.IsNullOrWhiteSpace(name))	
			{
				processedObjects[$"{type.Name}.{name}"] = deserialized;
			}

			// Return the object
			return deserialized;
		}

		private object DeserializeItem(XElement parentNode, SerializeAttribute serializationOptions, NameTypePair propertyInfo)
		{
			if (parentNode == null)
			{
				throw new ArgumentNullException(nameof(parentNode));
			}

			if (serializationOptions == null)
			{
				throw new ArgumentNullException(nameof(serializationOptions));
			}

			if (propertyInfo == null)
			{
				throw new ArgumentNullException(nameof(propertyInfo));
			}

			Type itemType = propertyInfo.Type;

			if (itemType == typeof(double))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				if (Double.TryParse(serializedString, out double d))
				{
					return d;
				}
				else
				{
					throw new SerializationException($"Error parsing as a double element '{serializationOptions.SerializedName}' with serialized value {serializedString}");
				}
			}
			else if (itemType == typeof(decimal))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				if (Decimal.TryParse(serializedString, out decimal de))
				{
					return de;
				}
				else
				{
					throw new SerializationException($"Error parsing as a decimal element '{serializationOptions.SerializedName}' with serialized value {serializedString}");
				}
			}
			else if (itemType == typeof(long))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				if (Int64.TryParse(serializedString, out long l))
				{
					return l;
				}
				else
				{
					throw new SerializationException($"Error parsing as a long element '{serializationOptions.SerializedName}' with serialized value {serializedString}");
				}
			}
			else if (itemType == typeof(bool))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				if (Boolean.TryParse(serializedString, out bool b))
				{
					return b;
				}
				else
				{
					throw new SerializationException($"Error parsing as a boolean element '{serializationOptions.SerializedName}' with serialized value {serializedString}");
				}
			}
			else if (itemType == typeof(int))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				if (Int32.TryParse(serializedString, out int i))
				{
					return i;
				}
				else
				{
					throw new SerializationException($"Error parsing as an int element '{serializationOptions.SerializedName}' with serialized value {serializedString}");
				}
			}
			else if (ImplementsInterface<IEnumerable<double>>(itemType))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);

				try
				{
					var array = DoubleVectorData.StringToValue(serializedString);

					if (itemType == typeof(double[]))
					{
						return array;
					}
					if (itemType.GetConstructor(new[] { typeof(IEnumerable<double>) }) is ConstructorInfo constructor)
					{
						return constructor.Invoke(new[] { array });
					}
					else
					{
						throw new SerializationException($"Error parsing as an double[] element '{serializationOptions.SerializedName}'. " +
							$"The target property type does not have a valid constructor (IEnumerable<double>)");
					}
					
				}
				catch (Exception ex)
				{
					throw new SerializationException($"Error parsing as an double[] element '{serializationOptions.SerializedName}' with serialized value {serializedString}", ex);
				}
			}
			else if (ImplementsInterface<IEnumerable<decimal>>(itemType))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);

				try
				{
					var array = serializedString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(s => Convert.ToDecimal(s)).ToArray();

					if (itemType == typeof(decimal[]))
					{
						return array;
					}
					if (itemType.GetConstructor(new[] { typeof(IEnumerable<decimal>) }) is ConstructorInfo constructor)
					{
						return constructor.Invoke(new[] { array });
					}
					else
					{
						throw new SerializationException($"Error parsing as an double[] element '{serializationOptions.SerializedName}'. " +
							$"The target property type does not have a valid constructor (IEnumerable<double>)");
					}

				}
				catch (Exception ex)
				{
					throw new SerializationException($"Error parsing as an double[] element '{serializationOptions.SerializedName}' with serialized value {serializedString}", ex);
				}
			}
			else if (ImplementsInterface<IEnumerable<int>>(itemType))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				try
				{
					var array = IntegerVectorData.StringToValue(serializedString);

					if (itemType == typeof(int[]))
					{
						return array;
					}
					if (itemType.GetConstructor(new[] { typeof(IEnumerable<int>) }) is ConstructorInfo constructor)
					{
						return constructor.Invoke(new[] { array });
					}
					else
					{
						throw new SerializationException($"Error parsing as an int[] element '{serializationOptions.SerializedName}'. " +
							$"The target property type does not have a valid constructor (IEnumerable<int>)");
					}

				}
				catch (Exception ex)
				{
					throw new SerializationException($"Error parsing as an int[] element '{serializationOptions.SerializedName}' with serialized value {serializedString}", ex);
				}
			}
			else if (itemType == typeof(double[,]))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				try
				{
					return DoubleMatrixData.StringToValue(serializedString);
				}
				catch (Exception ex)
				{
					throw new SerializationException($"Error parsing as an double[,] element '{serializationOptions.SerializedName}' with serialized value {serializedString}", ex);
				}
			}
			else if (ImplementsInterface<IEnumerable<string>>(itemType))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName, true);
				if (serializedString is string s)
				{
					var valueList = new List<string>();
					foreach (string item in s.Split('\n'))
						valueList.Add(item);
					if (itemType == typeof(string[]))
					{
						return valueList.ToArray();
					}
					if (itemType.GetConstructor(new[] { typeof(IEnumerable<string>) }) is ConstructorInfo constructor)
					{
						return constructor.Invoke(new[] { valueList.ToArray() });
					}
					else
					{
						throw new SerializationException($"Error parsing as an string[] element '{serializationOptions.SerializedName}'. " +
							$"The target property type does not have a valid constructor (IEnumerable<string>)");
					}
				}
				else
				{
					throw new SerializationException($"Error parsing as an string[] element '{serializationOptions.SerializedName}' with serialized value {serializedString}");
				}
			}
			else if (itemType == typeof(string))
			{
				if (serializationOptions.Type == SerializationType.Lines)
				{
					if (GetString(parentNode, serializationOptions.SerializedName, true) is string s)
					{
						return s;
					}
					else
					{
						throw new SerializationException($"Error parsing as a string element '{serializationOptions.SerializedName}'");
					}
				}
				else if (serializationOptions.Type == SerializationType.Path)
				{
					if (GetString(parentNode, serializationOptions.SerializedName) is string s)
					{
						return DeserializePath(s);
					}
					else
					{
						throw new SerializationException($"Error parsing as a string element '{serializationOptions.SerializedName}'");
					}
				}
				else
				{
					if (GetString(parentNode, serializationOptions.SerializedName) is string s)
					{
						return s;
					}
					else
					{
						throw new SerializationException($"Error parsing as a string element '{serializationOptions.SerializedName}'");
					}
				}
			}
			else if (ImplementsInterface<IEnumerable<char>>(itemType))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				if (serializedString is string s)
				{
					var valueList = new List<char>();
					foreach (string item in s.Split('\''))
						valueList.Add(item.FirstOrDefault());
					if (itemType == typeof(char[]))
					{
						return valueList.ToArray();
					}
					if (itemType.GetConstructor(new[] { typeof(IEnumerable<char>) }) is ConstructorInfo constructor)
					{
						return constructor.Invoke(new[] { valueList.ToArray() });
					}
					else
					{
						throw new SerializationException($"Error parsing as an char[] element '{serializationOptions.SerializedName}'. " +
							$"The target property type does not have a valid constructor (IEnumerable<char>)");
					}
				}
				else
				{
					throw new SerializationException($"Error parsing as an char[] element '{serializationOptions.SerializedName}' with serialized value {serializedString}");
				}
			}
			else if (itemType.BaseType == typeof(Enum))
			{
				string serializedString = GetString(parentNode, serializationOptions.SerializedName);
				try
				{
					return Enum.Parse(itemType, serializedString);
				}
				catch (Exception ex)
				{
					throw new SerializationException($"Error parsing as a {itemType.Name}Enum element '{serializationOptions.SerializedName}' with serialized value {serializedString}", ex);
				}
			}
			else if (ImplementsInterface<IDictionary>(itemType))
			{
				if (!(serializationOptions is SerializeDictionaryAttribute dictionaryOptions))
				{
					throw new SerializationException($"The property {serializationOptions.SerializedName} is marked as a collection of items to be referenced, but the attribute information is missing");
				}

				// Get method of final dict, e.g. Dictionary<string, Data>
				Type dictType = MakeGenericOf<Dictionary<object, object>>(itemType.GenericTypeArguments);
				var dict = Activator.CreateInstance(dictType) as IDictionary;

				XElement serializedElements = parentNode.Element(serializationOptions.SerializedName);

				if (dictionaryOptions.Type == SerializationType.Reference)
				{
					foreach (XElement element in serializedElements.Elements())
					{
						string key = GetString(element, "Key");
						if (String.IsNullOrWhiteSpace(key))
						{
							throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be dereferenced, but {element} does not have a Key and therefore it cannot be dereferenced");
						}

						string name = GetString(element, "Name");
						if (String.IsNullOrWhiteSpace(name))
						{
							throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be dereferenced, but {element} does not have a Name and therefore it cannot be dereferenced");
						}

						if (processedObjects.TryGetValue(name, out object value))
						{
							dict.Add(key, value);
						}
						else
						{
							throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be dereferenced, but {name} has not been deserializaed before");
						}
					}
				}
				else if (dictionaryOptions.Type == SerializationType.Value)
				{
					Type kvpType = MakeGenericOf<KeyValuePair<object, object>>(itemType.GenericTypeArguments);
					foreach (XElement element in serializedElements.Elements())
					{
						object key = DeserializeItem(element, new SerializeAttribute("Key"), new NameTypePair(kvpType.GetProperty("Key")));
						object value = DeserializeItem(element, new SerializeAttribute("Value"), new NameTypePair(kvpType.GetProperty("Value")));
						dict.Add(key, value);
					}
				}

				return dict;
			}
			else if (ImplementsInterface<IEnumerable>(itemType))
			{
				Type listType = MakeGenericOf<List<object>>(itemType.GenericTypeArguments.First());
				var list = Activator.CreateInstance(listType) as IList;

				XElement enumElement = parentNode.Element(serializationOptions.SerializedName)
					?? throw new SerializationException($"The property {serializationOptions.SerializedName} is marked as a collection of items to be referenced, but no element with susch name was found in {parentNode.Name}");

				if (serializationOptions.Type == SerializationType.Reference)
				{
					if (!(serializationOptions is SerializeEnumerableAttribute enumerableOptions))
					{
						throw new SerializationException($"The property {serializationOptions.SerializedName} is marked as a collection of items to be referenced, but the attribute information is missing");
					}

					foreach (XElement element in enumElement.Elements())
					{
						string name = GetString(element, "Name");
						if (String.IsNullOrWhiteSpace(name))
						{
							throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be dereferenced, but {element} does not have a Name and therefore it cannot be dereferenced");
						}

						if (processedObjects.TryGetValue(name, out object value))
						{
							list.Add(value);
						}
						else
						{
							throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be dereferenced, but {name} has not been deserializaed before");
						}
					}
				}
				else if (serializationOptions.Type == SerializationType.Value)
				{
					foreach (XElement element in enumElement.Elements())
					{
						list.Add(DeserializeObject(element));
					}
				}

				return list;
			}
			else // object
			{
				if (serializationOptions.Type == SerializationType.Reference)
				{
					string name = GetString(parentNode, serializationOptions.SerializedName);
					if (String.IsNullOrWhiteSpace(name))
					{
						throw new SerializationException($"The property {propertyInfo.Name} is marked as a collection of items to be dereferenced, but {parentNode.Name} does not have a Name and therefore it cannot be dereferenced");
					}

					if (processedObjects.TryGetValue(name, out object value))
					{
						return value;
					}
					else
					{
						throw new SerializationException($"The property {propertyInfo.Name} is marked as an item to be dereferenced, but {name} has not been deserializaed before");
					}
				}
				else
				{
					if (parentNode.Element(serializationOptions.SerializedName) is XElement objectContainer)
					{
						return DeserializeObject(objectContainer.FirstNode as XElement);
					}
					if (parentNode.Attribute(serializationOptions.SerializedName) is XAttribute objectAttribute && objectAttribute.Value == "None")
					{
						return null;
					}
					else
					{
						throw new SerializationException($"There is no element for {propertyInfo.Name} in {parentNode.Name}. The object cannot be deserialized");
					}
				}
			}
		}

		private static bool ImplementsInterface<T>(Type itemType)
		{
			Type targetType = typeof(T);
			foreach (var interfaceType in itemType.GetInterfaces())
			{
				if (interfaceType == targetType)
				{
					return true;
				}
			}
			
			return itemType == targetType;
		}

		private string Capitalize(string name) => Char.ToUpper(name[0]) + name.Substring(1);

		private ConstructorInfo GetDeserializeConstructor(Type type)
		{
			ConstructorInfo[] constructors = type.GetConstructors();
			ConstructorInfo constructor =  constructors.FirstOrDefault(c => Attribute.GetCustomAttribute(c, typeof(DeserializeConstructorAttribute)) is DeserializeConstructorAttribute);
			if (constructor == default(ConstructorInfo))
			{
				constructor = constructors.FirstOrDefault(c => c.GetParameters().Count() == 0);
			}
			return constructor;
		}

		//// Misc

		private static string SerializePath(string path)
		{
			if (path.Contains(Project.ProjectPath))
			{
				path = path.Replace(Project.ProjectPath, "*");
			}
			return path;
		}

		private static string DeserializePath(string path)
		{
			if (path.Contains("*"))
			{
				path = path.Replace("*", Project.ProjectPath);
			}
			return path;
		}

		private static XElement GetSolverElement(ISolver solver)
		{
			var solverElement = new XElement(solver.GetType().Name);
			var solverOptions= new NumericalMethodOptions(solver);
			//solverElement.Add(new XElement("Type", solver.Name));
			//XElement options = new XElement("Options");

			foreach (string key in solverOptions.Names)
			{
				solverElement.Add(new XAttribute(key, solverOptions[key]));
			}
			//solverElement.Add(options);
			return solverElement;
		}

		private static List<ISolver> GetSolvers(XElement solversElement)
		{
			if (solversElement == null)
				return null;
			var solvers = new List<ISolver>();
			foreach (XElement solverElement in solversElement?.Descendants())
			{
				var solverType = Type.GetType(SolversNamespaceName + solverElement.Name.LocalName);
				if (solverType == null) continue;

				var solver = Activator.CreateInstance(solverType) as ISolver;

				var solverOptions = new NumericalMethodOptions(solver);
				foreach (XAttribute optionElement in solverElement.Attributes())
					solverOptions[optionElement.Name.LocalName] = optionElement.Value;

				solvers.Add(solver);
			}
			return solvers;
		}

		private static string GetString(XElement element, string key, bool isLines = false)
		{
			if (isLines)
			{
				if (element.Element(key) is XElement linesElement)
				{
					IEnumerable<XElement> lines = linesElement.Elements("line");
					return lines.Aggregate(String.Empty, (t, l) => t += l.Value + "\r\n", t => t.TrimEnd('\n', '\r'));
				}
				else if (element.Attribute(key) is XAttribute linesAttribute)
				{
					return linesAttribute.Value;
				}
				else
				{
					throw new SerializationException($"The element '{key}' could not be found in '{element.Name}'");
				}
			}
			else
			{
				return element.Attribute(key)?.Value ?? element.Element(key)?.Value ?? String.Empty;
			}
		}

		private static XObject GetXObject(XElement element, string key) => element.Attribute(key) ?? element.Element(key) as XObject;
		private static string GetValueOrInner(XElement element, string key) => element.Attribute(key)?.Value ?? element.Value ?? String.Empty;

		private const string locationsNamespaceName = "ExeModelTextFileManager.DataLocations.";

		private static readonly string SolversNamespaceName = "Aircadia.Services.Solvers.";

		private static void SerializeLines(XElement parentNode, string name, string lines) => SerializeLines(parentNode, name, lines.Split('\n'));

		private static void SerializeLines(XElement parentNode, string name, IEnumerable<string> lines)
		{
			if (lines.Count() == 1)
			{
				AddAttribute(parentNode, name, lines.First());
			}
			else
			{
				var elements = new List<XElement>(lines.Count());
				foreach (string line in lines)
				{
					elements.Add(new XElement("line", line.TrimEnd('\n', '\r')));
				}
				//return elements.ToArray();
				AddElement(parentNode, name, elements);
			}
		}


		private static string DeserializeLines(string lines)
		{
			return lines;//.Replace("# \\n #", "\n");
		}

		private Type MakeGenericOf<T>(params Type[] typeArguments) => typeof(T).GetGenericTypeDefinition().MakeGenericType(typeArguments);

		[Serializable]
		public class SerializationVersionException : Exception
		{
			public SerializationVersionException()
			{
			}

			public SerializationVersionException(string message) : base(message)
			{
			}

			public SerializationVersionException(string message, Exception innerException) : base(message, innerException)
			{
			}

			protected SerializationVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
			{
			}
		}

		public class NameTypePair
		{
			public string Name { get; }

			public Type Type { get; }

			public NameTypePair(string name, Type type)
			{
				Name = name;
				Type = type;
			}

			public NameTypePair(PropertyInfo propertyInfo) : this(propertyInfo.Name, propertyInfo.PropertyType) {}

			public NameTypePair(ParameterInfo parameterInfo) : this(parameterInfo.Name, parameterInfo.ParameterType) {}
		}
	}
}
