using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using RoomArea.Model.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Xml.Linq;
using RoomArea.Settings;

namespace RoomArea.Services
{
    public class CalculateSideAreaService
    {
        Document doc;
        UIDocument uiDoc;
        public Result CalculateAndSetParameter()
        {

            IList<Reference> pickedRefs = uiDoc.Selection.PickObjects(ObjectType.Element, "Выберите помещения");

            if (pickedRefs == null || pickedRefs.Count == 0)
            {
                TaskDialog.Show("Ошибка", "Не выбрано ни одного помещения.");
                return Result.Failed;
            }
            var config = ParameterConfig.Load();
            string sideWallsParameterName = config.sideWallsParameterName;

            var roomHelper = new RoomHelper();
            var paramSetter = new ParamSetter();
            //string sideWallsParameterName = "Комментарии";

            using (Transaction trans = new Transaction(doc, $"Запись площади боковых поверхностей в {sideWallsParameterName}"))
            {
                trans.Start();

                //int updatedCount = 0;

                foreach (Reference pickedRef in pickedRefs)
                {
                    Room selectedRoom = doc.GetElement(pickedRef) as Room;

                    if (selectedRoom == null)
                        continue;

                    (List<PlanarFace> planarfaces, FilteredElementCollector walls) = roomHelper.FindWallsFromRoom(selectedRoom, doc);
                    List<ElementId> foundWallIds = roomHelper.FindLimitWalls(planarfaces, walls);
                    double area = roomHelper.CalculateRoomAreaByWallHeight(selectedRoom, foundWallIds, doc);
                    double openingsArea = roomHelper.GetOpeningAreaNearRoomBoundary(selectedRoom, foundWallIds, planarfaces, doc);

                    double sideWallsArea = (area - openingsArea) * 0.092903; // перевод в метры
                    paramSetter.SetParammeter(selectedRoom, sideWallsParameterName, sideWallsArea.ToString("F2"));

                    //updatedCount++;
                }

                trans.Commit();

                //TaskDialog.Show("Готово", $"Обработано помещений: {updatedCount}");
            }

            return Result.Succeeded;
        }
        public CalculateSideAreaService(Document doc, UIDocument uiDoc)
        {
            this.uiDoc = uiDoc;
            this.doc = doc;
        }
    }
}

