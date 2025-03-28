using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomArea.Model.Helpers
{
    public class ParamSetter
    {
        public void SetParammeter(Element element, string name, string value)
        {
            if (element == null || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Element or parameter name is invalid.");

            Parameter param = element.LookupParameter(name);

            if (param == null)
                throw new InvalidOperationException($"Parameter '{name}' not found on element {element.Id}.");

            if (param.IsReadOnly)
                throw new InvalidOperationException($"Parameter '{name}' is read-only.");

            switch (param.StorageType)
            {
                case StorageType.String:
                    param.Set(value);
                    break;

                case StorageType.Double:
                    if (double.TryParse(value, out double doubleVal))
                        param.Set(doubleVal);
                    else
                        throw new ArgumentException($"Cannot convert '{value}' to double for parameter '{name}'.");
                    break;

                case StorageType.Integer:
                    if (int.TryParse(value, out int intVal))
                        param.Set(intVal);
                    else
                        throw new ArgumentException($"Cannot convert '{value}' to int for parameter '{name}'.");
                    break;

                case StorageType.ElementId:
                    if (int.TryParse(value, out int elementIdVal))
                        param.Set(new ElementId(elementIdVal));
                    else
                        throw new ArgumentException($"Cannot convert '{value}' to ElementId for parameter '{name}'.");
                    break;

                case StorageType.None:
                default:
                    throw new NotSupportedException($"StorageType '{param.StorageType}' not supported.");
            }
        }

    }
}
