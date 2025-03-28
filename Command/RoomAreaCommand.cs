using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using System.Security.Cryptography;
using RoomArea.Model.Helpers;
using RoomArea.Services;

namespace RoomArea
{
    [Transaction(TransactionMode.Manual)]
    public class RoomAreaCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiDoc = commandData.Application.ActiveUIDocument;
            var doc = uiDoc.Document;
            var calculateSideAreaService = new CalculateSideAreaService(doc, uiDoc);
            return calculateSideAreaService.CalculateAndSetParameter();
        }


    }
}
