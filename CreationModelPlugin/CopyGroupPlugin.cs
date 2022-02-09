using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    #region ЗАНЯТИЕ 4. ПЛАГИН "СОЗДАНИЕ МОДЕЛИ". ЧАСТЬ 1.
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModelPlugin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            #region Пример фильтрации стен
            var res1 =new FilteredElementCollector(doc) //часть методов, это RevitApi, а часть Linq
                .OfClass(typeof(Wall)) //typeof - объектно ориентированное представление типа (карточка, информация о типе). Фильтры ревита быстрее сделают фильтрацию, и уже после можно Linq
                //.Cast<Wall>() //преобразование каждого элемента в списке в тип
                .OfType<Wall>() //метод, выполняющий фильтрацию на основе заданного типа. Этот метод будет работать даже без .OfClass(typeof(Wall)), но медленнее
                .ToList();
            //ищем типы стен (WallType)
            var res2 = new FilteredElementCollector(doc) //часть методов, это RevitApi, а часть Linq
                .OfClass(typeof(WallType)) //typeof - объектно ориентированное представление типа (карточка, информация о типе). Фильтры ревита быстрее сделают фильтрацию, и уже после можно Linq
                                       //.Cast<Wall>() //преобразование каждого элемента в списке в тип
                .OfType<WallType>() //метод, выполняющий фильтрацию на основе заданного типа. Этот метод будет работать даже без .OfClass(typeof(Wall)), но медленнее
                .ToList();
            //загружаемые семейства 
            var res3 = new FilteredElementCollector(doc) //часть методов, это RevitApi, а часть Linq
                .OfClass(typeof(FamilyInstance)) //typeof - объектно ориентированное представление типа (карточка, информация о типе). Фильтры ревита быстрее сделают фильтрацию, и уже после можно Linq
                .OfCategory(BuiltInCategory.OST_Doors) //метод для загружаемых семейств
                                       //.Cast<Wall>() //преобразование каждого элемента в списке в тип
                .OfType<FamilyInstance>() //метод, выполняющий фильтрацию на основе заданного типа. Этот метод будет работать даже без .OfClass(typeof(Wall)), но медленнее
                .ToList();
            //для конкретного типа загружаемого семейства
            //загружаемые семейства 
            var res4 = new FilteredElementCollector(doc) //часть методов, это RevitApi, а часть Linq
                .OfClass(typeof(FamilyInstance)) //typeof - объектно ориентированное представление типа (карточка, информация о типе). Фильтры ревита быстрее сделают фильтрацию, и уже после можно Linq
                .OfCategory(BuiltInCategory.OST_Doors) //метод для загружаемых семейств. Медленный Revit фильтр
                                                       //.Cast<Wall>() //преобразование каждого элемента в списке в тип
                .OfType<FamilyInstance>() //метод, выполняющий фильтрацию на основе заданного типа. Этот метод будет работать даже без .OfClass(typeof(Wall)), но медленнее
                .Where(x=>x.Name.Equals("0915 x 2314 мм"))
                .ToList();
            #endregion
            /* это может быть долго, поэтому переработаем код
            Level level1=new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Where(x => x.Name.Equals("Level 1"))
                .OfType<Level>()
                .FirstOrDefault(); //первый элемент
            */
            
            //List<Level> listLevel = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Level))
            //    .OfType<Level>()
            //    .ToList();
            Level level1 = GetLevel(doc)
                .Where(x => x.Name.Equals("Level 1"))
                .FirstOrDefault();
            Level level2 = GetLevel(doc)
                .Where(x => x.Name.Equals("Level 2"))
                .FirstOrDefault();
            //формируем список с координатами
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters); //ширина. Конвертируем в мм
            double dept = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters); //глубина. Конвертируем в мм
            double dx = width / 2;
            double dy = dept / 2;

            List<XYZ> points = new List<XYZ>
            {
                new XYZ(-dx, -dy, 0),
                new XYZ(dx, -dy, 0),
                new XYZ(dx, dy, 0),
                new XYZ(-dx, dy, 0),
                new XYZ(-dx, -dy, 0) //благодаря этому можно зациклить список и попарно построить линии из точек
            };

            List<Wall> walls = new List<Wall>(); //список созданных стен

            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            addWalls(doc, points, level1, level2);
            transaction.Commit();

            return Result.Succeeded;
        }

        private void addWalls(Document doc, List<XYZ> points, Level level1, Level level2)
        {
            List<Wall> walls = new List<Wall>();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, level1.Id, false);
                walls.Add(wall);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);
            }
        }
        public List<Level> GetLevel(Document doc)
        {
                      
            List<Level> listLevel = new FilteredElementCollector(doc)
               .OfClass(typeof(Level))
               .OfType<Level>()
               .ToList();

            return listLevel;
        }
    }
    #endregion
}
