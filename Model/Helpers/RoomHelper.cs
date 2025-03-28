using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomArea.Model.Helpers
{
    public class RoomHelper
    {
        public (List<PlanarFace>, FilteredElementCollector) FindWallsFromRoom(Room selectedRoom, Document doc)
        {
            List<PlanarFace> roomFaces = new List<PlanarFace>();
                var shell = selectedRoom.ClosedShell;
                foreach (GeometryObject obj in shell)
                {
                    if (obj is Solid solid && solid.Faces.Size > 0)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace planarFace)
                            {
                                roomFaces.Add(planarFace);
                            }
}
                    }
                }

                BoundingBoxXYZ bbox = selectedRoom.get_BoundingBox(null);
                Outline outline = new Outline(bbox.Min, bbox.Max);
                BoundingBoxIntersectsFilter filter = new BoundingBoxIntersectsFilter(outline);

                FilteredElementCollector wallCollector = new FilteredElementCollector(doc)
                    .OfClass(typeof(Wall))
                    .WherePasses(filter);
            return (roomFaces, wallCollector);

        }
        public List<ElementId> FindLimitWalls(List<PlanarFace> roomFaces, FilteredElementCollector wallCollector)
        {
            List<ElementId> foundWallIds = new List<ElementId>();
            foreach (Wall wall in wallCollector)
            {
                GeometryElement geomElem = wall.get_Geometry(new Options() { ComputeReferences = true });
                

                foreach (GeometryObject geomObj in geomElem)
                {
                    if (geomObj is Solid wallSolid && wallSolid.Faces.Size > 0)
                    {
                        foreach (Face face in wallSolid.Faces)
                        {
                            if (face is PlanarFace wallFace)
                            {
                                foreach (PlanarFace roomFace in roomFaces)
                                {
                                    XYZ n1 = wallFace.FaceNormal.Normalize();
                                    XYZ n2 = roomFace.FaceNormal.Normalize();

                                    if (n1.IsAlmostEqualTo(-n2, 1e-3))
                                    {
                                        double dist = Math.Abs((wallFace.Origin - roomFace.Origin).DotProduct(n1));
                                        if (dist < 0.01) 
                                        {
                                            foundWallIds.Add(wall.Id);
                                            goto NextWall;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            NextWall:;
            }
            return foundWallIds;


        }
        public double CalculateRoomAreaByWallHeight(Room room, List<ElementId> wallIds, Document doc)
        {

            Parameter perimeterParam = room.get_Parameter(BuiltInParameter.ROOM_PERIMETER);
            if (perimeterParam == null || !perimeterParam.HasValue)
                return 0;

            double perimeter = perimeterParam.AsDouble(); // в футах

            double maxHeight = 0;

            foreach (ElementId id in wallIds)
            {
                Wall wall = doc.GetElement(id) as Wall;
                if (wall == null) continue;

                Parameter heightParam = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (heightParam != null && heightParam.HasValue)
                {
                    double height = heightParam.AsDouble(); // в футах
                    if (height > maxHeight)
                    {
                        maxHeight = height;
                    }
                }
            }
            double area = perimeter * maxHeight; // в квадратных футах
            return area;
        }
        public double GetOpeningAreaNearRoomBoundary(Room room, List<ElementId> wallIds, List<PlanarFace> roomFaces, Document doc)
        {
            double totalArea = 0;

            foreach (ElementId wallId in wallIds)
            {
                Wall wall = doc.GetElement(wallId) as Wall;
                if (wall == null) continue;

                List<ElementId> inserts = GetAllWallInserts(wall);

                foreach (ElementId insertId in inserts)
                {
                    FamilyInstance fi = doc.GetElement(insertId) as FamilyInstance;
                    if (fi == null) continue;

                    LocationPoint loc = fi.Location as LocationPoint;
                    if (loc == null) continue;

                    XYZ insertPoint = loc.Point;
                    bool hit = false;

                    foreach (PlanarFace roomFace in roomFaces)
                    {
                        XYZ normal = roomFace.FaceNormal.Normalize();

                        Line ray = Line.CreateBound(insertPoint, insertPoint + normal.Multiply(100));

                        IntersectionResultArray resultArray;
                        SetComparisonResult result = roomFace.Intersect(ray, out resultArray);

                        if (result == SetComparisonResult.Overlap && resultArray != null && resultArray.Size > 0)
                        {
                            hit = true;
                            break;
                        }
                    }

                    if (hit)
                    {
                        double height = GetParamAsDouble(fi, BuiltInParameter.WINDOW_HEIGHT);
                        double width = GetParamAsDouble(fi, BuiltInParameter.WINDOW_WIDTH);
                        if (height == 0) height = GetParamAsDouble(fi, BuiltInParameter.DOOR_HEIGHT);
                        if (width == 0) width = GetParamAsDouble(fi, BuiltInParameter.DOOR_WIDTH);

                        if (height > 0 && width > 0)
                        {
                            totalArea += height * width;
                        }
                    }
                }
            }

            return totalArea;
        }


        private double GetParamAsDouble(FamilyInstance fi, BuiltInParameter bip)
        {
            Parameter p = fi.get_Parameter(bip);
            if (p != null && p.HasValue)
                return p.AsDouble(); // в футах
            return 0;
        }
        private List<ElementId> GetAllWallInserts(Wall wall)
        {
            HashSet<ElementId> allInserts = new HashSet<ElementId>();
            bool[] flags = { true, false };

            foreach (bool a in flags)
                foreach (bool b in flags)
                    foreach (bool c in flags)
                        foreach (bool d in flags)
                        {
                            foreach (var id in wall.FindInserts(a, b, c, d))
                                allInserts.Add(id);
                        }

            return allInserts.ToList();
        }



    }
}
